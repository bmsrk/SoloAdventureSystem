param(
  [string]$ProjectDir = 'SoloAdventureSystem.Web.UI',
  [string]$ImageName = 'soloadventuresystem-webui:local'
)

$projectPath = Join-Path -Path (Get-Location) -ChildPath $ProjectDir
Write-Host "Building project in WSL and creating docker image: $ImageName"

# Build in WSL to use the installed dotnet
wsl -- bash -lc "cd /home/bruno/source/repos/SoloAdventureSystem/$ProjectDir || cd /mnt/c/Users/bruno/source/repos/SoloAdventureSystem/$ProjectDir; dotnet publish -c Release -o /tmp/publish --nologo"

Write-Host "Building docker image"
docker build -f "$ProjectDir/Dockerfile" -t $ImageName .
Write-Host "Run with: docker run --rm -p 8080:80 $ImageName"