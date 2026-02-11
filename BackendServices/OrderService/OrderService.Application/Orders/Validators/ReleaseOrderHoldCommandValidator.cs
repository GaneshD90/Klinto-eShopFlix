using FluentValidation;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Validators
{
    public sealed class ReleaseOrderHoldCommandValidator : AbstractValidator<ReleaseOrderHoldCommand>
    {
        public ReleaseOrderHoldCommandValidator()
        {
            RuleFor(x => x.HoldId)
                .NotEmpty();
        }
    }
}
