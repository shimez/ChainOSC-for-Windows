# ChainOSC for Windows — Tauri prototype

## v0.1.0

This prototype verifies the minimum Tauri implementation:

- one configurable global hotkey;
- OSC Press and Release messages;
- Int, Float, and String OSC values;
- configurable OSC host, UDP port, and address;
- persistent settings using the application WebView's local storage;
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
