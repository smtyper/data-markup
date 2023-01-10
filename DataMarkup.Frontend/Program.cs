using System.Net;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DataMarkup.Frontend;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient<DataMarkupClient>()
    // .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    // {
    //     AutomaticDecompression =
    // })
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("https://localhost:5001/api/");
        client.DefaultRequestHeaders.Add("Accept", "*/*");
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        client.DefaultRequestHeaders.Add("Connection", "keep-alive");
    });

await builder.Build().RunAsync();
