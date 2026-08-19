# ChainOSC for Windows

ChainOSC for Windows sends OSC messages from configurable global hotkeys.

## v0.1.0

This first milestone verifies the complete path from a Windows global hotkey
to an OSC UDP message.

- Configurable key (`F1`–`F12` or `A`–`Z`) and modifiers
- Separate Press and Release values
- OSC `Int`, `Float`, and `String` values
- Configurable destination host, UDP port, and OSC Address
- Test buttons and an activity log
- The hotkey is not blocked from other applications

Settings are not persisted in v0.1.0. The preset-compatible multi-Key UI will
be expanded in later milestones.

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
