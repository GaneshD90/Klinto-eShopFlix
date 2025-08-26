using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("ocelot-dev.json");
}
else
{
    builder.Configuration.AddJsonFile("ocelot.json");
}
builder.Services.AddOcelot();

var endPointAuthKey = builder.Configuration["Keys:EndpointAuthKey"];
builder.Services.AddAuthentication().AddJwtBearer(endPointAuthKey, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.UseOcelot().Wait();
app.Run();
