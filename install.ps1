$ErrorActionPreference = 'Stop'
if (Get-Process Cities2 -ErrorAction SilentlyContinue) { throw 'Close Cities: Skylines II before installing.' }
$slopedSource = Join-Path $PSScriptRoot 'local\SlopedIt'
$slopedMods = [IO.Path]::GetFullPath((Join-Path $env:USERPROFILE 'AppData\LocalLow\Colossal Order\Cities Skylines II\Mods'))
$slopedTarget = [IO.Path]::GetFullPath((Join-Path $slopedMods 'SlopedIt'))
if ((Split-Path $slopedTarget -Parent) -ne $slopedMods -or (Split-Path $slopedTarget -Leaf) -ne 'SlopedIt') { throw 'Unexpected install target.' }
if (!(Test-Path (Join-Path $slopedSource 'SlopedIt.dll'))) { throw 'Build output is missing.' }
$slopedBackup = $null
if (Test-Path -LiteralPath $slopedTarget) {
    $slopedBackup = Join-Path $PSScriptRoot ('backups\SlopedIt-' + (Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
    New-Item -ItemType Directory -Path $slopedBackup -Force | Out-Null
    Copy-Item -LiteralPath $slopedTarget -Destination $slopedBackup -Recurse
}
New-Item -ItemType Directory -Path $slopedTarget -Force | Out-Null
$slopedFiles = @('SlopedIt.dll','SlopedIt.pdb','SlopedIt_win_x86_64.dll','SlopedIt_win_x86_64.pdb')
$slopedManifest = foreach ($name in $slopedFiles) {
    $source = Join-Path $slopedSource $name
    $target = Join-Path $slopedTarget $name
    Copy-Item -LiteralPath $source -Destination $target -Force
    $sourceHash = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
    $targetHash = (Get-FileHash -LiteralPath $target -Algorithm SHA256).Hash
    if ($sourceHash -ne $targetHash) { throw "Hash mismatch: $name" }
    [pscustomobject]@{ File=$name; SHA256=$targetHash; Bytes=(Get-Item -LiteralPath $target).Length }
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.ko.md') -Destination (Join-Path $slopedTarget 'README.ko.md') -Force
[pscustomobject]@{
    InstalledAt=(Get-Date -Format o); Target=$slopedTarget; Backup=$slopedBackup
    GameRunning=$false; InGameVerified=$false; Files=$slopedManifest
} | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $PSScriptRoot 'research\install.json') -Encoding utf8
Write-Output "Installed and verified: $slopedTarget"
$slopedManifest | Format-Table -AutoSize
