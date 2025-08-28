using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;
using Microsoft.JSInterop;
using Web;
using Web.Common;
using Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();

var environment = builder.HostEnvironment.Environment;

var configHttpClient = new HttpClient
{ 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
};

var config = await configHttpClient.GetFromJsonAsync<AppSettings>($"appsettings.{environment}.json");
configHttpClient.Dispose();

if (config?.ApiHosts == null || !config.ApiHosts.Any())
{
    throw new InvalidOperationException("ApiHosts cannot be null or empty, please update your appSettings.json configuration.");
}

var defaultApiHost = config.ApiHosts.First().Url;

// HttpClient principal pour WebAssembly
builder.Services.AddScoped(_ => new HttpClient
{ 
    BaseAddress = new Uri(defaultApiHost) 
});

builder.Services.AddScoped<IAuthValidator, AuthValidator>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ILinkService, LinkService>();
builder.Services.AddScoped<IApiHostService, ApiHostService>();

builder.Services.AddScoped<ILocalizationService>(serviceProvider =>
{
    var httpClientForLocalization = new HttpClient
    { 
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
    };
    var jsRuntime = serviceProvider.GetRequiredService<IJSRuntime>();
    return new LocalizationService(httpClientForLocalization, jsRuntime);
});

builder.Services.AddSingleton(config);

builder.Services.AddSingleton<AccountState>();

await builder.Build().RunAsync();
