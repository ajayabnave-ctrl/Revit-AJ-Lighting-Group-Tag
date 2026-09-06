@echo off
echo =======================================================
echo Building AJ Lighting Group Tag for Revit 2026 (.NET 8)
echo =======================================================

dotnet build "%~dp0AJ.LightingGroupTag.csproj" -c Release

if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Build failed! Please check errors above.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo =======================================================
echo Deploying to Revit 2026 Addins directory...
echo =======================================================

set "REVIT_ADDIN_DIR=%APPDATA%\Autodesk\Revit\Addins\2026"

if not exist "%REVIT_ADDIN_DIR%" (
    mkdir "%REVIT_ADDIN_DIR%"
)

copy /Y "%~dp0bin\Release\AJ.LightingGroupTag.dll" "%REVIT_ADDIN_DIR%\"
copy /Y "%~dp0AJ.LightingGroupTag.addin" "%REVIT_ADDIN_DIR%\"

echo.
echo [SUCCESS] Add-in successfully built and deployed to:
echo %REVIT_ADDIN_DIR%
echo.
pause
