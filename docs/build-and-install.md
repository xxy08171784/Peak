# Verified PCK build

Peak's Godot resources must be imported and exported serially with the blocking
`Godot_*_console.exe` executable. The GUI executable returns control before its
child process finishes on Windows, which can make export read partially written
`.ctex` files and produce a PCK whose character scenes cannot load.

The script also redirects Godot and .NET temporary publishing files to D: by
default. A full system drive makes Godot's .NET publish fail even though the
outer Godot process can still return exit code 0 and continue writing a PCK.

Run the verified build script with a new output path:

```powershell
.\build\build_verified_pack.ps1 -OutputPath D:\path\to\new-peak.pck
```

The script fails before installation when any of these checks fail:

- Debug or Release C# compilation;
- the complete Scout card/type/localization audit;
- Godot import or export emits an `ERROR:` line;
- any of the 57 Scout idle/death animation frames or character scenes cannot be
  loaded from the finished PCK;
- the Scout energy counter lacks its required layers, VFX nodes, or label font.

Never install an unverified PCK, and never overwrite an existing build artifact.
