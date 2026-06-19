"use client";

import { useState } from "react";
import { CheckInList } from "@/components/CheckInList";
import { FilterBar } from "@/components/FilterBar";
import { MoodTrendChart } from "@/components/MoodTrendChart";
import { useCurrentUser } from "@/lib/current-user";
import { useCheckIns, useDashboardStats } from "@/lib/hooks";
import { toQueryFilters } from "@/lib/filters";
import { moodMeta } from "@/lib/mood";
import type { CheckInFilters } from "@/lib/types";

function StatCard({
  label,
  value,
  hint,
  icon,
  accent = "text-gray-900",
}: {
  label: string;
  value: string;
  hint?: string;
  icon: string;
  accent?: string;
}) {
  return (
    <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm transition hover:shadow-md">
      <div className="flex items-center justify-between">
        <p className="text-sm font-medium text-gray-500">{label}</p>
        <span className="text-xl" aria-hidden>
          {icon}
        </span>
      </div>
      <p className={`mt-2 text-3xl font-bold tracking-tight ${accent}`}>{value}</p>
      <p className="mt-1 h-5 text-sm text-gray-400">{hint ?? ""}</p>
    </div>
  );
}

export default function DashboardPage() {
  const { users, currentUser } = useCurrentUser();
  const [filters, setFilters] = useState<CheckInFilters>({ page: 1, pageSize: 10 });
  const queryFilters = toQueryFilters(filters);

  const stats = useDashboardStats(queryFilters);
  const recent = useCheckIns(queryFilters);

  if (currentUser && currentUser.role !== "Manager") {
    return (
      <div className="rounded-xl border border-amber-200 bg-amber-50 p-6 text-amber-800">
        <h1 className="text-lg font-semibold">Managers only</h1>
        <p className="mt-1 text-sm">
          Switch to a manager account (e.g. <strong>Alice Manager</strong>) using the selector in
          the top-right to view the dashboard.
        </p>
      </div>
    );
  }

  const avg = stats.data?.averageMood ?? 0;
  const avgAccent =
    ["text-red-600", "text-orange-600", "text-yellow-600", "text-lime-600", "text-green-600"][
      Math.min(4, Math.max(0, Math.round(avg) - 1))
    ] ?? "text-gray-900";

  return (
    <div className="space-y-6">
      <header className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Team dashboard</h1>
          <p className="text-sm text-gray-500">Mood trends across all employees.</p>
        </div>
        <span className="rounded-full bg-gray-900 px-3 py-1 text-xs font-medium text-white">
          Manager view
        </span>
      </header>

      <FilterBar users={users} filters={filters} onChange={setFilters} />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <StatCard
          label="Total check-ins"
          value={String(stats.data?.totalCheckIns ?? 0)}
          icon="📝"
        />
        <StatCard
          label="Average mood"
          value={avg ? avg.toFixed(2) : "–"}
          accent={avg ? avgAccent : "text-gray-900"}
          icon={avg ? moodMeta(Math.round(avg)).emoji : "🙂"}
          hint={avg ? moodMeta(Math.round(avg)).label : undefined}
        />
        <StatCard
          label="Days tracked"
          value={String(stats.data?.moodOverTime.length ?? 0)}
          icon="📅"
        />
      </div>

      <section className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
        <div className="mb-5 flex flex-wrap items-center justify-between gap-2">
          <h2 className="text-lg font-semibold">Average mood over time</h2>
          <div className="flex items-center gap-3 text-xs text-gray-400">
            <span className="flex items-center gap-1.5">
              <span className="inline-block h-2.5 w-2.5 rounded-sm bg-gradient-to-t from-red-400 to-green-400" />
              daily average
            </span>
            <span className="flex items-center gap-1.5">
              <span className="inline-block h-0 w-4 border-t border-dashed border-blue-400" />
              overall average{avg ? ` (${avg.toFixed(1)})` : ""}
            </span>
          </div>
        </div>
        {stats.isLoading ? (
          <p className="text-sm text-gray-500">Loading…</p>
        ) : stats.error ? (
          <p className="text-sm text-red-600">Could not load stats. Is the API running?</p>
        ) : (
          <MoodTrendChart points={stats.data?.moodOverTime ?? []} />
        )}
      </section>

      <section>
        <h2 className="mb-3 text-lg font-semibold">All check-ins</h2>
        {recent.isLoading ? (
          <p className="text-sm text-gray-500">Loading…</p>
        ) : (
          <CheckInList items={recent.data?.items ?? []} showUser />
        )}
      </section>
    </div>
  );
}
