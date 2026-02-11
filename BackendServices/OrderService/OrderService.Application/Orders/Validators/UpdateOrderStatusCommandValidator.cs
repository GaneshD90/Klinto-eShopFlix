using FluentValidation;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Validators
{
    public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
    {
        private static readonly string[] AllowedStatuses = new[]
        {
            "Pending", "Confirmed", "Processing", "OnHold", "Shipped",
            "Delivered", "Completed", "Cancelled", "Refunded", "PartiallyRefunded",
            "AwaitingPayment", "PaymentFailed", "ReadyForPickup"
        };

        public UpdateOrderStatusCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty();

            RuleFor(x => x.NewStatus)
                .NotEmpty()
                .MaximumLength(50)
                .Must(s => AllowedStatuses.Contains(s))
                .WithMessage($"Status must be one of: {string.Join(", ", AllowedStatuses)}");

            RuleFor(x => x.Notes)
                .MaximumLength(2000);

            RuleFor(x => x.ChangeReason)
                .MaximumLength(500);
        }
    }
}
