using CatalogService.Application.Mappers;
using CatalogService.Application.Repositories;
using CatalogService.Application.Services.Abstractions;
using CatalogService.Application.Services.Implementation;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Infrastructure
{
    public class ServiceRegistration
    {
        public static void RegisterServices(IServiceCollection services,IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<CatalogServiceDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DbConnection")));

            // Register application services
            services.AddScoped<IProductAppService, ProductAppService>();

            // Register repositories
            services.AddScoped<IProductRepository, ProductRepository>();

            // Register AutoMapper
            services.AddAutoMapper(cfg => cfg.AddProfile<ProductMapper>());

        }
    }
}
