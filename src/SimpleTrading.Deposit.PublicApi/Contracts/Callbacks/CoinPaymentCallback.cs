namespace SimpleTrading.Deposit.PublicApi.Contracts.Callbacks
{
    public class CoinPaymentCallback
    {
        public string address { get; set; }
        public string amount { get; set; }
        public string amounti { get; set; }
        public string confirms { get; set; }
        public string currency { get; set; }
        public string deposit_id { get; set; }
        public string fee { get; set; }
        public string feei { get; set; }
        public string fiat_amount { get; set; }
        public string fiat_amounti { get; set; }
        public string fiat_coin { get; set; }
        public string fiat_fee { get; set; }
        public string fiat_feei { get; set; }
        public string ipn_id { get; set; }
        public string ipn_mode { get; set; }
        public string ipn_type { get; set; }
        public string ipn_version { get; set; }
        public string merchant { get; set; }
        public int status { get; set; }
        public string status_text { get; set; }
        public string txn_id { get; set; }
    }
}