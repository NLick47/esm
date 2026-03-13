using EventStreamManager.EventProcessor;
using EventStreamManager.EventProcessor.Processors;
using EventStreamManager.Infrastructure.Services;
using EventStreamManager.Infrastructure.Services.Data;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;

using EventStreamManager.JSFunction;
using EventStreamManager.JSFunction.Loader;
using EventStreamManager.WebApi.Common.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

//控制器
builder.Services.AddControllers();

//JS Function 插件系统
builder.Services.AddSingleton<JsFunctionLoader>(_ => new JsFunctionLoader());
builder.Services.AddSingleton<IEnumerable<IJSFunctionProvider>>(sp =>
    sp.GetRequiredService<JsFunctionLoader>().LoadAllProviders());
builder.Services.AddSingleton<JSFunctionRegistry>();


//核心服务
builder.Services.AddSingleton<IJavaScriptExecutionService, JavaScriptExecutionService>();
builder.Services.AddScoped<ISqlSugarContext, SqlSugarContext>();

//数据服务
builder.Services.AddSingleton<IDatabaseConnectionService, DatabaseConnectionService>();
builder.Services.AddSingleton<IDatabaseSchemeService, DatabaseSchemeService>();
builder.Services.AddSingleton<IEventListenerConfigService, EventListenerConfigService>();
builder.Services.AddSingleton<IInterfaceConfigService, InterfaceConfigService>();
builder.Services.AddSingleton<IDataService, JsonDataService>();
builder.Services.AddSingleton<IProcessorService, ProcessorService>();
builder.Services.AddSingleton<ISqlTemplateService, SqlTemplateService>();
builder.Services.AddScoped<ITableInitializationService, TableInitializationService>();
//调试服务
builder.Services.AddScoped<IDebugService, DebugService>();

//请求服务
builder.Services.AddHttpClient();
builder.Services.AddScoped<IHttpSendService, HttpSendService>();
//事件处理器
builder.Services.AddSingleton<ProcessorFactory>();
builder.Services.AddSingleton<EventProcessorService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<EventProcessorService>());


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
    }) .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

//跨域
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.SetIsOriginAllowed(_ => true).AllowAnyMethod().AllowAnyHeader());
});

//Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();


app.UseDefaultFiles();
app.UseStaticFiles(); 
app.MapControllers();

app.Run();
