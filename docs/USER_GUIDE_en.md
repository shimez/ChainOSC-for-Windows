---
layout: default
title: ChainOSC for Windows User Guide
permalink: /en/user-guide/
---

# ChainOSC for Windows User Guide

[日本語版](../../user-guide/)

ChainOSC for Windows sends configured OSC messages when Windows global hotkeys are pressed or released. It continues to operate while the settings window is hidden in the system tray.

## 1. System requirements

- 64-bit Windows 10 or Windows 11
- Microsoft Edge WebView2 Runtime
- Network access to the OSC receiver

WebView2 Runtime is already installed on most Windows 10 and Windows 11 systems. Install the latest runtime from Microsoft if ChainOSC for Windows does not start.

## 2. Starting and exiting

1. Extract the distributed ZIP to a folder of your choice.
2. Run `chainosc-for-windows.exe`.
3. If Windows Defender or SmartScreen displays a warning, verify the source and file before continuing.

Closing or minimizing the window does not exit the application. It remains in the system tray, and global hotkeys and OSC transmission continue to work.

- Double-click the tray icon: show the settings window
- `Show ChainOSC` in the tray menu: show the settings window
- `Exit` in the tray menu: completely exit the application

Starting the executable again does not create a duplicate process. It opens the settings window of the running instance.

## 3. Quick start

1. Enter the Host Name or IP Address and UDP Port of the OSC receiver.
2. Enter a Device Name for the Key.
3. Select Global Hotkey and press the desired key combination.
4. Configure the OSC Address, Type, and Value for Press and Release.
5. Use Test Press and Test Release to verify transmission.
6. Select Save All Settings.

After saving, the configured hotkey sends OSC even while another application is active.

For a complete first-use walkthrough, see the [English Quick Start](../quick-start/).

## 4. Common settings

### Language

Switches the interface between English and Japanese. The selected language is restored the next time the application starts. Changing the language does not discard settings currently being edited.

### System

Displays the product name, version, and runtime. Include this version when reporting a problem.

### OSC Target

| Setting | Description |
| --- | --- |
| Host Name or IP Address | The host name or IP address of the OSC receiver. Use `127.0.0.1` when sending to an application on the same computer. |
| UDP Port | The UDP port used by the receiver. VRChat's standard OSC input port is `9000`. |

### Application

Enable Start with Windows to launch ChainOSC automatically when the current user signs in. An automatic launch starts hidden in the system tray. Administrator privileges are not required.

> [!IMPORTANT]
> Before updating or removing ChainOSC for Windows, disable Start with Windows and select Save All Settings. If the old executable is moved or deleted first, its old path remains in the Windows startup configuration.

In the current Tauri version, settings saved with Save All Settings and the selected UI language are stored in the application's WebView2 local storage. The underlying data uses LevelDB files under the following directory and is not stored beside the executable:

```text
%LOCALAPPDATA%\io.github.shimez.chainosc\EBWebView\Default\Local Storage\leveldb\
```

Do not edit these files directly. Use the JSON export described below to back up settings or move them to another computer. The legacy .NET prototype's `%LOCALAPPDATA%\ChainOSC\settings.json` is not used by the current Tauri version.

### Settings backup and restore

Export or import all ChainOSC for Windows settings as versioned JSON. The backup includes:

- OSC destination
- Startup setting
- All Keys
- Device names
- Global hotkeys
- OSC messages and Sequence settings

After importing settings, review them and select Save All Settings.

## 5. Adding and configuring Keys

Select Add Key to append a new Key configuration. There is no fixed Key-count limit, but the same global hotkey cannot be assigned to multiple Keys.

| Setting | Description |
| --- | --- |
| Device Name | A descriptive name used to identify the Key. |
| Global Hotkey | A key or key combination recognized while any Windows application is active. |
| Key Mode | Select Press / Release or Sequence (press only). |

Select the Global Hotkey field, then press the desired key combination.

- Supports combinations with `Ctrl`, `Alt`, `Shift`, and the Windows key
- Supports function keys, letters, numbers, arrows, numpad keys, Space, Tab, Enter, punctuation, and other common keys
- `Backspace` or `Delete`: clear the assignment
- `Escape`: cancel recording
- Some Windows-reserved shortcuts cannot be assigned
- Duplicate hotkeys are reported while recording

A Key may be saved without a hotkey. A warning appears when saving, and that Key will not send OSC from keyboard input. Its test buttons remain available.

## 6. Press / Release mode

Sends independent OSC messages when the hotkey is pressed and released.

Press and Release can contain up to eight messages in total. Messages are sent in the displayed order. Leaving either event with zero messages disables transmission for that event.

| Setting | Description |
| --- | --- |
| OSC Address | An OSC Address beginning with `/`, such as `/input/Jump`. |
| Type | Select `Int`, `Float`, or `String`. |
| Value | Enter a value valid for the selected OSC type. |

Each message provides these controls:

- `↑` / `↓`: change transmission order
- Delete: remove the message
- Add OSC Message: add a message to the currently displayed Press or Release event

The add button is disabled when the combined total reaches eight.

## 7. Sequence (press only) mode

Each hotkey press sends the next value in a sequence. After the End value is passed, the sequence returns to Start. Releasing the hotkey sends nothing.

| Setting | Description |
| --- | --- |
| OSC Address | The OSC Address that receives the sequence value. |
| Start | The first value sent. |
| End | The final value in the sequence. |
| Step | The amount added after each press. Use a negative number for a descending sequence. `0` is not allowed. |
| Type | The output type, such as `Float` or `Int`. Decimal portions are discarded for `Int`. |

For example, Start `0`, End `2`, and Step `1` sends `0 → 1 → 2 → 0` on successive presses.

## 8. Testing and Debug Log

Use Test Press and Test Release to send the current form values without operating the global hotkey.

Open the normally collapsed **Debug Log** to see the Key, event, OSC Address, Type, Value, and any errors. Use it when diagnosing a receiver that does not react. Device Preset import/export results and test-send errors also appear inside the affected Key card.

## 9. Key presets

Open `…` in the upper-right corner of a Key card to export or import that Key as a JSON preset.

A preset contains:

- Key Mode
- Press and Release OSC messages
- Sequence settings

It does not contain these Windows-specific values:

- Device Name
- Global Hotkey
- Internal ID

This allows the same preset to be applied safely to another Key. After importing, review the values and select Save All Settings.

### Shared preset source

Key presets published for M5ChainOSC can also be used with ChainOSC for Windows.

- [M5ChainOSC Key Presets](https://github.com/shimez/M5ChainOSC/tree/main/presets/key)

To use a shared preset:

1. Download the desired JSON file.
2. Open `…` on the destination Key.
3. Select Import Preset (JSON).
4. Choose the downloaded JSON file.
5. Review the OSC Address, Type, and Value.
6. Assign a Device Name and Global Hotkey as needed.
7. Select Save All Settings.

ChainOSC for Windows supports both the current `ChainOSC-device-preset` format and legacy `M5ChainOSC-device-preset` Key files. Presets exported from the Windows application can also be imported into M5ChainOSC and ChainOSCmini.

Imports are validated according to ChainOSC Device Preset Import Error Registry v1. An invalid file is rejected without changing the Key settings, and its Error Code, correction guidance, and error context appear in the affected Key card and Debug Log.

## 10. Deleting a Key

Open `…` on the Key to remove, then select Delete Key. Select Save All Settings afterward to remove it from the saved configuration.

## 11. Unsaved changes and saving

Editing a setting displays Unsaved changes. Save All Settings validates the configuration, re-registers global hotkeys, and updates the startup setting.

Save before exiting and after importing settings. Export a complete settings backup before making significant changes.

## 12. Using VRChat

Enable OSC in VRChat:

1. Open the Action Menu.
2. Open Options.
3. Open OSC.
4. Enable OSC.

When VRChat runs on the same computer, normally use host `127.0.0.1` and UDP port `9000`. Configure OSC Addresses and values for the VRChat feature or avatar parameter you want to control.

## 13. Notes and precautions

- A global hotkey does not block the same key input from reaching other applications.
- If Windows or another application uses the same shortcut, both actions may occur.
- OSC uses UDP and does not guarantee delivery or retransmission.
- After importing a preset, verify that its destination and message contents are appropriate for your use.
- ChainOSC for Windows is an unofficial, independently developed project. It is not an official product of VRChat, M5Stack, or their affiliates.

### Updating

1. Disable Start with Windows.
2. Select Save All Settings.
3. Exit from the system tray menu.
4. Extract the new ZIP and overwrite the existing `chainosc-for-windows.exe`.
5. Start the new version and verify the settings.
6. If desired, enable Start with Windows again and select Save All Settings.

### Removing

1. Disable Start with Windows.
2. Select Save All Settings.
3. Exit from the system tray menu.
4. Delete the ChainOSC for Windows distribution folder.

If the executable was already deleted and its startup entry remains, remove it from PowerShell with:

```powershell
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "ChainOSCForWindows" /f
```

## 14. License

ChainOSC for Windows is released under the MIT License.

- [MIT License](https://github.com/shimez/ChainOSC-for-Windows/blob/main/LICENSE)
- [Third-Party Software Notices](https://github.com/shimez/ChainOSC-for-Windows/blob/main/THIRD_PARTY_NOTICES.md)
