using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Infrastructure;
using DirectoryService.web;
using DirectoryService.web.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Shared;
using Shared.EndpointResults;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Debug()
    .WriteTo.Seq(builder.Configuration.GetConnectionString("Seq") ?? throw new ArgumentNullException("Seq"))
    .Enrich.WithThreadId()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentUserName()
    .Enrich.FromLogContext()
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddProgramDependencies();

// На случай ошибки необходимо добавить код, позволяющий обрабатывать кастомный класс Error
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer((schema, context, _) =>
    {
        if (context.JsonTypeInfo.Type != typeof(Envelope<Errors>))
        {
            return Task.CompletedTask;
        }

        if (schema.Properties.TryGetValue("errors", out var errorsProp))
        {
            errorsProp.Items.Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "Error" };
        }

        return Task.CompletedTask;
    });
});

builder.Services.AddScoped<DirectoryServiceDbContext>(_ =>
    new DirectoryServiceDbContext(builder.Configuration));

var app = builder.Build();

// Этот middleware обрабатывает все исключения, и его вызываем в самом начале
app.UseExceptionMiddleware();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
}

app.MapPost(
    "api/location",
    async Task<EndpointResult<Guid>> ([FromBody]CreateLocationRequest request, [FromServices]CreateLocationHandler handler, CancellationToken cancellationToken)
        => await handler.Handle(request, cancellationToken));

app.MapControllers();

app.Run();