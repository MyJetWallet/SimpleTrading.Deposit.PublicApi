using System.Collections.Generic;
using NUnit.Framework;
using SimpleTrading.Deposit.PublicApi.Contracts;

namespace SimpleTrading.Deposit.PublicApi.Test
{
    public class PaymentSystemOrderTests
    {
        [Test]
        public void Sort_Bank_And_Bitcoin_1()
        {
            var paymentSystems = new List<PaymentSystem>();
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Bitcoin));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.BankCards));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Directa));
            paymentSystems.Sort(PaymentSystem.SortBitcoinLast);

            Assert.AreEqual(paymentSystems[0].PaymentSystemType, PaymentSystemType.BankCards);
            Assert.AreEqual(paymentSystems[paymentSystems.Count-1].PaymentSystemType, PaymentSystemType.Bitcoin);
        }


        [Test]
        public void Sort_Bank_And_Bitcoin_2()
        {
            var paymentSystems = new List<PaymentSystem>();
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Bitcoin));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.PayRetailers));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.BankCards));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Swiffy));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Volt));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Wiretransfer));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Payop));
            paymentSystems.Sort(PaymentSystem.SortBitcoinLast);

            Assert.AreEqual(paymentSystems[0].PaymentSystemType, PaymentSystemType.BankCards);
            Assert.AreEqual(paymentSystems[paymentSystems.Count - 1].PaymentSystemType, PaymentSystemType.Bitcoin);
        }


        [Test]
        public void Sort_Bank_Without_Bitcoin()
        {
            var paymentSystems = new List<PaymentSystem>();
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.PayRetailers));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.BankCards));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Swiffy));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Volt));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Payop));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Wiretransfer));
            paymentSystems.Sort(PaymentSystem.SortBitcoinLast);

            Assert.AreEqual(paymentSystems[0].PaymentSystemType, PaymentSystemType.BankCards);
            Assert.AreNotEqual(paymentSystems[paymentSystems.Count - 1].PaymentSystemType, PaymentSystemType.Bitcoin);
        }

        [Test]
        public void Sort_Bitcoin_Without_Bank()
        {
            var paymentSystems = new List<PaymentSystem>();
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Bitcoin));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.PayRetailers));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Swiffy));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Volt));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Wiretransfer));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Payop));
            paymentSystems.Sort(PaymentSystem.SortBitcoinLast);

            Assert.AreNotEqual(paymentSystems[0].PaymentSystemType, PaymentSystemType.BankCards);
            Assert.AreEqual(paymentSystems[paymentSystems.Count - 1].PaymentSystemType, PaymentSystemType.Bitcoin);
        }

        [Test]
        public void Sort_Without_Bank_And_Bitcoin()
        {
            var paymentSystems = new List<PaymentSystem>();
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.PayRetailers));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Swiffy));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Volt));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Wiretransfer));
            paymentSystems.Add(PaymentSystem.Create(PaymentSystemType.Payop));
            paymentSystems.Sort(PaymentSystem.SortBitcoinLast);

            Assert.AreNotEqual(paymentSystems[0].PaymentSystemType, PaymentSystemType.BankCards);
            Assert.AreNotEqual(paymentSystems[paymentSystems.Count - 1].PaymentSystemType, PaymentSystemType.Bitcoin);
        }
    }
}