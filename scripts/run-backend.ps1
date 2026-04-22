Write-Host "============================================" -ForegroundColor Cyan
Write-Host "   Whitebird Backend API" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$backendPath = Join-Path $PSScriptRoot "..\src\Whitebird"
Set-Location $backendPath

Write-Host "Starting Backend API..." -ForegroundColor Yellow
Write-Host "URL: https://localhost:5001/swagger" -ForegroundColor Cyan
Write-Host ""

dotnet run --launch-profile "Backend Only"

Read-Host "Press Enter to exit"