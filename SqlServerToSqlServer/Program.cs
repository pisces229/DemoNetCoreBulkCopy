using CsvHelper.Configuration;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlServerToSqlServer;
using System.Globalization;
using Microsoft.Extensions.Options;

var hostbuilder = Host.CreateDefaultBuilder(args);

hostbuilder.ConfigureAppConfiguration((hostContext, configurationBuilder) =>
{
    configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
    var appsettings = "appsettings.json";
    //if (!EnvironmentVariable.IsDevelopment())
    //{
    //    appsettings = $"appsettings.{EnvironmentVariable.ASPNETCORE_ENVIRONMENT}.json";
    //}
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
    services.Configure<AppOption>(hostContext.Configuration);
});

using var host = hostbuilder.Build();

var appOption = host.Services.GetRequiredService<IOptions<AppOption>>().Value;
Console.WriteLine("Start...");
var ouput = Directory.CreateDirectory($"{appOption.Output}.{DateTime.Now:yyyyMMdd.HHmmss}");
var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    Delimiter = ",",
    HasHeaderRecord = false,
};
var tasks = new List<Task>();
foreach (var file in appOption.Files)
{
    var mapperConfig = new MapperConfig()
    {
        ReadSize = appOption.ReadSize,
        BatchSize = appOption.BatchSize,
        SourceConnectionString = appOption.SourceConnectionString,
        TargetConnectionString = appOption.TargetConnectionString,
        Output = Path.Combine($"{ouput.FullName}", $"{file}"),
    };
    using (var streamReader = new StreamReader(Path.Combine(appOption.Input, $"{file}.csv")))
    using (var csvReader = new CsvReader(streamReader, csvConfiguration))
    {
        var records = csvReader.GetRecords<MapperName>().ToList();
        mapperConfig.SourceTableName = records.First().SourceName;
        mapperConfig.TargetTableName = records.First().TargetName;
        mapperConfig.MapperName = records.Skip(1).ToList();
    }
    var runner = new Runner(mapperConfig);
    tasks.Add(runner.Run());
}
Task.WaitAll(tasks.ToArray());
Console.WriteLine("...Start");