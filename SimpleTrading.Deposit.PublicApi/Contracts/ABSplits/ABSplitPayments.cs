using SimpleTrading.Deposit.PublicApi.Extentions;

namespace SimpleTrading.Deposit.PublicApi.Contracts.ABSplits
{
    public class ABSplitPayments
    {
        public PaymentSystemType PaymentType;
        public string Name;
        public ABSplitGroupType SplitType;

        public static ABSplitPayments Create(string name, string splitType, string paymentType)
        {
            return new ABSplitPayments
            {
                PaymentType = paymentType.GetPaymentType(), 
                Name = name, 
                SplitType = splitType.GetSplitType()
            };
        }

        public static ABSplitPayments Create(string name, ABSplitGroupType splitType, PaymentSystemType paymentType)
        {
            return new ABSplitPayments
            {
                PaymentType = paymentType,
                Name = name,
                SplitType = splitType   
            };
        }
    };
}