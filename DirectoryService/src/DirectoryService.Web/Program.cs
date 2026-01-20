using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Contracts.Locations;
using DirectoryService.Infrastructure;
using DirectoryService.web;
using DirectoryService.web.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;
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

builder.Services.AddProgramDependencies(builder.Configuration);

var app = builder.Build();

// Этот middleware обрабатывает все исключения, и его вызываем в самом начале
app.UseExceptionMiddleware();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
}

/*app.MapPost(
    "api/locations",
    async Task<EndpointResult<Guid>> (
            [FromBody]CreateLocationRequest request,
            [FromServices]CreateLocationHandler handler,
            CancellationToken cancellationToken)
        => await handler.Handle(new CreateLocationCommand(request), cancellationToken));*/

app.MapControllers();

app.Run();

// Для получения доступа к классу Program из другого проекта
namespace DirectoryService.Web
{
    public partial class Program;
}