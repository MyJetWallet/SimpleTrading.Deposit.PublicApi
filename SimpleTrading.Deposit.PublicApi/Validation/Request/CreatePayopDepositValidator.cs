using FluentValidation;
using SimpleTrading.Deposit.PublicApi.Controllers;

namespace SimpleTrading.Deposit.PublicApi.Validation.Request
{
    public class CreatePayopDepositValidator : AbstractValidator<CreatePayopInvoiceRequest>
    {
        public CreatePayopDepositValidator()
        {
            RuleFor(data => data.Amount)
                .NotEmpty()
                .WithMessage("Please specify a Amount.")
                .GreaterThan(0)
                .WithMessage("Should be greater than 0");

            RuleFor(data => data.AccountId).NotEmpty()
                .WithMessage("Please specify a AccountId.");

            RuleFor(data => data.ProcessId).NotEmpty()
                .WithMessage("Please specify a ProcessId.");
        }
    }
}
