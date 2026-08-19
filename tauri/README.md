# ChainOSC for Windows — Tauri prototype

## v0.2.0

This version extends the Tauri prototype with multiple independently configured Keys:

- add and delete any number of Keys;
- a name and independent global hotkey for every Key;
- OSC Press and Release messages;
- Int, Float, and String OSC values;
- configurable OSC host, UDP port, and address;
- persistent settings using the application WebView's local storage;
- automatic migration of the v0.1.0 Key configuration;
- duplicate global-hotkey validation;
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
