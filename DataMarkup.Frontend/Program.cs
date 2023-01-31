using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DataMarkup.Frontend;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
// builder.Services.AddScoped<ApiAuthenticationStateProvider>();
// builder.Services.AddScoped<AuthService>();

builder.Services.AddHttpClient<DataMarkupClient>()
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("https://localhost:5001/api/");
        client.DefaultRequestHeaders.Add("Accept", "*/*");
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        client.DefaultRequestHeaders.Add("Connection", "keep-alive");
    });

await builder.Build().RunAsync();
