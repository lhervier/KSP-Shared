# KSP-Shared

A library of shared components and utilities used across several Kerbal Space Program (KSP) mods.

The code is intentionally coupled to the KSP assemblies (`Assembly-CSharp`, `Assembly-CSharp-firstpass`) and Unity: it is meant to be consumed **from inside a KSP mod**, not as a general-purpose library.

## Contents

- `Src/shared/ugui/` — uGUI interface components (builders + controllers): buttons, checkboxes, popups, along with the associated styles (palettes) and sprite/texture helpers.

All code lives in the `com.github.lhervier.ksp.shared.*` namespace.

## How this repository is consumed

This repository is designed to be added as a git **submodule** inside a mod, then **compiled directly from sources** (including the `.cs` files via `<Compile Include="..." />`) so the consuming mod produces a **single DLL**. The `.csproj` below is for standalone development (editing, IntelliSense, build validation), not to produce a DLL referenced by the mod.

## Standalone build

Building requires the KSP DLLs. Set the `KSPDIR` environment variable to your KSP installation, then run the script for your OS.

### Windows

```bat
set "KSPDIR=C:\Program Files (x86)\Steam\steamapps\common\Kerbal Space Program"
build.bat
```

### Linux

```bash
export KSPDIR="$HOME/.steam/steam/steamapps/common/Kerbal Space Program"
./build.sh
```

The scripts auto-detect the `KSP_x64_Data` (Windows) or `KSP_Data` (Linux) layout, derive `KSP_DATA_DIR` from it, then run `dotnet build`.
