This is a minimal Blazor WebAssembly static host for SoloAdventureSystem UI.

Build and publish:

1. Copy wwwroot assets from `SoloAdventureSystem.Web.UI/wwwroot` into `SoloAdventureSystem.Web.Wasm/wwwroot` (css, images, fonts).
2. dotnet publish -c Release -o ./publish
3. Deploy contents of `publish/wwwroot` to GitHub Pages (gh-pages branch) or static host.

Note: This project references only the UI components. You must remove server-only services and API calls or stub them for static hosting.