using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Infrastructure;
using DirectoryService.web;
using DirectoryService.web.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Shared;
using Shared.EndpointResults;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<CreateLocationHandler>();

var app = builder.Build();

// Этот middleware обрабатывает все исключения, и его вызываем в самом начале
app.UseExceptionMiddleware();

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