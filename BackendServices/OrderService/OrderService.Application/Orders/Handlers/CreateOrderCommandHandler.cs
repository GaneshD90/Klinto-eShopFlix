using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OrderService.Application.CQRS;
using OrderService.Application.DTOs;
using OrderService.Application.IntegrationEvents;
using OrderService.Application.Messaging;
using OrderService.Application.Orders.Commands;
using OrderService.Application.Repositories;
using OrderService.Domain.Entities;

namespace OrderService.Application.Orders.Handlers
{
    public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, OrderDetailDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IIntegrationEventPublisher _eventPublisher;

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            IMapper mapper,
            IIntegrationEventPublisher eventPublisher)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _eventPublisher = eventPublisher;
        }

        public async Task<OrderDetailDto> Handle(CreateOrderCommand command, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var orderNumber = GenerateOrderNumber(now);

            var order = new Order
            {
                OrderId = Guid.NewGuid(),
                OrderNumber = orderNumber,
                CustomerId = command.CustomerId,
                OrderStatus = "Pending",
                OrderType = command.OrderType,
                OrderSource = command.OrderSource,
                OrderDate = now,
                CurrencyCode = command.CurrencyCode,
                ExchangeRate = 1.0m,
                PaymentStatus = "Pending",
                FulfillmentStatus = "Unfulfilled",
                CustomerEmail = command.CustomerEmail,
                CustomerPhone = command.CustomerPhone,
                Ipaddress = command.IpAddress,
                UserAgent = command.UserAgent,
                IsGuestCheckout = command.IsGuestCheckout,
                Priority = "Normal",
                CustomerNotes = command.CustomerNotes,
                CreatedAt = now,
                UpdatedAt = now
            };

            decimal subtotal = 0;

            foreach (var itemInput in command.Items)
            {
                var totalPrice = itemInput.UnitPrice * itemInput.Quantity;
                subtotal += totalPrice;

                var orderItem = new OrderItem
                {
                    OrderItemId = Guid.NewGuid(),
                    OrderId = order.OrderId,
                    ProductId = itemInput.ProductId,
                    VariationId = itemInput.VariationId,
                    ProductName = itemInput.ProductName,
                    Sku = itemInput.Sku,
                    Quantity = itemInput.Quantity,
                    UnitPrice = itemInput.UnitPrice,
                    OriginalPrice = itemInput.OriginalPrice,
                    DiscountAmount = 0,
                    TaxAmount = 0,
                    TotalPrice = totalPrice,
                    ItemStatus = "Pending",
                    IsGift = itemInput.IsGift,
                    GiftMessage = itemInput.GiftMessage,
                    CustomizationDetails = itemInput.CustomizationDetails,
                    ProductSnapshot = itemInput.ProductSnapshot,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                order.OrderItems.Add(orderItem);
            }

            order.SubtotalAmount = subtotal;
            order.TotalAmount = subtotal;

            if (command.BillingAddress is not null)
            {
                order.OrderAddresses.Add(MapAddress(command.BillingAddress, order.OrderId, "Billing", now));
            }

            if (command.ShippingAddress is not null)
            {
                order.OrderAddresses.Add(MapAddress(command.ShippingAddress, order.OrderId, "Shipping", now));
            }

            await _orderRepository.AddAsync(order, ct);

            await _eventPublisher.EnqueueAsync(
                nameof(OrderCreatedIntegrationEvent),
                new OrderCreatedIntegrationEvent(
                    order.OrderId,
                    order.OrderNumber,
                    order.CustomerId,
                    order.CustomerEmail,
                    order.OrderType,
                    order.OrderSource,
                    order.TotalAmount,
                    order.CurrencyCode,
                    order.OrderItems.Count),
                ct);

            return _mapper.Map<OrderDetailDto>(order);
        }

        private static OrderAddress MapAddress(CreateOrderAddressInput input, Guid orderId, string addressType, DateTime now)
        {
            return new OrderAddress
            {
                OrderAddressId = Guid.NewGuid(),
                OrderId = orderId,
                AddressType = addressType,
                FirstName = input.FirstName,
                LastName = input.LastName,
                CompanyName = input.CompanyName,
                AddressLine1 = input.AddressLine1,
                AddressLine2 = input.AddressLine2,
                City = input.City,
                StateProvince = input.StateProvince,
                PostalCode = input.PostalCode,
                CountryCode = input.CountryCode,
                Phone = input.Phone,
                Email = input.Email,
                IsValidated = false,
                CreatedAt = now
            };
        }

        private static string GenerateOrderNumber(DateTime now)
        {
            return $"ORD-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        }
    }
}
