@echo off
cd /d "%~dp0"

echo Updating repository in %cd%...
git pull --ff-only

if errorlevel 1 goto error

echo.
echo Done!
goto end

:error
echo.
echo Failed to update the repository.

:end
pause
