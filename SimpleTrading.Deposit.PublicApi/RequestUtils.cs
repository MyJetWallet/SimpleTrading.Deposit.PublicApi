using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Finance.DirectaIntegration.GrpcContracts.Contracts;
using Finance.DirectaPublic.HttpContracts.Requests;
using Finance.PayopIntegration.GrpcContracts.Contracts;
using Finance.PayRetailersIntegration.GrpcContracts.Contracts;
using Finance.PciDssIntegration.GrpcContracts.Contracts;
using Finance.PciDssPublic.HttpContracts.Requests;
using Finance.SwiffyIntegration.GrpcContracts.Contracts;
using Finance.SwiffyPublic.HttpContracts.Requests;
using Finance.VoltIntegration.GrpcContracts.Contracts;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SimpleTrading.Common.Helpers;
using SimpleTrading.Deposit.Grpc.Contracts;
using SimpleTrading.Deposit.Postgresql.Models;
using SimpleTrading.Deposit.PublicApi.Contracts;
using SimpleTrading.Deposit.PublicApi.Contracts.Callbacks;
using SimpleTrading.Deposit.PublicApi.Controllers;
using SimpleTrading.Payments.Abstractions;
using SimpleTrading.PersonalData.Grpc.Contracts;
using SimpleTrading.PersonalData.Grpc.Models;
using SimpleTrading.TokensManager;
using SimpleTrading.TokensManager.Tokens;
using SimpleTrading.TraderExternalData.Grpc.Contracts;
using BrandName = Finance.PciDssIntegration.GrpcContracts.Contracts.BrandName;

namespace SimpleTrading.Deposit.PublicApi
{
    public static class RequestUtils
    {
        private const string AuthorizationHeader = "authorization";
        private const string OriginHeader = "origin";
        private const string RefererHeader = "referer";

        public static ProcessDepositRequest ToGrpcCallbackRequest(this ExactlyCallbackModel model)
        {
            return new ProcessDepositRequest
            {
                TransactionId = model.Data.Charge.TransactionId,
                PsTransactionId = model.Data.Charge.TransactionId,
                Comment = "Handled callback on deposit rest service",
                Author = "System",
                PaymentInvoiceStatus = model.Data.Charge.Attributes.Status == "failed"
                    ? PaymentInvoiceStatusEnum.Failed
                    : PaymentInvoiceStatusEnum.Approved
            };
        }

        public static ProcessDepositRequest ToGrpcCallbackRequest(this TexcentCallback model)
        {
            return new ProcessDepositRequest
            {
                TransactionId = model.orderId,
                PsTransactionId = model.transactionId,
                Comment = "Handled callback on deposit rest service",
                Author = "System",
                PaymentInvoiceStatus = model.status == "successful"
                    ? PaymentInvoiceStatusEnum.Approved
                    : PaymentInvoiceStatusEnum.Failed
            };
        }

        public static ProcessDepositRequest ToGrpcCallbackRequest(this CreateRoyalPayInvoiceCallback model)
        {
            return new ProcessDepositRequest
            {
                TransactionId = model.Transaction.TrackingId,
                PsTransactionId = model.Transaction.Uid,
                Comment = "Handled callback on deposit rest service",
                Author = "System",
                PaymentInvoiceStatus = model.Transaction.Status == "successful"
                    ? PaymentInvoiceStatusEnum.Approved
                    : PaymentInvoiceStatusEnum.Failed
            };
        }

        public static HandleCryptoDepositCallbackRequest ToGrpcCallbackRequest(this CoinPaymentCallback model,
            string traderId, PaymentInvoiceStatusEnum status, string accountId)
        {
            return new HandleCryptoDepositCallbackRequest
            {
                PsTransactionId = model.deposit_id,
                PaymentProvider = "CoinPayment",
                Currency = model.currency,
                Amount = model.amount,
                TraderId = traderId,
                Status = status,
                DateTime = DateTime.UtcNow,
                Commission = JsonConvert.SerializeObject(new
                {
                    model.fee
                }),
                CallbackEntity = JsonConvert.SerializeObject(model),
                AccountId = accountId
            };
        }
        public static ProcessDepositRequest ToGrpcCallbackRequest(this OctaPayCallbackRequest model)
        {
            return new ProcessDepositRequest
            {
                TransactionId = model.CustomerOrderId,
                PsTransactionId = model.OrderId,
                Comment = "Handled callback on deposit rest service",
                Author = "System",
                PaymentInvoiceStatus = model.TransactionStatus == "success"
                    ? PaymentInvoiceStatusEnum.Approved
                    : PaymentInvoiceStatusEnum.Failed
            };
        }

        public static ProcessDepositRequest ToGrpcCallbackRequest(this PayopCallbackRequest model, DepositModel pendingInvoice)
        {
            return new()
            {
                TransactionId = pendingInvoice.Id,
                PsTransactionId = pendingInvoice.PsTransactionId,
                Comment = "Handled callback on deposit rest service",
                Author = "System",
                PaymentInvoiceStatus = model.Transaction.IsSuccess
                    ? PaymentInvoiceStatusEnum.Approved
                    : PaymentInvoiceStatusEnum.Failed
            };
        }

        public static ProcessDepositRequest ToGrpcCallbackRequest(this RealDepositsCallbackRequest model, DepositModel pendingInvoice)
        {
            return new ProcessDepositRequest
            {
                TransactionId = pendingInvoice.Id,
                PsTransactionId = pendingInvoice.PsTransactionId,
                Comment = "Handled callback on deposit rest service",
                Author = "System",
                PaymentInvoiceStatus = model.TransactionStatus.Equals("approved", StringComparison.OrdinalIgnoreCase)
                    ? PaymentInvoiceStatusEnum.Approved
                    : PaymentInvoiceStatusEnum.Failed
            };
        }

        public static ProcessDepositRequest ToGrpcCallbackRequest(this SwiffyCallback model, DepositModel pendingInvoice)
        {
            return new ProcessDepositRequest
            {
                TransactionId = pendingInvoice.Id,
                PsTransactionId = string.IsNullOrEmpty(pendingInvoice.PsTransactionId) ? model.CallpayTransactionId : pendingInvoice.PsTransactionId,
                Comment = "Handled callback on deposit rest service",
                Author = "System",
                PaymentInvoiceStatus = model.IsSuccess
                    ? PaymentInvoiceStatusEnum.Approved
                    : PaymentInvoiceStatusEnum.Failed
            };
        }

        public static ProcessDepositRequest ToGrpcCallbackRequest(this GetDirectaDepositGrpcResponse model,
            DepositModel pendingInvoice)
        {
            return new()
            {
                TransactionId = pendingInvoice.Id,
                PsTransactionId = string.IsNullOrEmpty(pendingInvoice.PsTransactionId)
                    ? model.Deposit.PsTransactionId
                    : pendingInvoice.PsTransactionId,
                Comment = "Handled callback on deposit rest service",
                Author = "System",
                PaymentInvoiceStatus = model.Deposit.IsSuccess()
                    ? PaymentInvoiceStatusEnum.Approved
                    : PaymentInvoiceStatusEnum.Failed
            };
        }

        public static ProcessDepositRequest ToGrpcCallbackRequest(this PayRetailersCallback callback,
            DepositModel pendingInvoice)
        {
            return new()
            {
                TransactionId = pendingInvoice.Id,
                PsTransactionId = string.IsNullOrEmpty(pendingInvoice.PsTransactionId)
                    ? callback.Uid
                    : pendingInvoice.PsTransactionId,
                Comment = "Handled callback on deposit rest service",
                Author = "System",
                PaymentInvoiceStatus = callback.IsSuccess
                    ? PaymentInvoiceStatusEnum.Approved
                    : PaymentInvoiceStatusEnum.Failed
            };
        }

        public static ProcessDepositRequest ToGrpcCallbackRequest(this CertusFinanceCallback callback,
            DepositModel pendingInvoice)
        {
            return new()
            {
                TransactionId = pendingInvoice.Id,
                PsTransactionId = string.IsNullOrEmpty(pendingInvoice.PsTransactionId)
                    ? callback.TxId
                    : pendingInvoice.PsTransactionId,
                Comment = "Handled callback on deposit rest service",
                Author = "System",
                PaymentInvoiceStatus = callback.IsSuccess
                    ? PaymentInvoiceStatusEnum.Approved
                    : PaymentInvoiceStatusEnum.Failed
            };
        }
        public static CreatePaymentInvoiceResponse CreateErrorResponseForCreatePaymentInvoice(
            this CreatePaymentInvoiceRequest request)
        {
            return new CreatePaymentInvoiceResponse
            {
                Status = CreateInvoiceErrorEnum.SystemError,
                RedirectUrl = null
            };
        }

        public static MakeDepositRequest CreatePciDssInvoiceRequest(
            this CreatePciDssInvoiceRequest request, string traderId, string ip, BrandName brand, 
            string kycVerified, double totalDeposit, string source)
        {
            return new MakeDepositRequest
            {
                BankNumber = request.CardNumber,
                Cvv = request.Cvv,
                ExpirationDate = request.ExpirationDate.UnixTimeToDateTime(),
                FullName = request.FullName,
                Amount = request.Amount,
                PostalCode = request.PostalCode,
                Country = request.Country,
                City = request.City,
                Address = request.Address,
                TraderId = traderId,
                AccountId = request.AccountId,
                ClientIp = ip,
                BrandName = brand,
                Brand = brand.ToString(),
                PhoneNumber = request.PhoneNumber,
                KycVerified = kycVerified,
                TotalDeposit = totalDeposit,
                Source = source
            };
        }

        public static void EnrichByPersonalData(
            this CreatePciDssInvoiceRequest request, PersonalDataGrpcResponseContract pd)
        {
            request.Country = string.IsNullOrEmpty(request.Country) ? pd.PersonalData.GetCountry() : request.Country;
            request.Address = string.IsNullOrEmpty(request.Address) ? pd.PersonalData.Address : request.Address;
            request.City = string.IsNullOrEmpty(request.City) ? pd.PersonalData.City : request.City;
            request.PostalCode = string.IsNullOrEmpty(request.PostalCode) ? pd.PersonalData.PostalCode : request.PostalCode;
            request.PhoneNumber = string.IsNullOrEmpty(request.PhoneNumber) ? pd.PersonalData.Phone : request.PhoneNumber;
        }

        public static async ValueTask<string> GetAffiliateTrafficSource(
            string traderId)
        {
            var affIdValue = (await ServiceLocator.TraderExternalDataGrpcService.Value.GetAsync(new GetGrpcContract
            {
                Key = "affId",
                TraderId = traderId
            })).Value;

            if (Int32.TryParse(affIdValue, out var affId) && affId != 0)
            {
                return "AFFILIATE";
            }

            return string.Empty;
        }

        public static async ValueTask<string> GetMediaTrafficSourceAsync(
            string traderId)
        {
            var stringMedia = await ServiceLocator.UtmRepository.GetTraderUtmAsync(traderId, "utm_campaign");
            if (stringMedia != null && string.Equals(stringMedia.Value, "mb", StringComparison.OrdinalIgnoreCase))
            {
                return "MEDIA";
            }
            return string.Empty;
        }

        public static async Task<string> GetTrafficSourceAsync(
            string traderId)
        {
            var sourceMedia = await GetMediaTrafficSourceAsync(traderId);
            if (!string.IsNullOrEmpty(sourceMedia))
                return sourceMedia;

            var sourceAffiliate = await GetAffiliateTrafficSource(traderId);
            if (!string.IsNullOrEmpty(sourceAffiliate))
                return sourceAffiliate;

            return string.Empty;
        }

        public static MakeSwiffyDepositGrpcRequest ToMakeSwiffyDepositGrpcRequest(this CreateSwiffyInvoiceRequest request, PersonalDataGrpcResponseContract pd, string brand)
        {
            return new MakeSwiffyDepositGrpcRequest
            {
                AccountId = request.AccountId,
                Country = pd.PersonalData.GetCountry(),
                Amount = request.Amount,
                Brand = brand,
                Currency = "USD",
                ProcessId = request.ProcessId,
                TraderId = pd.PersonalData.Id
            };
        }

        public static MakeDirectaDepositGrpcRequest ToMakeDirectaDepositGrpcRequest(
            this CreateDirectaInvoiceRequest request, PersonalDataGrpcResponseContract pd, string brand)
        {
            return new()
            {
                AccountId = request.AccountId,
                Country2 = CountryManager.Iso3ToIso2(pd.PersonalData.GetCountry()),
                Amount = request.Amount,
                Brand = brand,
                Currency = "USD",
                ProcessId = request.ProcessId,
                TraderId = pd.PersonalData.Id,
                BirthDate = pd.PersonalData.DateOfBirth?.ToString("yyyyMMdd"),
                City = pd.PersonalData.City,
                Email = pd.PersonalData.Email,
                FirstName = pd.PersonalData.FirstName,
                LastName = pd.PersonalData.LastName,
                Phone = pd.PersonalData.Phone,
                Street = pd.PersonalData.Address,
                Zipcode = pd.PersonalData.PostalCode
            };
        }

        public static MakeVoltDepositGrpcRequest ToMakeVoltDepositGrpcRequest(
            this CreateVoltInvoiceRequest request, PersonalDataGrpcResponseContract pd, string brand)
        {
            return new()
            {
                AccountId = request.AccountId,
                Amount = request.Amount,
                Brand = brand,
                Currency = "USD",
                TraderId = pd.PersonalData.Id,
                Email = pd.PersonalData.Email,
                FullName = string.Join(' ', pd.PersonalData.FirstName, pd.PersonalData.LastName),
                PhoneNumber = pd.PersonalData.Phone
            };
        }

        public static MakePayRetailersDepositGrpcRequest ToMakePayRetailersDepositGrpcRequest(
            this CreatePayRetailersInvoiceRequest request, PersonalDataGrpcResponseContract pd, string ip, string brand)
        {
            return new()
            {
                AccountId = request.AccountId,
                Amount = request.Amount,
                Brand = brand,
                Currency = "USD",
                TraderId = pd.PersonalData.Id,
                Email = pd.PersonalData.Email,
                Address = pd.PersonalData.Address,
                City = pd.PersonalData.City,
                FirstName = pd.PersonalData.FirstName,
                LastName = pd.PersonalData.LastName,
                Ip = ip,
                Country2 = CountryManager.Iso3ToIso2(pd.PersonalData.GetCountry()),
                Zip = pd.PersonalData.PostalCode,
                ProcessId = request.ProcessId,
                PhoneNumber = pd.PersonalData.Phone
            };
        }

        public static MakePayopDepositGrpcRequest ToMakePayopDepositGrpcRequest(
            this CreatePayopInvoiceRequest request, PersonalDataGrpcResponseContract pd, string brand)
        {
            return new()
            {
                AccountId = request.AccountId,
                Amount = request.Amount,
                Brand = brand,
                Currency = "USD",
                TraderId = pd.PersonalData.Id,
                Email = pd.PersonalData.Email,
                FullName = string.Join(' ', pd.PersonalData.FirstName, pd.PersonalData.LastName),
                PhoneNumber = pd.PersonalData.Phone
            };
        }

        public static bool TryGetTraderId(this HttpContext ctx, out string traderId)
        {
            try
            {
                if (!ctx.Request.Headers.ContainsKey(AuthorizationHeader))
                {
                    traderId = null;
                }
                else
                {
                    var itm = ctx.Request.Headers[AuthorizationHeader].ToString().Trim();
                    var items = itm.Split();
                    traderId = items[^1].GetTraderIdByToken();

                }
                return !string.IsNullOrEmpty(traderId);
            }
            catch (Exception)
            {
                traderId = null;
                return false;
            }
           
        }

        private static string GetTraderIdByToken(this string tokenString)
        {
            try
            {
                var (result, token) =
                    TokensManager.TokensManager.ParseBase64Token<AuthorizationToken>(tokenString,
                        ServiceLocator.EncodeKey, DateTime.UtcNow);

                if (result == TokenParseResult.Expired)
                    return null;

                return result == TokenParseResult.InvalidToken ? null : token.Id;
            }
            catch (Exception)
            {
                return null;
            } 
        }

        public static string GetMd5(this string str)
        {
            byte[] hash = Encoding.UTF8.GetBytes(str);
            using MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hashenc = md5.ComputeHash(hash);
            string result = "";
            foreach (var b in hashenc)
            {
                result += b.ToString("x2");
            }
            return result;
        }

        public static string GetHmacSha256Hash(this string data, string key)
        {
            HMAC hmac = HMAC.Create("HMACSHA256");
            hmac.Key = Encoding.UTF8.GetBytes(key);
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return ByteToString(hash).ToLowerInvariant();
        }

        private static string ByteToString(IEnumerable<byte> buff)
        {
            return buff.Aggregate("", (current, item) => current + item.ToString("X2"));
        }

        public static string GetSHA384Hash(this string data)
        {
            SHA384 sha384Hash = SHA384.Create();
            byte[] hash = sha384Hash.ComputeHash(Encoding.UTF8.GetBytes(data));
            return ByteToString(hash).ToLowerInvariant();
        }

        public static Dictionary<string, string> ConvertToDictionary<T>(this T request) where T: class
        {
            return request.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .OrderBy(x => x.Name)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(request, null)?.ToString());
        }

        public static bool TryGetDepositBrandByRequest(this HttpContext ctx, out BrandName? brandName)
        {
            var monfexBrands = ServiceLocator.Settings.MonfexBrandDomains;
            var handleProBrands = ServiceLocator.Settings.HandelProBrandDomains;
            var allianzmarketBrands = ServiceLocator.Settings.AllianzmarketBrandDomains;

            ServiceLocator.Logger.Information("Settings: {@settings}", ServiceLocator.Settings);
            if (ctx.Request.Headers.ContainsKey(OriginHeader))
            {
                var originHeader = ctx.Request.Headers[OriginHeader];
                ServiceLocator.Logger.Information("OriginHeader: {header}", originHeader);

                if (monfexBrands.Contains(originHeader, StringComparison.OrdinalIgnoreCase))
                {
                    brandName = BrandName.Monfex;
                    return true;
                }

                if (handleProBrands.Contains(originHeader, StringComparison.OrdinalIgnoreCase))
                {
                    brandName = BrandName.HandelPro;
                    return true;
                }

                if (allianzmarketBrands.Contains(originHeader, StringComparison.OrdinalIgnoreCase))
                {
                    brandName = BrandName.Allianzmarket;
                    return true;
                }
            }

            if (ctx.Request.Headers.ContainsKey(RefererHeader))
            {
                var refererHeader = ctx.Request.Headers[RefererHeader];
                ServiceLocator.Logger.Information("RefererHeader: {header}", refererHeader);
                if (monfexBrands.Contains(refererHeader, StringComparison.OrdinalIgnoreCase))
                {
                    brandName = BrandName.Monfex;
                    return true;
                }

                if (handleProBrands.Contains(refererHeader, StringComparison.OrdinalIgnoreCase))
                {
                    brandName = BrandName.HandelPro;
                    return true;
                }

                if (allianzmarketBrands.Contains(refererHeader, StringComparison.OrdinalIgnoreCase))
                {
                    brandName = BrandName.Allianzmarket;
                    return true;
                }
            }

            if (ctx.Request.Host.HasValue)
            {
                ServiceLocator.Logger.Information("Host: {host}", ctx.Request.Host);
                if (ctx.Request.Host.Value.Contains(BrandName.Monfex.ToString(), StringComparison.OrdinalIgnoreCase) || ctx.Request.Host.Value.Contains("mnftx", StringComparison.OrdinalIgnoreCase))
                {
                    brandName = BrandName.Monfex;
                    return true;
                }

                if (ctx.Request.Host.Value.Contains(BrandName.HandelPro.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    brandName = BrandName.HandelPro;
                    return true;
                }

                if (ctx.Request.Host.Value.Contains(BrandName.Allianzmarket.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    brandName = BrandName.Allianzmarket;
                    return true;
                }
            }

            brandName = null;
            return false;
        }

        public static string GetIp(this HttpContext httpContext)
        {
            const string forwardedForHeader = "X-Forwarded-For";
            if (httpContext.Request.Headers.ContainsKey(forwardedForHeader))
            {
                var headerValue = httpContext.Request.Headers[forwardedForHeader].FirstOrDefault();
                return headerValue.Split(",").FirstOrDefault();
            }
            else
            {
                ServiceLocator.Logger.Information("Request doesn't contain X-Forwarded-For header. Headers {@headers}", httpContext.Request.Headers);
            }
            return httpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }

        public static string GetCountry(this PersonalDataGrpcModel src)
        {
            var country = src.CountryOfResidence;
            if (string.IsNullOrWhiteSpace(country))
            {
                country = src.CountryOfCitizenship;
                if (string.IsNullOrWhiteSpace(country))
                {
                    country = src.CountryOfRegistration;
                }
                else
                {
                    country = string.Empty;
                }
            }

            return country;
        }
    }
}