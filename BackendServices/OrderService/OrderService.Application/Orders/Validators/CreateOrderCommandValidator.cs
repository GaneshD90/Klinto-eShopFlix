using FluentValidation;
using OrderService.Application.Orders.Commands;

namespace OrderService.Application.Orders.Validators
{
    public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty();

            RuleFor(x => x.CustomerEmail)
                .NotEmpty()
                .MaximumLength(255)
                .EmailAddress();

            RuleFor(x => x.CustomerPhone)
                .MaximumLength(50);

            RuleFor(x => x.OrderType)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.OrderSource)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.CurrencyCode)
                .NotEmpty()
                .Length(3);

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("Order must contain at least one item.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).NotEmpty();
                item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(500);
                item.RuleFor(i => i.Sku).NotEmpty().MaximumLength(100);
                item.RuleFor(i => i.Quantity).GreaterThan(0);
                item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
                item.RuleFor(i => i.OriginalPrice).GreaterThanOrEqualTo(0);
                item.RuleFor(i => i.GiftMessage).MaximumLength(500);
            });

            When(x => x.BillingAddress is not null, () =>
            {
                RuleFor(x => x.BillingAddress!.FirstName).NotEmpty().MaximumLength(100);
                RuleFor(x => x.BillingAddress!.LastName).NotEmpty().MaximumLength(100);
                RuleFor(x => x.BillingAddress!.AddressLine1).NotEmpty().MaximumLength(500);
                RuleFor(x => x.BillingAddress!.City).NotEmpty().MaximumLength(100);
                RuleFor(x => x.BillingAddress!.PostalCode).NotEmpty().MaximumLength(20);
                RuleFor(x => x.BillingAddress!.CountryCode).NotEmpty().Length(2, 3);
            });

            When(x => x.ShippingAddress is not null, () =>
            {
                RuleFor(x => x.ShippingAddress!.FirstName).NotEmpty().MaximumLength(100);
                RuleFor(x => x.ShippingAddress!.LastName).NotEmpty().MaximumLength(100);
                RuleFor(x => x.ShippingAddress!.AddressLine1).NotEmpty().MaximumLength(500);
                RuleFor(x => x.ShippingAddress!.City).NotEmpty().MaximumLength(100);
                RuleFor(x => x.ShippingAddress!.PostalCode).NotEmpty().MaximumLength(20);
                RuleFor(x => x.ShippingAddress!.CountryCode).NotEmpty().Length(2, 3);
            });
        }
    }
}
