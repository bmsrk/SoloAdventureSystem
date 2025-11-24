# Verify dotnet inside WSL and show environment
param(
    [string]$Distro = 'Ubuntu'
)

Write-Host "Shutting down WSL..."
wsl --shutdown

Write-Host "Verifying .NET inside WSL distro: $Distro"
wsl -d $Distro -- bash -lc "/usr/bin/dotnet --info || true; echo; echo 'DOTNET_ROOT=' \$DOTNET_ROOT; echo 'PATH=' \$PATH"

Write-Host "If dotnet is missing, run the setup script inside WSL: sudo /scripts/wsl-setup-dotnet.sh"