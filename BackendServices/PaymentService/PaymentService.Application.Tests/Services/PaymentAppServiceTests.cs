using AutoMapper;
using FluentAssertions;
using Moq;
using PaymentService.Application.DTOs;
using PaymentService.Application.Mappers;
using PaymentService.Application.Repositories;
using PaymentService.Application.Services.Implementations;
using PaymentService.Domain.Entities;
using Xunit;

namespace PaymentService.Application.Tests.Services;

public class PaymentAppServiceTests
{
    private static IMapper CreateMapper()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<PaymentMapper>());
        return cfg.CreateMapper();
    }

    [Fact]
    public void SavePaymentDetails_ShouldMapAndPersist()
    {
        var repo = new Mock<IPaymentRepository>();
        var mapper = CreateMapper();
        repo.Setup(r => r.SavePayementDetails(It.IsAny<PaymentDetail>())).Returns(true);

        var sut = new PaymentAppService(repo.Object, mapper);
        var dto = new PaymentDetailDTO
        {
            Id = "pay_2",
            TransactionId = "txn_789",
            Tax = 2m,
            Currency = "EUR",
            Total = 20m,
            Email = "buyer@example.com",
            Status = "Captured",
            CartId = 5,
            GrandTotal = 22m,
            CreatedDate = new DateTime(2024, 2, 2, 0, 0, 0, DateTimeKind.Utc),
            UserId = 11
        };

        var result = sut.SavePaymentDetails(dto);

        result.Should().BeTrue();
        repo.Verify(r => r.SavePayementDetails(It.Is<PaymentDetail>(p =>
            p.Id == dto.Id &&
            p.TransactionId == dto.TransactionId &&
            p.Tax == dto.Tax &&
            p.Currency == dto.Currency &&
            p.Total == dto.Total &&
            p.Email == dto.Email &&
            p.Status == dto.Status &&
            p.CartId == dto.CartId &&
            p.GrandTotal == dto.GrandTotal &&
            p.CreatedDate == dto.CreatedDate &&
            p.UserId == dto.UserId
        )), Times.Once);
    }
}
