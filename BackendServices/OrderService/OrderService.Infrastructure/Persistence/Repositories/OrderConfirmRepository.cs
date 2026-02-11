using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.DTOs;
using OrderService.Application.Repositories;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Persistence.Repositories
{
    public sealed class OrderConfirmRepository : IOrderConfirmRepository
    {
        private readonly OrderServiceDbContext _dbContext;

        public OrderConfirmRepository(OrderServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ConfirmOrderResultDto?> ConfirmAsync(Guid orderId, CancellationToken ct = default)
        {
            var results = await _dbContext.Procedures.SP_ConfirmOrderAsync(orderId, cancellationToken: ct);
            var result = results?.FirstOrDefault();
            if (result is null)
            {
                return null;
            }

            return new ConfirmOrderResultDto
            {
                Status = result.Status ?? string.Empty,
                Message = result.Message ?? string.Empty
            };
        }
    }
}
