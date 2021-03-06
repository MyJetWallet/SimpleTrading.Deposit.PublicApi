using System;
using NUnit.Framework;
using Newtonsoft.Json;
using SimpleTrading.Deposit.PublicApi.Contracts;
using SimpleTrading.Deposit.PublicApi.Contracts.ABSplits;
using SimpleTrading.Deposit.PublicApi.Extentions;

namespace SimpleTrading.Deposit.PublicApi.Test
{
    class ABSplitParsingTest
    {
        [TestCase("{\"ABSplitName\":\"Copy - testSplit\",\"ABSplitGroupType\":\"GroupA\"}", "GroupA")]
        [TestCase("{\"ABSplitName\":\"testSplit\",\"ABSplitGroupType\":\"GroupB\"}", "GroupB")]
        [TestCase("{\"ABSplitName\":\"\",\"ABSplitGroupType\":\"GroupB\"}", "GroupB")]
        [TestCase("{\"ABSplitName\":\"WBonus_30_vs_30_20_10_Monfex\",\"ABSplitGroupType\":\"GroupB\"}", "GroupB")]
        public void GetABSplitGroup(string rawSplitData, string group)
        {
            var splitType = group.Equals("GroupA", StringComparison.OrdinalIgnoreCase)
                ? ABSplitGroupType.GroupA
                : ABSplitGroupType.GroupB;

            var split = JsonConvert.DeserializeObject<ABSplit>(rawSplitData);


            Assert.AreEqual(split.Type.GetSplitType(), splitType);
        }


        [TestCase("WBonus_30_vs_30_20_10_Monfex@GroupA@BankCards|WBonus_30_vs_30_20_10_Monfex@GroupB@BankCards", "BankCards", "BankCards")]
        [TestCase("WBonus_30_vs_30_20_10_Monfex@GroupA@BankCards|WBonus_30_vs_30_20_10_Monfex@GroupB@BankCards", "BankCards", "BankCards")]
        public void GetGroupSettings(string settings, string groupA, string groupB)
        {
            var gA = settings
                .GetGroupPaymentSystemTypeOrDefault(ABSplitGroupType.GroupA, PaymentSystemType.BankCards);
            var gAType = groupA.ToEnum<PaymentSystemType>();

            var gB = settings
                .GetGroupPaymentSystemTypeOrDefault(ABSplitGroupType.GroupB, PaymentSystemType.BankCards);
            var gBType = groupB.ToEnum<PaymentSystemType>();

            Assert.AreEqual(gA.PaymentType, gAType);
            Assert.AreEqual(gB.PaymentType, gBType);
        }
    }
}
