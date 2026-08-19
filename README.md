# ChainOSC for Windows

ChainOSC for Windows sends OSC messages from configurable global hotkeys.

## v0.6.0

This milestone adds recordable global hotkeys and expanded key support while
retaining Windows startup, system tray, and ChainOSC-compatible Key behavior.

- Record a hotkey by pressing the desired key combination
- Supports `Ctrl`, `Alt`, `Shift`, and Windows-key modifiers
- Adds numbers, arrows, navigation keys, numpad keys, Space, Tab, Enter,
  punctuation, and other common Windows keys
- Escape cancels recording and Backspace clears the assignment
- Duplicate assignments are reported immediately while recording
- Reserved Windows shortcuts are rejected
- OSC sending is temporarily suspended while recording a hotkey

- Optional automatic startup when the current user signs in to Windows
- Optional tray-only startup when launched automatically
- No administrator rights required for startup registration
- Prevent multiple ChainOSC processes from running simultaneously
- Running the executable again opens the existing settings window

- Closing or minimizing the settings window keeps ChainOSC running in the tray
- Double-click the tray icon to reopen the settings window
- Tray menu commands for `Open Settings` and `Exit`
- Global hotkeys and OSC transmission remain active while the window is hidden

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

## Publish for another Windows PC

The ordinary build output is not a single portable executable. To copy the app
to another 64-bit Windows PC, publish the complete folder:

```powershell
dotnet publish "src\ChainOSC.Windows\ChainOSC.Windows.csproj" `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -o ".\publish\win-x64"
```

Copy the complete `publish\win-x64` folder to the other PC and start
`ChainOSC.Windows.exe`. The Microsoft Edge WebView2 Runtime is also required;
it is included with current Windows 10/11 installations in most environments.
