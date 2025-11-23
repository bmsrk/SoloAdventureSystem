using SoloAdventureSystem.Web.UI.Components;
using SoloAdventureSystem.ContentGenerator.Configuration;
using SoloAdventureSystem.ContentGenerator.Adapters;
using SoloAdventureSystem.ContentGenerator.Utils;
using SoloAdventureSystem.Web.UI.Services;
using SoloAdventureSystem.ContentGenerator.Generation;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Fluent UI services
builder.Services.AddFluentUIComponents();

// Configure AI settings
builder.Services.Configure<AISettings>(builder.Configuration.GetSection(AISettings.SectionName));

// Add AI and world generation services
builder.Services.AddSingleton<IImageAdapter, SimpleImageAdapter>();
builder.Services.AddSingleton<WorldValidator>();
builder.Services.AddSingleton<WorldExporter>();
builder.Services.AddSingleton<WorldGenerationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
