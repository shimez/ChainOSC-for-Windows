import { invoke } from "@tauri-apps/api/core";
import { register, unregisterAll, type ShortcutEvent } from "@tauri-apps/plugin-global-shortcut";

type ValueType = "int" | "float" | "string";
type KeySettings = { id: string; name: string; hotkey: string; address: string; valueType: ValueType; pressValue: string; releaseValue: string };
type Settings = { version: "0.2.0"; host: string; port: number; keys: KeySettings[] };
const storageKey = "chainosc-tauri-settings-v2";
const legacyStorageKey = "chainosc-tauri-settings-v1";
let nextKeyNumber = 1;
let recordingHotkeyField: HTMLInputElement | null = null;

const newKey = (): KeySettings => ({ id: crypto.randomUUID(), name: `Key ${nextKeyNumber++}`, hotkey: "", address: "/avatar/parameters/ChainOSCKey", valueType: "int", pressValue: "1", releaseValue: "0" });
const defaults = (): Settings => { const key = newKey(); key.hotkey = "F8"; return { version: "0.2.0", host: "127.0.0.1", port: 9000, keys: [key] }; };
const element = <T extends HTMLElement>(id: string) => document.getElementById(id) as T;
const errorText = (error: unknown) => error instanceof Error ? error.message : String(error);

function log(message: string, level = ""): void {
  const line = document.createElement("div"); line.className = `line ${level}`;
  line.textContent = `${new Date().toLocaleTimeString()}  ${message}`;
  const target = element<HTMLDivElement>("log"); target.append(line); target.scrollTop = target.scrollHeight;
}
function status(message: string, level = ""): void {
  const target = element<HTMLDivElement>("status"); target.textContent = message; target.className = `status ${level}`;
}

function hotkeyFeedback(field: HTMLInputElement, message: string): void {
  const feedback = field.closest("div")?.querySelector(".hotkey-feedback") as HTMLElement | null;
  if (feedback) feedback.textContent = message;
  feedback?.classList.toggle("error", Boolean(message));
}

function explainAssignedHotkey(field: HTMLInputElement, hotkey: string, owner: string): void {
  const message = `${hotkey} is already assigned to ${owner}. Choose a different hotkey.`;
  hotkeyFeedback(field, message);
  status(message, "error");
}
function loadSettings(): Settings {
  try {
    const stored = localStorage.getItem(storageKey);
    if (stored) { const parsed = JSON.parse(stored) as Settings; parsed.version = "0.2.0"; parsed.keys = Array.isArray(parsed.keys) ? parsed.keys : []; return parsed; }
    const legacy = localStorage.getItem(legacyStorageKey);
    if (!legacy) return defaults();
    const old = JSON.parse(legacy);
    return { version: "0.2.0", host: old.host ?? "127.0.0.1", port: old.port ?? 9000, keys: [{ id: crypto.randomUUID(), name: "Key 1", hotkey: old.hotkey ?? "F8", address: old.address ?? "/avatar/parameters/ChainOSCKey", valueType: old.valueType ?? "int", pressValue: old.pressValue ?? "1", releaseValue: old.releaseValue ?? "0" }] };
  } catch (error) { log(`Saved settings could not be loaded: ${errorText(error)}`, "error"); return defaults(); }
}

function shortcutKey(event: KeyboardEvent): string | null {
  if (["Control", "Shift", "Alt", "Meta"].includes(event.key)) return null;
  if (/^Key[A-Z]$/.test(event.code)) return event.code.slice(3);
  if (/^Digit[0-9]$/.test(event.code)) return event.code.slice(5);
  if (/^F([1-9]|1[0-9]|2[0-4])$/.test(event.key)) return event.key;
  const names: Record<string, string> = {
    " ": "Space", Escape: "Escape", Enter: "Enter", Tab: "Tab",
    ArrowUp: "ArrowUp", ArrowDown: "ArrowDown", ArrowLeft: "ArrowLeft", ArrowRight: "ArrowRight",
    Home: "Home", End: "End", PageUp: "PageUp", PageDown: "PageDown",
    Insert: "Insert", Delete: "Delete", Backspace: "Backspace",
  };
  return names[event.key] ?? (event.key.length === 1 ? event.key.toUpperCase() : null);
}

function recordShortcut(event: KeyboardEvent, field: HTMLInputElement): void {
  event.preventDefault();
  event.stopPropagation();
  if ((event.key === "Backspace" || event.key === "Delete") && !event.ctrlKey && !event.altKey && !event.shiftKey && !event.metaKey) {
    field.value = "";
    hotkeyFeedback(field, "");
    return;
  }
  const key = shortcutKey(event);
  if (!key) return;
  const parts: string[] = [];
  if (event.ctrlKey) parts.push("Ctrl");
  if (event.altKey) parts.push("Alt");
  if (event.shiftKey) parts.push("Shift");
  if (event.metaKey) parts.push("Super");
  parts.push(key);
  const shortcut = parts.join("+");
  const duplicate = [...document.querySelectorAll<HTMLInputElement>(".hotkey")]
    .find((candidate) => candidate !== field && candidate.value.toLowerCase() === shortcut.toLowerCase());
  if (duplicate) {
    const owner = (duplicate.closest(".key-card")?.querySelector(".key-name") as HTMLInputElement | null)?.value || "another Key";
    explainAssignedHotkey(field, shortcut, owner);
    return;
  }
  field.value = shortcut;
  hotkeyFeedback(field, "");
  status(`${shortcut} selected. Save Settings to activate it.`);
  field.dispatchEvent(new Event("change", { bubbles: true }));
}

function keyCard(key: KeySettings): HTMLElement {
  const card = document.createElement("section"); card.className = "card key-card"; card.dataset.keyId = key.id;
  card.innerHTML = `<div class="key-heading"><h2></h2><button class="danger delete-key" type="button">Delete Key</button></div><div class="grid two"><div><label>Key Name</label><input class="key-name" maxlength="64"></div><div><label>Global Hotkey</label><input class="hotkey" readonly placeholder="Click here, then press a shortcut"><small>Click the field and press a key combination. Backspace or Delete clears it.</small><small class="hotkey-feedback" aria-live="polite"></small></div><div class="wide"><label>OSC Address</label><input class="address" maxlength="192"></div><div><label>OSC Type</label><select class="value-type"><option value="int">Int</option><option value="float">Float</option><option value="string">String</option></select></div><div></div><div><label>Press Value</label><input class="press-value" maxlength="128"></div><div><label>Release Value</label><input class="release-value" maxlength="128"></div></div><div class="actions"><button class="secondary test-press" type="button">Test Press</button><button class="secondary test-release" type="button">Test Release</button></div>`;
  card.querySelector("h2")!.textContent = key.name;
  for (const [selector, value] of [[".key-name", key.name], [".hotkey", key.hotkey], [".address", key.address], [".press-value", key.pressValue], [".release-value", key.releaseValue]]) (card.querySelector(selector) as HTMLInputElement).value = value;
  (card.querySelector(".value-type") as HTMLSelectElement).value = key.valueType;
  const hotkeyField = card.querySelector(".hotkey") as HTMLInputElement;
  hotkeyField.addEventListener("keydown", (event) => recordShortcut(event, hotkeyField));
  hotkeyField.addEventListener("focus", () => { recordingHotkeyField = hotkeyField; hotkeyField.select(); hotkeyFeedback(hotkeyField, ""); });
  hotkeyField.addEventListener("blur", () => { if (recordingHotkeyField === hotkeyField) recordingHotkeyField = null; });
  card.querySelector(".key-name")!.addEventListener("input", (event) => { card.querySelector("h2")!.textContent = (event.target as HTMLInputElement).value || "Unnamed Key"; });
  card.querySelector(".delete-key")!.addEventListener("click", () => card.remove());
  card.querySelector(".test-press")!.addEventListener("click", () => testCard(card, true));
  card.querySelector(".test-release")!.addEventListener("click", () => testCard(card, false));
  return card;
}
function renderKey(key: KeySettings): void { element("keys").append(keyCard(key)); }
function readKey(card: Element, index: number): KeySettings {
  const value = (selector: string) => (card.querySelector(selector) as HTMLInputElement).value;
  const key: KeySettings = { id: (card as HTMLElement).dataset.keyId || crypto.randomUUID(), name: value(".key-name").trim(), hotkey: value(".hotkey").replace(/\s+/g, "").trim(), address: value(".address").trim(), valueType: (card.querySelector(".value-type") as HTMLSelectElement).value as ValueType, pressValue: value(".press-value"), releaseValue: value(".release-value") };
  if (!key.name) throw new Error(`Key ${index}: Key Name is required.`);
  if (!key.hotkey) throw new Error(`${key.name}: Global Hotkey is required.`);
  if (!key.address.startsWith("/")) throw new Error(`${key.name}: OSC Address must start with '/'.`);
  return key;
}
function readSettings(): Settings {
  const host = element<HTMLInputElement>("host").value.trim(); const port = Number(element<HTMLInputElement>("port").value);
  if (!host) throw new Error("OSC host is required.");
  if (!Number.isInteger(port) || port < 1 || port > 65535) throw new Error("UDP port must be 1–65535.");
  const keys = [...document.querySelectorAll(".key-card")].map((card, index) => readKey(card, index + 1));
  const shortcuts = new Set<string>();
  for (const key of keys) { const normalized = key.hotkey.toLowerCase(); if (shortcuts.has(normalized)) throw new Error(`Global Hotkey ${key.hotkey} is assigned more than once.`); shortcuts.add(normalized); }
  return { version: "0.2.0", host, port, keys };
}
async function send(settings: Settings, key: KeySettings, pressed: boolean): Promise<void> {
  const value = pressed ? key.pressValue : key.releaseValue;
  await invoke<number>("send_osc", { host: settings.host, port: settings.port, address: key.address, valueType: key.valueType, value });
  log(`${key.name} ${pressed ? "PRESSED" : "RELEASED"}: ${key.address} ${key.valueType} ${value}`, "sent");
}
async function testCard(card: HTMLElement, pressed: boolean): Promise<void> {
  try { const settings = readSettings(); const key = settings.keys.find((item) => item.id === card.dataset.keyId)!; await send(settings, key, pressed); }
  catch (error) { log(`OSC send failed: ${errorText(error)}`, "error"); }
}
async function registerHotkeys(settings: Settings): Promise<void> {
  await unregisterAll();
  for (const key of settings.keys) await register(key.hotkey, async (event: ShortcutEvent) => {
    if (recordingHotkeyField) {
      if (event.state === "Pressed") explainAssignedHotkey(recordingHotkeyField, key.hotkey, key.name);
      return;
    }
    try { await send(settings, key, event.state === "Pressed"); } catch (error) { log(`OSC send failed: ${errorText(error)}`, "error"); }
  });
}
async function save(): Promise<void> {
  try { const settings = readSettings(); await registerHotkeys(settings); localStorage.setItem(storageKey, JSON.stringify(settings)); status(`Saved. ${settings.keys.length} global hotkey(s) active.`, "ok"); log(`Settings saved; registered ${settings.keys.length} hotkey(s).`, "ok"); }
  catch (error) { status(errorText(error), "error"); log(`Save failed: ${errorText(error)}`, "error"); }
}
window.addEventListener("DOMContentLoaded", async () => {
  const settings = loadSettings(); nextKeyNumber = settings.keys.length + 1; element<HTMLInputElement>("host").value = settings.host; element<HTMLInputElement>("port").value = String(settings.port); settings.keys.forEach(renderKey);
  element("add-key").addEventListener("click", () => renderKey(newKey())); element("save").addEventListener("click", save);
  try { await registerHotkeys(settings); status(`${settings.keys.length} global hotkey(s) active.`, "ok"); log(`Loaded v0.2.0 settings; registered ${settings.keys.length} hotkey(s).`, "ok"); }
  catch (error) { status(`Hotkey registration failed: ${errorText(error)}`, "error"); log(`Startup failed: ${errorText(error)}`, "error"); }
});
