using FluentValidation;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Validators
{
    public sealed class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
    {
        public ProcessPaymentCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty();

            RuleFor(x => x.PaymentMethod)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.PaymentProvider)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Amount)
                .GreaterThan(0);

            RuleFor(x => x.TransactionId)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.AuthorizationCode)
                .MaximumLength(100);
        }
    }
}
