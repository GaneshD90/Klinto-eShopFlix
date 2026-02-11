using FluentValidation;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Validators
{
    public sealed class ProcessNextSubscriptionCommandValidator : AbstractValidator<ProcessNextSubscriptionCommand>
    {
        public ProcessNextSubscriptionCommandValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .NotEmpty();
        }
    }
}
