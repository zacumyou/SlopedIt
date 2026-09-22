$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    New-Item -ItemType Directory -Path .\research -Force | Out-Null
    dotnet build .\src\SlopedIt.csproj -c Release --nologo *> .\research\build.log
    if ($LASTEXITCODE -ne 0) { throw 'Build failed. See research/build.log.' }
    dotnet run --project .\tests\SlopeTests.csproj -c Release *> .\research\tests.log
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed. See research/tests.log.' }
    Get-Content .\research\build.log -Tail 5
    Get-Content .\research\tests.log
} finally { Pop-Location }
