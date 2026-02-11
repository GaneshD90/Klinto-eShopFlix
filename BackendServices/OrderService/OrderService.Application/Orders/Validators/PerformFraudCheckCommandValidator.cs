using FluentValidation;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Validators
{
    public sealed class PerformFraudCheckCommandValidator : AbstractValidator<PerformFraudCheckCommand>
    {
        public PerformFraudCheckCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty();

            RuleFor(x => x.FraudProvider)
                .NotEmpty()
                .MaximumLength(200);
        }
    }
}
