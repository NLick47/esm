using EventStreamManager.EventProcessor;
using EventStreamManager.Infrastructure;
using EventStreamManager.JSFunction.Loader;
using EventStreamManager.WebApi.Middleware;
using EventStreamManager.WebApi.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Serilog;


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
        .Build())
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 60,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

// 控制器
builder.Services.AddControllers();

// JS Function 插件系统
builder.Services.AddJsFunctionLoader();

// 基础设施服务
builder.Services.AddInfrastructureServices();

// 事件处理器服务
builder.Services.AddEventProcessorServices();

// 配置 API 行为选项
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // 模型验证失败处理
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value?.Errors.Select(err => $"{e.Key}: {err.ErrorMessage}") ?? Array.Empty<string>())
                .ToList();

            var errorMessage = "请求参数验证失败";
            if (errors.Any())
            {
                errorMessage += ": " + string.Join(" | ", errors);
            }

            var response = ApiResponse.Fail(
                message: errorMessage,
                data: null
            );

            return new OkObjectResult(response);
        };
    }).AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 跨域
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader());
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();

// 配置静态文件中间件，用于访问前端发布后的index.html
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapFallbackToFile("index.html");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
