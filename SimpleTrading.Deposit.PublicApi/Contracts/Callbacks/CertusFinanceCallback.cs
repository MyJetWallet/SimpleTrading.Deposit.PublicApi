using Destructurama.Attributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Grpc.Core;

namespace SimpleTrading.Deposit.PublicApi.Contracts.Callbacks
{
    public class CertusFinanceCallback
    {
        public enum CertusFinanceTransactionResultCode
        {
            InProcess = -4,        // Transaction is in process
            Redirected3Ds = -3,    // Transaction 3d auth response processing.
            // Redirect to acquirer for response.
            Enrolled3Ds = -2,      // Transaction is 3DS enrolled OR checking for 3DS enrollment.
            // Redirect to card issuer for 3DS authentication.
            Process = -1,          // Transaction is in process.
            Failed = 0,            // Transaction has failed.
            CompletedSuccessfully = 1,    // Transaction has completed successfully.
            Queued = 2,            // Transaction was successfully received and is now queued
            // for transmission to the provider
            CreatedSuccessfully = 3, // Transaction created Successfully.
            Cancelled = 4,         // Transaction was cancelled.
            Expired = 5,           // Transaction is expired. So its failed.
            Incomplete = 6,        // Transaction is Incomplete
            PartiallyCompleted = 9 // Transaction is partially completed
        }

        [JsonProperty("responseTime")] public string ResponseTime { get; set; }
        [JsonProperty("result")] public CertusFinanceResult Result { get; set; }
        [JsonProperty("signature")] public string Signature { get; set; }
        [JsonProperty("metaData")] public CertusFinanceMetaData MetaData { get; set; }
        [JsonProperty("txId")] public string TxId { get; set; }
        [JsonProperty("txTypeId")] public string TxTypeId { get; set; }
        [JsonProperty("txType")] public string TxType { get; set; }
        [JsonProperty("recurrentTypeId")] public string RecurrentTypeId { get; set; }
        [JsonProperty("requestId")] public string RequestId { get; set; }
        [JsonProperty("orderId")] public string OrderId { get; set; }
        [JsonProperty("sourceAmount")] public CertusFinanceAmount SourceAmount { get; set; }
        [JsonProperty("amount")] public CertusFinanceAmount Amount { get; set; }
        [JsonProperty("returnUrl")] public string ReturnUrl { get; set; }
        [JsonProperty("cancelUrl")] public string CancelUrl { get; set; }
        [JsonProperty("ccNumber")] public string CcNumber { get; set; }
        [JsonProperty("cardId")] public string CardId { get; set; }
        [JsonProperty("redirect3DUrl")] public string Redirect3DUrl { get; set; }

        public bool IsFailed
        {
            get
            {
                if (string.IsNullOrEmpty(Result.ResultCode))
                {
                    return true;
                }

                var status = (CertusFinanceTransactionResultCode) Enum.Parse(typeof(CertusFinanceTransactionResultCode),
                    Result.ResultCode);
                return (status == CertusFinanceTransactionResultCode.Expired ||
                        status == CertusFinanceTransactionResultCode.Cancelled ||
                        status == CertusFinanceTransactionResultCode.Failed);
            }
        }

        public bool IsSuccess
        {
            get
            {
                if (string.IsNullOrEmpty(Result.ResultCode))
                {
                    return false;
                }

                var status = (CertusFinanceTransactionResultCode) Enum.Parse(typeof(CertusFinanceTransactionResultCode),
                    Result.ResultCode);
                return status == CertusFinanceTransactionResultCode.CompletedSuccessfully;
            }
        }

        //public bool IsPending => Status.Equals("PENDING", StringComparison.OrdinalIgnoreCase);
        public bool IsPending => !IsFailed && !IsSuccess;

        public class CertusFinanceResult
        {
            [JsonProperty("resultCode")] public string ResultCode { get; set; }
            [JsonProperty("resultMessage")] public string ResultMessage { get; set; }
            [JsonProperty("errorId")] public string ErrorId { get; set; }
            [JsonProperty("error")] public IEnumerable<CertusFinanceResultError> Error { get; set; }
            [JsonProperty("reasonCode")] public string ReasonCode { get; set; }
        }
        public class CertusFinanceResultError
        {
            [JsonProperty("errorCode")] public string ErrorCode { get; set; }
            [JsonProperty("errorMessage")] public string ErrorMessage { get; set; }
            [JsonProperty("advice")] public string Advice { get; set; }
        }
        public class CertusFinanceMetaData
        {
            [JsonProperty("isShowResultMsgScreen")] public string IsShowResultMsgScreen { get; set; }
        }
        public class CertusFinanceAmount
        {
            [JsonProperty("amount")] public string Amount { get; set; }
            [JsonProperty("currencyCode")] public string CurrencyCode { get; set; }
        }

    }
}
