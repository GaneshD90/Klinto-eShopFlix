using FluentValidation;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Validators
{
    public sealed class ProcessRefundCommandValidator : AbstractValidator<ProcessRefundCommand>
    {
        public ProcessRefundCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty();

            RuleFor(x => x.RefundAmount)
                .GreaterThan(0);

            RuleFor(x => x.RefundType)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.RefundMethod)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.RefundReason)
                .NotEmpty()
                .MaximumLength(1000);
        }
    }
}
