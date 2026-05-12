using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Seeding;
using DirectoryService.Web;
using DirectoryService.Web.Middlewares;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    Path.Combine("Properties", $"appsettings.{builder.Environment.EnvironmentName}.json"),
    optional: true,
    reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

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

if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
    await app.Services.MigrateAsync();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Этот middleware обрабатывает все исключения, и его вызываем в самом начале
app.UseExceptionMiddleware();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("OpenApi:Enabled"))
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
}

if (app.Environment.IsDevelopment())
{
    if (args.Contains("--seeding"))
    {
        await app.Services.RunSeeding();
    }
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