using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.DTOs;
using OrderService.Application.Repositories;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Persistence.Repositories
{
    public sealed class OrderReturnRepository : IOrderReturnRepository
    {
        private readonly OrderServiceDbContext _dbContext;

        public OrderReturnRepository(OrderServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CreateReturnRequestResultDto?> CreateReturnRequestAsync(
            Guid orderId,
            Guid customerId,
            string returnType,
            string returnReason,
            string returnItemsJson,
            string customerComments,
            CancellationToken ct = default)
        {
            var returnIdParam = new OutputParameter<Guid?>();
            var results = await _dbContext.Procedures.SP_CreateReturnRequestAsync(
                orderId,
                customerId,
                returnType,
                returnReason,
                returnItemsJson,
                customerComments,
                returnIdParam,
                cancellationToken: ct);

            var result = results?.FirstOrDefault();
            if (result is null)
            {
                return null;
            }

            return new CreateReturnRequestResultDto
            {
                ReturnId = result.ReturnId ?? returnIdParam.Value,
                ReturnNumber = result.ReturnNumber ?? string.Empty,
                Status = result.Status ?? string.Empty,
                Message = result.Message ?? string.Empty
            };
        }

        public async Task<IReadOnlyList<OrderReturn>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderReturn>()
                .Include(r => r.OrderReturnItems)
                .Where(r => r.OrderId == orderId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<VwReturnManagement>> GetReturnManagementByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.VwReturnManagements
                .Where(r => r.OrderId == orderId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync(ct);
        }
    }
}
