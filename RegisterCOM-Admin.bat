@echo off
:: Run this file as Administrator (right-click -> Run as administrator)
:: It registers PVDesigner.dll so Inventor can load it as a COM add-in.

set DLL=%~dp0bin\Release\PVDesigner.dll
set REGASM=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe

echo === PV Designer - COM Registration ===
echo DLL: %DLL%
echo.

if not exist "%DLL%" (
    echo ERROR: DLL not found. Run Install.ps1 first to build the project.
    pause
    exit /b 1
)

"%REGASM%" "%DLL%" /codebase /nologo
if %ERRORLEVEL% == 0 (
    echo.
    echo COM registration successful!
    echo You can now start Inventor 2025.
) else (
    echo.
    echo ERROR: RegAsm failed. Make sure you are running as Administrator.
)
pause
