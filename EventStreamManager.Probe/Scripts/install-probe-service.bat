@echo off
chcp 65001 >nul
title Install EventStreamManager Probe as Windows Service
echo ==========================================
echo  Install EventStreamManager Probe Service
echo ==========================================
echo.

setlocal

set "SERVICE_NAME=EventStreamManagerProbe"
set "SERVICE_DISPLAY=EventStreamManager Probe Service"
set "WORKDIR=%~dp0.."
for %%F in ("%WORKDIR%") do set "WORKDIR=%%~dpF"
set "WORKDIR=%WORKDIR:~0,-1%"

if not exist "%WORKDIR%\EventStreamManager.Probe.exe" (
    echo [ERROR] EventStreamManager.Probe.exe not found.
    echo Expected path: %WORKDIR%\EventStreamManager.Probe.exe
    echo Please publish the Probe project with self-contained first.
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

set "BIN_PATH=\"%WORKDIR%\EventStreamManager.Probe.exe\""

echo Creating service...
sc create %SERVICE_NAME% binPath= "%BIN_PATH%" start= auto displayname= "%SERVICE_DISPLAY%"
if %errorlevel% neq 0 (
    echo [ERROR] Failed to create service.
    pause
    exit /b 1
)

sc description %SERVICE_NAME% "EventStreamManager health probe and auto-restart service."

echo.
echo [SUCCESS] Probe service installed successfully.
echo.
echo To start now, run: net start %SERVICE_NAME%
echo.
pause
endlocal
