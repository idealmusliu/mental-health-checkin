import { formatDate } from "@/lib/mood";
import type { MoodTrendPoint } from "@/lib/types";

const MOOD_COLORS = ["#ef4444", "#f97316", "#eab308", "#84cc16", "#22c55e"];
const colorFor = (avg: number) => MOOD_COLORS[Math.min(4, Math.max(0, Math.round(avg) - 1))];

// Headroom at the top so the value label above a mood-5 bar isn't clipped.
const HEADROOM = 8;
const SCALE = 100 - HEADROOM;
const PLOT_HEIGHT = 224;

const topForMood = (mood: number) => HEADROOM + (1 - mood / 5) * SCALE;
const heightForMood = (mood: number) => (mood / 5) * SCALE;

export function MoodTrendChart({ points }: { points: MoodTrendPoint[] }) {
  if (points.length === 0) {
    return (
      <div className="flex h-56 items-center justify-center rounded-lg border border-dashed border-gray-200 text-sm text-gray-400">
        No data to chart yet.
      </div>
    );
  }

  const totalCount = points.reduce((sum, p) => sum + p.count, 0);
  const overallAvg =
    totalCount > 0 ? points.reduce((sum, p) => sum + p.averageMood * p.count, 0) / totalCount : 0;

  const gridMoods = [5, 4, 3, 2, 1];

  return (
    <div className="flex gap-2">
      <div className="relative w-4 shrink-0" style={{ height: PLOT_HEIGHT }}>
        {gridMoods.map((m) => (
          <span
            key={m}
            className="absolute right-0 -translate-y-1/2 text-[10px] tabular-nums text-gray-400"
            style={{ top: `${topForMood(m)}%` }}
          >
            {m}
          </span>
        ))}
      </div>

      <div className="flex-1 overflow-x-auto">
        <div style={{ minWidth: `${points.length * 36}px` }}>
          <div className="relative" style={{ height: PLOT_HEIGHT }}>
            {gridMoods.map((m) => (
              <div
                key={m}
                className="absolute inset-x-0 border-t border-gray-100"
                style={{ top: `${topForMood(m)}%` }}
              />
            ))}

            {overallAvg > 0 && (
              <div
                className="pointer-events-none absolute inset-x-0 border-t border-dashed border-blue-400/70"
                style={{ top: `${topForMood(overallAvg)}%` }}
              />
            )}

            <div className="absolute inset-0 flex items-end gap-2">
              {points.map((p) => (
                <div
                  key={p.date}
                  className="group flex h-full min-w-[2rem] flex-1 items-end justify-center"
                >
                  <div
                    className="relative w-full max-w-[2.25rem] rounded-t-md shadow-sm transition-all duration-200 group-hover:brightness-105"
                    style={{
                      height: `${heightForMood(p.averageMood)}%`,
                      background: `linear-gradient(to top, ${colorFor(p.averageMood)}b3, ${colorFor(
                        p.averageMood,
                      )})`,
                    }}
                    title={`${formatDate(p.date)} · avg ${p.averageMood.toFixed(2)} · ${p.count} check-in${
                      p.count === 1 ? "" : "s"
                    }`}
                  >
                    <span className="absolute inset-x-0 -top-5 text-center text-[11px] font-semibold text-gray-600">
                      {p.averageMood.toFixed(1)}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="mt-2 flex gap-2">
            {points.map((p) => (
              <span
                key={p.date}
                className="min-w-[2rem] flex-1 text-center text-[10px] text-gray-400"
              >
                {formatDate(p.date)}
              </span>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
