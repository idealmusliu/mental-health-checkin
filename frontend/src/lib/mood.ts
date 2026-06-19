export interface MoodMeta {
  value: number;
  label: string;
  emoji: string;
  badgeClass: string;
}

export const MOODS: MoodMeta[] = [
  { value: 1, label: "Very low", emoji: "😞", badgeClass: "bg-red-100 text-red-800 border-red-200" },
  { value: 2, label: "Low", emoji: "🙁", badgeClass: "bg-orange-100 text-orange-800 border-orange-200" },
  { value: 3, label: "Okay", emoji: "😐", badgeClass: "bg-yellow-100 text-yellow-800 border-yellow-200" },
  { value: 4, label: "Good", emoji: "🙂", badgeClass: "bg-lime-100 text-lime-800 border-lime-200" },
  { value: 5, label: "Great", emoji: "😄", badgeClass: "bg-green-100 text-green-800 border-green-200" },
];

export function moodMeta(value: number): MoodMeta {
  return MOODS.find((m) => m.value === value) ?? MOODS[2];
}

export function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}

export function formatDateTime(iso: string): string {
  return new Date(iso).toLocaleString(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}
