
//using Serilog;
//using SmartInventorySystem.API.Extensions;
//using SmartInventorySystem.API.Middleware;
//using SmartInventorySystem.Infrastructure.Data;

//var builder = WebApplication.CreateBuilder(args);

//// Configure Serilog
//Log.Logger = new LoggerConfiguration()
//    .ReadFrom.Configuration(builder.Configuration)
//    .Enrich.FromLogContext()
//    .WriteTo.Console()
//    .WriteTo.File("logs/smartinventory-.txt", rollingInterval: RollingInterval.Day)
//    .CreateLogger();

//builder.Host.UseSerilog();

//// Add services
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();


using Serilog;
using SmartInventorySystem.API.Extensions;
using SmartInventorySystem.API.Middleware;
using SmartInventorySystem.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ✅ Serilog (clean)
//Log.Logger = new LoggerConfiguration()
//    .ReadFrom.Configuration(builder.Configuration)
//    .CreateLogger();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console() // ✅ ADD THIS
    .CreateLogger();
builder.Host.UseSerilog();

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDatabaseServices(builder.Configuration);
builder.Services.AddCacheServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCorsPolicy();
builder.Services.AddApplicationServices();

var app = builder.Build();

// Seed DB
using (var scope = app.Services.CreateScope())
{
    await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(); // ✅ IMPORTANT
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();