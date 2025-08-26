using CartService.Application.HttpClients;
using CartService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ServiceRegistration.RegisterServices(builder.Services, builder.Configuration);
builder.Services.AddControllers();

builder.Services.AddHttpClient<CatalogServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiAddress:CatalogApi"]);
});


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
