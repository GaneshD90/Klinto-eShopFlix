using System;
using System.Threading;
using System.Threading.Tasks;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;
using OrderService.Application.Exceptions;
using OrderService.Application.IntegrationEvents;
using OrderService.Application.Messaging;
using OrderService.Application.Orders.Commands;
using OrderService.Application.Repositories;

namespace OrderService.Application.Orders.Handlers
{
    public sealed class CreateOrderFromCartCommandHandler : ICommandHandler<CreateOrderFromCartCommand, CreateOrderFromCartResultDto>
    {
        private readonly IOrderFromCartRepository _cartOrderRepository;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public CreateOrderFromCartCommandHandler(
            IOrderFromCartRepository cartOrderRepository,
            IIntegrationEventPublisher eventPublisher)
        {
            _cartOrderRepository = cartOrderRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task<CreateOrderFromCartResultDto> Handle(CreateOrderFromCartCommand command, CancellationToken ct)
        {
            var result = await _cartOrderRepository.CreateFromCartAsync(
                command.CartId,
                command.CustomerId,
                command.CustomerEmail,
                command.OrderSource,
                command.BillingAddressJson,
                command.ShippingAddressJson,
                command.PaymentMethod,
                command.IpAddress,
                ct);

            if (result is null)
            {
                throw AppException.Business("order.cart.creation.failed", "Failed to create order from cart.");
            }

            if (!string.Equals(result.Status, "Success", StringComparison.OrdinalIgnoreCase))
            {
                throw AppException.Business("order.cart.creation.failed", result.Message ?? "Failed to create order from cart.");
            }

            if (result.OrderId != Guid.Empty)
            {
                await _eventPublisher.EnqueueAsync(
                    nameof(OrderFromCartCreatedIntegrationEvent),
                    new OrderFromCartCreatedIntegrationEvent(
                        result.OrderId,
                        result.OrderNumber,
                        command.CartId,
                        command.CustomerId,
                        command.CustomerEmail),
                    ct);
            }

            return result;
        }
    }
}
