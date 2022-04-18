using Finance.SwiffyPublic.HttpContracts.Requests;
using FluentValidation;
using SimpleTrading.Deposit.PublicApi.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleTrading.Deposit.PublicApi.Validation.Request
{
    public class CreateSwiffyDepositValidator : AbstractValidator<CreateSwiffyInvoiceRequest>
    {
        public CreateSwiffyDepositValidator()
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
