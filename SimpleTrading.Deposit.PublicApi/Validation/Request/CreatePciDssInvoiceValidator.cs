using System;
using DotNetCoreDecorators;
using Finance.PciDssPublic.HttpContracts.Requests;
using FluentValidation;
using SimpleTrading.Deposit.PublicApi.Contracts;

namespace SimpleTrading.Deposit.PublicApi.Validation.Request
{
    public class CreatePciDssInvoiceValidator : AbstractValidator<CreatePciDssInvoiceRequest>
    {
        public CreatePciDssInvoiceValidator()
        {
            RuleFor(data => data.CardNumber).NotEmpty().Matches("^[0-9]+$")
                .WithMessage("Please, specify a CardNumber. Should contains only numbers");
            
            RuleFor(data => data.Cvv).NotEmpty()
                .WithMessage("Please, specify a Cvv.");

            RuleFor(data => data.ExpirationDate).NotNull()
                .Must(date =>
                {
                    var currentDatetime = DateTime.UtcNow;
                    return date >= new DateTime(currentDatetime.Year, currentDatetime.Month, 1).UnixTime();
                })
                .WithMessage("Please, specify a ExpirationDate. ExpirationDate >= Current date");

            RuleFor(data => data.FullName).NotEmpty()
                .WithMessage("Please, specify a FullName.");

            RuleFor(data => data.Amount).NotEmpty()
                .WithMessage("Please, specify a Amount.")
                .GreaterThan(0)
                .WithMessage("Should be greater than 0");
                
            RuleFor(data => data.AccountId).NotEmpty()
                .WithMessage("Please, specify a AccountId.");
        }
    }
}