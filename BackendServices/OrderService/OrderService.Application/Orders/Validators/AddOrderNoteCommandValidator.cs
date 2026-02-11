using FluentValidation;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Validators
{
    public sealed class AddOrderNoteCommandValidator : AbstractValidator<AddOrderNoteCommand>
    {
        public AddOrderNoteCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty();

            RuleFor(x => x.NoteType)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Note)
                .NotEmpty()
                .MaximumLength(4000);
        }
    }
}
