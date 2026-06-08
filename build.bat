@echo off
setlocal enabledelayedexpansion

echo.
echo -------------------------------------------
echo Detection de la structure KSP
echo -------------------------------------------

REM Vérifier si KSPDIR est défini
if not defined KSPDIR (
    echo ERREUR: La variable d'environnement KSPDIR n'est pas définie
    echo Veuillez définir KSPDIR avec le chemin vers votre installation KSP
    echo Exemple: set "KSPDIR=C:\Program Files ^(x86^)\Steam\steamapps\common\Kerbal Space Program"
    exit /b 1
)

REM Vérifier que les DLLs KSP existent (Windows ou Linux)
REM Utiliser !KSPDIR! dans les blocs ( ) : les parenthèses de "(x86)" cassent sinon le parsing batch
if exist "!KSPDIR!\KSP_x64_Data\Managed\Assembly-CSharp.dll" (
    echo Structure Windows détectée ^(KSP_x64_Data^)
    set "KSP_DATA_DIR=!KSPDIR!\KSP_x64_Data"
) else if exist "!KSPDIR!\KSP_Data\Managed\Assembly-CSharp.dll" (
    echo Structure Linux détectée ^(KSP_Data^)
    set "KSP_DATA_DIR=!KSPDIR!\KSP_Data"
) else (
    echo ERREUR: Assembly-CSharp.dll non trouvé dans !KSPDIR!\KSP_x64_Data\Managed\ ou !KSPDIR!\KSP_Data\Managed\
    echo Vérifiez que KSPDIR pointe vers le bon répertoire KSP
    exit /b 1
)

echo Utilisation de KSPDIR: !KSPDIR!
echo Utilisation de KSP_DATA_DIR: !KSP_DATA_DIR!

echo.
echo -------------------------------------------
echo Building dotnet library
echo -------------------------------------------
dotnet build KSP-Shared.csproj -p:KSP_DATA_DIR="%KSP_DATA_DIR%"
if errorlevel 1 (
    echo ERROR: Failed to build dotnet library
    exit /b 1
)

echo.
echo Library Build completed successfully
