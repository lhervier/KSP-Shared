#!/usr/bin/env bash
#
# Generic mod install (bash). Deploys Release/$MOD_NAME.zip into KSP's GameData.
#
# Invoked from a mod root by the mod's thin install.sh wrapper, which exports:
#   MOD_NAME  the mod folder / zip base name (e.g. VesselBookmarkMod)
#
# The mod's runtime config lives in GameData/$MOD_NAME/PluginData/. That folder is
# backed up before the mod folder is wiped, then restored on top of the freshly
# unzipped files, so a reinstall never discards a previous install's settings.
set -euo pipefail

die() {
    echo "ERROR: $*" >&2
    exit 1
}

require_command() {
    command -v "$1" >/dev/null 2>&1 || die "command not found: $1"
}

[[ -n "${MOD_NAME:-}" ]] || die "MOD_NAME is not set (the wrapper must export it)"

if [[ -z "${KSPDIR:-}" ]]; then
    die "the KSPDIR environment variable is not defined (KSP installation directory)"
fi
if [[ ! -d "$KSPDIR/GameData" ]]; then
    die "KSPDIR does not point to a valid KSP installation: $KSPDIR"
fi

require_command unzip

ZIP_FILE="Release/$MOD_NAME.zip"
[[ -f "$ZIP_FILE" ]] || die "archive not found: $ZIP_FILE (run ./build.sh first)"

MOD_DIR="$KSPDIR/GameData/$MOD_NAME"
PLUGIN_DATA="$MOD_DIR/PluginData"
BACKUP="${TMPDIR:-/tmp}/KSP-$MOD_NAME-PluginData-backup"

echo "====================================="
echo "Backing up existing PluginData (config)"
echo "====================================="
rm -rf "$BACKUP"
if [[ -d "$PLUGIN_DATA" ]]; then
    echo "Saving $PLUGIN_DATA"
    cp -a "$PLUGIN_DATA" "$BACKUP"
else
    echo "No existing PluginData folder to back up"
fi

echo
echo "====================================="
echo "Removing the existing installation"
echo "====================================="
rm -rf "$MOD_DIR"

echo
echo "====================================="
echo "Extracting the mod"
echo "====================================="
mkdir -p "$MOD_DIR"
# unzip returns 1 for warnings only (e.g. a zip written by PowerShell's
# Compress-Archive uses backslash separators) while still extracting correctly.
# Capture the code instead of letting `set -e` abort here: the PluginData restore
# below must run no matter what, so a previous install's config is never lost.
unzip_rc=0
unzip -oq "$ZIP_FILE" -d "$MOD_DIR" || unzip_rc=$?

echo
echo "====================================="
echo "Restoring PluginData"
echo "====================================="
if [[ -d "$BACKUP" ]]; then
    echo "Restoring config to $PLUGIN_DATA"
    mkdir -p "$PLUGIN_DATA"
    cp -a "$BACKUP/." "$PLUGIN_DATA/"
    rm -rf "$BACKUP"
else
    echo "No PluginData backup to restore"
fi

# Only a genuine unzip error (code >= 2) is fatal, and only after the config has
# been restored.
if [[ "$unzip_rc" -gt 1 ]]; then
    die "unzip reported errors (exit $unzip_rc): the mod files may be incomplete"
fi

echo
echo "Mod installed in: $MOD_DIR"
echo "Run at: $(date)"
