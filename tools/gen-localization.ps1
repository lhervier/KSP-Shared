<#
.SYNOPSIS
    Generate KSP Localization .cfg files from flat "key = value" sources (Windows path).

.DESCRIPTION
    Merges the shared common keys with the mod-specific keys, prefixes every key with
    #LOC_<ModName>_ (the runtime prefix, see ModLocalization.GetString) and wraps them in
    the ConfigNode envelope KSP expects. Mod keys override shared keys (with a warning).

    The flat sources staged with the payload are consumed and replaced by the generated
    .cfg files, so only the .cfg ship in the zip.

    Kept in PowerShell (not batch) because localization values contain characters cmd's
    echo mangles (< > ( ) in e.g. "Suborbital (<<1>>)"). build.bat already shells to
    powershell for Compress-Archive, so this adds no new dependency.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$ModName,
    [Parameter(Mandatory = $true)][string]$SharedDir,
    [Parameter(Mandatory = $true)][string]$StageDir
)
$ErrorActionPreference = 'Stop'

# Parse a flat file into an ordered map. Blank lines and lines starting with '#' are
# ignored; the split is on the FIRST '=' only so values may themselves contain '='.
function Read-Props($path) {
    $map = [ordered]@{}
    if (-not (Test-Path $path)) { return $map }
    foreach ($raw in [System.IO.File]::ReadAllLines($path)) {
        $line = $raw.Trim()
        if ($line.Length -eq 0 -or $line.StartsWith('#')) { continue }
        $idx = $line.IndexOf('=')
        if ($idx -lt 0) { continue }
        $key = $line.Substring(0, $idx).Trim()
        $val = $line.Substring($idx + 1).Trim()
        if ($key.Length -gt 0) { $map[$key] = $val }
    }
    return $map
}

# Every language present as either a shared or a mod source file.
$langs = @{}
foreach ($dir in @($SharedDir, $StageDir)) {
    if (Test-Path $dir) {
        Get-ChildItem -Path $dir -Filter '*.properties' -File | ForEach-Object { $langs[$_.BaseName] = $true }
    }
}
if ($langs.Count -eq 0) {
    Write-Host "gen-localization: no .properties source found (shared or mod); nothing to generate"
    return
}

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

foreach ($lang in $langs.Keys) {
    # Shared keys first, then mod keys override.
    $merged = Read-Props (Join-Path $SharedDir "$lang.properties")
    $modMap = Read-Props (Join-Path $StageDir  "$lang.properties")
    foreach ($k in $modMap.Keys) {
        if ($merged.Contains($k)) { Write-Host "gen-localization: WARNING [$lang] mod overrides shared key '$k'" }
        $merged[$k] = $modMap[$k]
    }

    $sb = New-Object System.Text.StringBuilder
    [void]$sb.AppendLine('Localization')
    [void]$sb.AppendLine('{')
    [void]$sb.AppendLine("    $lang")
    [void]$sb.AppendLine('    {')
    foreach ($k in $merged.Keys) {
        $v = $merged[$k]
        if ($v -match '//|[{}]') {
            Write-Host "gen-localization: WARNING [$lang] value of '$k' has a ConfigNode-special char (// { }): '$v'"
        }
        [void]$sb.AppendLine("        #LOC_${ModName}_${k} = $v")
    }
    [void]$sb.AppendLine('    }')
    [void]$sb.AppendLine('}')

    if (-not (Test-Path $StageDir)) { New-Item -ItemType Directory -Path $StageDir | Out-Null }
    $outPath = Join-Path $StageDir "$lang.cfg"
    [System.IO.File]::WriteAllText($outPath, $sb.ToString(), $utf8NoBom)
    Write-Host "gen-localization: wrote $outPath ($($merged.Count) keys)"
}

# Flat sources are build inputs, never shipped.
if (Test-Path $StageDir) {
    Get-ChildItem -Path $StageDir -Filter '*.properties' -File | Remove-Item -Force
}
