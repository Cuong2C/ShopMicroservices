
using Discount.Grpc;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCarter();

var assembly = typeof(Program).Assembly;
var connectionString = builder.Configuration.GetConnectionString("Database")!;
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")!;

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

builder.Services.AddMarten(options =>
{
    options.Connection(connectionString!);
    options.Schema.For<ShoppingCart>().Identity(u => u.UserName);
}).UseLightweightSessions();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.Decorate<IBasketRepository, CacheBasketRepository>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
});

// Grpc services
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler() {
        // allow any certificates that are untrusted/invalid, only for dev/test environments
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
    return handler;
});

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddRedis(redisConnectionString);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapCarter();

app.UseExceptionHandler(options =>
{
    options.Run(async context =>
    {
        var exceptionHandler = context.RequestServices.GetRequiredService<IExceptionHandler>();
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception != null)
        {
            await exceptionHandler.TryHandleAsync(context, exception, context.RequestAborted);
        }
    });
});

app.UseHealthChecks("/health", 
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Run();
