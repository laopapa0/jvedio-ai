@echo off
setlocal enabledelayedexpansion

REM ========================================
REM Jvedio Build Script
REM ========================================

echo ========================================
echo Jvedio Build Script
echo ========================================
echo.

REM Check MSBuild
where msbuild >nul 2>nul
if errorlevel 1 (
    echo [ERROR] MSBuild not found
    echo Please install Visual Studio or .NET Framework SDK
    echo Download: https://dotnet.microsoft.com/download/dotnet-framework
    pause
    exit /b 1
)

echo [1/5] Restore NuGet packages...
cd /d "%~dp0Jvedio"
if exist packages.config (
    nuget restore packages.config -PackagesDirectory ..\packages
    if errorlevel 1 (
        echo [ERROR] NuGet restore failed
        pause
        exit /b 1
    )
)

echo [2/5] Clean old build...
msbuild Jvedio.csproj /t:Clean /p:Configuration=Release /v:m /nologo

echo [3/5] Build Release version...
msbuild Jvedio.csproj /t:Build /p:Configuration=Release /v:m /nologo
if errorlevel 1 (
    echo [ERROR] Build failed
    pause
    exit /b 1
)

echo [4/5] Prepare release package...
cd /d "%~dp0"
if not exist "Release" mkdir Release

REM Copy build output
xcopy "Jvedio\bin\Release\*" "Release\" /E /I /Y /Q

REM Copy AI config
copy ".env.example" "Release\.env" /Y

REM Create folders
if not exist "Release\plugins\crawlers" mkdir Release\plugins\crawlers
if not exist "Release\Data\x64" mkdir Release\Data\x64
if not exist "Release\Data\x86" mkdir Release\Data\x86

REM Copy SQLite
xcopy "Jvedio\Data\x64\*" "Release\Data\x64\" /Y /Q
xcopy "Jvedio\Data\x86\*" "Release\Data\x86\" /Y /Q

echo [5/5] Clean temp files...
del /Q "Release\*.pdb" 2>nul
del /Q "Release\*.xml" 2>nul
del /Q "Release\*.vshost.*" 2>nul

echo.
echo ========================================
echo Build Complete!
echo Output: %~dp0Release
echo ========================================
echo.
echo Next steps:
echo 1. Edit Release\.env file
echo 2. Run Release\Jvedio.exe
echo.
pause
