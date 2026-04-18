@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion
title Stop EventStreamManager Service
echo Stopping EventStreamManager processes...

:: Try stopping via Windows Service first
sc query EventStreamManager >NUL 2>&1
if %errorlevel%==0 (
    echo Windows service found. Stopping via SC...
    net stop EventStreamManager >NUL 2>&1
    if !errorlevel!==0 (
        echo Service stopped successfully.
        goto :end
    )
)

:: Fallback: kill EventStreamManager.WebApi.exe processes
for /f "skip=1 tokens=2 delims=," %%a in ('tasklist /FI "IMAGENAME eq EventStreamManager.WebApi.exe" /FO CSV 2^>NUL') do (
    for /f "delims=" %%b in ("%%a") do (
        set "pid=%%~b"
        if not "!pid!"=="" (
            echo Killing EventStreamManager.WebApi PID: !pid!
            taskkill /PID !pid! /T /F >NUL 2>&1
        )
    )
)

echo Done.

:end
endlocal
timeout /t 2 /nobreak >NUL
