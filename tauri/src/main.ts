import { invoke } from "@tauri-apps/api/core";
import {
  register,
  unregisterAll,
  type ShortcutEvent,
} from "@tauri-apps/plugin-global-shortcut";

type ValueType = "int" | "float" | "string";
type Settings = {
  version: "0.1.0";
  host: string;
  port: number;
  hotkey: string;
  address: string;
  valueType: ValueType;
  pressValue: string;
  releaseValue: string;
};

const storageKey = "chainosc-tauri-settings-v1";
const defaults: Settings = {
  version: "0.1.0",
  host: "127.0.0.1",
  port: 9000,
  hotkey: "F8",
  address: "/avatar/parameters/ChainOSCKey",
  valueType: "int",
  pressValue: "1",
  releaseValue: "0",
};

const element = <T extends HTMLElement>(id: string) =>
  document.getElementById(id) as T;
const input = (id: string) => element<HTMLInputElement>(id);
const select = (id: string) => element<HTMLSelectElement>(id);

function errorText(error: unknown): string {
  return error instanceof Error ? error.message : String(error);
}

function log(message: string, level = ""): void {
  const line = document.createElement("div");
  line.className = `line ${level}`;
  line.textContent = `${new Date().toLocaleTimeString()}  ${message}`;
  const target = element<HTMLDivElement>("log");
  target.append(line);
  target.scrollTop = target.scrollHeight;
}

function status(message: string, level = ""): void {
  const target = element<HTMLDivElement>("status");
  target.textContent = message;
  target.className = `status ${level}`;
}

function loadSettings(): Settings {
  try {
    const stored = localStorage.getItem(storageKey);
    if (!stored) return { ...defaults };
    return { ...defaults, ...JSON.parse(stored), version: "0.1.0" };
  } catch (error) {
    log(`Saved settings could not be loaded: ${errorText(error)}`, "error");
    return { ...defaults };
  }
}

function readSettings(): Settings {
  const settings: Settings = {
    version: "0.1.0",
    host: input("host").value.trim(),
    port: Number(input("port").value),
    hotkey: input("hotkey").value.replace(/\s+/g, "").trim(),
    address: input("address").value.trim(),
    valueType: select("value-type").value as ValueType,
    pressValue: input("press-value").value,
    releaseValue: input("release-value").value,
  };
  if (!settings.host) throw new Error("OSC host is required.");
  if (!Number.isInteger(settings.port) || settings.port < 1 || settings.port > 65535)
    throw new Error("UDP port must be 1–65535.");
  if (!settings.hotkey) throw new Error("Global Hotkey is required.");
  if (!settings.address.startsWith("/"))
    throw new Error("OSC Address must start with '/'.");
  return settings;
}

function showSettings(settings: Settings): void {
  input("host").value = settings.host;
  input("port").value = String(settings.port);
  input("hotkey").value = settings.hotkey;
  input("address").value = settings.address;
  select("value-type").value = settings.valueType;
  input("press-value").value = settings.pressValue;
  input("release-value").value = settings.releaseValue;
}

async function send(settings: Settings, pressed: boolean): Promise<void> {
  const value = pressed ? settings.pressValue : settings.releaseValue;
  await invoke<number>("send_osc", {
    host: settings.host,
    port: settings.port,
    address: settings.address,
    valueType: settings.valueType,
    value,
  });
  log(`${pressed ? "PRESSED" : "RELEASED"}: ${settings.address} ${settings.valueType} ${value}`, "sent");
}

async function onShortcut(event: ShortcutEvent): Promise<void> {
  try {
    await send(readSettings(), event.state === "Pressed");
  } catch (error) {
    log(`OSC send failed: ${errorText(error)}`, "error");
  }
}

async function registerCurrentHotkey(settings: Settings): Promise<void> {
  await unregisterAll();
  await register(settings.hotkey, onShortcut);
}

async function save(): Promise<void> {
  try {
    const settings = readSettings();
    await registerCurrentHotkey(settings);
    localStorage.setItem(storageKey, JSON.stringify(settings));
    status(`Saved. Global hotkey ${settings.hotkey} is active.`, "ok");
    log(`Settings saved; registered ${settings.hotkey}.`, "ok");
  } catch (error) {
    status(errorText(error), "error");
    log(`Save failed: ${errorText(error)}`, "error");
  }
}

window.addEventListener("DOMContentLoaded", async () => {
  const settings = loadSettings();
  showSettings(settings);
  element<HTMLButtonElement>("save").addEventListener("click", save);
  element<HTMLButtonElement>("test-press").addEventListener("click", async () => {
    try { await send(readSettings(), true); } catch (error) { log(errorText(error), "error"); }
  });
  element<HTMLButtonElement>("test-release").addEventListener("click", async () => {
    try { await send(readSettings(), false); } catch (error) { log(errorText(error), "error"); }
  });
  try {
    await registerCurrentHotkey(settings);
    status(`Global hotkey ${settings.hotkey} is active.`, "ok");
    log(`Loaded v0.1.0 settings; registered ${settings.hotkey}.`, "ok");
  } catch (error) {
    status(`Hotkey registration failed: ${errorText(error)}`, "error");
    log(`Startup failed: ${errorText(error)}`, "error");
  }
});
