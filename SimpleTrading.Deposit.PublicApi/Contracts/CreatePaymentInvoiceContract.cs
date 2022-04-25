
namespace SimpleTrading.Deposit.PublicApi.Contracts
{
    public class CreatePaymentInvoiceRequest
    {
        public string PaymentMethod { get; set; }
        public double DepositSum { get; set; }
        public string Currency { get; set; }
        public string AccountId { get; set; }
    }


    public class CreatePaymentInvoiceResponse
    {
        public CreateInvoiceErrorEnum Status { get; set; }
        public string RedirectUrl { get; set; }
        public static CreatePaymentInvoiceResponse Create(string redirectUrl)
        {
            return new CreatePaymentInvoiceResponse
            {
                Status = CreateInvoiceErrorEnum.Success,
                RedirectUrl = redirectUrl
            };
        }
    }
}