@echo off
chcp 65001 >nul
title Install EventStreamManager as Windows Service
echo ==========================================
echo  Install EventStreamManager Windows Service
echo ==========================================
echo.

setlocal

set "SERVICE_NAME=EventStreamManager"
set "SERVICE_DISPLAY=EventStreamManager WebApi Service"
set "WORKDIR=%~dp0.."
for %%F in ("%WORKDIR%") do set "WORKDIR=%%~dpF"
set "WORKDIR=%WORKDIR:~0,-1%"

if not exist "%WORKDIR%\EventStreamManager.WebApi.exe" (
    echo [ERROR] EventStreamManager.WebApi.exe not found.
    echo Expected path: %WORKDIR%\EventStreamManager.WebApi.exe
    echo Please publish the WebApi project with self-contained first.
    pause
    exit /b 1
)

echo Service Name : %SERVICE_NAME%
echo Working Dir  : %WORKDIR%
echo.

:: Check admin rights
net session >NUL 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Administrator rights required.
    echo Please right-click and select "Run as administrator".
    pause
    exit /b 1
)

:: Uninstall first if exists
sc query %SERVICE_NAME% >NUL 2>&1
if %errorlevel%==0 (
    echo Existing service found. Removing...
    sc stop %SERVICE_NAME% >NUL 2>&1
    sc delete %SERVICE_NAME% >NUL 2>&1
    timeout /t 2 /nobreak >NUL
)

:: Create service
:: Note: For production, consider using Microsoft.Extensions.Hosting.WindowsServices
:: and wrapping with a proper service host, or use nssm for better reliability.
set "BIN_PATH=\"%WORKDIR%\EventStreamManager.WebApi.exe\""

echo Creating service...
sc create %SERVICE_NAME% binPath= "%BIN_PATH%" start= auto displayname= "%SERVICE_DISPLAY%"
if %errorlevel% neq 0 (
    echo [ERROR] Failed to create service.
    pause
    exit /b 1
)

sc description %SERVICE_NAME% "EventStreamManager WebApi background service."

echo.
echo [SUCCESS] Service installed successfully.
echo.
echo To start the service now, run: net start %SERVICE_NAME%
echo Or run: start-service.bat for console mode.
echo.
pause
endlocal
