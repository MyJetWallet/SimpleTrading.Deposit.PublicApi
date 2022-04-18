using System;
using System.Collections.Generic;

namespace SimpleTrading.Deposit.PublicApi.Contracts
{
    public class GetSupportedPaymentSystemsRequest
    {
        public string AccountId { get; set; }
    }

    public class GetSupportedPaymentSystemsResponse
    {
        public IReadOnlyCollection<PaymentSystem> SupportedPaymentSystems { get; set; } = new List<PaymentSystem>();
    }

    public class PaymentSystem
    {
        public PaymentSystemType PaymentSystemType { get; set; }
        public BasePaymentSystemMetadata Metadata { get; set; }

        private PaymentSystem(PaymentSystemType paymentSystemType, BasePaymentSystemMetadata metadata)
        {
            PaymentSystemType = paymentSystemType;
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public static PaymentSystem Create(PaymentSystemType paymentSystemType, BasePaymentSystemMetadata metadata = null)
        {
            return new PaymentSystem(paymentSystemType, metadata ?? new BasePaymentSystemMetadata());
        }

        public static int SortBitcoinLast(PaymentSystem x, PaymentSystem y)
        {
            if (x.PaymentSystemType == y.PaymentSystemType)
                return 0;

            if (x.PaymentSystemType == PaymentSystemType.BankCards)
                return -1;

            if (y.PaymentSystemType == PaymentSystemType.BankCards)
                return 1;

            if (x.PaymentSystemType == PaymentSystemType.Bitcoin)
                return 1;

            if (y.PaymentSystemType == PaymentSystemType.Bitcoin)
                return -1;

            return x.PaymentSystemType - y.PaymentSystemType;
        }
    }

    public class BasePaymentSystemMetadata
    {
    }

    public enum PaymentSystemType
    {
        Undefined = 0,
        BankCards,
        Wiretransfer,
        Bitcoin,
        Swiffy,
        Directa,
        Volt,
        PayRetailers,
        Payop
    }
}
