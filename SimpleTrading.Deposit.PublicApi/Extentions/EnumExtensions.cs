using System;
using SimpleTrading.Deposit.PublicApi.Contracts;
using SimpleTrading.Deposit.PublicApi.Contracts.ABSplits;

namespace SimpleTrading.Deposit.PublicApi.Extentions
{
    public static class EnumExtensions
    {
        public static T MapEnum<T>(this object value)
            where T : struct, IConvertible
        {
            if (value == null)
            {
                throw new ArgumentException($"Value was null while mapping {typeof(T)}");
            }

            var sourceType = value.GetType();
            if (!sourceType.IsEnum)
                throw new ArgumentException($"Source type is not enum, while mapping {typeof(T)}");
            if (!typeof(T).IsEnum)
                throw new ArgumentException($"Destination type is not enum, while mapping {typeof(T)}");
            return (T) Enum.Parse(typeof(T), value.ToString()!);
        }

        public static T ToEnum<T>(this string value)
        {
            return (T) Enum.Parse(typeof(T), value, true);
        }

        public static T ToEnumOrDefault<T>(this string value, T defaultValue)
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            return (T) Enum.Parse(typeof(T), value, true);
        }

        public static ABSplitGroupType GetSplitType(this string splitType)
        {
            return splitType.ToEnum<ABSplitGroupType>();
        }

        public static PaymentSystemType GetPaymentType(this string paymentType)
        {
            return paymentType.ToEnum<PaymentSystemType>();
        }
    }
}
