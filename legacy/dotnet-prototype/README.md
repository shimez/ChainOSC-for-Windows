# Legacy .NET/WPF Prototype

This directory contains the original .NET/WPF prototype of ChainOSC for
Windows. It is retained as development history and reference material.

This prototype is no longer the primary or supported implementation. Current
development and official releases use the Tauri project in [`../../tauri/`](../../tauri/).

The prototype reached version 0.6.0. Its version numbers, settings format, and
build instructions are preserved as they were and should not be interpreted as
the version of the current Tauri application.

## Build the archived prototype

From this directory:

```powershell
dotnet build "ChainOSC.slnx"
```

Do not run the prototype and the current Tauri application at the same time
with identical global hotkeys, because only one application can register a
given Windows global hotkey.

## License

The prototype is covered by the repository's [MIT License](../../LICENSE).
