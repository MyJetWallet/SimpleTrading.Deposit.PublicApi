using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SimpleTrading.Deposit.Grpc.Contracts;
using SimpleTrading.Deposit.Postgresql.Models;
using SimpleTrading.Deposit.PublicApi.Contracts;
using SimpleTrading.Deposit.PublicApi.Contracts.ABSplits;
using BrandName = Finance.PciDssIntegration.GrpcContracts.Contracts.BrandName;

namespace SimpleTrading.Deposit.PublicApi
{
    public static class SettingUtils
    {
        private const string MtAccountType = "mt";
        private const string StAccountType = "st";

        public static void SetupSwagger(this IServiceCollection services)
        {
            services.AddSwaggerDocument(o =>
            {
                o.Title = "Monfex Deposit Api";
                o.GenerateEnumMappingDescription = true;
            });
        }

        public static string GetHmacFromSecretKeyAndRequest(this string secretKey, string requestRawData)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var postDataBytes = Encoding.UTF8.GetBytes(requestRawData);
            var hmacsha512 = new HMACSHA512(keyBytes);
            var hmac = BitConverter.ToString(hmacsha512.ComputeHash(postDataBytes)).Replace("-", string.Empty);
            return hmac;
        }

        public static string GetRedirectUrl(this DepositModel deposit)
        {
            if (deposit.Brand == Postgresql.Models.BrandName.Monfex)
            {
                if (deposit.AccountId.Contains(MtAccountType)) return ServiceLocator.Settings.MonfexMtUrl;

                if (deposit.AccountId.Contains(StAccountType)) return ServiceLocator.Settings.MonfexStUrl;

                throw new NotSupportedException($"Account {deposit.AccountId} not supported");
            }

            if (deposit.Brand == Postgresql.Models.BrandName.HandelPro)
            {
                if (deposit.AccountId.Contains(MtAccountType)) return ServiceLocator.Settings.HandleProMtUrl;

                if (deposit.AccountId.Contains(StAccountType)) return ServiceLocator.Settings.HandleProStUrl;

                throw new NotSupportedException($"Account {deposit.AccountId} not supported");
            }

            if (deposit.Brand == Postgresql.Models.BrandName.Allianzmarket)
            {
                if (deposit.AccountId.Contains(MtAccountType)) return ServiceLocator.Settings.AllianzmarketMtUrl;

                if (deposit.AccountId.Contains(StAccountType)) return ServiceLocator.Settings.AllianzmarketStUrl;

                throw new NotSupportedException($"Account {deposit.AccountId} not supported");
            }

            throw new NotSupportedException($"Brand {deposit.Brand} not supported");
        }

        public static string GetStRedirectUrl(
            this BrandName? depositBrand)
        {
            if (depositBrand == BrandName.Monfex)
                return ServiceLocator.Settings.MonfexStUrl;

            if (depositBrand == BrandName.HandelPro)
                return ServiceLocator.Settings.HandleProStUrl;

            if (depositBrand == BrandName.Allianzmarket)
                return ServiceLocator.Settings.AllianzmarketStUrl;

            throw new NotSupportedException($"Brand {depositBrand} not supported");
        }

        public static PaymentSystem ToPaymentSystem(this PaymentSystemsEntity paymentSystemsEntity)
        {
            if (paymentSystemsEntity is null)
            {
                return null;
            }

            return PaymentSystem.Create(paymentSystemsEntity.PaymentSystemId.ToPaymentSystemType());
        }

        public static PaymentSystemType ToPaymentSystemType(this string paymentSystemId)
        {
            return paymentSystemId switch
            {
                "BANK_CARDS" => PaymentSystemType.BankCards,
                "BITCOIN" => PaymentSystemType.Bitcoin,
                "SWIFFY" => PaymentSystemType.Swiffy,
                "WIRETRANSFER" => PaymentSystemType.Wiretransfer,
                "DIRECTA" => PaymentSystemType.Directa,
                "VOLT" => PaymentSystemType.Volt,
                "PAYRETAILERS" => PaymentSystemType.PayRetailers,
                "PAYOP_LATAM" => PaymentSystemType.Payop,
                _ => PaymentSystemType.Undefined
            };
        }

        public static PaymentSystem ToPaymentSystemViaClientGroup(this PaymentSystemsEntity paymentSystemsEntity,
            ABSplitGroupType group)
        {
            if (paymentSystemsEntity is null)
            {
                return null;
            }

            return group == ABSplitGroupType.GroupA ? 
                PaymentSystem.Create(paymentSystemsEntity.PaymentSystemId.ToPaymentSystemTypeGroupA()) : 
                PaymentSystem.Create(paymentSystemsEntity.PaymentSystemId.ToPaymentSystemTypeGroupB());
        }
        public static PaymentSystemType ToPaymentSystemTypeGroupA(this string paymentSystemId)
        {
            return paymentSystemId switch
            {
                "BANK_CARDS" => PaymentSystemType.BankCards,
                "BITCOIN" => PaymentSystemType.Bitcoin,
                "SWIFFY" => PaymentSystemType.Swiffy,
                "WIRETRANSFER" => PaymentSystemType.Wiretransfer,
                "DIRECTA" => PaymentSystemType.Directa,
                "VOLT" => PaymentSystemType.Volt,
                "PAYRETAILERS" => PaymentSystemType.PayRetailers,
                _ => PaymentSystemType.Undefined
            };
        }

        public static PaymentSystemType ToPaymentSystemTypeGroupB(this string paymentSystemId)
        {
            return paymentSystemId switch
            {
                "BANK_CARDS" => PaymentSystemType.BankCards,
                "BITCOIN" => PaymentSystemType.Bitcoin,
                "SWIFFY" => PaymentSystemType.Swiffy,
                "WIRETRANSFER" => PaymentSystemType.Wiretransfer,
                "DIRECTA" => PaymentSystemType.Directa,
                "VOLT" => PaymentSystemType.Volt,
                "PAYOP_LATAM" => PaymentSystemType.Payop,
                _ => PaymentSystemType.Undefined
            };
        }
    }
}