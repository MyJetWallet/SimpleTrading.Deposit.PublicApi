using Finance.PciDssPublic.HttpContracts.Requests;
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

        public static ValidationResult Validate(this CreatePayopInvoiceRequest request)
        {
            var validator = new CreatePayopDepositValidator();
            return validator.Validate(request);
        }
    }
}