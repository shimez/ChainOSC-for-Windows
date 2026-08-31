---
layout: default
title: ChainOSC for Windows Quick Start
permalink: /en/quick-start/
---

# ChainOSC for Windows Quick Start

[日本語版](../../quick-start/)

This guide takes you from downloading ChainOSC for Windows to sending OSC to VRChat with a global hotkey. See the [English User Guide](../user-guide/) for complete documentation.

> [!IMPORTANT]
> ChainOSC for Windows is an unofficial, independently developed project. It is not an official product of VRChat Inc., M5Stack Technology Co., Ltd., or their affiliates.

## What you need

- A 64-bit Windows 10 or Windows 11 computer
- An OSC-capable application (VRChat is used in this guide)
- Microsoft Edge WebView2 Runtime

WebView2 Runtime is already installed on most Windows 10 and Windows 11 systems. Install the latest runtime from Microsoft if the application does not start.

## 1. Download ChainOSC for Windows

1. Open the [latest GitHub Release](https://github.com/shimez/ChainOSC-for-Windows/releases/latest).
2. Download `ChainOSC-for-Windows-vX.Y.Z-win-x64-portable.zip` from Assets.
3. Extract the complete ZIP to a folder of your choice.
4. Run `chainosc-for-windows.exe` from the extracted folder.

Do not run the executable from inside the ZIP. If Windows Defender SmartScreen appears, verify the source and file name before continuing.

## 2. Enable OSC in VRChat

Open the VRChat Action Menu and enable OSC:

```text
Action Menu → Options → OSC → Enabled
```

## 3. Configure the OSC destination

Use these values when VRChat is running on the same computer:

- Host name or IP address: `127.0.0.1`
- UDP port: `9000`

To send to another computer or device, enter the receiver's IP address and UDP port.

## 4. Configure a Key and hotkey

1. Enter a descriptive name under Device Name.
2. Select the Global Hotkey field.
3. Press the key or key combination you want to use. `Ctrl`, `Alt`, `Shift`, and the Windows key can be included.
4. Configure the OSC Address, Type, and Value under Press.
5. Configure Release if needed.

Example VRChat jump configuration:

- OSC Address: `/input/Jump`
- Type: `Int`
- Press value: `1`
- Release value: `0`

## 5. Test and save

1. Use Test Press and Test Release to check the current settings.
2. Open the **Debug Log** at the bottom of the window and verify the OSC Address, Type, and Value.
3. Select Save All Settings.
4. Press the global hotkey and confirm the result in VRChat.

## 6. Use a shared preset

Key presets published under [M5ChainOSC Key Presets](https://github.com/shimez/M5ChainOSC/tree/main/presets/key) are compatible with ChainOSC for Windows.

1. Download the desired JSON file.
2. Open `…` in the target Key card.
3. Select Import Preset (JSON).
4. Choose the downloaded file.
5. Review the OSC settings, then assign a Device Name and Global Hotkey.
6. Select Save All Settings.

## 7. Use the system tray

Closing or minimizing the window keeps ChainOSC for Windows running in the system tray. Global hotkeys and OSC transmission continue while the settings window is hidden.

- Double-click the tray icon: show the settings window
- `Show ChainOSC`: show the settings window
- `Exit`: completely exit the application

Basic setup is now complete. See the [English User Guide](../user-guide/) for multiple messages, Sequence mode, full backup, startup with Windows, and other features.
