using FluentValidation;
using OrderService.Application.Orders.Queries;

namespace OrderService.Application.Orders.Validators
{
    public sealed class SearchOrdersQueryValidator : AbstractValidator<SearchOrdersQuery>
    {
        public SearchOrdersQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100);

            RuleFor(x => x.OrderStatus)
                .MaximumLength(50);

            RuleFor(x => x.PaymentStatus)
                .MaximumLength(50);

            RuleFor(x => x.FulfillmentStatus)
                .MaximumLength(50);

            RuleFor(x => x.ToDate)
                .GreaterThanOrEqualTo(x => x.FromDate)
                .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
                .WithMessage("ToDate must be greater than or equal to FromDate.");
        }
    }
}
