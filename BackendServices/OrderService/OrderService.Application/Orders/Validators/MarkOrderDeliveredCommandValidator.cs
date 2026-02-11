using FluentValidation;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Validators
{
    public sealed class MarkOrderDeliveredCommandValidator : AbstractValidator<MarkOrderDeliveredCommand>
    {
        public MarkOrderDeliveredCommandValidator()
        {
            RuleFor(x => x.ShipmentId)
                .NotEmpty();

            RuleFor(x => x.DeliverySignature)
                .MaximumLength(200);

            RuleFor(x => x.DeliveryProofImage)
                .MaximumLength(500);
        }
    }
}
