@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo Обновление репозитория в папке %cd%...
git pull --ff-only

if errorlevel 1 (
    echo.
    echo Не удалось обновить репозиторий.
) else (
    echo.
    echo Готово!
)

pause
