using eShopFlix.Web.HttpClients;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("HttpClient" ,client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApigatewayAddress"]);
});
builder.Services.AddScoped<CatalogServiceClient>(client =>
{
    var httpClientFactory = builder.Services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

    var httpClient = httpClientFactory.CreateClient("HttpClient");
    return new CatalogServiceClient(httpClient);
});
builder.Services.AddScoped<AuthServiceClient>(client =>
{
    var httpClientFactory = builder.Services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

    var httpClient = httpClientFactory.CreateClient("HttpClient");
    return new AuthServiceClient(httpClient);
});

builder.Services.AddScoped<CartServiceClient>(client =>
{
    var httpClientFactory = builder.Services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

    var httpClient = httpClientFactory.CreateClient("HttpClient");
    return new CartServiceClient(httpClient);
});
builder.Services.AddScoped<PaymentServiceClient>(client =>
{
    var httpClientFactory = builder.Services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

    var httpClient = httpClientFactory.CreateClient("HttpClient");
    return new PaymentServiceClient(httpClient);
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "eShopFlixCookie";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
  );

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();