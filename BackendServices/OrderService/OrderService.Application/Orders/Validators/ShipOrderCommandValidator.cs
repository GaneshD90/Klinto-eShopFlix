using FluentValidation;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Validators
{
    public sealed class ShipOrderCommandValidator : AbstractValidator<ShipOrderCommand>
    {
        public ShipOrderCommandValidator()
        {
            RuleFor(x => x.ShipmentId)
                .NotEmpty();

            RuleFor(x => x.TrackingNumber)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.TrackingUrl)
                .MaximumLength(500);
        }
    }
}
