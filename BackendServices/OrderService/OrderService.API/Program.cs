using OrderService.API.Extensions;
using OrderService.API.Http;
using OrderService.API.Logging;
using OrderService.API.Middleware;
using OrderService.API.Observability;
using OrderService.Application.Orders.Validators;
using OrderService.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

SerilogExtensions.ConfigureSerilog(builder.Configuration);
builder.Host.UseSerilog();
builder.Services.AddOrderServiceOpenTelemetry(builder.Configuration);

ServiceRegistration.RegisterServices(builder.Services, builder.Configuration);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(o => o.SuppressModelStateInvalidFilter = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OrderService API",
        Version = "v1",
        Description = "HTTP endpoints for OrderService"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderCommandValidator>();
builder.Services.AddProblemDetails();
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

builder.Services.AddHttpClient("order-payment", client =>
{
    var paymentApiAddress = builder.Configuration["ApiAddress:PaymentApi"];
    if (!string.IsNullOrWhiteSpace(paymentApiAddress))
    {
        client.BaseAddress = new Uri(paymentApiAddress);
    }
    client.Timeout = Timeout.InfiniteTimeSpan;
})
.AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
.AddPolicyHandler(PollyPolicies.CreatePolicy("payment"));

builder.Services.AddHttpClient("order-stock", client =>
{
    var stockApiAddress = builder.Configuration["ApiAddress:StockApi"];
    if (!string.IsNullOrWhiteSpace(stockApiAddress))
    {
        client.BaseAddress = new Uri(stockApiAddress);
    }
    client.Timeout = Timeout.InfiniteTimeSpan;
})
.AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
.AddPolicyHandler(PollyPolicies.CreatePolicy("stock"));

builder.Services.AddHttpClient("order-cart", client =>
{
    var cartApiAddress = builder.Configuration["ApiAddress:CartApi"];
    if (!string.IsNullOrWhiteSpace(cartApiAddress))
    {
        client.BaseAddress = new Uri(cartApiAddress);
    }
    client.Timeout = Timeout.InfiniteTimeSpan;
})
.AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
.AddPolicyHandler(PollyPolicies.CreatePolicy("cart"));

builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(opts =>
{
    opts.EnrichDiagnosticContext = (diag, http) =>
    {
        diag.Set("TraceId", Activity.Current?.TraceId.ToString());
        diag.Set("CorrelationId", http.TraceIdentifier);
        diag.Set("RequestPath", http.Request.Path);
        diag.Set("RequestMethod", http.Request.Method);
        diag.Set("StatusCode", http.Response.StatusCode);
        diag.Set("Application", "OrderService");
    };
});
app.UseMiddleware<ExceptionHandlingMiddleware>();

var enableSwagger = app.Configuration.GetValue<bool?>("Swagger:Enabled") ?? app.Environment.IsDevelopment();
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderService API v1");
        c.RoutePrefix = "swagger";
    });

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = WriteHealthResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = WriteHealthResponse
});

app.Run();

static Task WriteHealthResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";
    var payload = new
    {
        status = report.Status.ToString(),
        checks = report.Entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString(),
            error = entry.Value.Exception?.Message,
            duration = entry.Value.Duration.TotalMilliseconds
        }),
        totalDuration = report.TotalDuration.TotalMilliseconds,
        at = DateTime.UtcNow
    };

    return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
}
