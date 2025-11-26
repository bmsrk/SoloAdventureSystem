using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Components.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// copy static assets from Web.UI /wwwroot via build script or manual step

await builder.Build().RunAsync();