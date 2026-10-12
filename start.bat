@echo off
chcp 65001 >nul
cd /d "%~dp0Game1"
echo Запуск сайта... Через несколько секунд откроется браузер.
echo Чтобы остановить сайт — закройте это окно.
start "" cmd /c "timeout /t 10 >nul & start http://localhost:5177/Account/Login"
dotnet run --launch-profile http
pause
