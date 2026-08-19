# ChainOSC for Windows — Tauri prototype

## v0.6.0

This version improves backup, migration, and distribution readiness:

- add and delete any number of Keys;
- a name and independent global hotkey for every Key;
- OSC Press and Release messages;
- Int, Float, and String OSC values;
- configurable OSC host, UDP port, and address;
- persistent settings using the application WebView's local storage;
- automatic migration of the v0.1.0 Key configuration;
- duplicate global-hotkey validation;
- up to eight Press and Release OSC messages per Key;
- message addition, deletion, and ordering;
- Press / Release and Sequence modes;
- import and export of `ChainOSC-device-preset` Key JSON files;
- import support for legacy `M5ChainOSC-device-preset` files.
- minimizing or closing the window hides it in the system tray;
- global hotkeys continue working while the window is hidden;
- double-clicking the tray icon restores the window;
- the tray menu provides Show ChainOSC and Exit commands;
- Exit fully terminates the application and releases its hotkeys;
- release builds use a ChainOSC-specific executable name.
- optional startup with Windows, beginning hidden in the system tray;
- single-instance enforcement;
- launching ChainOSC again restores the existing window instead of starting a duplicate.
- complete versioned JSON settings backup and restore;
- an unsaved-changes indicator;
- product, version, and runtime information in the UI;
- a custom ChainOSC application icon for executables, installers, windows, and the tray.
- test buttons and an activity log.

The `.NET 10` implementation in the repository root remains the reference
implementation while the Tauri version is developed.

## Run

```powershell
cd tauri
npm install
npm run tauri dev
```

Exit the `.NET` ChainOSC prototype before testing, because both applications
cannot register the same global hotkey simultaneously.
