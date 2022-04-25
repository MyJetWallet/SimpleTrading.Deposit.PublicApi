namespace SimpleTrading.Deposit.PublicApi.Contracts.Callbacks
{
    public class TexcentCallback
    {
        public string orderId { get; set; }
        public string transactionId { get; set; }
        public int finaleResponseCode { get; set; }
        public string finalResponseMsg { get; set; }
        public string status { get; set; }
        public int amount { get; set; }
        public double netAmount { get; set; }
        public string ccy { get; set; }
        public double transactionAmount { get; set; }
        public double transactionNetAmount { get; set; }
        public string transactionCcy { get; set; }
        public string signature { get; set; }
    }
}