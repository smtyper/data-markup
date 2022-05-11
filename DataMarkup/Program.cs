using DataMarkup;

var host = Host.CreateDefaultBuilder()
    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
    .Build();

await host.RunAsync();
