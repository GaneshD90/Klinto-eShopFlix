using System;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.DTOs;

namespace OrderService.Application.Repositories
{
    public interface IOrderConfirmRepository
    {
        Task<ConfirmOrderResultDto?> ConfirmAsync(Guid orderId, CancellationToken ct = default);
    }
}
