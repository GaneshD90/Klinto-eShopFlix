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
    public sealed class OrderPaymentRepository : IOrderPaymentRepository
    {
        private readonly OrderServiceDbContext _dbContext;

        public OrderPaymentRepository(OrderServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProcessPaymentResultDto?> ProcessPaymentAsync(
            Guid orderId,
            string paymentMethod,
            string paymentProvider,
            decimal amount,
            string transactionId,
            string authorizationCode,
            string paymentGatewayResponse,
            CancellationToken ct = default)
        {
            var results = await _dbContext.Procedures.SP_ProcessPaymentAsync(
                orderId,
                paymentMethod,
                paymentProvider,
                amount,
                transactionId,
                authorizationCode,
                paymentGatewayResponse,
                cancellationToken: ct);

            var result = results?.FirstOrDefault();
            if (result is null)
            {
                return null;
            }

            return new ProcessPaymentResultDto
            {
                PaymentId = result.PaymentId,
                Status = result.Status ?? string.Empty,
                Message = result.Message ?? string.Empty
            };
        }

        public async Task<IReadOnlyList<OrderPayment>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbContext.Set<OrderPayment>()
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(ct);
        }
    }
}
