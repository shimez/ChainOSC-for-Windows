export type PresetMessage = { address: string; value: string; type: number };
export type DevicePreset = {
  format: string;
  schemaVersion: number;
  deviceType: number;
  deviceTypeName: string;
  key: {
    mode: number;
    press: PresetMessage[];
    release: PresetMessage[];
    sequence: { address: string; type: number; start: number; end: number; step: number };
  };
};

export type PresetErrorCode =
  | "E_PRESET_FILE_EMPTY"
  | "E_PRESET_FILE_TOO_LARGE"
  | "E_PRESET_JSON_MALFORMED"
  | "E_PRESET_FORMAT_INVALID"
  | "E_PRESET_SCHEMA_UNSUPPORTED"
  | "E_PRESET_DEVICE_TYPE_UNSUPPORTED"
  | "E_PRESET_DEVICE_TYPE_MISMATCH"
  | "E_PRESET_REQUIRED_FIELD_MISSING"
  | "E_PRESET_FIELD_TYPE_INVALID"
  | "E_OSC_ADDRESS_INVALID"
  | "E_OSC_ADDRESS_TOO_LONG"
  | "E_OSC_VALUE_TOO_LONG"
  | "E_OSC_TYPE_INVALID"
  | "E_OSC_INT32_INVALID"
  | "E_OSC_FLOAT32_INVALID"
  | "E_OSC_MESSAGE_COUNT_EXCEEDED"
  | "E_SEQUENCE_REQUIRED_FIELD_MISSING"
  | "E_SEQUENCE_VALUE_INVALID"
  | "E_SEQUENCE_STEP_ZERO"
  | "E_SEQUENCE_DIRECTION_INVALID"
  | "E_PRESET_DEVICE_SETTING_INVALID"
  | "E_PRESET_STORAGE_WRITE_FAILED";

export type PresetErrorContext = {
  deviceType?: string;
  section?: string;
  messageIndex?: number;
  field?: string;
  expectedDeviceType?: string;
  actualDeviceType?: string;
  limit?: string;
};

type Language = "en" | "ja";
type CatalogEntry = { en: string; ja: string };

const catalog: Record<PresetErrorCode, CatalogEntry> = {
  E_PRESET_FILE_EMPTY: {
    en: "The preset file is empty. Select a Device Preset JSON file that contains data.",
    ja: "プリセットファイルが空です。内容を含むDevice Preset JSONファイルを選択してください。",
  },
  E_PRESET_FILE_TOO_LARGE: {
    en: "The preset file exceeds 16 KiB. Select a Device Preset JSON file no larger than 16 KiB.",
    ja: "プリセットファイルが16 KiBを超えています。16 KiB以内のDevice Preset JSONファイルを選択してください。",
  },
  E_PRESET_JSON_MALFORMED: {
    en: "The JSON syntax is invalid. Check brackets, quotation marks, commas, and other JSON syntax.",
    ja: "JSONの構文が正しくありません。括弧、引用符、カンマなどを確認してください。",
  },
  E_PRESET_FORMAT_INVALID: {
    en: "This is not a supported ChainOSC Device Preset. Confirm that `format` is `ChainOSC-device-preset`.",
    ja: "対応するChainOSC Device Presetではありません。`format`が`ChainOSC-device-preset`であることを確認してください。",
  },
  E_PRESET_SCHEMA_UNSUPPORTED: {
    en: "The preset `schemaVersion` is missing or unsupported. Use a preset exported by a compatible product version.",
    ja: "プリセットの`schemaVersion`がないか、対応していません。対応するバージョンの製品からエクスポートしたプリセットを使用してください。",
  },
  E_PRESET_DEVICE_TYPE_UNSUPPORTED: {
    en: "The preset device type is missing or unsupported. Use a preset for a supported ChainOSC device.",
    ja: "プリセットのデバイス種類がないか、対応していません。対応するChainOSCデバイスのプリセットを使用してください。",
  },
  E_PRESET_DEVICE_TYPE_MISMATCH: {
    en: "The preset device type does not match the import target. Select a preset for the same device type.",
    ja: "プリセットのデバイス種類がインポート先と一致しません。選択したデバイスと同じ種類のプリセットを使用してください。",
  },
  E_PRESET_REQUIRED_FIELD_MISSING: {
    en: "A required preset field is missing. Use a file that contains all fields required by Device Preset v1.",
    ja: "プリセットに必須項目がありません。Device Preset v1の必須項目を含むファイルを使用してください。",
  },
  E_PRESET_FIELD_TYPE_INVALID: {
    en: "A preset field has an invalid JSON type. Use the JSON type defined by Device Preset v1.",
    ja: "プリセット項目の型が正しくありません。Device Preset v1で定義されたJSON型を使用してください。",
  },
  E_OSC_ADDRESS_INVALID: {
    en: "OSC Address must start with `/` and must not contain whitespace or `# * , ? [ ] { }`.",
    ja: "OSC Addressは「/」から始め、空白および`# * , ? [ ] { }`を含めないでください。",
  },
  E_OSC_ADDRESS_TOO_LONG: {
    en: "OSC Address is too long. Keep it within 192 bytes in UTF-8.",
    ja: "OSC Addressが長すぎます。UTF-8で192バイト以内にしてください。",
  },
  E_OSC_VALUE_TOO_LONG: {
    en: "OSC Value is too long. Keep it within 128 bytes in UTF-8.",
    ja: "OSC Valueが長すぎます。UTF-8で128バイト以内にしてください。",
  },
  E_OSC_TYPE_INVALID: {
    en: "OSC Type is invalid. Select Float, Int, or String as allowed for this field.",
    ja: "OSC Typeが正しくありません。この項目で使用できるFloat、Int、Stringのいずれかを指定してください。",
  },
  E_OSC_INT32_INVALID: {
    en: "The Int value is invalid. Specify a decimal integer from `-2147483648` to `2147483647`.",
    ja: "Int値が正しくありません。`-2147483648`～`2147483647`の範囲の10進整数を指定してください。",
  },
  E_OSC_FLOAT32_INVALID: {
    en: "The Float value is invalid. Specify a decimal number representable as a finite OSC float32.",
    ja: "Float値が正しくありません。有限のOSC float32として表現できる10進数を指定してください。",
  },
  E_OSC_MESSAGE_COUNT_EXCEEDED: {
    en: "Press and Release OSC messages must total 8 or fewer.",
    ja: "PressとReleaseのOSCメッセージは、合計8件以内にしてください。",
  },
  E_SEQUENCE_REQUIRED_FIELD_MISSING: {
    en: "A required Sequence field is missing. Specify `address`, `type`, `start`, `end`, and `step`.",
    ja: "Sequenceの必須項目がありません。`address`、`type`、`start`、`end`、`step`を指定してください。",
  },
  E_SEQUENCE_VALUE_INVALID: {
    en: "A Sequence number is invalid. Specify finite numbers for Start, End, and Step.",
    ja: "Sequenceの数値が正しくありません。Start、End、Stepには有限の数値を指定してください。",
  },
  E_SEQUENCE_STEP_ZERO: {
    en: "Sequence Step must not be zero. Specify a non-zero value that moves from Start toward End.",
    ja: "SequenceのStepには0を指定できません。StartからEndへ進む0以外の値を指定してください。",
  },
  E_SEQUENCE_DIRECTION_INVALID: {
    en: "Sequence direction is invalid. Use a positive Step when Start is below End and a negative Step when Start is above End.",
    ja: "Sequenceの進行方向が正しくありません。StartがEndより小さい場合は正のStep、大きい場合は負のStepを指定してください。",
  },
  E_PRESET_DEVICE_SETTING_INVALID: {
    en: "A device setting is invalid. Check the allowed value range and type for the target device.",
    ja: "デバイス設定値が正しくありません。対象デバイスで使用できる値の範囲と型を確認してください。",
  },
  E_PRESET_STORAGE_WRITE_FAILED: {
    en: "The preset could not be written to storage. Existing settings were not changed. Check available storage and try again.",
    ja: "プリセットをストレージへ書き込めませんでした。既存の設定は変更されていません。空き容量を確認してから再試行してください。",
  },
};

export class PresetValidationError extends Error {
  constructor(public readonly code: PresetErrorCode, public readonly context: PresetErrorContext = {}) {
    super(code);
    this.name = "PresetValidationError";
  }
}

const utf8Encoder = new TextEncoder();
const supportedDeviceTypes = new Set([1, 2, 3, 4, 5]);
const allowedOscTypes = new Set([0, 1, 2]);
const int32Minimum = -2147483648n;
const int32Maximum = 2147483647n;
const decimalFloatPattern = /^[+-]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][+-]?\d+)?$/;
const forbiddenOscAddressPattern = /[\s#*,?\[\]{}]/;

const isObject = (value: unknown): value is Record<string, unknown> => typeof value === "object" && value !== null && !Array.isArray(value);
const has = (value: Record<string, unknown>, field: string): boolean => Object.prototype.hasOwnProperty.call(value, field);
function fail(code: PresetErrorCode, context: PresetErrorContext = {}): never { throw new PresetValidationError(code, context); }

function requireField(value: Record<string, unknown>, field: string, context: PresetErrorContext): unknown {
  if (!has(value, field)) fail("E_PRESET_REQUIRED_FIELD_MISSING", { ...context, field });
  return value[field];
}

function validateAddress(address: string, context: PresetErrorContext): void {
  if (!address.startsWith("/") || forbiddenOscAddressPattern.test(address)) fail("E_OSC_ADDRESS_INVALID", context);
  if (utf8Encoder.encode(address).length > 192) fail("E_OSC_ADDRESS_TOO_LONG", { ...context, limit: "192 bytes" });
}

function validateOscValue(value: string, type: number, context: PresetErrorContext): void {
  if (utf8Encoder.encode(value).length > 128) fail("E_OSC_VALUE_TOO_LONG", { ...context, limit: "128 bytes" });
  if (type === 1) {
    if (!/^[+-]?\d+$/.test(value.trim())) fail("E_OSC_INT32_INVALID", context);
    const parsed = BigInt(value.trim());
    if (parsed < int32Minimum || parsed > int32Maximum) fail("E_OSC_INT32_INVALID", context);
  }
  if (type === 0) {
    const normalized = value.trim();
    if (!decimalFloatPattern.test(normalized)) fail("E_OSC_FLOAT32_INVALID", context);
    const parsed = Number(normalized);
    if (!Number.isFinite(parsed) || !Number.isFinite(Math.fround(parsed))) fail("E_OSC_FLOAT32_INVALID", context);
  }
}

function validateMessage(value: unknown, section: "press" | "release", index: number): PresetMessage {
  const base = { deviceType: "Key", section, messageIndex: index };
  if (!isObject(value)) fail("E_PRESET_FIELD_TYPE_INVALID", base);
  for (const field of ["address", "value", "type"]) if (!has(value, field)) fail("E_PRESET_REQUIRED_FIELD_MISSING", { ...base, field });
  if (typeof value.address !== "string") fail("E_PRESET_FIELD_TYPE_INVALID", { ...base, field: "address" });
  if (typeof value.value !== "string") fail("E_PRESET_FIELD_TYPE_INVALID", { ...base, field: "value" });
  if (typeof value.type !== "number" || !Number.isInteger(value.type) || !allowedOscTypes.has(value.type)) fail("E_OSC_TYPE_INVALID", { ...base, field: "type" });
  validateAddress(value.address, { ...base, field: "address" });
  validateOscValue(value.value, value.type, { ...base, field: "value" });
  return { address: value.address, value: value.value, type: value.type };
}

function validateSequence(value: unknown, legacy: boolean): DevicePreset["key"]["sequence"] {
  const base = { deviceType: "Key", section: "sequence" };
  if (!isObject(value)) fail("E_PRESET_FIELD_TYPE_INVALID", base);
  for (const field of ["address", "type", "start", "end", "step"]) if (!has(value, field)) fail("E_SEQUENCE_REQUIRED_FIELD_MISSING", { ...base, field });
  if (typeof value.address !== "string") fail("E_PRESET_FIELD_TYPE_INVALID", { ...base, field: "address" });
  if (typeof value.type !== "number" || !Number.isInteger(value.type)) fail("E_OSC_TYPE_INVALID", { ...base, field: "type" });
  const type = allowedOscTypes.has(value.type) ? value.type : legacy ? 0 : fail("E_OSC_TYPE_INVALID", { ...base, field: "type" });
  validateAddress(value.address, { ...base, field: "address" });
  for (const field of ["start", "end", "step"] as const) if (typeof value[field] !== "number" || !Number.isFinite(value[field])) fail("E_SEQUENCE_VALUE_INVALID", { ...base, field });
  const start = value.start as number;
  const end = value.end as number;
  const step = value.step as number;
  if (step === 0) fail("E_SEQUENCE_STEP_ZERO", { ...base, field: "step" });
  if ((start < end && step < 0) || (start > end && step > 0)) fail("E_SEQUENCE_DIRECTION_INVALID", { ...base, field: "step" });
  return { address: value.address, type, start, end, step };
}

export function validateKeyPreset(value: unknown): DevicePreset {
  if (!isObject(value) || !has(value, "format") || typeof value.format !== "string" || !["ChainOSC-device-preset", "M5ChainOSC-device-preset"].includes(value.format)) fail("E_PRESET_FORMAT_INVALID", { field: "format" });
  if (!has(value, "schemaVersion") || typeof value.schemaVersion !== "number" || !Number.isInteger(value.schemaVersion) || value.schemaVersion !== 1) fail("E_PRESET_SCHEMA_UNSUPPORTED", { field: "schemaVersion" });
  if (!has(value, "deviceType") || typeof value.deviceType !== "number" || !Number.isInteger(value.deviceType) || !supportedDeviceTypes.has(value.deviceType)) fail("E_PRESET_DEVICE_TYPE_UNSUPPORTED", { field: "deviceType", actualDeviceType: String(value.deviceType ?? "missing") });
  if (value.deviceType !== 3) fail("E_PRESET_DEVICE_TYPE_MISMATCH", { expectedDeviceType: "Key", actualDeviceType: String(value.deviceType) });
  const deviceTypeName = requireField(value, "deviceTypeName", { deviceType: "Key" });
  if (typeof deviceTypeName !== "string") fail("E_PRESET_FIELD_TYPE_INVALID", { deviceType: "Key", field: "deviceTypeName" });
  const keyValue = requireField(value, "key", { deviceType: "Key" });
  if (!isObject(keyValue)) fail("E_PRESET_FIELD_TYPE_INVALID", { deviceType: "Key", field: "key" });
  const mode = requireField(keyValue, "mode", { deviceType: "Key" });
  const pressValue = requireField(keyValue, "press", { deviceType: "Key" });
  const releaseValue = requireField(keyValue, "release", { deviceType: "Key" });
  const sequenceValue = requireField(keyValue, "sequence", { deviceType: "Key" });
  if (typeof mode !== "number" || !Number.isInteger(mode)) fail("E_PRESET_FIELD_TYPE_INVALID", { deviceType: "Key", field: "mode" });
  if (!Array.isArray(pressValue)) fail("E_PRESET_FIELD_TYPE_INVALID", { deviceType: "Key", section: "press", field: "press" });
  if (!Array.isArray(releaseValue)) fail("E_PRESET_FIELD_TYPE_INVALID", { deviceType: "Key", section: "release", field: "release" });
  if (!isObject(sequenceValue)) fail("E_PRESET_FIELD_TYPE_INVALID", { deviceType: "Key", section: "sequence", field: "sequence" });
  if (pressValue.length + releaseValue.length > 8) fail("E_OSC_MESSAGE_COUNT_EXCEEDED", { deviceType: "Key", limit: "8 messages" });
  const press = pressValue.map((message, index) => validateMessage(message, "press", index + 1));
  const release = releaseValue.map((message, index) => validateMessage(message, "release", index + 1));
  const sequence = validateSequence(sequenceValue, value.format === "M5ChainOSC-device-preset");
  if (![0, 1].includes(mode)) fail("E_PRESET_DEVICE_SETTING_INVALID", { deviceType: "Key", field: "mode", limit: "0 or 1" });
  return { format: value.format, schemaVersion: 1, deviceType: 3, deviceTypeName, key: { mode, press, release, sequence } };
}

export function parseKeyPreset(text: string, fileBytes: number): DevicePreset {
  if (fileBytes === 0 || text.trim().length === 0) fail("E_PRESET_FILE_EMPTY");
  if (fileBytes > 16 * 1024) fail("E_PRESET_FILE_TOO_LARGE", { limit: "16 KiB" });
  let value: unknown;
  try { value = JSON.parse(text); } catch { fail("E_PRESET_JSON_MALFORMED"); }
  return validateKeyPreset(value);
}

export function formatPresetError(error: unknown, language: Language): string {
  if (!(error instanceof PresetValidationError)) return error instanceof Error ? error.message : String(error);
  const entry = catalog[error.code];
  const contextParts: string[] = [];
  if (error.context.deviceType) contextParts.push(error.context.deviceType);
  if (error.context.section) contextParts.push(error.context.section[0].toUpperCase() + error.context.section.slice(1));
  if (error.context.messageIndex) contextParts.push(`Message ${error.context.messageIndex}`);
  if (error.context.field) contextParts.push(error.context.field);
  if (error.context.expectedDeviceType) contextParts.push(`Expected ${error.context.expectedDeviceType}`);
  if (error.context.actualDeviceType) contextParts.push(`Actual ${error.context.actualDeviceType}`);
  if (error.context.limit) contextParts.push(`Limit ${error.context.limit}`);
  return `${error.code}: ${entry[language]}${contextParts.length ? `\nContext: ${contextParts.join(" / ")}` : ""}`;
}
