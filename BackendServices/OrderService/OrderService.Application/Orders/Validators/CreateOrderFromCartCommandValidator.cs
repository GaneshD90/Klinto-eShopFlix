using FluentValidation;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Validators
{
    public sealed class CreateOrderFromCartCommandValidator : AbstractValidator<CreateOrderFromCartCommand>
    {
        public CreateOrderFromCartCommandValidator()
        {
            RuleFor(x => x.CartId)
                .NotEmpty();

            RuleFor(x => x.CustomerId)
                .NotEmpty();

            RuleFor(x => x.CustomerEmail)
                .NotEmpty()
                .MaximumLength(255)
                .EmailAddress();

            RuleFor(x => x.OrderSource)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.PaymentMethod)
                .MaximumLength(50);
        }
    }
}
