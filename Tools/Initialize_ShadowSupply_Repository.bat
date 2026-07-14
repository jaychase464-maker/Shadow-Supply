@echo off
setlocal
cd /d "%~dp0.."
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Initialize_ShadowSupply_Repository.ps1"
echo.
pause
