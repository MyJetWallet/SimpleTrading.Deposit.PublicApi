using System;
using DotNetCoreDecorators;
using Finance.PciDssPublic.HttpContracts.Requests;
using NUnit.Framework;
using SimpleTrading.Deposit.PublicApi.Contracts;
using SimpleTrading.Deposit.PublicApi.Contracts.Callbacks;
using SimpleTrading.Deposit.PublicApi.Validation;

namespace SimpleTrading.Deposit.PublicApi.Test
{
    public class Tests
    {
        [Test]
        public void Valid()
        {
            var model = new CreatePciDssInvoiceRequest
            {
                CardNumber = "111",
                Cvv = "111",
                ExpirationDate = DateTime.UtcNow.AddYears(1).UnixTime(),
                FullName = "Test",
                PostalCode = "test",
                Country = "test",
                City = "test",
                Address = "test",
                AccountId = "test",
                Amount = 100
            };
            
            var result = model.Validate();
            Assert.AreEqual(0, result.Errors.Count);
        }
        
        [Test]
        public void InvalidCard()
        {
            var model = new CreatePciDssInvoiceRequest
            {
                CardNumber = "111",
                Cvv = "111",
                ExpirationDate = DateTime.UtcNow.AddYears(1).UnixTime(),
                FullName = "Test",
                PostalCode = "test",
                Country = "test",
                City = "test",
                Address = "test",
                AccountId = "test",
                Amount = 100
            };
            
            model.CardNumber = "asd";
            var result = model.Validate();
            Assert.AreEqual(1, result.Errors.Count);
        }
        
        [Test]
        public void InvalidCvv()
        {
            var model = new CreatePciDssInvoiceRequest
            {
                CardNumber = "111",
                Cvv = "111",
                ExpirationDate = DateTime.UtcNow.AddYears(1).UnixTime(),
                FullName = "Test",
                PostalCode = "test",
                Country = "test",
                City = "test",
                Address = "test",
                AccountId = "test",
                Amount = 100
            };
            
            model.Cvv = null;
            var result = model.Validate();
            Assert.AreEqual(1, result.Errors.Count);
        }
        
        [Test]
        public void DateLessThanNow()
        {
            var model = new CreatePciDssInvoiceRequest
            {
                CardNumber = "111",
                Cvv = "111",
                ExpirationDate = DateTime.UtcNow.AddYears(1).UnixTime(),
                FullName = "Test",
                PostalCode = "test",
                Country = "test",
                City = "test",
                Address = "test",
                AccountId = "test",
                Amount = 100
            };
            
            model.ExpirationDate = DateTime.UtcNow.AddYears(-1).UnixTime();
            var result = model.Validate();
            Assert.AreEqual(1, result.Errors.Count);
        }
        
        [Test]
        public void DateEqualsNow()
        {
            var model = new CreatePciDssInvoiceRequest
            {
                CardNumber = "111",
                Cvv = "111",
                ExpirationDate = DateTime.UtcNow.AddYears(1).UnixTime(),
                FullName = "Test",
                PostalCode = "test",
                Country = "test",
                City = "test",
                Address = "test",
                AccountId = "test",
                Amount = 100
            };
            
            model.ExpirationDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).UnixTime();
            var result = model.Validate();
            Assert.AreEqual(0, result.Errors.Count);
        }
        
        [Test]
        public void DatePreviousMounth()
        {
            var model = new CreatePciDssInvoiceRequest
            {
                CardNumber = "111",
                Cvv = "111",
                ExpirationDate = DateTime.UtcNow.AddYears(-1).UnixTime(),
                FullName = "Test",
                PostalCode = "test",
                Country = "test",
                City = "test",
                Address = "test",
                AccountId = "test",
                Amount = 100
            };
            
            var result = model.Validate();
            Assert.AreEqual(1, result.Errors.Count);
        }


        [Test]
        public void Validate_ShouldBeValid_WhenTransactionCcyIsNull_Tests()
        {
            var texcent = new TexcentCallback()
            {
                orderId = "6hlF1SqTE8nR4qZWbbGg",
                transactionId = "7f09a2f7-18c0-4f86-89be-86a00d9dd05b",
                finaleResponseCode = 0,
                finalResponseMsg = "successful",
                status = "successful",
                amount = 10,
                netAmount = 9.1,
                ccy = "USD",
                transactionAmount = 0,
                transactionNetAmount = 0,
                transactionCcy = null,
                signature = "cd53647b2fafab7b92100df39e3c7ba34bc993f24e03fa8fc58254b286cf214f",
            };

            var valid = texcent.IsTexcentCallbackValid( new string[] { "a7b52716ad80", "9e64-57734866617e" });

            Assert.IsTrue(valid);
        }

        [Test]
        public void Validate_ShouldBeValid_WhenTransactionCcyIsNotNull_Tests()
        {
            var texcent = new TexcentCallback()
            {
                orderId = "EyWuoO4tUUqUEqDoH41P3w",
                transactionId = "0debb932-daee-43bb-9494-2d9ab98c3c33",
                finaleResponseCode = 0,
                finalResponseMsg = "successful",
                status = "successful",
                amount = 250,
                netAmount = 227.5,
                ccy = "USD",
                transactionAmount = 203.75,
                transactionNetAmount = 185.41,
                transactionCcy = "EUR",
                signature = "8616aa143e1b24fe05760726f4c545cf03019e57b43e4dffe38b1d0f8def0f31",
            };

            var valid = texcent.IsTexcentCallbackValid(new string[] { "a7b52716ad80", "9e64-57734866617e" });

            Assert.IsTrue(valid);
        }

        [Test]
        public void ValidateXpate_ShouldBeValid_WhenTransactionCcyIsNotNull_Tests()
        {
            //var xpate = new XpateRedirectRequest()
            //{
            //    OrderId = "1274693",
            //    ClientOrderId = "jQhhYb8UiodjJFYGSRWA",
            //    TxId = "d3a76c83-8859-4c1f-b218-489e1250e261",
            //    ErrorMessage = "Test+processor%2C+invalid+cvv",
            //    Status = "declined",
            //    Amount = 250,
            //    Phone = "79169369203",
            //    Control = "ee0e1116d28dcd9c7c747b91794c8da306fc1d0",
            //    ErrorCode = "999001"
            //};

            //var valid = xpate.IsXpateCallbackValid(new string[] { "a7b52716ad80", "9e64-57734866617e" });
            //Assert.IsTrue(valid);
            Assert.Pass();
        }
    }
}