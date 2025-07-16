using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System;
using System.Net.Http.Json;
using Web;
using Web.Common;
using Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();

var environment = builder.HostEnvironment.Environment;
var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var config = await httpClient.GetFromJsonAsync<AppSettings>($"appsettings.{environment}.json");

Console.WriteLine(config.ApiHost);

if (config.ApiHost is null)
    throw new InvalidOperationException("apiBaseUrl (AppSettings:ApiHost) cannot be null, please update your appSettings.json configuration.");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(config.ApiHost) });
builder.Services.AddScoped<IAuthValidator, AuthValidator>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ILinkService, LinkService>();

builder.Services.AddSingleton<AccountState>();

await builder.Build().RunAsync();
