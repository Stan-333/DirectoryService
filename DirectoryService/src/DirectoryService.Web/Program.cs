using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Infrastructure;
using DirectoryService.web;
using DirectoryService.web.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProgramDependencies();

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

app.MapControllers();

app.Run();