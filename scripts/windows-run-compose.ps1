param(
  [string]$ComposeFile = 'docker-compose.yml'
)

Write-Host "Building and starting containers from $ComposeFile"
# Use docker compose v2
docker compose -f $ComposeFile up --build -d

Write-Host "Containers started. To view logs: docker compose -f $ComposeFile logs -f"