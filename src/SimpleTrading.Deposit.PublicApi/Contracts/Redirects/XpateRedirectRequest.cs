using System;
using System.Web;
using Destructurama.Attributed;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SimpleTrading.Deposit.PublicApi.Contracts.Callbacks
{
    public class XpateRedirectRequest
    {
        [ModelBinder(Name = "error_message")] public string ErrorMessage { get; set; }
        [ModelBinder(Name = "error_code")] public string ErrorCode { get; set; }
        [ModelBinder(Name = "orderid")] public string OrderId { get; set; }
        [ModelBinder(Name = "client_orderid")] public string ClientOrderId { get; set; }
        [ModelBinder(Name = "processor-tx-id")] public string TxId { get; set; }
        [ModelBinder(Name = "status")] public string Status { get; set; }
        [ModelBinder(Name = "amount")] public decimal Amount { get; set; }
        [ModelBinder(Name = "control")] public string Control { get; set; }
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [ModelBinder(Name = "merchant_order")] public string MerchantOrder { get; set; }
    }
}