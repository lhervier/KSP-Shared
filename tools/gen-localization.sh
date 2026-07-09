#!/usr/bin/env bash
#
# Generate KSP Localization .cfg files from flat "key = value" sources (Linux path).
#
# Merges the shared common keys with the mod-specific keys, prefixes every key with
# #LOC_<ModName>_ (the runtime prefix, see ModLocalization.GetString) and wraps them in
# the ConfigNode envelope KSP expects. Mod keys override shared keys (with a warning).
# The flat sources staged with the payload are consumed and replaced by the .cfg files.
#
# Args: <ModName> <SharedDir> <StageDir>
#   SharedDir  flat sources shared by all mods (KSP-Shared/Localization); may be absent
#   StageDir   the mod's staged Localization dir; its flat sources are consumed here
set -euo pipefail

MOD_NAME="$1"
SHARED_DIR="$2"
STAGE_DIR="$3"

# Basenames (without extension) of every language present as a shared or mod source.
list_langs() {
    {
        [[ -d "$SHARED_DIR" ]] && ls "$SHARED_DIR"/*.properties 2>/dev/null || true
        [[ -d "$STAGE_DIR"  ]] && ls "$STAGE_DIR"/*.properties  2>/dev/null || true
    } | sed 's#.*/##; s#\.properties$##' | sort -u
}

# Emit "key<TAB>value" for each entry of a flat file. First '=' splits; blank and
# '#' lines are skipped; key and value are trimmed. Missing file => no output.
read_props() {
    local file="$1" line key val
    [[ -f "$file" ]] || return 0
    while IFS= read -r line || [[ -n "$line" ]]; do
        line="${line%$'\r'}"
        line="${line#"${line%%[![:space:]]*}"}"; line="${line%"${line##*[![:space:]]}"}"
        [[ -z "$line" || "${line:0:1}" == "#" || "$line" != *"="* ]] && continue
        key="${line%%=*}"; val="${line#*=}"
        key="${key#"${key%%[![:space:]]*}"}"; key="${key%"${key##*[![:space:]]}"}"
        val="${val#"${val%%[![:space:]]*}"}"; val="${val%"${val##*[![:space:]]}"}"
        [[ -z "$key" ]] && continue
        printf '%s\t%s\n' "$key" "$val"
    done < "$file"
}

langs="$(list_langs)"
if [[ -z "$langs" ]]; then
    echo "gen-localization: no .properties source found (shared or mod); nothing to generate"
    exit 0
fi

mkdir -p "$STAGE_DIR"

while IFS= read -r lang; do
    [[ -z "$lang" ]] && continue
    out="$STAGE_DIR/$lang.cfg"
    {
        printf 'Localization\n{\n    %s\n    {\n' "$lang"
        # Shared stream then mod stream; awk keeps the LAST value per key (mod wins)
        # while preserving first-seen order, and warns on override / ConfigNode chars.
        {
            read_props "$SHARED_DIR/$lang.properties"
            read_props "$STAGE_DIR/$lang.properties"
        } | awk -F '\t' -v mod="$MOD_NAME" -v lang="$lang" '
            {
                key = $1
                val = substr($0, index($0, "\t") + 1)
                if (key in seen) {
                    printf("gen-localization: WARNING [%s] mod overrides shared key %s\n", lang, key) > "/dev/stderr"
                } else {
                    order[++n] = key
                }
                seen[key] = val
            }
            END {
                for (i = 1; i <= n; i++) {
                    k = order[i]; v = seen[k]
                    if (v ~ /\/\/|[{}]/) {
                        printf("gen-localization: WARNING [%s] value of %s has a ConfigNode-special char: %s\n", lang, k, v) > "/dev/stderr"
                    }
                    printf("        #LOC_%s_%s = %s\n", mod, k, v)
                }
            }'
        printf '    }\n}\n'
    } > "$out"
    echo "gen-localization: wrote $out"
done <<< "$langs"

# Flat sources are build inputs, never shipped.
find "$STAGE_DIR" -maxdepth 1 -name '*.properties' -delete
