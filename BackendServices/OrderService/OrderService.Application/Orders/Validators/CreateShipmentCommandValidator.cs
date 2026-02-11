using FluentValidation;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Validators
{
    public sealed class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
    {
        public CreateShipmentCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty();

            RuleFor(x => x.ShippingMethod)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.CarrierName)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.ShipmentItemsJson)
                .NotEmpty();
        }
    }
}
