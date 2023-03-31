using Microsoft.Extensions.Options;
using Serilog;
using Serilog.AspNetCore;
using serilog_library.middlewares;
using serilog_library.services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    // .WriteTo.File(new DefaultJsonFormatter(), 'log.json')
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

app.UseMiddleware<RequestMiddleware>(opts);

app.Run();
