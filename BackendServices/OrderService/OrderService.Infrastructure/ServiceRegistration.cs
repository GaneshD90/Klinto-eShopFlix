using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.CQRS;
using OrderService.Application.Mappers;
using OrderService.Application.Messaging;
using OrderService.Application.Orders.Handlers;
using OrderService.Application.Repositories;
using OrderService.Application.Sagas.Services;
using OrderService.Application.Services.Abstractions;
using OrderService.Application.Services.Implementations;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Persistence.Repositories;
using OrderService.Infrastructure.Persistence.Services;
using OrderService.Infrastructure.Sagas;
using Microsoft.Extensions.Hosting;
using Scrutor;

namespace OrderService.Infrastructure
{
    public static class ServiceRegistration
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            // DbContext
            services.AddDbContext<OrderServiceDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DbConnection"));
            });

            // MassTransit + Azure Service Bus
            services.AddOrderServiceMessaging(configuration);

            // Outbox Dispatcher (publishes outbox messages to MassTransit)
            services.AddHostedService<OutboxDispatcherHostedService>();

            // CQRS
            services.AddScoped<IDispatcher, Dispatcher>();

            var applicationAssembly = typeof(CreateOrderCommandHandler).Assembly;

            services.Scan(scan => scan
                .FromAssemblies(applicationAssembly)
                .AddClasses(classes => classes.AssignableToAny(typeof(ICommandHandler<,>), typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Repositories
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderFromCartRepository, OrderFromCartRepository>();
            services.AddScoped<IOrderConfirmRepository, OrderConfirmRepository>();
            services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();
            services.AddScoped<IOrderTimelineRepository, OrderTimelineRepository>();
            services.AddScoped<IOrderNoteRepository, OrderNoteRepository>();
            services.AddScoped<IOrderPaymentRepository, OrderPaymentRepository>();
            services.AddScoped<IOrderShipmentRepository, OrderShipmentRepository>();
            services.AddScoped<IOrderReturnRepository, OrderReturnRepository>();
            services.AddScoped<IOrderRefundRepository, OrderRefundRepository>();
            services.AddScoped<IOrderFraudCheckRepository, OrderFraudCheckRepository>();
            services.AddScoped<IOrderHoldRepository, OrderHoldRepository>();
            services.AddScoped<IOrderSubscriptionRepository, OrderSubscriptionRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderAnalyticsRepository, OrderAnalyticsRepository>();
            services.AddScoped<IOrderAncillaryRepository, OrderAncillaryRepository>();

            // Cross-Cutting
            services.AddScoped<IIntegrationEventPublisher, OutboxIntegrationEventPublisher>();
            services.AddScoped<IIdempotentAppRequest, IdempotentRequestStore>();
            services.AddScoped<IIdempotencyAppService, IdempotencyAppService>();

            // Saga Monitoring Service
            services.AddScoped<ISagaMonitoringService, SagaMonitoringService>();

            // Application Services
            services.AddScoped<IOrderAppService, OrderAppService>();

            // AutoMapper
            services.AddAutoMapper(cfg => cfg.AddProfile<OrderMapper>(), applicationAssembly);
        }
    }
}
