using PlcLoggerService.Services;
Host.CreateDefaultBuilder(args)
    .UseWindowsService()
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