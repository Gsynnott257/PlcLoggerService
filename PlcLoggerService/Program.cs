using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlcLoggerService.Services;

Host.CreateDefaultBuilder(args)
    .UseWindowsService() // host as Windows Service [1](https://flowfuse.com/blog/2025/10/plc-to-mqtt-using-flowfuse/)
    .ConfigureAppConfiguration(cfg =>
    {
        cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddHostedService<PlcLoggerWorker>();
    })
    .Build()
    .Run();