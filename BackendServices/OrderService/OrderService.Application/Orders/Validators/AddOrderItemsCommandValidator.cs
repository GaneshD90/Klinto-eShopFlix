using FluentValidation;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Validators
{
    public sealed class AddOrderItemsCommandValidator : AbstractValidator<AddOrderItemsCommand>
    {
        public AddOrderItemsCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty();

            RuleFor(x => x.OrderItemsJson)
                .NotEmpty();
        }
    }
}
