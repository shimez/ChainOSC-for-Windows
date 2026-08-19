# ChainOSC for Windows

ChainOSC for Windows sends OSC messages from configurable global hotkeys.

## v0.3.0

This milestone aligns Windows Key behavior and device presets with the
ChainOSC series.

- Add and delete any number of Key configurations
- Configurable key (`F1`–`F12` or `A`–`Z`) and modifiers
- Press / Release or Sequence mode
- Up to 8 Press + Release OSC messages per Key
- Add, delete, reorder, or leave either event with zero messages
- OSC `Int`, `Float`, and `String` values
- Configurable destination host, UDP port, and OSC Address
- Test buttons and an activity log
- The hotkey is not blocked from other applications
- Duplicate hotkey validation
- Automatic restoration from `%LOCALAPPDATA%\ChainOSC\settings.json`
- Import and export compatible `ChainOSC-device-preset` Key JSON files
- Automatic migration of v0.2.0 settings

Windows-only fields such as the global hotkey, local Key name, and internal ID
are intentionally not included in shared device presets.

## Run

```powershell
dotnet restore
dotnet build "ChainOSC.slnx"
dotnet run --project "src\ChainOSC.Windows\ChainOSC.Windows.csproj"
```

To verify OSC without another application, start the included receiver in a
second terminal, leave the target at `127.0.0.1:9000`, and press the hotkey:

```powershell
python "scripts\osc_receiver.py"
```
