# ChainOSC for Windows

ChainOSC for Windows sends OSC messages from configurable global hotkeys.

## Documentation

- [Documentation Portal](https://shimez.github.io/ChainOSC-for-Windows/)
- [日本語クイックスタート](https://shimez.github.io/ChainOSC-for-Windows/quick-start/)
- [English Quick Start](https://shimez.github.io/ChainOSC-for-Windows/en/quick-start/)
- [日本語ユーザーガイド](https://shimez.github.io/ChainOSC-for-Windows/user-guide/)
- [English User Guide](https://shimez.github.io/ChainOSC-for-Windows/en/user-guide/)
- [Latest Release](https://github.com/shimez/ChainOSC-for-Windows/releases/latest)
- [変更履歴](CHANGELOG.md)
- [M5ChainOSC Key Presets](https://github.com/shimez/M5ChainOSC/tree/main/presets/key)

## Repository structure

- [`tauri/`](tauri/) — Current supported implementation and the source of official releases
- [`legacy/dotnet-prototype/`](legacy/dotnet-prototype/) — Archived .NET/WPF prototype retained for reference

The .NET prototype is no longer the primary implementation. New development,
support, and releases are based on the Tauri implementation.

## v1.0.0

The first stable release provides English and Japanese UI, Key action menus, recordable
global hotkeys, Windows startup, system tray operation, and
ChainOSC-compatible Key presets.

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
cd tauri
npm install
npm run tauri dev
```

To verify OSC without another application, start the included receiver in a
second terminal, leave the target at `127.0.0.1:9000`, and press the hotkey:

```powershell
python "..\scripts\osc_receiver.py"
```

## Publish for another Windows PC

Create a release build from the Tauri directory:

```powershell
npm run tauri build
```

The portable executable is generated under
`tauri\src-tauri\target\release\chainosc-for-windows.exe`. Installer packages
are generated under `tauri\src-tauri\target\release\bundle`.

The Microsoft Edge WebView2 Runtime is required. It is included with current
Windows 10/11 installations in most environments.

Include `LICENSE`, `THIRD_PARTY_NOTICES.md`, and the complete `licenses`
directory with every distributed binary package.

## License

ChainOSC for Windows is licensed under the [MIT License](LICENSE).

Distributed binaries include third-party open-source software. Copyright,
license, and source-availability information for those components is provided
in [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md). These files and the
`licenses` directory should be included with every binary distribution.
