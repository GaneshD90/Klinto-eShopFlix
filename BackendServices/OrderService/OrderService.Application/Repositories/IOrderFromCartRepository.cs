using System;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.DTOs;

namespace OrderService.Application.Repositories
{
    public interface IOrderFromCartRepository
    {
        Task<CreateOrderFromCartResultDto?> CreateFromCartAsync(
            Guid cartId,
            Guid customerId,
            string customerEmail,
            string orderSource,
            string? billingAddressJson,
            string? shippingAddressJson,
            string? paymentMethod,
            string? ipAddress,
            CancellationToken ct = default);
    }
}
