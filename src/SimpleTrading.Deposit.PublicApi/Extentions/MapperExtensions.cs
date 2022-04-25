using System.Linq;
using SimpleTrading.Deposit.PublicApi.Contracts;
using SimpleTrading.Deposit.PublicApi.Contracts.ABSplits;

namespace SimpleTrading.Deposit.PublicApi.Extentions
{
    public static class MapperExtensions
    {
        public static ABSplitPayments GetGroupPaymentSystemTypeOrDefault(this string mappingString, 
            ABSplitGroupType splitGroupType, PaymentSystemType defaultPaymentSystem)
        {
            var mapping =
                mappingString
                    .Split("|")
                    .Select(item => item.Split("@"))
                    .Select(item => ABSplitPayments.Create(item[0], item[1], item[2]));

            foreach (var abSplit in mapping)
            {
                if (abSplit.SplitType == splitGroupType)
                {
                    return abSplit;
                }
            }
            return ABSplitPayments.Create("", splitGroupType, defaultPaymentSystem);
        }
    }
}
