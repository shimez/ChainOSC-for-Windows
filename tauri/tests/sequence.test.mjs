import test from "node:test";
import assert from "node:assert/strict";
import { advanceSequence } from "../src/sequence.ts";
import { validateKeyPreset, PresetValidationError } from "../src/preset-validation.ts";

// Matches the shared ChainOSC Device Preset v3 runtime vectors.
for (const [name, mode, start, end, step, expected] of [
  ["ascending exact", 1, 0, 3, 1, [0, 1, 2, 3, 2, 1, 0, 1]],
  ["ascending endpoint clamp", 1, 0, 10, 3, [0, 3, 6, 9, 10, 7, 4, 1, 0, 3]],
  ["descending endpoint clamp", 1, 10, 0, -3, [10, 7, 4, 1, 0, 3, 6, 9, 10, 7]],
  ["step exceeds range", 1, 0, 10, 20, [0, 10, 0, 10]],
  ["equal endpoints", 1, 5, 5, 1, [5, 5, 5, 5]],
  ["legacy loop", 0, 0, 10, 3, [0, 3, 6, 9, 0, 3]],
]) {
  test(name, () => {
    const sequence = { start, end, step, progressionMode: mode };
    let runtime = { current: start, direction: "forward" };
    const actual = [];
    for (const _ of expected) {
      actual.push(runtime.current);
      runtime = advanceSequence(sequence, runtime);
    }
    assert.deepEqual(actual, expected);
  });
}

const keyPreset = (schemaVersion, progressionMode) => ({
  format: "ChainOSC-device-preset", schemaVersion, deviceType: 3, deviceTypeName: "Key",
  key: { mode: 1, press: [], release: [], sequence: {
    address: "/test/sequence", type: 1, start: 0, end: 3, step: 1,
    ...(progressionMode === undefined ? {} : { progressionMode }),
  } },
});

test("v1 and v3 without progressionMode import as Loop; v3 Ping-Pong survives validation", () => {
  assert.equal(validateKeyPreset(keyPreset(1)).key.sequence.progressionMode, 0);
  assert.equal(validateKeyPreset(keyPreset(3)).key.sequence.progressionMode, 0);
  assert.equal(validateKeyPreset(keyPreset(3, 1)).key.sequence.progressionMode, 1);
  assert.equal(validateKeyPreset(keyPreset(3, 1)).schemaVersion, 3);
});

test("invalid progressionMode is rejected instead of silently becoming Loop", () => {
  for (const mode of [-1, 2, "1", null]) {
    assert.throws(() => validateKeyPreset(keyPreset(3, mode)),
      (error) => error instanceof PresetValidationError &&
        error.code === "E_PRESET_DEVICE_SETTING_INVALID" &&
        error.context.field === "progressionMode");
  }
});

test("legacy v1 preset remains supported, legacy v3 and unsupported v2 are rejected", () => {
  const legacy = { ...keyPreset(1), format: "M5ChainOSC-device-preset" };
  assert.equal(validateKeyPreset(legacy).key.sequence.progressionMode, 0);
  assert.throws(() => validateKeyPreset({ ...legacy, schemaVersion: 3 }),
    (error) => error.code === "E_PRESET_SCHEMA_UNSUPPORTED");
  assert.throws(() => validateKeyPreset(keyPreset(2)),
    (error) => error.code === "E_PRESET_SCHEMA_UNSUPPORTED");
});
