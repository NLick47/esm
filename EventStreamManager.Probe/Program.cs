using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/probe-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}{NewLine}")
    .CreateLogger();

Log.Information("EventStreamManager Probe started. Press Ctrl+C to exit.");

try
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("probeSettings.json", optional: false, reloadOnChange: true)
        .Build();

    var probeConfig = configuration.GetSection("Probe").Get<ProbeConfig>()
        ?? throw new InvalidOperationException("Failed to load Probe configuration.");

    // 解析相对路径
    var workingDir = Path.IsPathRooted(probeConfig.WorkingDirectory)
        ? probeConfig.WorkingDirectory
        : Path.Combine(AppContext.BaseDirectory, probeConfig.WorkingDirectory);

    var httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    int consecutiveFailures = 0;

    while (true)
    {
        bool isHealthy;
        try
        {
            var response = await httpClient.GetAsync(probeConfig.TargetUrl);
            isHealthy = response.IsSuccessStatusCode;

            if (isHealthy)
            {
                if (consecutiveFailures > 0)
                {
                    Log.Information("Service recovered. URL: {Url} (Status: {StatusCode})",
                        probeConfig.TargetUrl, (int)response.StatusCode);
                }
                else
                {
                    Log.Debug("Service is healthy. URL: {Url} (Status: {StatusCode})",
                        probeConfig.TargetUrl, (int)response.StatusCode);
                }
                consecutiveFailures = 0;
            }
            else
            {
                consecutiveFailures++;
                Log.Warning("Service returned unhealthy status. URL: {Url} (Status: {StatusCode}, Failures: {Failures}/{Threshold})",
                    probeConfig.TargetUrl, (int)response.StatusCode, consecutiveFailures, probeConfig.FailureThreshold);
            }
        }
        catch (Exception ex)
        {
            consecutiveFailures++;
            Log.Warning("Service check failed. URL: {Url} (Error: {Error}, Failures: {Failures}/{Threshold})",
                probeConfig.TargetUrl, ex.Message, consecutiveFailures, probeConfig.FailureThreshold);
        }

        if (consecutiveFailures >= probeConfig.FailureThreshold)
        {
            Log.Error("Failure threshold reached ({Threshold}). Attempting to restart service...", probeConfig.FailureThreshold);
            await RestartServiceAsync(probeConfig, workingDir);
            consecutiveFailures = 0;
        }

        await Task.Delay(TimeSpan.FromSeconds(probeConfig.CheckIntervalSeconds));
    }
}
catch (OperationCanceledException)
{
    Log.Information("Probe shutting down...");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Probe terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

static async Task RestartServiceAsync(ProbeConfig config, string workingDir)
{
    try
    {
        // 先尝试终止已有的主服务进程
        var processName = Path.GetFileNameWithoutExtension(config.RestartCommand);
        var existingProcesses = Process.GetProcessesByName(processName);
        foreach (var proc in existingProcesses)
        {
            try
            {
                Log.Information("Killing existing process {Name} (PID: {Pid})...", processName, proc.Id);
                proc.Kill();
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await proc.WaitForExitAsync(cts.Token);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to kill process PID {Pid}", proc.Id);
            }
            finally
            {
                proc.Dispose();
            }
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = config.RestartCommand,
            Arguments = config.RestartArguments,
            WorkingDirectory = workingDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = false,
            RedirectStandardError = false
        };

        Log.Information("Starting service: {Command} {Arguments} (WorkingDir: {WorkingDir})",
            config.RestartCommand, config.RestartArguments, workingDir);

        using var process = Process.Start(startInfo);
        if (process != null)
        {
            Log.Information("Service started. PID: {Pid}", process.Id);
            // 等待几秒让服务启动
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
        else
        {
            Log.Error("Failed to start service process.");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Exception occurred while restarting service.");
    }
}

public class ProbeConfig
{
    public string TargetUrl { get; set; } = "http://localhost:7138";
    public int CheckIntervalSeconds { get; set; } = 30;
    public int FailureThreshold { get; set; } = 3;
    public string RestartCommand { get; set; } = "dotnet";
    public string RestartArguments { get; set; } = "EventStreamManager.WebApi.dll";
    public string WorkingDirectory { get; set; } = ".";
    public string LogPath { get; set; } = "logs/probe-.txt";
}
