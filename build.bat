@echo off
echo ========================================
echo   AstralParty Mod Manager - Build
echo ========================================
echo.

where dotnet >nul 2>&1
if %errorlevel% equ 0 (
    echo [OK] .NET SDK found
    dotnet --version
    echo.
    goto :build
) else (
    echo [Error] .NET SDK not found
    echo.
    echo Please install .NET 8.0 SDK or later:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

:build
echo ========================================
echo   Start Build
echo ========================================
echo.

cd /d "%~dp0src"

if not exist "AstralPartyModManager.csproj" (
    echo [Error] Project file not found!
    pause
    exit /b 1
)

echo [1/3] Restoring dependencies...
dotnet restore

echo [2/3] Building project...
dotnet build -c Release --no-restore

if %errorlevel% neq 0 (
    echo.
    echo [Error] Build failed!
    pause
    exit /b 1
)

echo [3/3] Publishing to publish folder...
dotnet publish -c Release -r win-x64 --self-contained true -o ../publish --no-build

echo.
echo ========================================
echo   Build Complete!
echo ========================================
echo.
echo Output: %~dp0publish
echo.
echo Files:
dir /b "..\publish" *.exe 2>nul
echo.
echo Press any key to open publish folder...
pause >nul

if exist "..\publish\APmodManager.exe" (
    explorer "..\publish"
)
