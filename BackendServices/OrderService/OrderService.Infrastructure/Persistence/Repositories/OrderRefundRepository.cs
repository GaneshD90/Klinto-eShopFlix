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
    public sealed class OrderRefundRepository : IOrderRefundRepository
    {
        private readonly OrderServiceDbContext _dbContext;

        public OrderRefundRepository(OrderServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProcessRefundResultDto?> ProcessRefundAsync(
            Guid orderId,
            Guid? returnId,
            decimal refundAmount,
            string refundType,
            string refundMethod,
            string refundReason,
            CancellationToken ct = default)
        {
            var refundIdParam = new OutputParameter<Guid?>();
            var results = await _dbContext.Procedures.SP_ProcessRefundAsync(
                orderId,
                returnId,
                refundAmount,
                refundType,
                refundMethod,
                refundReason,
                refundIdParam,
                cancellationToken: ct);

            var result = results?.FirstOrDefault();
            if (result is null)
            {
                return null;
            }

            return new ProcessRefundResultDto
            {
                RefundId = result.RefundId ?? refundIdParam.Value,
                RefundNumber = result.RefundNumber ?? string.Empty,
                Status = result.Status ?? string.Empty,
                Message = result.Message ?? string.Empty
            };
        }

        public async Task<IReadOnlyList<OrderRefund>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderRefund>()
                .Where(r => r.OrderId == orderId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);
        }
    }
}
