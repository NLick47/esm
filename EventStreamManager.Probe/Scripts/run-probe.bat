@echo off
chcp 65001 >nul
title EventStreamManager Probe
echo ==========================================
echo  EventStreamManager Probe (Health Monitor)
echo ==========================================
echo.

set WORKDIR=%~dp0..
cd /d "%WORKDIR%"

if not exist "EventStreamManager.Probe.exe" (
    echo [ERROR] EventStreamManager.Probe.exe not found in %WORKDIR%
    echo Please publish the probe project with self-contained first.
    pause
    exit /b 1
)

if not exist "probeSettings.json" (
    echo [WARNING] probeSettings.json not found. Using defaults.
)

EventStreamManager.Probe.exe

if errorlevel 1 (
    echo.
    echo [ERROR] Probe exited with code %errorlevel%
    pause
)
