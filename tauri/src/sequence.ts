export type ProgressionMode = 0 | 1; // Loop / Ping-Pong (Device Preset v3)
export type SequenceDirection = "forward" | "backward";
export type SequenceRuntime = { current: number; direction: SequenceDirection };
export type SequenceProgression = { start: number; end: number; step: number; progressionMode: ProgressionMode };

export function advanceSequence(sequence: SequenceProgression, runtime: SequenceRuntime): SequenceRuntime {
  const value = runtime.current;
  if (sequence.progressionMode === 1) {
    if (sequence.start === sequence.end) return runtime;
    if (runtime.direction === "forward") {
      const next = value + sequence.step;
      if (sequence.step > 0 ? next >= sequence.end : next <= sequence.end)
        return { current: sequence.end, direction: "backward" };
      return { current: next, direction: "forward" };
    }
    const next = value - sequence.step;
    if (sequence.step > 0 ? next <= sequence.start : next >= sequence.start)
      return { current: sequence.start, direction: "forward" };
    return { current: next, direction: "backward" };
  }
  const next = value + sequence.step;
  const beyond = sequence.step > 0 ? next > sequence.end : next < sequence.end;
  return { current: beyond ? sequence.start : next, direction: "forward" };
}
