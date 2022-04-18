namespace SimpleTrading.Deposit.PublicApi.Contracts
{
    public class GetCryptoWalletRequest
    {
        public string AccountId { get; set; }
        public string Currency { get; set; }
    }
}