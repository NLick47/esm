@echo off
chcp 65001 >nul
title EventStreamManager Service (Console)
echo ==========================================
echo  EventStreamManager WebApi (Console Mode)
echo ==========================================
echo.
echo Press Ctrl+C to stop the service.
echo.

set WORKDIR=%~dp0..
cd /d "%WORKDIR%"

if not exist "EventStreamManager.WebApi.exe" (
    echo [ERROR] EventStreamManager.WebApi.exe not found in %WORKDIR%
    echo Please publish the project with self-contained first.
    pause
    exit /b 1
)

EventStreamManager.WebApi.exe

if errorlevel 1 (
    echo.
    echo [ERROR] Service exited with code %errorlevel%
    pause
)
