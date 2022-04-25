using FluentValidation;
using SimpleTrading.Deposit.PublicApi.Contracts;

namespace SimpleTrading.Deposit.PublicApi.Validation.Request
{
    public class CreateDepositValidator : AbstractValidator<CreatePaymentInvoiceRequest>
    {
        public CreateDepositValidator()
        {
            RuleFor(data => data.PaymentMethod).NotEmpty()
                .WithMessage("Please specify a PaymentMethod. Allowed: BANK_CARDS");
            RuleFor(data => data.Currency).NotEmpty()
                .WithMessage("Please specify a Currency.");
            RuleFor(data => data.DepositSum).NotEmpty()
                .WithMessage("Please specify a DepositSum.");
            RuleFor(data => data.AccountId).NotEmpty()
                .WithMessage("Please specify a AccountId.");
        }
    }
}