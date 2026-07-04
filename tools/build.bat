@echo off
REM
REM Generic mod build (cmd). Produces Release\%MOD_NAME%.zip.
REM
REM Invoked from a mod root by the mod's thin build.bat wrapper, which sets:
REM   MOD_NAME  the mod folder / DLL / zip base name (e.g. VesselBookmarkMod)
REM   MOD_SLN   the solution file to build (e.g. VesselBookmark.sln)
REM
REM Convention: the packaged payload is everything under GameData\%MOD_NAME%\
REM (minus PluginData\, which is runtime data), plus the shared TMP sprite
REM textures and the built DLL.
setlocal enabledelayedexpansion

if not defined MOD_NAME (
    echo ERROR: MOD_NAME is not set ^(the wrapper must set it^)
    exit /b 1
)
if not defined MOD_SLN (
    echo ERROR: MOD_SLN is not set ^(the wrapper must set it^)
    exit /b 1
)

echo.
echo -------------------------------------------
echo Detecting KSP structure
echo -------------------------------------------

if not defined KSPDIR (
    echo ERROR: The KSPDIR environment variable is not defined
    exit /b 1
)

REM Use !KSPDIR! inside ( ) blocks: the parentheses of "(x86)" otherwise break batch parsing.
if exist "!KSPDIR!\KSP_x64_Data\Managed\Assembly-CSharp.dll" (
    echo Windows layout detected ^(KSP_x64_Data^)
    set "KSP_DATA_DIR=!KSPDIR!\KSP_x64_Data"
) else if exist "!KSPDIR!\KSP_Data\Managed\Assembly-CSharp.dll" (
    echo Linux layout detected ^(KSP_Data^)
    set "KSP_DATA_DIR=!KSPDIR!\KSP_Data"
) else (
    echo ERROR: Assembly-CSharp.dll not found under !KSPDIR!\KSP_x64_Data\Managed\ or !KSPDIR!\KSP_Data\Managed\
    exit /b 1
)

echo Using KSPDIR: !KSPDIR!
echo Using KSP_DATA_DIR: !KSP_DATA_DIR!

set "STAGE=Release\%MOD_NAME%"

echo.
echo ===============================
echo Building %MOD_NAME%
echo ===============================

echo Removing Release folder
if exist Release rmdir /s /q Release

echo Creating stage folder
mkdir "%STAGE%"
if errorlevel 1 (
    echo ERROR: Failed to create the stage folder
    exit /b 1
)

echo Building Mod DLL
dotnet build "%MOD_SLN%" -p:KSPDIR="!KSPDIR!" -p:KSP_DATA_DIR="!KSP_DATA_DIR!"
if errorlevel 1 (
    echo ERROR: Failed to build the Mod DLL
    exit /b 1
)

REM Payload = the whole GameData\%MOD_NAME% tree, minus PluginData (runtime config,
REM never shipped).
echo Copying mod payload from GameData\%MOD_NAME%
xcopy /E /I /Y "GameData\%MOD_NAME%\*" "%STAGE%\" >nul
if errorlevel 1 (
    echo ERROR: Failed to copy the mod payload
    exit /b 1
)
if exist "%STAGE%\PluginData" rmdir /s /q "%STAGE%\PluginData"

REM Shared TMP sprite textures (e.g. refresh_icon), read at runtime from GameData\...\Textures.
echo Copying shared textures (TMP sprites)
if not exist "%STAGE%\Textures" mkdir "%STAGE%\Textures"
xcopy /E /I /Y "KSP-Shared\GameData\Textures\*" "%STAGE%\Textures\" >nul
if errorlevel 1 (
    echo ERROR: Failed to copy the shared textures
    exit /b 1
)

echo Copying Mod dll file
copy /y "Output\bin\%MOD_NAME%.dll" "%STAGE%"
if errorlevel 1 (
    echo ERROR: Failed to copy the Mod DLL
    exit /b 1
)

echo Zipping Mod
powershell -NoProfile -Command "Compress-Archive -Path '%STAGE%\*' -DestinationPath 'Release\%MOD_NAME%.zip' -Force"
if errorlevel 1 (
    echo ERROR: Failed to zip the Mod
    exit /b 1
)

echo Removing stage folder
rmdir /s /q "%STAGE%"

echo.
echo Build Complete
echo.
echo Run at: %date% %time%
