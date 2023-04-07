using Microsoft.Extensions.Options;
using Refit;
using Serilog;
using Serilog.AspNetCore;
using serilog_library.Handlers;
using serilog_library.middlewares;
using serilog_library.services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<RefitHttpHandler>();
// builder.Services.AddLogging();

builder.Services
    .AddRefitClient<TestApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5094"))
    .AddHttpMessageHandler<RefitHttpHandler>();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.File(new DefaultJsonFormatter(), "log.json")
    .WriteTo.Console(new DefaultJsonFormatter()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var opts = app.Services.GetService<IOptions<RequestLoggingOptions>>()?.Value ?? new RequestLoggingOptions();

app.UseMiddleware<CustomMiddleware>();

app.UseMiddleware<RequestMiddleware>(opts);


app.Run();
