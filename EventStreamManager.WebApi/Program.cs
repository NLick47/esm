using EventStreamManager.EventProcessor;
using EventStreamManager.EventProcessor.Processors;
using EventStreamManager.Infrastructure.Services;
using EventStreamManager.Infrastructure.Services.Data;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using EventStreamManager.Infrastructure.Services.Mock;
using EventStreamManager.JSFunction;
using EventStreamManager.JSFunction.Loader;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

//控制器
builder.Services.AddControllers();

//JS Function 插件系统
builder.Services.AddSingleton<JSFunctionLoader>(sp => new JSFunctionLoader("Plugins"));
builder.Services.AddSingleton<IEnumerable<IJSFunctionProvider>>(sp =>
    sp.GetRequiredService<JSFunctionLoader>().LoadAllProviders());
builder.Services.AddSingleton<JSFunctionRegistry>();
builder.Services.AddSingleton<MockDataGenerator>();

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

//事件处理器
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ProcessorFactory>();
builder.Services.AddSingleton<EventProcessorService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<EventProcessorService>());


builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray());
            return new BadRequestObjectResult(new { message = "请求数据验证失败", errors });
        };
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

//跨域
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.SetIsOriginAllowed(origin => true).AllowAnyMethod().AllowAnyHeader());
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
app.MapControllers();

app.Run();
