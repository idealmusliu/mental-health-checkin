import { moodMeta } from "@/lib/mood";

export function MoodBadge({ mood }: { mood: number }) {
  const meta = moodMeta(mood);
  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-sm font-medium ${meta.badgeClass}`}
    >
      <span aria-hidden>{meta.emoji}</span>
      <span>
        {meta.label} ({meta.value})
      </span>
    </span>
  );
}
