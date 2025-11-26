$repo = 'C:\Users\bruno\source\repos\SoloAdventureSystem'
Set-Location $repo
$projDirs = Get-ChildItem -Path $repo -Recurse -Filter *.csproj | ForEach-Object { $_.DirectoryName } | Select-Object -Unique
$allCs = Get-ChildItem -Path $repo -Recurse -Filter *.cs -File | Where-Object { $_.FullName -notmatch '\\bin\\' -and $_.FullName -notmatch '\\obj\\' } | ForEach-Object { $_.FullName }
$orphans = @()
foreach ($f in $allCs) {
  $inAny = $false
  foreach ($d in $projDirs) {
    if ($f.StartsWith($d, [System.StringComparison]::OrdinalIgnoreCase)) {
      $inAny = $true
      break
    }
  }
  if (-not $inAny) { $orphans += $f }
}
if ($orphans.Count -eq 0) {
  Write-Output 'No orphaned .cs files found'
} else {
  Write-Output "Orphaned files (not under any project dir):"
  $orphans | ForEach-Object { Write-Output $_ }
}
