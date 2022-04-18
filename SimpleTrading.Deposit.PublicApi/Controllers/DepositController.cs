using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Finance.CardValidator;
using Finance.DirectaIntegration.GrpcContracts.Contracts;
using Finance.DirectaPublic.HttpContracts.Requests;
using Finance.DirectaPublic.HttpContracts.Responses;
using Finance.PayopIntegration.GrpcContracts.Contracts;
using Finance.PayRetailersIntegration.GrpcContracts.Contracts;
using Finance.PciDssIntegration.GrpcContracts.Contracts;
using Finance.PciDssPublic.HttpContracts.Requests;
using Finance.PciDssPublic.HttpContracts.Responses;
using Finance.SwiffyIntegration.GrpcContracts.Contracts;
using Finance.SwiffyPublic.HttpContracts.Requests;
using Finance.SwiffyPublic.HttpContracts.Responses;
using Finance.VoltIntegration.GrpcContracts.Contracts;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NSwag.Annotations;
using SimpleTrading.Common.MyNoSql.DepositRestrictions;
using SimpleTrading.Payments.Abstractions;
using SimpleTrading.Deposit.Grpc;
using SimpleTrading.Deposit.Grpc.Contracts;
using SimpleTrading.Deposit.PublicApi.Contracts;
using SimpleTrading.Deposit.PublicApi.Contracts.ABSplits;
using SimpleTrading.Deposit.PublicApi.Extentions;
using SimpleTrading.Deposit.PublicApi.Validation;
using SimpleTrading.Payments.Abstractions.DepositRestrictions;
using SimpleTrading.PersonalData.Grpc.Contracts;
using SimpleTrading.TraderExternalData.Grpc.Contracts;

namespace SimpleTrading.Deposit.PublicApi.Controllers
{
    [Route("deposit")]
    [Route("v1/deposit")]
    [ApiController]
    [Produces("application/json")]
    public class DepositController : Controller
    {
        [HttpPost("create")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(CreatePaymentInvoiceResponse))]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(string))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(string))]
        public async Task<IActionResult> CreatePaymentInvoice([FromBody] CreatePaymentInvoiceRequest request)
        {
            if (request.AccountId.Contains("std") || request.AccountId.Contains("mtd"))
                return Ok(CreatePciDssInvoiceResponse.Create(DepositRequestStatus.ServerError, null));

            if (!HttpContext.TryGetTraderId(out var traderId))
            {
                ServiceLocator.Logger.Information("TraderId was not found for request {@request}", request);
                return Unauthorized("Unauthorized");
            }

            var validationResult = request.Validate();
            if (!validationResult.IsValid)
                return Ok(validationResult.Errors);

            try
            {
                var createInvoiceRequest = new CreatePaymentInvoiceGrpcRequest()
                {
                    DepositSum = request.DepositSum,
                    Currency = request.Currency,
                    AccountId = request.AccountId,
                    TraderId = traderId,
                    Comment = "Createing invoice deposit rest service",
                    PaymentSystemId = request.PaymentMethod,
                    Author = "System"
                };

                var response =
                    await ServiceLocator.DepositManagerGrpcService.CreatePaymentInvoiceAsync(createInvoiceRequest);

                return Json(CreatePaymentInvoiceResponse.Create(response.RedirectUrl));
            }
            catch (Exception e)
            {
                ServiceLocator.Logger.Error(e, e.Message);
                if (e is UnauthorizedAccessException)
                    return Unauthorized("Unauthorized");

                return Json(request.CreateErrorResponseForCreatePaymentInvoice());
            }
        }

        [HttpPost("GetCryptoWallet")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(GetCryptoWalletAddressResponse))]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(string))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(string))]
        public async Task<IActionResult> GetCryptoWallet([FromBody] GetCryptoWalletRequest request)
        {
            if (!HttpContext.TryGetTraderId(out var traderId))
            {
                ServiceLocator.Logger.Information("TraderId was not found for request {@request}", request);
                return Unauthorized("Unauthorized");
            }

            var validationResult = request.Validate();
            if (!validationResult.IsValid)
                return Ok(validationResult.Errors);

            try
            {
                var getCryptoWalletRequest =
                    GetCryptoWalletAddressRequest.Create(traderId, request.Currency,
                        request.AccountId);

                var response = await ServiceLocator.DepositManagerGrpcService.GetCryptoWalletForClient(
                    getCryptoWalletRequest);
                return Json(response);
            }
            catch (Exception e)
            {
                ServiceLocator.Logger.Error(e, e.Message);
                return Json(new GetCryptoWalletAddressResponse
                {
                    WalletAddress = null,
                    Status = DepositManagerStatusEnum.Error
                });
            }
        }


        [HttpPost("CreatePciDssInvoice")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(CreatePciDssInvoiceResponse))]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(CreatePciDssInvoiceBadResponse))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(string))]
        public async Task<IActionResult> CreatePciDssInvoice([FromBody] CreatePciDssInvoiceRequest request)
        {
            ServiceLocator.Logger.Information("Process CreatePciDssInvoiceRequest for request {@request} with headers {@headers}", request, HttpContext.Request.Headers);
            var validationResult = request.Validate();
            if (!validationResult.IsValid)
            {
                var errors =
                    validationResult.Errors.Select(item => ErrorEntity.Create(item.PropertyName, item.ErrorMessage));
                ServiceLocator.Logger.Information("Validation failed with error {@error} for request {@request}", errors, request);
                return Ok(
                    CreatePciDssInvoiceBadResponse.Create(DepositRequestStatus.InvalidCredentials, null, errors));
            }

            if (!request.AccountId.Contains("stl") && !request.AccountId.Contains("mtl"))
            {
                ServiceLocator.Logger.Information("Account {account} is not stl or mtl for request {@request}", request.AccountId, request);
                return Ok(CreatePciDssInvoiceResponse.Create(DepositRequestStatus.ServerError, null));
            }

            if (!HttpContext.TryGetTraderId(out var traderId))
            {
                ServiceLocator.Logger.Information("TraderId was not found for request {@request}", request);
                return Unauthorized("Unauthorized");
            }

            var cardValidator = new CardValidator(request.CardNumber);

            if (!cardValidator.IsCardValid())
            {
                ServiceLocator.Logger.Information("Card is not valid for request {@request}", request);
                return Ok(CreatePciDssInvoiceResponse.Create(DepositRequestStatus.InvalidCardNumber, null));
            }
            
            var restrictionsModel = ServiceLocator.DepositRestrictionsReader.Get(DepositRestrictionNoSqlEntity.GenerateRowKey(traderId));
            ServiceLocator.Logger.Information("Trader restrictionsModel: {@traderId}, {@restrictionsModel}", traderId, restrictionsModel);

            if (restrictionsModel != null && restrictionsModel.Restrictions.HasFlag(DepositRestrictions.BankCards))
            {
                ServiceLocator.Logger.Warning("Card deposit is disabled for trader {@traderId}, {@restrictionsModel}", traderId, restrictionsModel);
                return Ok(CreatePciDssInvoiceResponse.Create(DepositRequestStatus.PaymentDisabled, null));
            }
            
            //if (cardValidator.GetCardType() != CardType.Visa && cardValidator.GetCardType() != CardType.MasterCard)
            //{
            //    ServiceLocator.Logger.Information("Card type {cardType} not supported for request {@request}", cardValidator.GetCardType(), request);
            //    return Ok(CreatePciDssInvoiceResponse.Create(DepositRequestStatus.UnsupportedCardType, null));
            //}

            var pd = await ServiceLocator.PersonalDataServiceGrpc.GetByIdAsync(traderId);
            request.EnrichByPersonalData(pd);

            var ip = HttpContext.GetIp();
            if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                depositBrand = Enum.Parse<BrandName>(pd.PersonalData.BrandId, true);
            if (depositBrand is null)
            {
                ServiceLocator.Logger.Error("Brand is null");
                return Ok(CreatePciDssInvoiceResponse.Create(DepositRequestStatus.ServerError, null));
            }

            double totalDepositInUsd = 0;
            var deposits = await ServiceLocator.DepositRepository.FindByTraderId(traderId);
            foreach (var deposit in deposits)
            {
                if (deposit.Status == PaymentInvoiceStatusEnum.Approved)
                    totalDepositInUsd += deposit.Amount;
            }

            ServiceLocator.Logger.Information("Using {brand} brand and card type {cardType} for request {@request}",
                depositBrand.ToString(), cardValidator.GetCardType(), request);

            var source = await RequestUtils.GetTrafficSourceAsync(pd.PersonalData.Id);

            try
            {
                var response =
                    await ServiceLocator.FinancePciDssIntegrationGrpcService.MakeDepositAsync(
                        request.CreatePciDssInvoiceRequest(traderId, ip, (BrandName)depositBrand,
                            pd.PersonalData?.KYC.ToString(), totalDepositInUsd, source));
                ServiceLocator.Logger.Information("CreatePciDssInvoice. Got response {@response} from integration service", response);
                return Ok(CreatePciDssInvoiceResponse.Create(response.Status, response.SecureRedirectUrl));
            }
            catch (Exception e)
            {
                ServiceLocator.Logger.Error(e, e.Message);
                return Ok(CreatePciDssInvoiceResponse.Create(DepositRequestStatus.ServerError, null));
            }
        }


        [HttpGet("supported-payment-systems")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(DepositResponse<GetSupportedPaymentSystemsResponse>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(string))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(string))]
        public async Task<IActionResult> GetSupportedPaymentSystems([FromQuery] GetSupportedPaymentSystemsRequest request)
        {
            if (!HttpContext.TryGetTraderId(out var traderId))
            {
                ServiceLocator.Logger.Information("TraderId was not found for request {@request}", request);
                return Unauthorized("Unauthorized");
            }

            GetSupportedPaymentSystemsResponse response = null;
            try
            {
                var pd = await ServiceLocator.PersonalDataServiceGrpc.GetByIdAsync(traderId);
                if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                    depositBrand = Enum.Parse<BrandName>(pd.PersonalData.BrandId, true);
                if (depositBrand is null)
                {
                    ServiceLocator.Logger.Error("Brand is null");
                    throw new Exception("Brand is null");
                }

                var result = await ServiceLocator.DepositManagerGrpcService.GetPaymentSystemsAsync(
                    GetPaymentSystemsRequest.Create(traderId, depositBrand.ToString(), pd.PersonalData.GetCountry()));

                var notSortedBankAndBitcoin = await NotSortedBankAndBitcoinAsync(traderId, result);

                response = new GetSupportedPaymentSystemsResponse
                {
                    SupportedPaymentSystems = notSortedBankAndBitcoin
                };
            }
            catch (Exception ex)
            {
                ServiceLocator.Logger.Error(ex, ex.Message);
            }
            finally
            {
                var defaultPaymentSystem = new GetSupportedPaymentSystemsResponse
                {
                    SupportedPaymentSystems = new List<PaymentSystem>
                    {
                        PaymentSystem.Create(PaymentSystemType.BankCards),
                        PaymentSystem.Create(PaymentSystemType.Wiretransfer),
                        PaymentSystem.Create(PaymentSystemType.Bitcoin)
                    }
                };

                response ??= defaultPaymentSystem;
            }

            return Ok(DepositResponse<GetSupportedPaymentSystemsResponse>.Success(response));
        }

        [HttpPost("swiffy/invoice")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(DepositResponse<CreateSwiffyInvoiceResponse>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(DepositResponse<CreateSwiffyInvoiceBadResponse>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(string))]
        public async Task<IActionResult> CreateSwiffyInvoice([FromBody] CreateSwiffyInvoiceRequest request)
        {
            ServiceLocator.Logger.Information("Process CreateSwiffyInvoice for request {@request} with headers {@headers}", request, HttpContext.Request.Headers);
            var validationResult = request.Validate();
            if (!validationResult.IsValid)
            {
                var errors =
                    validationResult.Errors.Select(item => ErrorEntity.Create(item.PropertyName, item.ErrorMessage));
                ServiceLocator.Logger.Information("Validation failed with error {@error}", errors);
                return Ok(DepositResponse<CreateSwiffyInvoiceBadResponse>.Create(
                    CreateSwiffyInvoiceBadResponse.Create(errors), DepositRequestStatus.ServerError));
            }

            if (!request.AccountId.Contains("stl") && !request.AccountId.Contains("mtl"))
            {
                ServiceLocator.Logger.Information("Account {account} is not stl or mtl", request.AccountId);
                return Ok(DepositResponse<CreateSwiffyInvoiceResponse>.Create(CreateSwiffyInvoiceResponse.Empty, DepositRequestStatus.ServerError));
            }

            if (!HttpContext.TryGetTraderId(out var traderId))
            {
                ServiceLocator.Logger.Information("TraderId was not found for request {@request}", request);
                return Unauthorized("Unauthorized");
            }

            PersonalDataGrpcResponseContract pd = await ServiceLocator.PersonalDataServiceGrpc.GetByIdAsync(traderId);

            var ip = HttpContext.GetIp();
            if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                depositBrand = Enum.Parse<BrandName>(pd.PersonalData.BrandId, true);

            if (depositBrand is null)
            {
                ServiceLocator.Logger.Error("Brand is null");
                return Ok(DepositResponse<CreateSwiffyInvoiceResponse>.Create(CreateSwiffyInvoiceResponse.Empty,
                    DepositRequestStatus.ServerError));
            }

            ServiceLocator.Logger.Information("Using {brand} brand", depositBrand.ToString());

            try
            {
                DepositResponse<CreateSwiffyInvoiceResponse> response = null;
                var paymentSystem = await ServiceLocator.DepositManagerGrpcService.GetPaymentSystemsAsync(GetPaymentSystemsRequest.Create(traderId, depositBrand.ToString(), pd.PersonalData.GetCountry()));
                if (paymentSystem.PaymentSystems?.Any(x => x.PaymentSystemId.Contains("swiffy", StringComparison.OrdinalIgnoreCase)) == true)
                {
                    var swiffyDepositGrpcResponse = await ServiceLocator.MakeSwiffyDepositProcessIdService
                        .GetOrCreateAsync(request.ProcessId + traderId,
                            () => ServiceLocator.FinanceSwiffyIntegrationGrpcService.MakeDepositAsync(
                                request.ToMakeSwiffyDepositGrpcRequest(pd, depositBrand.ToString())));

                    if (swiffyDepositGrpcResponse.Status == SwiffyDepositRequestStatus.Success)
                    {
                        response = DepositResponse<CreateSwiffyInvoiceResponse>.Success(CreateSwiffyInvoiceResponse.Create(swiffyDepositGrpcResponse.RedirectUrl));
                    }
                    else
                    {
                        response = DepositResponse<CreateSwiffyInvoiceResponse>.Create(CreateSwiffyInvoiceResponse.Empty, DepositRequestStatus.ServerError);
                    }

                }
                else
                {
                    ServiceLocator.Logger.Warning("CreateSwiffyInvoice. paymentSystem not supported {@paymentSystem}", paymentSystem);
                    response = DepositResponse<CreateSwiffyInvoiceResponse>.Create(CreateSwiffyInvoiceResponse.Empty, DepositRequestStatus.ServerError);
                }
                ServiceLocator.Logger.Information("CreateSwiffyInvoice. Return response {@response}", response);
                return Ok(response);
            }
            catch (Exception e)
            {
                ServiceLocator.Logger.Error(e, e.Message);
                return Ok(DepositResponse<CreateSwiffyInvoiceResponse>.Create(CreateSwiffyInvoiceResponse.Empty, DepositRequestStatus.ServerError));
            }
        }

        [HttpPost("directa/invoice")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(DepositResponse<CreateDirectaInvoiceResponse>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(DepositResponse<CreateDirectaInvoiceBadResponse>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(string))]
        public async Task<IActionResult> CreateDirectaInvoice([FromBody] CreateDirectaInvoiceRequest request)
        {
            ServiceLocator.Logger.Information(
                "Process CreateDirectaInvoice for request {@request} with headers {@headers}", request,
                HttpContext.Request.Headers);
            var validationResult = request.Validate();
            if (!validationResult.IsValid)
            {
                var errors =
                    validationResult.Errors.Select(item => ErrorEntity.Create(item.PropertyName, item.ErrorMessage));
                ServiceLocator.Logger.Information("Validation failed with error {@error}", errors);
                return Ok(DepositResponse<CreateDirectaInvoiceBadResponse>.Create(
                    CreateDirectaInvoiceBadResponse.Create(errors), DepositRequestStatus.ServerError));
            }

            if (!request.AccountId.Contains("stl") && !request.AccountId.Contains("mtl"))
            {
                ServiceLocator.Logger.Information("Account {account} is not stl or mtl", request.AccountId);
                return Ok(DepositResponse<CreateDirectaInvoiceResponse>.Create(CreateDirectaInvoiceResponse.Empty,
                    DepositRequestStatus.ServerError));
            }

            if (!HttpContext.TryGetTraderId(out var traderId))
            {
                ServiceLocator.Logger.Information("TraderId was not found for request {@request}", request);
                return Unauthorized("Unauthorized");
            }

            var pd = await ServiceLocator.PersonalDataServiceGrpc.GetByIdAsync(traderId);

            var ip = HttpContext.GetIp();
            if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                depositBrand = Enum.Parse<BrandName>(pd.PersonalData.BrandId, true);
            if (depositBrand is null)
            {
                ServiceLocator.Logger.Error("Brand is null");
                return Ok(DepositResponse<CreateDirectaInvoiceResponse>.Create(CreateDirectaInvoiceResponse.Empty,
                    DepositRequestStatus.ServerError));
            }

            ServiceLocator.Logger.Information("Using {brand} brand", depositBrand.ToString());

            try
            {
                DepositResponse<CreateDirectaInvoiceResponse> response = null;
                var paymentSystem = await ServiceLocator.DepositManagerGrpcService.GetPaymentSystemsAsync(
                    GetPaymentSystemsRequest.Create(traderId, depositBrand.ToString(), pd.PersonalData.GetCountry()));
                if (paymentSystem.PaymentSystems?.Any(x =>
                    x.PaymentSystemId.Contains("directa", StringComparison.OrdinalIgnoreCase)) == true)
                {
                    var depositGrpcResponse = await ServiceLocator.MakeDirectaDepositProcessIdService
                        .GetOrCreateAsync(request.ProcessId + traderId,
                            () => ServiceLocator.FinanceDirectaIntegrationGrpcService.MakeDepositAsync(
                                request.ToMakeDirectaDepositGrpcRequest(pd, depositBrand.ToString())));

                    if (depositGrpcResponse.Status == DirectaDepositRequestStatus.Success)
                        response = DepositResponse<CreateDirectaInvoiceResponse>.Success(
                            CreateDirectaInvoiceResponse.Create(depositGrpcResponse.RedirectUrl));
                    else
                        response = DepositResponse<CreateDirectaInvoiceResponse>.Create(
                            CreateDirectaInvoiceResponse.Empty, DepositRequestStatus.ServerError);
                }
                else
                {
                    ServiceLocator.Logger.Warning("CreateDirectaInvoice. paymentSystem not supported {@paymentSystem}",
                        paymentSystem);
                    response = DepositResponse<CreateDirectaInvoiceResponse>.Create(CreateDirectaInvoiceResponse.Empty,
                        DepositRequestStatus.ServerError);
                }

                ServiceLocator.Logger.Information("CreateDirectaInvoice. Return response {@response}", response);
                return Ok(response);
            }
            catch (Exception e)
            {
                ServiceLocator.Logger.Error(e, e.Message);
                return Ok(DepositResponse<CreateDirectaInvoiceResponse>.Create(CreateDirectaInvoiceResponse.Empty,
                    DepositRequestStatus.ServerError));
            }
        }

        [HttpPost("volt/invoice")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(DepositResponse<CreateVoltInvoiceResponse>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(DepositResponse<CreateVoltInvoiceBadResponse>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(string))]
        public async Task<IActionResult> CreateVoltInvoice([FromBody] CreateVoltInvoiceRequest request)
        {
            ServiceLocator.Logger.Information(
                "Process CreateVoltInvoice for request {@request} with headers {@headers}", request,
                HttpContext.Request.Headers);
            var validationResult = request.Validate();
            if (!validationResult.IsValid)
            {
                var errors =
                    validationResult.Errors.Select(item => ErrorEntity.Create(item.PropertyName, item.ErrorMessage));
                ServiceLocator.Logger.Information("Validation failed with error {@error}", errors);
                return Ok(DepositResponse<CreateVoltInvoiceBadResponse>.Create(
                    CreateVoltInvoiceBadResponse.Create(errors), DepositRequestStatus.ServerError));
            }

            if (!request.AccountId.Contains("stl") && !request.AccountId.Contains("mtl"))
            {
                ServiceLocator.Logger.Information("Account {account} is not stl or mtl", request.AccountId);
                return Ok(DepositResponse<CreateVoltInvoiceResponse>.Create(CreateVoltInvoiceResponse.Empty,
                    DepositRequestStatus.ServerError));
            }

            if (!HttpContext.TryGetTraderId(out var traderId))
            {
                ServiceLocator.Logger.Information("TraderId was not found for request {@request}", request);
                return Unauthorized("Unauthorized");
            }

            var pd = await ServiceLocator.PersonalDataServiceGrpc.GetByIdAsync(traderId);

            var ip = HttpContext.GetIp();
            if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                depositBrand = Enum.Parse<BrandName>(pd.PersonalData.BrandId, true);
            if (depositBrand is null)
            {
                ServiceLocator.Logger.Error("Brand is null");
                return Ok(DepositResponse<CreateVoltInvoiceResponse>.Create(CreateVoltInvoiceResponse.Empty,
                    DepositRequestStatus.ServerError));
            }

            ServiceLocator.Logger.Information("Using {brand} brand", depositBrand.ToString());

            try
            {
                DepositResponse<CreateVoltInvoiceResponse> response = null;
                var paymentSystem = await ServiceLocator.DepositManagerGrpcService.GetPaymentSystemsAsync(
                    GetPaymentSystemsRequest.Create(traderId, depositBrand.ToString(), pd.PersonalData.GetCountry()));
                if (paymentSystem.PaymentSystems?.Any(x =>
                    x.PaymentSystemId.Contains("Volt", StringComparison.OrdinalIgnoreCase)) == true)
                {
                    var depositGrpcResponse = await ServiceLocator.MakeVoltDepositProcessIdService
                        .GetOrCreateAsync(request.ProcessId + traderId,
                            () => ServiceLocator.FinanceVoltIntegrationGrpcService.MakeDepositAsync(
                                request.ToMakeVoltDepositGrpcRequest(pd, depositBrand.ToString())));

                    if (depositGrpcResponse.Status == VoltDepositRequestStatus.Success)
                        response = DepositResponse<CreateVoltInvoiceResponse>.Success(
                            CreateVoltInvoiceResponse.Create(depositGrpcResponse.RedirectUrl));
                    else
                        response = DepositResponse<CreateVoltInvoiceResponse>.Create(
                            CreateVoltInvoiceResponse.Empty, DepositRequestStatus.ServerError);
                }
                else
                {
                    ServiceLocator.Logger.Warning("CreateVoltInvoice. paymentSystem not supported {@paymentSystem}",
                        paymentSystem);
                    response = DepositResponse<CreateVoltInvoiceResponse>.Create(CreateVoltInvoiceResponse.Empty,
                        DepositRequestStatus.ServerError);
                }

                ServiceLocator.Logger.Information("CreateVoltInvoice. Return response {@response}", response);
                return Ok(response);
            }
            catch (Exception e)
            {
                ServiceLocator.Logger.Error(e, e.Message);
                return Ok(DepositResponse<CreateVoltInvoiceResponse>.Create(CreateVoltInvoiceResponse.Empty,
                    DepositRequestStatus.ServerError));
            }
        }

        [HttpPost("payretailers/invoice")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(DepositResponse<CreatePayRetailersInvoiceResponse>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(DepositResponse<CreatePayRetailersInvoiceBadResponse>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(string))]
        public async Task<IActionResult> CreatePayRetailersInvoice([FromBody] CreatePayRetailersInvoiceRequest request)
        {
            ServiceLocator.Logger.Information(
                "Process CreatePayRetailersInvoice for request {@request} with headers {@headers}", request,
                HttpContext.Request.Headers);
            var validationResult = request.Validate();
            if (!validationResult.IsValid)
            {
                var errors =
                    validationResult.Errors.Select(item => ErrorEntity.Create(item.PropertyName, item.ErrorMessage));
                ServiceLocator.Logger.Information("Validation failed with error {@error}", errors);
                return Ok(DepositResponse<CreatePayRetailersInvoiceBadResponse>.Create(
                    CreatePayRetailersInvoiceBadResponse.Create(errors), DepositRequestStatus.ServerError));
            }

            if (!request.AccountId.Contains("stl") && !request.AccountId.Contains("mtl"))
            {
                ServiceLocator.Logger.Information("Account {account} is not stl or mtl", request.AccountId);
                return Ok(DepositResponse<CreatePayRetailersInvoiceResponse>.Create(CreatePayRetailersInvoiceResponse.Empty,
                    DepositRequestStatus.ServerError));
            }

            if (!HttpContext.TryGetTraderId(out var traderId))
            {
                ServiceLocator.Logger.Information("TraderId was not found for request {@request}", request);
                return Unauthorized("Unauthorized");
            }

            var pd = await ServiceLocator.PersonalDataServiceGrpc.GetByIdAsync(traderId);

            var ip = HttpContext.GetIp();
            if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                depositBrand = Enum.Parse<BrandName>(pd.PersonalData.BrandId, true);
            if (depositBrand is null)
            {
                ServiceLocator.Logger.Error("Brand is null");
                return Ok(DepositResponse<CreatePayRetailersInvoiceResponse>.Create(CreatePayRetailersInvoiceResponse.Empty,
                    DepositRequestStatus.ServerError));
            }

            ServiceLocator.Logger.Information("Using {brand} brand", depositBrand.ToString());

            try
            {
                DepositResponse<CreatePayRetailersInvoiceResponse> response = null;
                var paymentSystem = await ServiceLocator.DepositManagerGrpcService.GetPaymentSystemsAsync(
                    GetPaymentSystemsRequest.Create(traderId, depositBrand.ToString(), pd.PersonalData.GetCountry()));
                if (paymentSystem.PaymentSystems?.Any(x =>
                    x.PaymentSystemId.Contains("PayRetailers", StringComparison.OrdinalIgnoreCase)) == true)
                {
                    var depositGrpcResponse = await ServiceLocator.MakePayRetailersDepositProcessIdService
                        .GetOrCreateAsync(request.ProcessId + traderId,
                            () => ServiceLocator.FinancePayRetailersIntegrationGrpcService.MakeDepositAsync(
                                request.ToMakePayRetailersDepositGrpcRequest(pd, ip, depositBrand.ToString())));

                    if (depositGrpcResponse.Status == PayRetailersDepositIntagrationStatus.Success)
                        response = DepositResponse<CreatePayRetailersInvoiceResponse>.Success(
                            CreatePayRetailersInvoiceResponse.Create(depositGrpcResponse.RedirectUrl));
                    else
                        response = DepositResponse<CreatePayRetailersInvoiceResponse>.Create(
                            CreatePayRetailersInvoiceResponse.Empty, DepositRequestStatus.ServerError);
                }
                else
                {
                    ServiceLocator.Logger.Warning("CreatePayRetailersInvoice. paymentSystem not supported {@paymentSystem}",
                        paymentSystem);
                    response = DepositResponse<CreatePayRetailersInvoiceResponse>.Create(CreatePayRetailersInvoiceResponse.Empty,
                        DepositRequestStatus.ServerError);
                }

                ServiceLocator.Logger.Information("CreatePayRetailersInvoice. Return response {@response}", response);
                return Ok(response);
            }
            catch (Exception e)
            {
                ServiceLocator.Logger.Error(e, e.Message);
                return Ok(DepositResponse<CreatePayRetailersInvoiceResponse>.Create(CreatePayRetailersInvoiceResponse.Empty,
                    DepositRequestStatus.ServerError));
            }
        }

        [HttpPost("payop/invoice")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(DepositResponse<CreatePayopInvoiceResponse>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, typeof(DepositResponse<CreatePayopInvoiceBadResponse>))]
        [SwaggerResponse(HttpStatusCode.Unauthorized, typeof(string))]
        public async Task<IActionResult> CreatePayopInvoice([FromBody] CreatePayopInvoiceRequest request)
        {
            ServiceLocator.Logger.Information(
                "Process CreatePayopInvoice for request {@request} with headers {@headers}", request,
                HttpContext.Request.Headers);
            var validationResult = request.Validate();
            if (!validationResult.IsValid)
            {
                var errors =
                    validationResult.Errors.Select(item => ErrorEntity.Create(item.PropertyName, item.ErrorMessage));
                ServiceLocator.Logger.Information("Validation failed with error {@error}", errors);
                return Ok(DepositResponse<CreatePayopInvoiceBadResponse>.Create(
                    CreatePayopInvoiceBadResponse.Create(errors), DepositRequestStatus.ServerError));
            }

            if (!request.AccountId.Contains("stl") && !request.AccountId.Contains("mtl"))
            {
                ServiceLocator.Logger.Information("Account {account} is not stl or mtl", request.AccountId);
                return Ok(DepositResponse<CreatePayopInvoiceResponse>.Create(CreatePayopInvoiceResponse.Empty,
                    DepositRequestStatus.ServerError));
            }

            if (!HttpContext.TryGetTraderId(out var traderId))
            {
                ServiceLocator.Logger.Information("TraderId was not found for request {@request}", request);
                return Unauthorized("Unauthorized");
            }

            var pd = await ServiceLocator.PersonalDataServiceGrpc.GetByIdAsync(traderId);

            var ip = HttpContext.GetIp();
            if (!HttpContext.TryGetDepositBrandByRequest(out var depositBrand))
                depositBrand = Enum.Parse<BrandName>(pd.PersonalData.BrandId, true);
            if (depositBrand is null)
            {
                ServiceLocator.Logger.Error("Brand is null");
                return Ok(DepositResponse<CreatePayopInvoiceResponse>.Create(CreatePayopInvoiceResponse.Empty,
                    DepositRequestStatus.ServerError));
            }

            ServiceLocator.Logger.Information("Using {brand} brand", depositBrand.ToString());

            try
            {
                DepositResponse<CreatePayopInvoiceResponse> response = null;
                var paymentSystem = await ServiceLocator.DepositManagerGrpcService.GetPaymentSystemsAsync(
                    GetPaymentSystemsRequest.Create(traderId, depositBrand.ToString(), pd.PersonalData.GetCountry()));
                if (paymentSystem.PaymentSystems?.Any(x =>
                    x.PaymentSystemId.Contains("Payop", StringComparison.OrdinalIgnoreCase)) == true)
                {
                    var depositGrpcResponse = await ServiceLocator.MakePayopDepositProcessIdService
                        .GetOrCreateAsync(request.ProcessId + traderId,
                            () => ServiceLocator.FinancePayopIntegrationGrpcService.MakeDepositAsync(
                                request.ToMakePayopDepositGrpcRequest(pd, depositBrand.ToString())));

                    if (depositGrpcResponse.Status == PayopDepositRequestStatus.Success)
                        response = DepositResponse<CreatePayopInvoiceResponse>.Success(
                            CreatePayopInvoiceResponse.Create(depositGrpcResponse.RedirectUrl));
                    else
                        response = DepositResponse<CreatePayopInvoiceResponse>.Create(
                            CreatePayopInvoiceResponse.Empty, DepositRequestStatus.ServerError);
                }
                else
                {
                    ServiceLocator.Logger.Warning("CreatePayopInvoice. paymentSystem not supported {@paymentSystem}",
                        paymentSystem);
                    response = DepositResponse<CreatePayopInvoiceResponse>.Create(CreatePayopInvoiceResponse.Empty,
                        DepositRequestStatus.ServerError);
                }

                ServiceLocator.Logger.Information("CreatePayopInvoice. Return response {@response}", response);
                return Ok(response);
            }
            catch (Exception e)
            {
                ServiceLocator.Logger.Error(e, e.Message);
                return Ok(DepositResponse<CreatePayopInvoiceResponse>.Create(CreatePayopInvoiceResponse.Empty,
                    DepositRequestStatus.ServerError));
            }
        }

        private static async Task<List<PaymentSystem>> NotSortedBankAndBitcoinAsync(string traderId, GetPaymentSystemsResponse result)
        {
            var notSortedBankAndBitcoin = result.PaymentSystems?
                                              .Select(x => x.ToPaymentSystemViaClientGroup(ABSplitGroupType.GroupA))
                                              .OrderBy(ps => ps.PaymentSystemType)
                                              .ToList() ??
                                          new List<PaymentSystem>();

            string abSplitValue = string.Empty;
            try
            {
                abSplitValue = (await ServiceLocator.TraderExternalDataGrpcService.Value.GetAsync(
                    new GetGrpcContract
                    {
                        Key = "ABSplitData",
                        TraderId = traderId
                    })).Value;

                if (!string.IsNullOrEmpty(abSplitValue) && ServiceLocator.Settings.ABSplits != null)
                {
                    var splitTrader = JsonConvert.DeserializeObject<ABSplit>(abSplitValue);

                    if (ServiceLocator.Settings.ABSplits.ContainsValue(splitTrader?.Name))
                    {
                        if (splitTrader?.Type.GetSplitType() == ABSplitGroupType.GroupB)
                        {
                            notSortedBankAndBitcoin = result.PaymentSystems?
                                                          .Select(x =>
                                                              x.ToPaymentSystemViaClientGroup(ABSplitGroupType.GroupB))
                                                          .OrderBy(ps => ps.PaymentSystemType)
                                                          .ToList() ??
                                                      new List<PaymentSystem>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceLocator.Logger.Error("Can't parse ABSplitData, " +
                                            "will use default bank cards payments {@TraderId} {@ABSplitData} {@Payments} {@error}", 
                    traderId, abSplitValue, result, ex.Message);
            }

            notSortedBankAndBitcoin.Sort(PaymentSystem.SortBitcoinLast);
            return notSortedBankAndBitcoin;
        }
    }

    public class CreatePayRetailersInvoiceBadResponse
    {
        public IEnumerable<ErrorEntity> Errors { get; set; }

        public static CreatePayRetailersInvoiceBadResponse Create(
            IEnumerable<ErrorEntity> errors = null)
        {
            return new()
            {
                Errors = errors
            };
        }
    }

    public class CreatePayRetailersInvoiceResponse
    {
        public string RedirectLink { get; set; }

        public CreatePayRetailersInvoiceResponse(string redirectLink)
        {
            RedirectLink = redirectLink;
        }

        public static CreatePayRetailersInvoiceResponse Create(
            string redirectLink)
        {
            return new(redirectLink);
        }

        public static CreatePayRetailersInvoiceResponse Empty => Create(string.Empty);
    }

    public class CreatePayRetailersInvoiceRequest
    {
        public string ProcessId { get; set; }
        public string AccountId { get; set; }
        public double Amount { get; set; }
    }

    public class CreateVoltInvoiceResponse
    {
        public string RedirectLink { get; set; }

        public CreateVoltInvoiceResponse(string redirectLink)
        {
            RedirectLink = redirectLink;
        }

        public static CreateVoltInvoiceResponse Create(
            string redirectLink)
        {
            return new(redirectLink);
        }

        public static CreateVoltInvoiceResponse Empty => Create(string.Empty);
    }

    public class CreateVoltInvoiceBadResponse
    {
        public IEnumerable<ErrorEntity> Errors { get; set; }

        public static CreateVoltInvoiceBadResponse Create(
            IEnumerable<ErrorEntity> errors = null)
        {
            return new()
            {
                Errors = errors
            };
        }
    }

    public class CreateVoltInvoiceRequest
    {
        public string ProcessId { get; set; }
        public string AccountId { get; set; }
        public double Amount { get; set; }
    }

    public class CreatePayopInvoiceResponse
    {
        public string RedirectLink { get; set; }

        public CreatePayopInvoiceResponse(string redirectLink)
        {
            RedirectLink = redirectLink;
        }

        public static CreatePayopInvoiceResponse Create(
            string redirectLink)
        {
            return new(redirectLink);
        }

        public static CreatePayopInvoiceResponse Empty => Create(string.Empty);
    }

    public class CreatePayopInvoiceBadResponse
    {
        public IEnumerable<ErrorEntity> Errors { get; set; }

        public static CreatePayopInvoiceBadResponse Create(
            IEnumerable<ErrorEntity> errors = null)
        {
            return new()
            {
                Errors = errors
            };
        }
    }

    public class CreatePayopInvoiceRequest
    {
        public string ProcessId { get; set; }
        public string AccountId { get; set; }
        public double Amount { get; set; }
    }
}