using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlServerToSqlServer;
using Microsoft.Extensions.Options;

var hostbuilder = Host.CreateDefaultBuilder(args);

hostbuilder.ConfigureAppConfiguration((hostContext, configurationBuilder) =>
{
    configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
    var appsettings = "appsettings.json";
    configurationBuilder.AddJsonFile(path: appsettings, optional: false, reloadOnChange: false);
    var configuration = configurationBuilder.Build();
});

hostbuilder.ConfigureLogging((hostContext, loggingBuilder) =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.SetMinimumLevel(LogLevel.Trace);
});

hostbuilder.ConfigureServices((hostContext, services) =>
{
    services.AddOptions();
    services.Configure<AppOption>(hostContext.Configuration.GetSection(AppOption.Section));
    services.Configure<List<ThreadOption>>(hostContext.Configuration.GetSection(ThreadOption.Section));
    services.AddTransient<Runner>();
});

using var host = hostbuilder.Build();

var appOption = host.Services.GetRequiredService<IOptions<AppOption>>().Value;
if (Directory.Exists(appOption.Output)) Directory.Delete(appOption.Output, true);
Directory.CreateDirectory(appOption.Output);
var threadOptions = host.Services.GetRequiredService<IOptions<List<ThreadOption>>>().Value;

var result = 0L;
var infoLog = Path.Combine($"{appOption.Output}", "app.Log.log");
await Utility.Log(infoLog, "Log", "[Start]");
var tasks = new List<Task<long>>();
foreach (var threadOption in threadOptions)
{
    var runner = host.Services.GetRequiredService<Runner>();
    tasks.Add(runner.Run(threadOption));
}
Task.WaitAll(tasks.ToArray());
foreach (var task in tasks)
{
    result += await task;
}
await Utility.Log(infoLog, "Log", $"[Complete][{result}]");
await Utility.Log(infoLog, "Log", "[Stop]");