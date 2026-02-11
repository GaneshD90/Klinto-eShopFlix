using FluentValidation;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Validators
{
    public sealed class PlaceOrderOnHoldCommandValidator : AbstractValidator<PlaceOrderOnHoldCommand>
    {
        public PlaceOrderOnHoldCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty();

            RuleFor(x => x.HoldType)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.HoldReason)
                .NotEmpty()
                .MaximumLength(1000);
        }
    }
}
