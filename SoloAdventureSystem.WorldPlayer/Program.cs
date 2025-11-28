using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoloAdventureSystem.ContentGenerator.Generation;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var aiSettings = new AISettings
{
    Provider = "LLamaSharp",
    LLamaModelKey = "tinyllama-q4",
    Model = "tinyllama-q4",
    ContextSize = 2048,
    UseGPU = true,
    MaxInferenceThreads = 1
};
builder.Services.AddSingleton(Options.Create(aiSettings));

// Register factory to create SLMAdapter when needed
builder.Services.AddSingleton<ILocalSLMAdapter>(sp => SLMAdapterFactory.Create(sp));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
