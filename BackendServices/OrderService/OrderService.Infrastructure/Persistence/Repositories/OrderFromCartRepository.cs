using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.DTOs;
using OrderService.Application.Repositories;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Persistence.Repositories
{
    public sealed class OrderFromCartRepository : IOrderFromCartRepository
    {
        private readonly OrderServiceDbContext _dbContext;

        public OrderFromCartRepository(OrderServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CreateOrderFromCartResultDto?> CreateFromCartAsync(
            Guid cartId,
            Guid customerId,
            string customerEmail,
            string orderSource,
            string? billingAddressJson,
            string? shippingAddressJson,
            string? paymentMethod,
            string? ipAddress,
            CancellationToken ct = default)
        {
            var outputOrderId = new OutputParameter<Guid?>();

            var results = await _dbContext.Procedures.SP_CreateOrderFromCartAsync(
                cartId,
                customerId,
                customerEmail,
                orderSource,
                billingAddressJson,
                shippingAddressJson,
                paymentMethod,
                ipAddress,
                outputOrderId,
                cancellationToken: ct);

            var result = results?.FirstOrDefault();
            if (result is null)
            {
                return null;
            }

            return new CreateOrderFromCartResultDto
            {
                OrderId = result.OrderId ?? outputOrderId.Value ?? Guid.Empty,
                OrderNumber = result.OrderNumber ?? string.Empty,
                Status = result.Status ?? string.Empty,
                Message = result.Message ?? string.Empty
            };
        }
    }
}
