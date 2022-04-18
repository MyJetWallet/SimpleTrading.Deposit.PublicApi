using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using SimpleTrading.Deposit.PublicApi.Contracts.Callbacks;

namespace SimpleTrading.Deposit.PublicApi.Validation
{
    public static class CallbackValidator
    {
        public static bool IsTexcentCallbackValid(this TexcentCallback callback, IEnumerable<string> texcentUserIds)
        {
            var data = new List<string>
            {
                string.IsNullOrEmpty(callback.transactionCcy)?callback.amount.ToString(System.Globalization.CultureInfo.InvariantCulture): callback.transactionAmount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                string.IsNullOrEmpty(callback.transactionCcy)?callback.ccy: callback.transactionCcy,
                callback.orderId,
                callback.status
            };
            var joined = string.Join("", data);
            foreach (var texcentUserId in texcentUserIds)
            {
                var sha256Key = $"{texcentUserId}{callback.transactionId}".GetMd5();
                var hmacSha256Hash = joined.GetHmacSha256Hash(sha256Key);
                if (callback.signature == hmacSha256Hash)
                {
                    return true;
                } 
            }
            return false;
        }

        public static bool IsRoyalPayCallbackValid(this string authString)
        {
            var royalPayBasicAuth = AuthenticationHeaderValue.Parse(authString);

            var credentials = Encoding.UTF8.GetString(
                Convert.FromBase64String(royalPayBasicAuth.Parameter)
            ).Split(new[] {':'}, 2);

            var username = credentials[0];
            var password = credentials[1];

            return IsHendelPro(username, password) || IsMonfex(username, password) || IsAllianzmarket(username, password);
        }

        //public static bool IsXpateCallbackValid(this XpateCallback callback, IEnumerable<string> xpateUserIds)
        //{
        //    return true;
        //}

        private static bool IsHendelPro(string username, string password)
        {
            return ServiceLocator.Settings.RoyalPayUsername == username &&
                   ServiceLocator.Settings.RoyalPayPassword == password;
        }

        private static bool IsMonfex(string username, string password)
        {
            return ServiceLocator.Settings.MonfexRoyalPayUsername == username &&
                   ServiceLocator.Settings.MonfexRoyalPayPassword == password;
        }

        private static bool IsAllianzmarket(string username, string password)
        {
            return ServiceLocator.Settings.AllianzmarketRoyalPayUsername == username &&
                   ServiceLocator.Settings.AllianzmarketRoyalPayPassword == password;
        }
    }
}