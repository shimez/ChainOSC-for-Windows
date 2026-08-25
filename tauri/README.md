# ChainOSC for Windows — Tauri

## v1.0.2

This stable release aligns the interface with the ChainOSC device projects:

- add and delete any number of Keys;
- a name and independent global hotkey for every Key;
- OSC Press and Release messages;
- Int, Float, and String OSC values;
- configurable OSC host, UDP port, and address;
- persistent settings using the application WebView's local storage;
- automatic migration of settings from earlier Tauri versions;
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
- English and Japanese UI with a persistent language selector;
- Key actions consolidated into an ellipsis menu;
- terminology aligned with M5ChainOSC and ChainOSCmini.
- test buttons and an activity log.

This directory contains the current supported implementation. Official
ChainOSC for Windows releases are built from this Tauri project.

## Run

```powershell
cd tauri
npm install
npm run tauri dev
```

## License

ChainOSC for Windows is licensed under the [MIT License](../LICENSE).
Third-party notices and source-availability information are provided in
[THIRD_PARTY_NOTICES.md](../THIRD_PARTY_NOTICES.md).
