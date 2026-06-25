export function formatDuration(durationType?: number, durationValue?: number, distance?: number): string {
  if (distance !== undefined) return `${distance}m`;
  if (durationValue === undefined) return "Open";
  if (durationType === 0) {
    const mins = Math.round(durationValue / 60000);
    return mins > 0 ? `${mins}min` : `${Math.round(durationValue / 1000)}s`;
  }
  if (durationType === 1) {
    const m = durationValue / 100;
    return m >= 1000 ? `${m / 1000}km` : `${m}m`;
  }
  return "Open";
}

export function formatTotalDuration(ms: number): string {
  if (ms <= 0) return "";
  const totalMins = Math.round(ms / 60000);
  const h = Math.floor(totalMins / 60);
  const m = totalMins % 60;
  if (h === 0) return `${m}min`;
  if (m === 0) return `${h}h`;
  return `${h}h${m}m`;
}
