using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SoloAdventureSystem.ProtoWasm;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<ProtoGeneratorService>();

await builder.Build().RunAsync();