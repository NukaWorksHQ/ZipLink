using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Web;
using Web.Common;
using Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var apiBaseUrl = builder.Configuration.GetValue<string>("AppSettings:ApiHost");
if (apiBaseUrl is null)
    throw new InvalidOperationException("apiBaseUrl (AppSettings:ApiHost) cannot be null, please update your appSettings.json configuration.");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<IAuthValidator, AuthValidator>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ILinkService, LinkService>();

builder.Services.AddSingleton<AccountState>();

await builder.Build().RunAsync();
