using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Finance.DirectaIntegration.GrpcContracts.Contracts;
using Microsoft.AspNetCore.Mvc;
using MyCrm.AuditLog.Grpc.Models;
using NSwag.Annotations;
using SimpleTrading.Payments.Abstractions;
using SimpleTrading.Deposit.Grpc.Contracts;
using SimpleTrading.Deposit.PublicApi.Contracts.Callbacks;
using SimpleTrading.Deposit.PublicApi.Validation;

namespace SimpleTrading.Deposit.PublicApi.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("deposit/callback")]
    [Route("v1/deposit/callback")]
    public class CallbackController : Controller
    {
        [HttpPost("exactly")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(string))]
        public async Task<IActionResult> HandleExactlyCallback([FromBody] ExactlyCallbackModel model)
        {
            ServiceLocator.Logger.Information("Got exactly callback. Callback status: {status}. Transaction Id: {trId}",
                model.Data.Charge.Attributes.Status, model.Data.Charge.TransactionId);

            await ServiceLocator.DepositManagerGrpcService.ProcessDepositAsync(model.ToGrpcCallbackRequest());
            return Ok("success");
        }

        [HttpPost("coinpay")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(string))]
        public async Task<IActionResult> HandleCoinPayCallback([FromForm] CoinPaymentCallback data)
        {
            var wallet = await ServiceLocator.WalletRepository.GetById(data.address);
            var status = data.status >= 100 ? PaymentInvoiceStatusEnum.Approved : PaymentInvoiceStatusEnum.Failed;

            ServiceLocator.Logger.Information("Got coinpay callback. Callback status: {status}. Transaction Id: {trId}",
                status.ToString(), data.txn_id);

            if (status != PaymentInvoiceStatusEnum.Approved)
                return Ok(status);

            var result =
                await ServiceLocator.DepositManagerGrpcService.HandleCryptoDepositCallback(
                    data.ToGrpcCallbackRequest(wallet.TraderId, status, wallet.AccountId));

            return Ok(result.Status);
        }

        [HttpPost("texcent")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(string))]
        public async Task<IActionResult> HandleTexcentCallback([FromBody] TexcentCallback callback,
            [FromQuery] string activityId)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId)
                ? Activity.Current
                : new Activity("callback").SetParentId(activityId).Start();
            ServiceLocator.Logger.Information("Got texcent callback. Callback status: {status}. Transaction Id: {trId}. callback {@callback}",
                callback.status, callback.orderId, callback);


            var callbackValidation = callback.IsTexcentCallbackValid(new[]
                {ServiceLocator.Settings.TexcentUserId, ServiceLocator.Settings.TexcentHandelProUserId});
            
            Console.WriteLine($"Is valid: {callbackValidation}");
            if (!callbackValidation)
            {
                ServiceLocator.Logger.Information(
                    "texcent callback Unauthorized. No Authorization header. Transaction Id: {trId}",
                    callback.orderId);
                return Unauthorized();
            }

            var pendingInvoice = await ServiceLocator.DepositRepository.FindById(callback.orderId);
            if (string.IsNullOrEmpty(pendingInvoice?.PaymentProvider) || !pendingInvoice.PaymentProvider.Contains("texcent", StringComparison.OrdinalIgnoreCase))
            {
                ServiceLocator.Logger.Information(
                    "Texcent callback Transaction PaymentProvider {paymentProvider} did not match or null. Transaction Id: {trId}. Transaction {@Transaction}",
                    pendingInvoice?.PaymentProvider, callback.orderId, pendingInvoice);
                return Ok("success");
            }

            await ServiceLocator.DepositManagerGrpcService.ProcessDepositAsync(callback.ToGrpcCallbackRequest());
            return Ok("success");
        }

        [HttpPost("royalpay")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(string))]
        public async Task<IActionResult> HandleRoyalPayCallback([FromBody] CreateRoyalPayInvoiceCallback request,
            [FromQuery] string activityId)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId)
                ? Activity.Current
                : new Activity("callback").SetParentId(activityId).Start();
            ServiceLocator.Logger.Information(
                "Got royalpay callback. Callback status: {status}. Transaction Id: {trId}. callback {@callback}",
                request.Transaction.Status, request.Transaction.TrackingId, request);

            if (!Request.Headers.ContainsKey("Authorization"))
            {
                ServiceLocator.Logger.Information(
                    "RoyalPay callback Unauthorized. No Authorization header. Transaction Id: {trId}",
                    request.Transaction.TrackingId);
                return Unauthorized();
            }

            if (!CallbackValidator.IsRoyalPayCallbackValid(Request.Headers["Authorization"]))
            {
                ServiceLocator.Logger.Information(
                    "RoyalPay callback Unauthorized. Invalid Authorization header. Transaction Id: {trId}",
                    request.Transaction.TrackingId);
                return Unauthorized();
            }

            var pendingInvoice = await ServiceLocator.DepositRepository.FindById(request.Transaction.TrackingId);
            if (string.IsNullOrEmpty(pendingInvoice?.PaymentProvider) || !pendingInvoice.PaymentProvider.Contains("royalpay", StringComparison.OrdinalIgnoreCase))
            {
                ServiceLocator.Logger.Information(
                    "RoyalPay callback Transaction PaymentProvider {paymentProvider} did not match or null by {TrackingId}. Transaction {@Transaction}",
                    pendingInvoice?.PaymentProvider, request.Transaction.TrackingId, pendingInvoice);
                return Ok("success");
            }
            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = pendingInvoice.TraderId,
                Action = "deposit",
                ActionId = pendingInvoice.Id,
                DateTime = DateTime.UtcNow,
                Message =
                    $"Got callback. Ps status: ${request.Transaction.Status}. Ps message: {request.Transaction.Message}",
                Author = "system"
            });

            await ServiceLocator.DepositManagerGrpcService.ProcessDepositAsync(request.ToGrpcCallbackRequest());
            return Ok("success");
        }

        [HttpPost("octapay")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(string))]
        public async Task<IActionResult> HandleOctapayCallback([FromBody] OctaPayCallbackRequest request,
            [FromQuery] string activityId)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId)
                ? Activity.Current
                : new Activity("callback").SetParentId(activityId).Start();
            ServiceLocator.Logger.Information(
                "Got Octapay callback. Callback status: {status}. Transaction Id: {trId}. callback {@callback}",
                request.TransactionStatus, request.CustomerOrderId, request);

            //TODO Go to Octapay and get transaction

            var pendingInvoice = await ServiceLocator.DepositRepository.FindById(request.CustomerOrderId);
            if (string.IsNullOrEmpty(pendingInvoice?.PaymentProvider) || !pendingInvoice.PaymentProvider.Contains("octapay", StringComparison.OrdinalIgnoreCase))
            {
                ServiceLocator.Logger.Information(
                    "Octapay callback Transaction PaymentProvider {paymentProvider} did not match or null by {TrackingId}. Transaction {@Transaction}",
                    pendingInvoice?.PaymentProvider, request.CustomerOrderId, pendingInvoice);
                return Ok("success");
            }
            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = pendingInvoice.TraderId,
                Action = "deposit",
                ActionId = pendingInvoice.Id,
                DateTime = DateTime.UtcNow,
                Message =
                    $"Got callback. Ps status: ${request.TransactionStatus}. Ps message: {request.Reason}",
                Author = "system"
            });

            await ServiceLocator.DepositManagerGrpcService.ProcessDepositAsync(request.ToGrpcCallbackRequest());
            return Ok("success");
        }

        [HttpPost("payop")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(string))]
        public async Task<IActionResult> HandlePayopCallback([FromBody] PayopCallbackRequest request,
            [FromQuery] string activityId)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId)
                ? Activity.Current
                : new Activity("callback").SetParentId(activityId).Start();
            ServiceLocator.Logger.Information(
                "Got Payop callback. Callback status: {status}. Transaction Id: {trId}. callback {@callback}",
                request.Invoice.Status, request.Transaction.Id, request);

            //TODO Go to Payop and get transaction

            var pendingInvoice = await ServiceLocator.DepositRepository.FindById(request.Transaction.Order.Id);
            //Postgresql.Models.DepositModel pendingInvoice = pendingInvoices.FirstOrDefault(x => x.PaymentProvider.Contains("payop", StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(pendingInvoice?.PaymentProvider) || !pendingInvoice.PaymentProvider.Contains("payop", StringComparison.OrdinalIgnoreCase))
            {
                ServiceLocator.Logger.Information(
                    "Payop callback Transaction PaymentProvider {paymentProvider} did not match or null by {InvoiceId}. Transaction {@Transaction}. Request {@request}",
                    pendingInvoice?.PaymentProvider, request.Invoice.Id, pendingInvoice, request);
                return Ok("success");
            }

            if (request.Transaction.IsPending)
            {
                ServiceLocator.Logger.Information(
                    "Payop callback Transaction PaymentProvider {paymentProvider} is pending. Skip processing.",
                    pendingInvoice?.PaymentProvider);
                return Ok("success");
            }

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = pendingInvoice.TraderId,
                Action = "deposit",
                ActionId = pendingInvoice.Id,
                DateTime = DateTime.UtcNow,
                Message =
                    $"Got callback. Ps status: ${request.Invoice.Status}. Ps message: {request.Transaction.Error.Message}",
                Author = "system"
            });

            await ServiceLocator.DepositManagerGrpcService.ProcessDepositAsync(request.ToGrpcCallbackRequest(pendingInvoice));
            return Ok("success");
        }

        [HttpPost("volt")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(string))]
        public async Task<IActionResult> HandleVoltCallback([FromBody] PayopCallbackRequest request,
            [FromQuery] string activityId)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId)
                ? Activity.Current
                : new Activity("callback").SetParentId(activityId).Start();
            ServiceLocator.Logger.Information(
                "Got Volt callback. Callback status: {status}. Transaction Id: {trId}. callback {@callback}",
                request.Invoice.Status, request.Transaction.Id, request);

            //TODO Go to Volt and get transaction

            var pendingInvoice = await ServiceLocator.DepositRepository.FindById(request.Transaction.Order.Id);
            if (string.IsNullOrEmpty(pendingInvoice?.PaymentProvider) ||
                !pendingInvoice.PaymentProvider.Contains("volt", StringComparison.OrdinalIgnoreCase))
            {
                ServiceLocator.Logger.Information(
                    "Volt callback Transaction PaymentProvider {paymentProvider} did not match or null by {InvoiceId}. Transaction {@Transaction}. Request {@request}",
                    pendingInvoice?.PaymentProvider, request.Invoice.Id, pendingInvoice, request);
                return Ok("success");
            }

            if (request.Transaction.IsPending)
            {
                ServiceLocator.Logger.Information(
                    "Volt callback Transaction PaymentProvider {paymentProvider} is pending. Skip processing.",
                    pendingInvoice?.PaymentProvider);
                return Ok("success");
            }

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = pendingInvoice.TraderId,
                Action = "deposit",
                ActionId = pendingInvoice.Id,
                DateTime = DateTime.UtcNow,
                Message =
                    $"Got callback. Ps status: ${request.Invoice.Status}. Ps message: {request.Transaction.Error.Message}",
                Author = "system"
            });

            await ServiceLocator.DepositManagerGrpcService.ProcessDepositAsync(
                request.ToGrpcCallbackRequest(pendingInvoice));
            return Ok("success");
        }

        [HttpPost("realdeposits")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(string))]
        public async Task<IActionResult> HandleRealDepositsCallback([FromBody] RealDepositsCallbackRequest request,
            [FromQuery] string activityId)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId)
                ? Activity.Current
                : new Activity("redirect").SetParentId(activityId).Start();
            ServiceLocator.Logger.Information(
                "Got RealDeposits callback. Callback status: {status}. Transaction Id: {trId}. callback {@callback}",
                request.TransactionStatus, request.TransactionId, request);

            //TODO Go to realdeposits and get transaction

            var pendingInvoice = await ServiceLocator.DepositRepository.FindById(request.OrderId);
            if (string.IsNullOrEmpty(pendingInvoice?.PaymentProvider) || !pendingInvoice.PaymentProvider.Contains("realdeposits", StringComparison.OrdinalIgnoreCase))
            {
                ServiceLocator.Logger.Information(
                    "RealDeposits callback Transaction PaymentProvider {paymentProvider} did not match by orderId {OrderId}. Transaction {@Transaction}. Request {@request}",
                    pendingInvoice?.PaymentProvider, request.OrderId, pendingInvoice, request);
                return Ok("success");
            }

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = pendingInvoice.TraderId,
                Action = "deposit",
                ActionId = pendingInvoice.Id,
                DateTime = DateTime.UtcNow,
                Message =
                    $"Got callback. Ps status: ${request.TransactionStatus}. Ps message: {request.ErrorCode}.{request.ErrorDetails}",
                Author = "system"
            });

            await ServiceLocator.DepositManagerGrpcService.ProcessDepositAsync(request.ToGrpcCallbackRequest(pendingInvoice));
            return Ok("success");
        }

        [HttpPost("swiffy")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(string))]
        public async Task<IActionResult> HandleSwiffyCallback([FromForm] SwiffyCallback request,
            [FromQuery] string activityId)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId)
                ? Activity.Current
                : new Activity("redirect").SetParentId(activityId).Start();
            ServiceLocator.Logger.Information(
                "Got Swiffy callback. Callback status: {status}. Transaction Id: {trId}. callback {@callback}",
                request.Status, request.CallpayTransactionId, request);

            //TODO Go to swiffy and get transaction

            var pendingInvoice = await ServiceLocator.DepositRepository.FindById(request.MerchantReference);

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = pendingInvoice.TraderId,
                Action = "deposit",
                ActionId = pendingInvoice.Id,
                DateTime = DateTime.UtcNow,
                Message =
                    $"Got callback. Ps status: ${request.Status}. Ps message: {request.Reason}.",
                Author = "system"
            });

            await ServiceLocator.DepositManagerGrpcService.ProcessDepositAsync(request.ToGrpcCallbackRequest(pendingInvoice));
            return Ok("success");
        }

        [HttpPost("directa")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(string))]
        public async Task<IActionResult> HandleDirectaCallback([FromBody] DirectaCallback request,
            [FromQuery] string activity)
        {
            using var currentActivity = new Activity("callback").SetParentId(activity).Start();
            ServiceLocator.Logger.Information(
                "Got directa callback. callback {@callback}. ActivityId {activity}", request, activity);

            var pendingInvoices =
                await ServiceLocator.DepositRepository.FindByPsId(request.PsTransactionId);

            var pendingInvoice = pendingInvoices.FirstOrDefault(x =>
                x.PaymentSystem.Contains("directa", StringComparison.OrdinalIgnoreCase));

            if (pendingInvoice is null)
            {
                ServiceLocator.Logger.Error("Directa invoice is null. PsTransactionId {PsTransactionId}",
                    request.PsTransactionId);
                return new NotFoundResult();
            }

            var deposit = await ServiceLocator.FinanceDirectaIntegrationGrpcService.GetDepositAsync(
                new GetDirectaDepositGrpcRequest
                {
                    PsTransactionId = request.PsTransactionId,
                    Brand = pendingInvoice.Brand.ToString()
                });
            if (deposit.RequestStatus != DirectaDepositRequestStatus.Success)
            {
                ServiceLocator.Logger.Error("Directa get deposit failed. deposit {@deposit}", deposit);
                return new BadRequestResult();
            }

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = pendingInvoice.TraderId,
                Action = "deposit",
                ActionId = pendingInvoice.Id,
                DateTime = DateTime.UtcNow,
                Message =
                    $"Got callback. Ps status: ${deposit.Deposit.Status}. Ps message: {deposit.ErrorMessage}.",
                Author = "system"
            });

            if (deposit.Deposit.IsPending())
            {
                ServiceLocator.Logger.Information(
                    "Directa return Ok result. deposit has status pending. deposit {@deposit}", deposit);
                return Ok();
            }

            await ServiceLocator.DepositManagerGrpcService.ProcessDepositAsync(
                deposit.ToGrpcCallbackRequest(pendingInvoice));
            return Ok("success");
        }

        [HttpPost("payretailers")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(string))]
        public async Task<IActionResult> HandlePayRetailersCallback([FromBody] PayRetailersCallback request,
            [FromQuery] string activityId)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId)
                ? Activity.Current
                : new Activity("callback").SetParentId(activityId).Start();
            ServiceLocator.Logger.Information(
                "Got PayRetailers callback. Callback status: {status}. Transaction Id: {trId}. callback {@callback}",
                request.Status, request.TrackingId, request);

            //TODO Go to PayRetailers and get transaction

            var pendingInvoice = await ServiceLocator.DepositRepository.FindById(request.TrackingId);
            if (string.IsNullOrEmpty(pendingInvoice?.PaymentProvider) ||
                !pendingInvoice.PaymentProvider.Contains("PayRetailers", StringComparison.OrdinalIgnoreCase))
            {
                ServiceLocator.Logger.Information(
                    "PayRetailers callback Transaction PaymentProvider {paymentProvider} did not match or null by {InvoiceId}. Transaction {@Transaction}. Request {@request}",
                    pendingInvoice?.PaymentProvider, request.TrackingId, pendingInvoice, request);
                return Ok("success");
            }

            if (request.IsPending)
            {
                ServiceLocator.Logger.Information(
                    "PayRetailers callback Transaction PaymentProvider {paymentProvider} is pending. Skip processing.",
                    pendingInvoice?.PaymentProvider);
                return Ok("success");
            }

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = pendingInvoice.TraderId,
                Action = "deposit",
                ActionId = pendingInvoice.Id,
                DateTime = DateTime.UtcNow,
                Message =
                    $"Got callback. Ps status: ${request.Status}. Ps message: {request.Message}",
                Author = "system"
            });

            await ServiceLocator.DepositManagerGrpcService.ProcessDepositAsync(
                request.ToGrpcCallbackRequest(pendingInvoice));
            return Ok("success");
        }


        [HttpGet("xpate")]
        public async Task<IActionResult> HandleXpateCallback([FromQuery(Name = "client_orderid")] string clientOrderId,
            [FromQuery(Name = "status")] string status, [FromQuery(Name = "error_message")] string errorMessage,
            [FromQuery(Name = "amount")] string amount, [FromQuery(Name = "currency")] string currency,
            [FromQuery(Name = "error_code")] string errorCode, [FromQuery(Name = "processor-tx-id")] string txId,
            [FromQuery(Name = "orderid")] string orderid, [FromQuery] string activityId)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId)
                ? Activity.Current
                : new Activity("callback").SetParentId(activityId).Start();
            ServiceLocator.Logger.Information(
                "Got XpateDeposits callback. Callback status: {status} order: {clientOrderId} xpate transaction: {txId} ",
                status, clientOrderId, txId);

            var pendingInvoice = await ServiceLocator.DepositRepository.FindById(clientOrderId);
            if (string.IsNullOrEmpty(pendingInvoice?.PaymentProvider) || !pendingInvoice.PaymentProvider.Contains("xpate", StringComparison.OrdinalIgnoreCase))
            {
                ServiceLocator.Logger.Information(
                    "XpateDeposits callback Transaction PaymentProvider {paymentProvider} did not match by orderId {OrderId}. Transaction {@Transaction}.",
                    pendingInvoice?.PaymentProvider, clientOrderId, pendingInvoice);
                return Ok("success");
            }

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = pendingInvoice.TraderId,
                Action = "deposit",
                ActionId = pendingInvoice.Id,
                DateTime = DateTime.UtcNow,
                Message =
                    $"Got callback. Ps status: ${status}. Ps message: {errorCode}.{errorMessage}",
                Author = "system"
            });

            ProcessDepositRequest request = new ProcessDepositRequest()
            {
                PsTransactionId = txId,
                TransactionId = clientOrderId,
                Comment = "Handled callback on deposit rest service",
                Author = "System",
                PaymentInvoiceStatus = status.Equals("approved", StringComparison.OrdinalIgnoreCase)
                    ? PaymentInvoiceStatusEnum.Approved
                    : PaymentInvoiceStatusEnum.Failed
            };

            try
            {
                await ServiceLocator.DepositManagerGrpcService.ProcessDepositAsync(request);
            }
            catch (Exception e)
            {
                ServiceLocator.Logger.Error(
                    "XpateDeposits callback exception {paymentProvider} {@Request} {@Message}",
                    pendingInvoice?.PaymentProvider, request, e.Message);
            }
            
            ServiceLocator.Logger.Information(
                "XpateDeposits callback success {paymentProvider} {@Request}.",
                pendingInvoice?.PaymentProvider, request);
            return Ok("success");
        }

        [HttpPost("certusfinance")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(string))]
        public async Task<IActionResult> HandleCertusFinanceCallback([FromBody] CertusFinanceCallback request,
        [FromQuery] string activityId)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId)
                ? Activity.Current
                : new Activity("callback").SetParentId(activityId).Start();
            ServiceLocator.Logger.Information(
                "Got CertusFinance callback. Callback status: {status} message: {message}. Transaction Id: {trId}. Order Id: {orderId} callback {@callback}",
                request.Result.ResultCode, request.Result.ResultMessage, request.TxId, request.OrderId, request);

            //TODO Go to CertusFinance and get transaction

            var pendingInvoice = await ServiceLocator.DepositRepository.FindById(request.OrderId);
            if (string.IsNullOrEmpty(pendingInvoice?.PaymentProvider) ||
                !pendingInvoice.PaymentProvider.Contains("CertusFinance", StringComparison.OrdinalIgnoreCase))
            {
                ServiceLocator.Logger.Information(
                    "CertusFinance callback Transaction PaymentProvider {paymentProvider} did not match or null by {InvoiceId}. Transaction {@Transaction}. Request {@request}",
                    pendingInvoice?.PaymentProvider, request.OrderId, pendingInvoice, request);
                return Ok("success");
            }

            if (request.IsPending)
            {
                ServiceLocator.Logger.Information(
                    "CertusFinance callback Transaction PaymentProvider {paymentProvider} is pending. Skip processing.",
                    pendingInvoice?.PaymentProvider);
                return Ok("success");
            }

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = pendingInvoice.TraderId,
                Action = "deposit",
                ActionId = pendingInvoice.Id,
                DateTime = DateTime.UtcNow,
                Message =
                    $"Got callback. Ps status: ${request.Result.ResultCode}. Ps message: {request.Result.ResultMessage}",
                Author = "system"
            });

            await ServiceLocator.DepositManagerGrpcService.ProcessDepositAsync(
                request.ToGrpcCallbackRequest(pendingInvoice));
            return Ok("success");
        }

        [HttpPost("icemarket")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(string))]
        public async Task<IActionResult> HandleIceMarketCallback([FromForm] IceMarketCallBack request)
        {
            ServiceLocator.Logger.Information("Got IceMarket callback. Callback request {@request}", request);

            var pendingInvoice = await ServiceLocator.DepositRepository.FindById(request.TransactionId);
            
            if (string.IsNullOrEmpty(pendingInvoice?.PaymentProvider) ||
                !pendingInvoice.PaymentProvider.Contains("icemarket", StringComparison.OrdinalIgnoreCase))
            {
                ServiceLocator.Logger.Information(
                    "IceMarket callback. Transaction PaymentProvider {paymentProvider} did not match. Request {@request}",
                    pendingInvoice?.PaymentProvider, request);
                return Ok("success");
            }

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = pendingInvoice.TraderId,
                Action = "deposit",
                ActionId = pendingInvoice.Id,
                DateTime = DateTime.UtcNow,
                Message =
                    $"Got callback. Ps status: ${request.Status}. Ps code: {request.ResponseCode}",
                Author = "system"
            });

            try
            {
                await ServiceLocator.DepositManagerGrpcService.ProcessDepositAsync(new ProcessDepositRequest
                {
                    PsTransactionId = request.TransactionId,
                    TransactionId = request.TransactionId,
                    Comment = "Handled callback on deposit rest service",
                    Author = "System",
                    PaymentInvoiceStatus = request.Status.Equals("approved", StringComparison.OrdinalIgnoreCase)
                        ? PaymentInvoiceStatusEnum.Approved
                        : PaymentInvoiceStatusEnum.Failed
                });

            }
            catch (Exception e)
            {
                ServiceLocator.Logger.Error(e,
                    "IceMarket callback exception {paymentProvider} {@Message}",
                    pendingInvoice?.PaymentProvider, e.Message);
            }
            return Ok("success");
        }
    }
}