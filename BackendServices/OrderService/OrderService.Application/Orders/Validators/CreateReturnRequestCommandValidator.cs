using FluentValidation;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Validators
{
    public sealed class CreateReturnRequestCommandValidator : AbstractValidator<CreateReturnRequestCommand>
    {
        public CreateReturnRequestCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty();

            RuleFor(x => x.CustomerId)
                .NotEmpty();

            RuleFor(x => x.ReturnType)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.ReturnReason)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.ReturnItemsJson)
                .NotEmpty();
        }
    }
}
