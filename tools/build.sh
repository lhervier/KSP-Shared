#!/usr/bin/env bash
#
# Generic mod build (bash). Produces Release/$MOD_NAME.zip.
#
# Invoked from a mod root by the mod's thin build.sh wrapper, which exports:
#   MOD_NAME  the mod folder / DLL / zip base name (e.g. VesselBookmarkMod)
#   MOD_SLN   the solution file to build (e.g. VesselBookmark.sln)
#
# Convention: the packaged payload is everything under GameData/$MOD_NAME/ (minus
# PluginData/, which is runtime data), plus the shared TMP sprite textures and the
# built DLL. The mod-specific asset list lives on disk, not in this script.
set -euo pipefail

die() {
    echo "ERROR: $*" >&2
    exit 1
}

require_command() {
    command -v "$1" >/dev/null 2>&1 || die "command not found: $1"
}

detect_ksp_data_dir() {
    if [[ -z "${KSPDIR:-}" ]]; then
        die "the KSPDIR environment variable is not defined (KSP installation directory)"
    fi
    if [[ -f "$KSPDIR/KSP_x64_Data/Managed/Assembly-CSharp.dll" ]]; then
        echo "Windows layout detected (KSP_x64_Data)"
        KSP_DATA_DIR="$KSPDIR/KSP_x64_Data"
    elif [[ -f "$KSPDIR/KSP_Data/Managed/Assembly-CSharp.dll" ]]; then
        echo "Linux layout detected (KSP_Data)"
        KSP_DATA_DIR="$KSPDIR/KSP_Data"
    else
        die "Assembly-CSharp.dll not found in $KSPDIR/KSP_x64_Data/Managed/ or $KSPDIR/KSP_Data/Managed/"
    fi
    echo "Using KSPDIR: $KSPDIR"
    echo "Using KSP_DATA_DIR: $KSP_DATA_DIR"
}

[[ -n "${MOD_NAME:-}" ]] || die "MOD_NAME is not set (the wrapper must export it)"
[[ -n "${MOD_SLN:-}" ]] || die "MOD_SLN is not set (the wrapper must export it)"

echo "========="
echo "Building $MOD_NAME"
echo "========="

require_command dotnet
require_command zip
detect_ksp_data_dir

MSBUILD_PROPS=(-p:KSPDIR="$KSPDIR" -p:KSP_DATA_DIR="$KSP_DATA_DIR")
STAGE="Release/$MOD_NAME"

echo "Removing Release folder"
rm -rf Release

echo "Creating stage folder"
mkdir -p "$STAGE"

echo "Restoring NuGet packages"
dotnet restore "$MOD_SLN" "${MSBUILD_PROPS[@]}"

echo "Building the mod DLL (.NET Framework 4.7.2)"
dotnet build "$MOD_SLN" "${MSBUILD_PROPS[@]}" --no-restore

# Payload = the whole GameData/$MOD_NAME tree, minus PluginData (runtime config,
# never shipped).
echo "Copying mod payload from GameData/$MOD_NAME"
cp -a "GameData/$MOD_NAME/." "$STAGE/"
rm -rf "$STAGE/PluginData"

# Shared TMP sprite textures (e.g. refresh_icon), rendered inline via <sprite> in
# labels and read at runtime from GameData/.../Textures.
echo "Copying shared textures (TMP sprites)"
mkdir -p "$STAGE/Textures"
cp -a KSP-Shared/GameData/Textures/. "$STAGE/Textures/"

echo "Copying the mod DLL"
cp -v "Output/bin/$MOD_NAME.dll" "$STAGE/"

echo "Creating the archive"
(
    cd "$STAGE"
    zip -qr "../$MOD_NAME.zip" .
)

echo "Removing the intermediate folder"
rm -rf "$STAGE"

echo
echo "Build complete: Release/$MOD_NAME.zip"
echo "Run at: $(date)"
