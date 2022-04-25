using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Flurl;
using Microsoft.AspNetCore.Mvc;
using MyCrm.AuditLog.Grpc.Models;
using NSwag.Annotations;
using SimpleTrading.Payments.Abstractions;
using SimpleTrading.Deposit.Postgresql.Models;
using SimpleTrading.Deposit.PublicApi.Contracts.Callbacks;
using SimpleTrading.Deposit.PublicApi.Contracts.Redirects;

namespace SimpleTrading.Deposit.PublicApi.Controllers
{
    [Route("deposit/redirect")]
    [Route("v1/deposit/redirect")]
    public class RedirectController : Controller
    {
        private const string PayRetailers = "PayRetailers";

        [HttpGet("texcent")]
        public async Task<IActionResult> TexcentRedirect([FromQuery] string transaction_id, [FromQuery] string status,
            [FromQuery] string orderId, [FromQuery] string activityId)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId)
                ? Activity.Current
                : new Activity("redirect").SetParentId(activityId).Start();

            ServiceLocator.Logger.Information("TexcentRedirect transaction_id {transactionid},status {status},orderId {orderId} ", transaction_id, status, orderId);
            var targetTransaction = await ServiceLocator.DepositRepository.FindById(orderId);

            if (targetTransaction == null)
            {
                if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                    throw new Exception("Brand not found");

                var defaultRedirectUrl = depositBrand.GetStRedirectUrl();
                if (depositBrand is null) ServiceLocator.Logger.Error("Brand is null");
                ServiceLocator.Logger.Information(
                    "Transaction is null. {paymentSystem} Request redirect to {redirectLink}", "texcent",
                    defaultRedirectUrl);
                return Redirect(defaultRedirectUrl);
            }

            var baseRedirectUrl = targetTransaction.GetRedirectUrl();

            var redirectUrl = baseRedirectUrl.SetQueryParam("status", status == "successful" ? "success" : "failed");

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = targetTransaction.TraderId,
                ActionId = targetTransaction.Id,
                Action = "deposit",
                DateTime = DateTime.UtcNow,
                Message = $"texcent 3ds redirected client on redirect service. Redirection on {redirectUrl}",
                Author = "system"
            });
            ServiceLocator.Logger.Information("{paymentSystem} 3ds redirected client on redirect service. Redirection on {redirectUrl} cause status: {status}", "texcent", redirectUrl, targetTransaction.Status);

            return Redirect(redirectUrl);
        }

        [HttpGet("royalpay")]
        public async Task<IActionResult> RoyalpayRedirect([FromQuery] string uid, [FromQuery] string id,
            [FromQuery] string activityId)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId)
                ? Activity.Current
                : new Activity("redirect").SetParentId(activityId).Start();
            ServiceLocator.Logger.Information("RoyalpayRedirect uid {uid}, orderId {id} ", uid, id);
            var targetTransaction = (await ServiceLocator.DepositRepository.FindByPsId(id)).FirstOrDefault();

            if (targetTransaction == null)
            {
                if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                    throw new Exception("Brand not found");
                var defaultRedirectUrl = depositBrand.GetStRedirectUrl();
                ServiceLocator.Logger.Information(
                    "Transaction is null. {paymentSystem} Request redirect to {redirectLink}", "royalpay",
                    defaultRedirectUrl);
                return Redirect(defaultRedirectUrl);
            }

            Console.WriteLine($"Before loop: {targetTransaction.Status}");
            var currentLoop = 0;
            var maxLoopCount = 30;
            while (targetTransaction.Status == PaymentInvoiceStatusEnum.Registered && currentLoop < maxLoopCount)
            {
                targetTransaction = (await ServiceLocator.DepositRepository.FindByPsId(id)).FirstOrDefault();
                Console.WriteLine($"Loop status: {targetTransaction.Status}");
                await Task.Delay(1000);
                currentLoop++;
            }

            var baseRedirectUrl = targetTransaction.GetRedirectUrl();

            var redirectUrl = baseRedirectUrl.SetQueryParam("status",
                targetTransaction.Status == PaymentInvoiceStatusEnum.Approved ? "success" : "failed");

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = targetTransaction.TraderId,
                ActionId = targetTransaction.Id,
                Action = "deposit",
                DateTime = DateTime.UtcNow,
                Message = $"royalpay 3ds redirected client on redirect service. Redirection on {redirectUrl} cause status: {targetTransaction.Status}",
                Author = "system"
            });
            ServiceLocator.Logger.Information("{paymentSystem} 3ds redirected client on redirect service. Redirection on {redirectUrl} cause status: {status}", "royalpay", redirectUrl, targetTransaction.Status);

            return Redirect(redirectUrl);
        }

        [HttpGet("octapay")]
        public async Task<IActionResult> OctapayRedirect([FromQuery(Name = "order_id")] string orderId,
            [FromQuery(Name = "customer_order_id")]
            string customerOrderId, [FromQuery(Name = "status")] string status,
            [FromQuery(Name = "message")] string message, [FromQuery] string activityId)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId)
                ? Activity.Current
                : new Activity("redirect").SetParentId(activityId).Start();
            ServiceLocator.Logger.Information("OctapayRedirect order_id {orderId}, customer_order_id {customerOrderId}, status {status}, message {message}", orderId, customerOrderId, status, message);
            var targetTransaction = await ServiceLocator.DepositRepository.FindById(customerOrderId);

            if (targetTransaction == null)
            {
                if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                    throw new Exception("Brand not found");
                var defaultRedirectUrl = depositBrand.GetStRedirectUrl();
                ServiceLocator.Logger.Information(
                    "Transaction is null. {paymentSystem} Request redirect to {redirectLink}", "octapay",
                    defaultRedirectUrl);
                return Redirect(defaultRedirectUrl);
            }

            var baseRedirectUrl = targetTransaction.GetRedirectUrl();

            var redirectUrl = baseRedirectUrl.SetQueryParam("status", status == "success" ? "success" : "failed");

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = targetTransaction.TraderId,
                ActionId = targetTransaction.Id,
                Action = "deposit",
                DateTime = DateTime.UtcNow,
                Message = $"octapay 3ds redirected client on redirect service. Redirection on {redirectUrl} cause status: {targetTransaction.Status}",
                Author = "system"
            });
            ServiceLocator.Logger.Information("{paymentSystem} 3ds redirected client on redirect service. Redirection on {redirectUrl} cause status: {status}", "octapay", redirectUrl, targetTransaction.Status);

            return Redirect(redirectUrl);
        }

        [HttpGet("payop")]
        public async Task<IActionResult> PayopRedirect([FromQuery(Name = "orderId")] string orderId, 
            [FromQuery(Name = "invoiceId")] string invoiceId, [FromQuery(Name = "status")] string status,
            [FromQuery(Name = "txid")] string txid, [FromQuery] string activityId)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId)
                ? Activity.Current
                : new Activity("redirect").SetParentId(activityId).Start();
            ServiceLocator.Logger.Information(
                "PayopRedirect orderId {orderId}, invoiceId {invoiceId}, status {status}, txid {txid}, activityId {activityId}",
                orderId,
                invoiceId, status, txid, activityId);
            var targetTransaction = await GetTransactionByOrderIdAsync(orderId, 5);

            if (targetTransaction == null)
            {
                if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                    throw new Exception("Brand not found");
                var defaultRedirectUrl = depositBrand.GetStRedirectUrl();
                ServiceLocator.Logger.Information(
                    "Transaction is null. {paymentSystem} Request redirect to {redirectLink}", "payop",
                    defaultRedirectUrl);
                return Redirect(defaultRedirectUrl);
            }

            var redirectUrl = targetTransaction.GetRedirectUrl();
            if (targetTransaction.Status == PaymentInvoiceStatusEnum.Approved)
                redirectUrl = redirectUrl.SetQueryParam("status", "success");
            else if (targetTransaction.Status == PaymentInvoiceStatusEnum.Registered)
                redirectUrl = redirectUrl.SetQueryParam("status", "pending");
            else
                redirectUrl = redirectUrl.SetQueryParam("status", "failed");

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = targetTransaction.TraderId,
                ActionId = targetTransaction.Id,
                Action = "deposit",
                DateTime = DateTime.UtcNow,
                Message =
                    $"payop redirected client on redirect service. Redirection on {redirectUrl} cause status: {targetTransaction.Status}",
                Author = "system"
            });
            ServiceLocator.Logger.Information(
                "{paymentSystem} redirected client on redirect service. Redirection on {redirectUrl} cause status: {status}",
                "payop", redirectUrl, targetTransaction.Status);

            return Redirect(redirectUrl);
        }

        [HttpGet("volt")]
        public async Task<IActionResult> VoltRedirect([FromQuery(Name = "orderId")] string orderId,
            [FromQuery(Name = "invoiceId")] string invoiceId, [FromQuery(Name = "status")] string status,
            [FromQuery(Name = "txid")] string txid, [FromQuery] string activityId)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId)
                ? Activity.Current
                : new Activity("redirect").SetParentId(activityId).Start();
            ServiceLocator.Logger.Information(
                "VoltRedirect orderId {orderId}, invoiceId {invoiceId}, status {status}, txid {txid}, activityId {activityId}",
                orderId,
                invoiceId, status, txid, activityId);
            var targetTransaction = await GetTransactionByOrderIdAsync(orderId, 5);

            if (targetTransaction == null)
            {
                if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                    throw new Exception("Brand not found");
                var defaultRedirectUrl = depositBrand.GetStRedirectUrl();
                ServiceLocator.Logger.Information(
                    "Transaction is null. {paymentSystem} Request redirect to {redirectLink}", "volt",
                    defaultRedirectUrl);
                return Redirect(defaultRedirectUrl);
            }

            var redirectUrl = targetTransaction.GetRedirectUrl();
            if (targetTransaction.Status == PaymentInvoiceStatusEnum.Approved)
                redirectUrl = redirectUrl.SetQueryParam("status", "success");
            else if (targetTransaction.Status == PaymentInvoiceStatusEnum.Registered)
                redirectUrl = redirectUrl.SetQueryParam("status", "pending");
            else
                redirectUrl = redirectUrl.SetQueryParam("status", "failed");

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = targetTransaction.TraderId,
                ActionId = targetTransaction.Id,
                Action = "deposit",
                DateTime = DateTime.UtcNow,
                Message =
                    $"volt redirected client on redirect service. Redirection on {redirectUrl} cause status: {targetTransaction.Status}",
                Author = "system"
            });
            ServiceLocator.Logger.Information(
                "{paymentSystem} redirected client on redirect service. Redirection on {redirectUrl} cause status: {status}",
                "volt", redirectUrl, targetTransaction.Status);

            return Redirect(redirectUrl);
        }

        [HttpGet("realdeposits")]
        public async Task<IActionResult> RealdepositsRedirect([FromQuery(Name = "orderId")] string orderId,
            [FromQuery] string activityId)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId)
                ? Activity.Current
                : new Activity("redirect").SetParentId(activityId).Start();
            ServiceLocator.Logger.Information("RealdepositsRedirect orderId {orderId}", orderId);
            var targetTransaction = await GetTransactionByOrderIdAsync(orderId);
            if (targetTransaction == null)
            {
                if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                    throw new Exception("Brand not found");
                var defaultRedirectUrl = depositBrand.GetStRedirectUrl();
                ServiceLocator.Logger.Information(
                    "Transaction is null. {paymentSystem} Request redirect to {redirectLink}", "Realdeposits",
                    defaultRedirectUrl);
                return Redirect(defaultRedirectUrl);
            }

            var baseRedirectUrl = targetTransaction.GetRedirectUrl();
            var redirectUrl = baseRedirectUrl.SetQueryParam("status", targetTransaction.Status == PaymentInvoiceStatusEnum.Approved ? "success" : "failed");
            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = targetTransaction.TraderId,
                ActionId = targetTransaction.Id,
                Action = "deposit",
                DateTime = DateTime.UtcNow,
                Message = $"Realdeposits 3ds redirected client on redirect service. Redirection on {redirectUrl} cause status: {targetTransaction.Status}",
                Author = "system"
            });
            ServiceLocator.Logger.Information("{paymentSystem} 3ds redirected client on redirect service. Redirection on {redirectUrl} cause status: {status}", "realdeposits", redirectUrl, targetTransaction.Status);
            return Redirect(redirectUrl);
        }




        [HttpPost("xpate")]
        [Consumes("application/x-www-form-urlencoded")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(string))]
        public async Task<IActionResult> XpateRedirect(
            [FromQuery] string activityId, [FromForm] XpateRedirectRequest request)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId) ? Activity.Current 
                : new Activity("redirect").SetParentId(activityId).Start();

            ServiceLocator.Logger.Information(
                "XpateRedirect activityId {activityId} {@Request}",
                activityId, request);

            var targetTransaction = await GetTransactionByOrderIdAsync(request.ClientOrderId, 5);

            if (targetTransaction == null)
            {
                if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                    throw new Exception("Brand not found");
                var defaultRedirectUrl = depositBrand.GetStRedirectUrl();
                ServiceLocator.Logger.Information(
                    "Transaction is null. {paymentSystem} Request redirect to {redirectLink}", "xpate",
                    defaultRedirectUrl);
                return Redirect(defaultRedirectUrl);
            }

            var redirectUrl = targetTransaction.GetRedirectUrl();
            if (targetTransaction.Status == PaymentInvoiceStatusEnum.Approved)
                redirectUrl = redirectUrl.SetQueryParam("status", "success");
            else if (targetTransaction.Status == PaymentInvoiceStatusEnum.Registered)
                redirectUrl = redirectUrl.SetQueryParam("status", "pending");
            else
                redirectUrl = redirectUrl.SetQueryParam("status", "failed");

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = targetTransaction.TraderId,
                ActionId = targetTransaction.Id,
                Action = "deposit",
                DateTime = DateTime.UtcNow,
                Message =
                    $"xpate redirected client on redirect service. Redirection on {redirectUrl} cause status: {targetTransaction.Status}",
                Author = "system"
            });

            ServiceLocator.Logger.Information(
                "{paymentSystem} redirected client on redirect service. Redirection on {redirectUrl} cause status: {status}",
                "xpate", redirectUrl, targetTransaction.Status);

            return Redirect(redirectUrl);
        }
        
        private async Task<DepositModel> GetTransactionByOrderIdAsync(string orderId, int maxLoopCount = 15)
        {
            DepositModel targetTransaction = await ServiceLocator.DepositRepository.FindById(orderId);
            if (targetTransaction == null)
                return null;
            var currentLoop = 0;
            while (targetTransaction.Status == PaymentInvoiceStatusEnum.Registered && currentLoop < maxLoopCount)
            {
                targetTransaction = await ServiceLocator.DepositRepository.FindById(orderId);
                ServiceLocator.Logger.Information("orderId {orderId} Loop status: {status}", orderId,
                    targetTransaction.Status);
                await Task.Delay(1000);
                currentLoop++;
            }

            return targetTransaction;
        }

        [HttpGet("swiffy")]
        public async Task<IActionResult> SwiffyRedirect([FromQuery] SwiffyRedirectRequest request)
        {
            using var currentActivity = string.IsNullOrEmpty(request.ActivityId)
                ? Activity.Current
                : new Activity("redirect").SetParentId(request.ActivityId).Start();
            ServiceLocator.Logger.Information("swiffy redirect Request {@request}", request);
            var targetTransaction = await GetTransactionByOrderIdAsync(request.MerchantReference);
            if (targetTransaction == null)
            {
                if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                    throw new Exception("Brand not found");
                var defaultRedirectUrl = depositBrand.GetStRedirectUrl();
                ServiceLocator.Logger.Information(
                    "Transaction is null. {paymentSystem} Request redirect to {redirectLink}", "swiffy",
                    defaultRedirectUrl);
                return Redirect(defaultRedirectUrl);
            }

            var baseRedirectUrl = targetTransaction.GetRedirectUrl();
            var redirectUrl = baseRedirectUrl.SetQueryParam("status", targetTransaction.Status == PaymentInvoiceStatusEnum.Approved ? "success" : "failed");
            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = targetTransaction.TraderId,
                ActionId = targetTransaction.Id,
                Action = "deposit",
                DateTime = DateTime.UtcNow,
                Message = $"swiffy redirected client on redirect service. Redirection on {redirectUrl} cause status: {targetTransaction.Status}",
                Author = "system"
            });
            ServiceLocator.Logger.Information("{paymentSystem} 3ds redirected client on redirect service. Redirection on {redirectUrl} cause status: {status}", "swiffy", redirectUrl, targetTransaction.Status);

            return Redirect(redirectUrl);
        }

        [HttpGet("directa")]
        public async Task<IActionResult> DirectaRedirect([FromQuery] DirectaRedirectRequest request)
        {
            using var currentActivity = new Activity("redirect").SetParentId(request.Activity).Start();
            ServiceLocator.Logger.Information("directa redirect Request {@request}", request);
            var targetTransaction = await GetTransactionByOrderIdAsync(request.OrderId);
            if (targetTransaction == null)
            {
                if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                    throw new Exception("Brand not found");
                var defaultRedirectUrl = depositBrand.GetStRedirectUrl();
                ServiceLocator.Logger.Information(
                    "Transaction is null. {paymentSystem} Request redirect to {redirectLink}", "direct",
                    defaultRedirectUrl);
                return Redirect(defaultRedirectUrl);
            }

            var baseRedirectUrl = targetTransaction.GetRedirectUrl();
            Url redirectUrl;
            var status = "failed";
            if (targetTransaction.Status == PaymentInvoiceStatusEnum.Registered &&
                request.Status.Contains("success", StringComparison.OrdinalIgnoreCase))
            {
                ServiceLocator.Logger.Information(
                    "Directa Deposit has status {currentStatus}. But request status is {psStatus}",
                    targetTransaction.Status, request.Status);
                status = "success";
            }
            else
            {
                status = targetTransaction.Status == PaymentInvoiceStatusEnum.Approved ? "success" : "failed";
            }

            redirectUrl = baseRedirectUrl.SetQueryParam("status", status);

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = targetTransaction.TraderId,
                ActionId = targetTransaction.Id,
                Action = "deposit",
                DateTime = DateTime.UtcNow,
                Message =
                    $"directa redirected client on redirect service. Redirection on {redirectUrl} cause status: {targetTransaction.Status}",
                Author = "system"
            });
            ServiceLocator.Logger.Information(
                "{paymentSystem} 3ds redirected client on redirect service. Redirection on {redirectUrl} cause status: {status}",
                "directa", redirectUrl, targetTransaction.Status);

            return Redirect(redirectUrl);
        }

        [HttpGet("payretailers")]
        public async Task<IActionResult> PayRetailersRedirect([FromQuery(Name = "orderId")] string orderId, [FromQuery(Name = "status")] string status, [FromQuery] string activityId)
        {
            using var currentActivity = string.IsNullOrEmpty(activityId)
                ? Activity.Current
                : new Activity("redirect").SetParentId(activityId).Start();
            ServiceLocator.Logger.Information(
                "PayRetailersRedirect orderId {orderId}, status {status}, activityId {activityId}",
                orderId, status, activityId);
            var targetTransaction = await GetTransactionByOrderIdAsync(orderId, 5);

            if (targetTransaction == null)
            {
                if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                    throw new Exception("Brand not found");
                var defaultRedirectUrl = depositBrand.GetStRedirectUrl();
                ServiceLocator.Logger.Information(
                    "Transaction is null. {paymentSystem} Request redirect to {redirectLink}", PayRetailers,
                    defaultRedirectUrl);
                return Redirect(defaultRedirectUrl);
            }

            var redirectUrl = targetTransaction.GetRedirectUrl();
            if (targetTransaction.Status == PaymentInvoiceStatusEnum.Approved)
                redirectUrl = redirectUrl.SetQueryParam("status", "success");
            else if (targetTransaction.Status == PaymentInvoiceStatusEnum.Registered)
                redirectUrl = redirectUrl.SetQueryParam("status", "pending");
            else
                redirectUrl = redirectUrl.SetQueryParam("status", "failed");

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = targetTransaction.TraderId,
                ActionId = targetTransaction.Id,
                Action = "deposit",
                DateTime = DateTime.UtcNow,
                Message =
                    $"{PayRetailers} redirected client on redirect service. Redirection on {redirectUrl} cause status: {targetTransaction.Status}",
                Author = "system"
            });
            ServiceLocator.Logger.Information(
                "{paymentSystem} redirected client on redirect service. Redirection on {redirectUrl} cause status: {status}",
                PayRetailers, redirectUrl, targetTransaction.Status);

            return Redirect(redirectUrl);
        }

        [HttpGet("certusfinance")]
        public async Task<IActionResult> CertusFinanceRedirect([FromQuery] CertusFinancRedirectRequest request)
        {
            using var currentActivity = string.IsNullOrEmpty(request.ActivityId)
                ? Activity.Current
                : new Activity("redirect").SetParentId(request.ActivityId).Start();
            ServiceLocator.Logger.Information("certusfinance redirect Request {@request}", request);
            var targetTransaction = await GetTransactionByOrderIdAsync(request.OrderId);
            if (targetTransaction == null)
            {
                if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                    throw new Exception("Brand not found");
                var defaultRedirectUrl = depositBrand.GetStRedirectUrl();
                ServiceLocator.Logger.Information(
                    "Transaction is null. {paymentSystem} Request redirect to {redirectLink}", "certusfinance",
                    defaultRedirectUrl);
                return Redirect(defaultRedirectUrl);
            }

            var redirectUrl = targetTransaction.GetRedirectUrl();
            if (targetTransaction.Status == PaymentInvoiceStatusEnum.Approved)
                redirectUrl = redirectUrl.SetQueryParam("status", "success");
            else if (targetTransaction.Status == PaymentInvoiceStatusEnum.Registered)
                redirectUrl = redirectUrl.SetQueryParam("status", "pending");
            else
                redirectUrl = redirectUrl.SetQueryParam("status", "failed");

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = targetTransaction.TraderId,
                ActionId = targetTransaction.Id,
                Action = "deposit",
                DateTime = DateTime.UtcNow,
                Message = $"certusfinance redirected client on redirect service. Redirection on {redirectUrl} cause status: {targetTransaction.Status}",
                Author = "system"
            });
            ServiceLocator.Logger.Information("{paymentSystem} 3ds redirected client on redirect service. Redirection on {redirectUrl} cause status: {status}", "certusfinance", redirectUrl, targetTransaction.Status);

            return Redirect(redirectUrl);
        }
        
        [HttpGet("icemarket")]
        public async Task<IActionResult> IceMarketRedirect([FromQuery] IceMarketRedirectRequest request)
        {
            using var currentActivity =  Activity.Current;
            var paymentProviderName = "IceMarket";
            
            ServiceLocator.Logger.Information("@{PaymentProvider} redirect Request {@request}", paymentProviderName, request);
            
            var targetTransaction = await GetTransactionByOrderIdAsync(request.TransactionId);
            
            if (targetTransaction == null)
            {
                if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                    throw new Exception("Brand not found");
                var defaultRedirectUrl = depositBrand.GetStRedirectUrl();
                ServiceLocator.Logger.Information(
                    "Transaction is null. {paymentSystem} Request redirect to {redirectLink}", paymentProviderName,
                    defaultRedirectUrl);
                
                return Redirect(defaultRedirectUrl);
            }

            var redirectUrl = targetTransaction.GetRedirectUrl();
            
            if (targetTransaction.Status == PaymentInvoiceStatusEnum.Approved)
                redirectUrl = redirectUrl.SetQueryParam("status", "success");
            else if (targetTransaction.Status == PaymentInvoiceStatusEnum.Registered)
                redirectUrl = redirectUrl.SetQueryParam("status", "pending");
            else
                redirectUrl = redirectUrl.SetQueryParam("status", "failed");

            await ServiceLocator.AuditLogGrpcService.SaveAsync(new AuditLogEventGrpcModel
            {
                TraderId = targetTransaction.TraderId,
                ActionId = targetTransaction.Id,
                Action = "deposit",
                DateTime = DateTime.UtcNow,
                Message = $"{paymentProviderName} redirected client on redirect service. Redirection on {redirectUrl} cause status: {targetTransaction.Status}",
                Author = "system"
            });
            ServiceLocator.Logger.Information(
                "{paymentSystem} 3ds redirected client on redirect service. Redirection on {redirectUrl} cause status: {status}",
                paymentProviderName, redirectUrl, targetTransaction.Status);

            return Redirect(redirectUrl);
        }
    }
}