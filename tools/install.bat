@echo off
REM
REM Generic mod install (cmd). Deploys Release\%MOD_NAME%.zip into KSP's GameData.
REM
REM Invoked from a mod root by the mod's thin install.bat wrapper, which sets:
REM   MOD_NAME  the mod folder / zip base name (e.g. VesselBookmarkMod)
REM
REM The mod's runtime config lives in GameData\%MOD_NAME%\PluginData\. That folder
REM is backed up before the mod folder is wiped, then restored on top of the freshly
REM unzipped files, so a reinstall never discards a previous install's settings.
setlocal enabledelayedexpansion

if not defined MOD_NAME (
    echo ERROR: MOD_NAME is not set ^(the wrapper must set it^)
    exit /b 1
)

if not defined KSPDIR (
    echo ERROR: The KSPDIR environment variable is not defined
    exit /b 1
)

set "MOD_DIR=!KSPDIR!\GameData\%MOD_NAME%"
set "PLUGIN_DATA=!MOD_DIR!\PluginData"
set "BACKUP=!TEMP!\KSP-%MOD_NAME%-PluginData-backup"
set "ZIP_FILE=Release\%MOD_NAME%.zip"

if not exist "%ZIP_FILE%" (
    echo ERROR: archive not found: %ZIP_FILE% ^(run build.bat first^)
    exit /b 1
)

echo.
echo -------------------------------------------
echo Backing up existing PluginData (config)
echo -------------------------------------------

if exist "!BACKUP!" rmdir /s /q "!BACKUP!"
if exist "!PLUGIN_DATA!" (
    echo Saving !PLUGIN_DATA!
    mkdir "!BACKUP!" 2>nul
    xcopy /E /I /Y "!PLUGIN_DATA!" "!BACKUP!\" >nul
    if errorlevel 1 (
        echo ERROR: Failed to back up PluginData
        exit /b 1
    )
) else (
    echo No existing PluginData folder to back up
)

echo.
echo -------------------------------------------
echo Removing the existing installation
echo -------------------------------------------
if exist "!MOD_DIR!" rmdir /s /q "!MOD_DIR!"

echo.
echo -------------------------------------------
echo Extracting the mod
echo -------------------------------------------
if not exist "!KSPDIR!\GameData" mkdir "!KSPDIR!\GameData"
mkdir "!MOD_DIR!"
powershell -NoProfile -ExecutionPolicy Bypass -Command "Expand-Archive -Path '%ZIP_FILE%' -DestinationPath '!MOD_DIR!' -Force"
if errorlevel 1 (
    echo ERROR: Failed to unzip the Mod
    exit /b 1
)

echo.
echo -------------------------------------------
echo Restoring PluginData
echo -------------------------------------------
if exist "!BACKUP!" (
    echo Restoring config to !PLUGIN_DATA!
    if not exist "!PLUGIN_DATA!" mkdir "!PLUGIN_DATA!"
    xcopy /E /I /Y "!BACKUP!\*" "!PLUGIN_DATA!\" >nul
    if errorlevel 1 (
        echo ERROR: Failed to restore PluginData
        exit /b 1
    )
    rmdir /s /q "!BACKUP!"
) else (
    echo No PluginData backup to restore
)

echo.
echo Mod installed
echo.
echo Run at: %date% %time%
