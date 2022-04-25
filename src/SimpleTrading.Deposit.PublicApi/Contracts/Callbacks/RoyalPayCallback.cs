using Destructurama.Attributed;
using Newtonsoft.Json;

namespace SimpleTrading.Deposit.PublicApi.Contracts.Callbacks
{
    public class CreateRoyalPayInvoiceCallback
    {
        [JsonProperty("transaction")] public CreateRoyalPayInvoiceCallbackTransaction Transaction { get; set; }
    }

    public class CreateRoyalPayInvoiceCallbackTransaction
    {
        [JsonProperty("uid")] public string Uid { get; set; }
        [JsonProperty("Id")] public string Id { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("amount")] public string Amount { get; set; }
        [JsonProperty("currency")] public string Currency { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("payment_method_type")] public string PaymentMethodType { get; set; }
        [JsonProperty("tracking_id")] public string TrackingId { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("test")] public string Test { get; set; }
        [JsonProperty("created_at")] public string CreatedAt { get; set; }
        [JsonProperty("updated_at")] public string UpdatedAt { get; set; }
        [JsonProperty("paid_at")] public string PaidAt { get; set; }
        [JsonProperty("expired_at")] public string ExpiredAt { get; set; }
        [JsonProperty("closed_at")] public string ClosedAt { get; set; }
        [JsonProperty("settled_at")] public string SettledAt { get; set; }
        [JsonProperty("language")] public string Language { get; set; }
        [JsonProperty("redirect_url")] public string RedirectUrl { get; set; }
        [JsonProperty("payment")] public CreateRoyalPayInvoicePayment Payment { get; set; }
        [JsonProperty("credit_card")] public CreateRoyalPayInvoiceCreditCard CreditCard { get; set; }
        [JsonProperty("customer")] public CreateRoyalPayInvoiceCallbackCustomer Customer { get; set; }
        [JsonProperty("billing_address")] public CreateRoyalPayInvoiceCallbackBillingAddress BillingAddress { get; set; }
    }

    public class CreateRoyalPayInvoiceBeProtectedVerification
    {
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("white_black_list")] public string WhiteBlackList { get; set; }
        [JsonProperty("rules")] public string Rules { get; set; }
    }

    public class CreateRoyalPayInvoiceCreditCard
    {
        [LogMasked(ShowFirst = 1, ShowLast = 1, PreserveLength = true)]
        [JsonProperty("holder")] public string Holder { get; set; }
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("stamp")] public string Stamp { get; set; }
        [JsonProperty("brand")] public string Brand { get; set; }
        [JsonProperty("last_4")] public string Last4 { get; set; }
        [JsonProperty("first_1")] public string First1 { get; set; }
        [JsonProperty("bin")] public string Bin { get; set; }
        [JsonProperty("issuer_country")] public string IssuerCountry { get; set; }
        [JsonProperty("issuer_name")] public string IssuerName { get; set; }
        [JsonProperty("product")] public string Product { get; set; }
        [NotLogged]
        [JsonProperty("exp_month")] public string ExpMonth { get; set; }
        [NotLogged]
        [JsonProperty("exp_year")] public string ExpYear { get; set; }
        [JsonProperty("token_provider")] public string TokenProvider { get; set; }
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("token")] public string Token { get; set; }
    }

    public class CreateRoyalPayInvoicePayment
    {
        [JsonProperty("auth_code")] public string AuthCode { get; set; }
        [JsonProperty("bank_code")] public string BankCode { get; set; }
        [JsonProperty("rrn")] public string Rrn { get; set; }
        [JsonProperty("ref_id")] public string RefId { get; set; }
        [JsonProperty("message")] public string Message { get; set; }
        [JsonProperty("amount")] public double Amount { get; set; }
        [JsonProperty("currency")] public string Currency { get; set; }
        [JsonProperty("billing_descriptor")] public string BillingDescriptor { get; set; }
        [JsonProperty("gateway_id")] public long GatewayId { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
    }

    public class CreateRoyalPayInvoiceCallbackCustomer
    {
        [JsonProperty("ip")] public string Ip { get; set; }
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("device_id")] public string DeviceId { get; set; }
        [JsonProperty("birth_date")] public string BirthDate { get; set; }
    }

    public class CreateRoyalPayInvoiceCallbackBillingAddress
    {
        [LogMasked(ShowFirst = 1, ShowLast = 1, PreserveLength = true)]
        [JsonProperty("first_name")] public string FirstName { get; set; }
        [LogMasked(ShowFirst = 1, ShowLast = 1, PreserveLength = true)]
        [JsonProperty("last_name")] public string LastName { get; set; }
        [JsonProperty("address")] public string Address { get; set; }
        [JsonProperty("country")] public string Country { get; set; }
        [JsonProperty("city")] public string City { get; set; }
        [JsonProperty("zip")] public string Zip { get; set; }
        [JsonProperty("state")] public string State { get; set; }
        [JsonProperty("phone")] public string Phone { get; set; }
    }

    public class CreateRoyalPayInvoiceCallbackBeProtectedVerification
    {
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("message")] public string Message { get; set; }

        [JsonProperty("white_black_list")]
        public CreateRoyalPayInvoiceCallbackWhiteBlackList WhiteBlackList { get; set; }
    }

    public class CreateRoyalPayInvoiceCallbackWhiteBlackList
    {
        [LogMasked(ShowFirst = 3, ShowLast = 3, PreserveLength = true)]
        [JsonProperty("email")] public string Email { get; set; }
        [JsonProperty("ip")] public string Ip { get; set; }
        [LogMasked(ShowFirst = 6, ShowLast = 4, PreserveLength = true)]
        [JsonProperty("card_number")] public string CardNumber { get; set; }
    }

    public class CreateRoyalPayInvoiceCallbackAvsCvcVerification
    {
        [JsonProperty("avs_verification")]
        public CreateRoyalPayInvoiceCallbackAvsVerification AvsVerification { get; set; }

        [JsonProperty("cvc_verification")]
        public CreateRoyalPayInvoiceCallbackCvcVerification CvcVerification { get; set; }
    }

    public class CreateRoyalPayInvoiceCallbackAvsVerification
    {
        [JsonProperty("result_code")] public string ResultCode { get; set; }
    }

    public class CreateRoyalPayInvoiceCallbackCvcVerification
    {
        [JsonProperty("result_code")] public string ResultCode { get; set; }
    }
}