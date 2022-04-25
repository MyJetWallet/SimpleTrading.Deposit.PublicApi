using FluentValidation;
using SimpleTrading.Deposit.PublicApi.Contracts;

namespace SimpleTrading.Deposit.PublicApi.Validation.Request
{
    public class GetCryptoWalletValidator : AbstractValidator<GetCryptoWalletRequest>
    {
        public GetCryptoWalletValidator()
        {
            RuleFor(data => data.Currency).NotEmpty()
                .WithMessage("Please specify a Currency. Allowed: BTC, LTCT");
            RuleFor(data => data.AccountId).NotEmpty()
                .WithMessage("Please specify a AccountId.");
        }
    }
}