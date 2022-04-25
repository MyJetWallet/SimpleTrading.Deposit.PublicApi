using Finance.DirectaPublic.HttpContracts.Requests;
using Finance.PciDssPublic.HttpContracts.Requests;
using Finance.SwiffyPublic.HttpContracts.Requests;
using FluentValidation.Results;
using SimpleTrading.Deposit.PublicApi.Contracts;
using SimpleTrading.Deposit.PublicApi.Controllers;
using SimpleTrading.Deposit.PublicApi.Validation.Request;

namespace SimpleTrading.Deposit.PublicApi.Validation
{
    public static class ValidationUtils
    {
        public static ValidationResult Validate(this CreatePaymentInvoiceRequest request)
        {
            var validator = new CreateDepositValidator();
            return validator.Validate(request);
        }
        
        public static ValidationResult Validate(this GetCryptoWalletRequest request)
        {
            var validator = new GetCryptoWalletValidator();
            return validator.Validate(request);
        }
        
        public static ValidationResult Validate(this CreatePciDssInvoiceRequest request)
        {
            var validator = new CreatePciDssInvoiceValidator();
            return validator.Validate(request);
        }

        public static ValidationResult Validate(this CreateSwiffyInvoiceRequest request)
        {
            var validator = new CreateSwiffyDepositValidator();
            return validator.Validate(request);
        }

        public static ValidationResult Validate(this CreateDirectaInvoiceRequest request)
        {
            var validator = new CreateDirectaDepositValidator();
            return validator.Validate(request);
        }

        public static ValidationResult Validate(this CreateVoltInvoiceRequest request)
        {
            var validator = new CreateVoltDepositValidator();
            return validator.Validate(request);
        }

        public static ValidationResult Validate(this CreatePayRetailersInvoiceRequest request)
        {
            var validator = new CreatePayRetailersDepositValidator();
            return validator.Validate(request);
        }

        public static ValidationResult Validate(this CreatePayopInvoiceRequest request)
        {
            var validator = new CreatePayopDepositValidator();
            return validator.Validate(request);
        }
    }
}