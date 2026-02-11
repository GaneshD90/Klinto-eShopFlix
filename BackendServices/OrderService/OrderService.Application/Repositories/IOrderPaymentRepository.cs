using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Repositories
{
    public interface IOrderPaymentRepository
    {
        Task<ProcessPaymentResultDto?> ProcessPaymentAsync(
            Guid orderId,
            string paymentMethod,
            string paymentProvider,
            decimal amount,
            string transactionId,
            string authorizationCode,
            string paymentGatewayResponse,
            CancellationToken ct = default);

        Task<IReadOnlyList<OrderPayment>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    }
}
