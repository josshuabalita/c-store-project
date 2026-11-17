# Runs the same checks as CI. Run before every push.
$ErrorActionPreference = 'Stop'
Set-Location (Split-Path $PSScriptRoot -Parent)

function Step($name, [scriptblock]$cmd) {
    Write-Host "`n== $name" -ForegroundColor Cyan
    & $cmd
    if ($LASTEXITCODE -ne 0) { Write-Host "FAILED: $name" -ForegroundColor Red; exit $LASTEXITCODE }
}

Step 'restore' { dotnet restore }
Step 'format check (fix with: dotnet format)' { dotnet format ColumnStore.sln --verify-no-changes --no-restore }
Step 'build, warnings are errors' { dotnet build -c Release --no-restore }
Step 'tests + 85% line coverage gate on ColumnStore.Core' {
    dotnet test -c Release --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura `
        /p:Include="[ColumnStore.Core]*" /p:Threshold=85 /p:ThresholdType=line
}
Write-Host "`nAll checks passed." -ForegroundColor Green
