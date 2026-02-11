using FluentValidation;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Validators
{
    public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
    {
        public CancelOrderCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty();

            RuleFor(x => x.CancellationType)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.CancellationReason)
                .NotEmpty()
                .MaximumLength(1000);

            RuleFor(x => x.CancelledByType)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}
