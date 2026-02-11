using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Repositories
{
    public interface IOrderRefundRepository
    {
        Task<ProcessRefundResultDto?> ProcessRefundAsync(
            Guid orderId,
            Guid? returnId,
            decimal refundAmount,
            string refundType,
            string refundMethod,
            string refundReason,
            CancellationToken ct = default);

        Task<IReadOnlyList<OrderRefund>> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    }
}
