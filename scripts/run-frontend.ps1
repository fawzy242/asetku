Write-Host "============================================" -ForegroundColor Cyan
Write-Host "   Whitebird Frontend (React)" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$frontendPath = Join-Path $PSScriptRoot "..\src\Whitebird"
Set-Location $frontendPath

Write-Host "Installing dependencies (if needed)..." -ForegroundColor Gray
npm install --silent

Write-Host ""
Write-Host "Starting Frontend..." -ForegroundColor Yellow
Write-Host "URL: http://localhost:3000" -ForegroundColor Cyan
Write-Host "Backend Proxy: https://localhost:5001/api" -ForegroundColor Gray
Write-Host ""

$env:VITE_BACKEND_URL = "https://localhost:5001"
npm run dev -- --host

Read-Host "Press Enter to exit"