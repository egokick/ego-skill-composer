@echo off
REM Get the current directory (assumed to be the project root)
set "projectDir=%cd%"
set "dllPath=%projectDir%\bin\Debug\net8.0\skill-composer.dll"

REM Check if the DLL exists
if not exist "%dllPath%" (
    echo DLL not found. Building project...
    dotnet build --configuration Debug
    if errorlevel 1 (
        echo Build failed. Exiting.
        pause
        exit /b 1
    )
)

REM Change to the directory containing the DLL and run it
cd /d "%projectDir%\bin\Debug\net8.0"
echo Running skill-composer.dll...
dotnet skill-composer.dll

pause
