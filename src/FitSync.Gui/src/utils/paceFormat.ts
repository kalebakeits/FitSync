export function secondsToPaceDisplay(seconds: number): string {
  const mins = Math.floor(seconds / 60);
  const secs = seconds % 60;
  return `${mins}:${secs.toString().padStart(2, "0")}`;
}

export function paceDisplayToSeconds(display: string): number {
  const parts = display.split(":");
  const mins = parseInt(parts[0]) || 0;
  const secs = parseInt(parts[1]) || 0;
  return mins * 60 + secs;
}
