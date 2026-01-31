@echo off
setlocal enabledelayedexpansion

REM ========================================
REM Jvedio Quick Start Script
REM ========================================

echo ========================================
echo Jvedio Quick Start
echo ========================================
echo.

REM Check .env file
if not exist ".env" (
    echo [INFO] .env not found
    echo.
    echo Create .env file? (Y/N)
    choice /C YN /N /M "Select"
    if errorlevel 2 goto start_app

    echo.
    echo [1/2] Copying template...
    copy ".env.example" ".env" >nul
    echo [2/2] Config file created
    echo.
    pause
    notepad .env
    goto start_app
)

:start_app
echo [START] Launching Jvedio...
echo.

REM Check main executable
if not exist "Jvedio.exe" (
    echo [ERROR] Jvedio.exe not found
    echo Please run from correct directory
    echo.
    pause
    exit /b 1
)

REM Start application
start "" "Jvedio.exe"

echo [SUCCESS] Jvedio launched
echo.
pause
