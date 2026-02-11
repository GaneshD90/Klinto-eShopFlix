using FluentAssertions;
using PaymentService.Domain.Entities;
using Xunit;

namespace PaymentService.Domain.Tests;

public class PaymentDetailTests
{
    [Fact]
    public void ShouldAllowSettersAndGetters()
    {
        var detail = new PaymentDetail
        {
            Id = "pay_1",
            TransactionId = "txn_123",
            Tax = 5m,
            Currency = "USD",
            Total = 50m,
            Email = "user@example.com",
            Status = "Authorized",
            CartId = 10,
            GrandTotal = 55m,
            CreatedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UserId = 7
        };

        detail.Id.Should().Be("pay_1");
        detail.TransactionId.Should().Be("txn_123");
        detail.Tax.Should().Be(5m);
        detail.Currency.Should().Be("USD");
        detail.Total.Should().Be(50m);
        detail.Email.Should().Be("user@example.com");
        detail.Status.Should().Be("Authorized");
        detail.CartId.Should().Be(10);
        detail.GrandTotal.Should().Be(55m);
        detail.CreatedDate.Should().Be(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        detail.UserId.Should().Be(7);
    }
}
