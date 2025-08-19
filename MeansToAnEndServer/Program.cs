using MeansToAnEndServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


// _ = Task.Run(async () =>
// {
//     await Task.Delay(2000);
//     await ClientTester.RunTestsAsync();
// });

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices(services =>
{
    services.AddHostedService<Worker>();
});

var host = builder.Build();
await host.RunAsync();


