using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DataMarkup.Frontend;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthenticationStateProvider, ApplicationAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();

builder.Services.AddHttpClient<ApiHttpClient>()
    .ConfigureHttpClient(httpClient => httpClient.BaseAddress = new Uri(builder.Configuration["ApiUri"]!));

await builder.Build().RunAsync();
