using Finance.DirectaPublic.HttpContracts.Requests;
using FluentValidation;

namespace SimpleTrading.Deposit.PublicApi.Validation.Request
{
    public class CreateDirectaDepositValidator : AbstractValidator<CreateDirectaInvoiceRequest>
    {
        public CreateDirectaDepositValidator()
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
