@echo off
title Whitebird Full Stack Application

echo ============================================
echo    Whitebird Full Stack Application
echo ============================================
echo.

echo [1/2] Starting Backend API...
start "Whitebird Backend" /MIN cmd /c "cd ..\src\Whitebird && dotnet run --launch-profile ""Backend (No Browser)"""

echo Waiting for backend to initialize...
timeout /t 5 /nobreak > nul

echo.
echo [2/2] Starting Frontend...
cd ..\src\Whitebird
set VITE_BACKEND_URL=https://localhost:5001

echo.
echo ============================================
echo    Application Started!
echo ============================================
echo.
echo    Backend API:  https://localhost:5001/swagger
echo    Frontend:     http://localhost:3000
echo.
echo    Close this window to stop frontend only
echo    (Backend runs in separate window)
echo ============================================
echo.

npm run dev -- --host

pause