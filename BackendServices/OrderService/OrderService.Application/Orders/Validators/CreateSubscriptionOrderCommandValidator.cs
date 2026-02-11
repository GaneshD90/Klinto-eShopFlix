using FluentValidation;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Validators
{
    public sealed class CreateSubscriptionOrderCommandValidator : AbstractValidator<CreateSubscriptionOrderCommand>
    {
        public CreateSubscriptionOrderCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty();

            RuleFor(x => x.CustomerId)
                .NotEmpty();

            RuleFor(x => x.Frequency)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.TotalOccurrences)
                .GreaterThan(0)
                .When(x => x.TotalOccurrences.HasValue);
        }
    }
}
