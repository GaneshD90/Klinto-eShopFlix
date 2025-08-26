using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Application.Mappers;
using PaymentService.Application.Repositories;
using PaymentService.Application.Services.Abstractions;
using PaymentService.Application.Services.Implementations;
using PaymentService.Infrastructure.Persistence;
using PaymentService.Infrastructure.Providers.Abstractions;
using PaymentService.Infrastructure.Providers.Implementations;
using PaymentService.Infrastructure.Repositories;

namespace PaymentService.Infrastructure
{
    public class ServiceRegistration
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register your services here
            services.AddDbContext<PaymentServiceDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            //Repositories
            services.AddScoped<IPaymentRepository, PaymentRepository>();

            //Application Services
            services.AddScoped<IPaymentAppService, PaymentAppService>();

            //Providers
            services.AddScoped<IPaymentProvider, PaymentProvider>();

            //AutoMapper
            services.AddAutoMapper(confg => confg.AddProfile<PaymentMapper>());
        }
    }
}
