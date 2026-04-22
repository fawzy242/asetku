Write-Host "============================================" -ForegroundColor Cyan
Write-Host "   Whitebird Full Stack Application" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$rootPath = Join-Path $PSScriptRoot "..\src\Whitebird"

# Start Backend
Write-Host "[1/2] Starting Backend API..." -ForegroundColor Yellow
$backendJob = Start-Job -ScriptBlock {
    param($path)
    Set-Location $path
    dotnet run --launch-profile "Backend (No Browser)" 2>&1
} -ArgumentList $rootPath

Write-Host "      Waiting for backend to start..." -ForegroundColor Gray
Start-Sleep -Seconds 5

# Start Frontend
Write-Host "[2/2] Starting Frontend..." -ForegroundColor Yellow
Set-Location $rootPath

$env:VITE_BACKEND_URL = "https://localhost:5001"

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "   Application Started Successfully!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "   Backend API:  https://localhost:5001/swagger" -ForegroundColor Cyan
Write-Host "   Frontend:     http://localhost:3000" -ForegroundColor Cyan
Write-Host ""
Write-Host "   Press Ctrl+C to stop frontend" -ForegroundColor Yellow
Write-Host "   (Backend will stop automatically when this window closes)" -ForegroundColor Gray
Write-Host "============================================" -ForegroundColor Green
Write-Host ""

try {
    npm run dev -- --host
} finally {
    Write-Host ""
    Write-Host "Stopping backend..." -ForegroundColor Yellow
    Stop-Job -Job $backendJob
    Remove-Job -Job $backendJob
    Write-Host "All services stopped." -ForegroundColor Green
    Write-Host ""
    Read-Host "Press Enter to exit"
}