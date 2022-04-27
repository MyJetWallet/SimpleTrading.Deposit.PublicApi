using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Finance.CardValidator;
using Finance.PciDssIntegration.GrpcContracts.Contracts;
using Finance.PciDssPublic.HttpContracts.Requests;
using Finance.PciDssPublic.HttpContracts.Responses;
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
}