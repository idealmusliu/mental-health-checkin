import { MOODS } from "@/lib/mood";

interface MoodSelectProps {
  value: number | null;
  onChange: (value: number) => void;
}

export function MoodSelect({ value, onChange }: MoodSelectProps) {
  return (
    <div role="group" aria-label="Mood" className="grid grid-cols-5 gap-2">
      {MOODS.map((mood) => {
        const selected = value === mood.value;
        return (
          <button
            key={mood.value}
            type="button"
            aria-pressed={selected}
            aria-label={`${mood.label} (${mood.value})`}
            onClick={() => onChange(mood.value)}
            className={`flex flex-col items-center gap-1 rounded-lg border-2 px-2 py-3 text-center transition ${
              selected
                ? "border-blue-500 bg-blue-50 ring-2 ring-blue-200"
                : "border-gray-200 hover:border-gray-300 hover:bg-gray-50"
            }`}
          >
            <span className="text-2xl" aria-hidden>
              {mood.emoji}
            </span>
            <span className="text-xs font-medium text-gray-700">{mood.label}</span>
          </button>
        );
      })}
    </div>
  );
}
