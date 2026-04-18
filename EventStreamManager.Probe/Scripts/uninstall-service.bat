@echo off
chcp 65001 >nul
title Uninstall EventStreamManager Windows Service
echo ==========================================
echo  Uninstall EventStreamManager Windows Service
echo ==========================================
echo.

set "SERVICE_NAME=EventStreamManager"

:: Check admin rights
net session >NUL 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Administrator rights required.
    echo Please right-click and select "Run as administrator".
    pause
    exit /b 1
)

sc query %SERVICE_NAME% >NUL 2>&1
if %errorlevel% neq 0 (
    echo Service '%SERVICE_NAME%' not found.
    pause
    exit /b 0
)

echo Stopping service...
sc stop %SERVICE_NAME% >NUL 2>&1
timeout /t 2 /nobreak >NUL

echo Deleting service...
sc delete %SERVICE_NAME%
if %errorlevel%==0 (
    echo.
    echo [SUCCESS] Service uninstalled.
) else (
    echo.
    echo [ERROR] Failed to uninstall service.
)

pause
