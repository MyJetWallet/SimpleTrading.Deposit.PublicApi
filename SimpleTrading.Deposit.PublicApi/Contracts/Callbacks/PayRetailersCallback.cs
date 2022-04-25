using Destructurama.Attributed;
using Newtonsoft.Json;
using System;

namespace SimpleTrading.Deposit.PublicApi.Contracts.Callbacks
{
    public class PayRetailersCallback
    {
        [JsonProperty("uid")] public string Uid { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("trackingId")] public string TrackingId { get; set; }
        [JsonProperty("amount")] public int Amount { get; set; }
        [JsonProperty("currency")] public string Currency { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("createdAt")] public DateTime? CreatedAt { get; set; }
        [JsonProperty("updatedAt")] public DateTime? UpdatedAt { get; set; }
        [JsonProperty("paymentChannelTypeCode")] public string PaymentChannelTypeCode { get; set; }
        [JsonProperty("customer")] public Customer Customer { get; set; }

        public bool IsPending => Status.Equals("PENDING", StringComparison.OrdinalIgnoreCase);
        public bool IsSuccess => Status.Equals("APPROVED", StringComparison.OrdinalIgnoreCase);
        public bool IsFailed => !IsPending && !IsSuccess;
    }


    public class Customer
    {
        [LogMasked(ShowFirst =1, ShowLast = 1, PreserveLength = true)]
        [JsonProperty("firstName")]public string FirstName { get; set; }
        [LogMasked(ShowFirst = 1, ShowLast = 1, PreserveLength = true)]
        [JsonProperty("lastName")]public string LastName { get; set; }
        [JsonProperty("personalId")]public string PersonalId { get; set; }
        [LogMasked(ShowFirst =3, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("email")]public string Email { get; set; }
        [JsonProperty("country")]public string Country { get; set; }
        [JsonProperty("city")]public string City { get; set; }
        [JsonProperty("zip")]public string Zip { get; set; }
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("address")]public string Address { get; set; }
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("phone")]public string Phone { get; set; }
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("deviceId")]public string DeviceId { get; set; }
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("ip")] public string Ip { get; set; }
    }
}
