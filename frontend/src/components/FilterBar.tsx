"use client";

import type { CheckInFilters, User } from "@/lib/types";

interface FilterBarProps {
  users: User[];
  filters: CheckInFilters;
  onChange: (filters: CheckInFilters) => void;
  showUserFilter?: boolean;
}

export function FilterBar({ users, filters, onChange, showUserFilter = true }: FilterBarProps) {
  // Any filter change resets to page 1.
  const update = (patch: Partial<CheckInFilters>) =>
    onChange({ ...filters, ...patch, page: 1 });

  return (
    <div className="flex flex-wrap items-end gap-3 rounded-lg border border-gray-200 bg-white p-4">
      {showUserFilter && (
        <label className="flex flex-col gap-1 text-sm">
          <span className="font-medium text-gray-600">User</span>
          <select
            value={filters.userId ?? ""}
            onChange={(e) => update({ userId: e.target.value || undefined })}
            className="rounded-md border border-gray-300 px-2 py-1.5 text-sm"
          >
            <option value="">All users</option>
            {users.map((u) => (
              <option key={u.id} value={u.id}>
                {u.name}
              </option>
            ))}
          </select>
        </label>
      )}

      <label className="flex flex-col gap-1 text-sm">
        <span className="font-medium text-gray-600">From</span>
        <input
          type="date"
          value={filters.from ?? ""}
          onChange={(e) => update({ from: e.target.value || undefined })}
          className="rounded-md border border-gray-300 px-2 py-1.5 text-sm"
        />
      </label>

      <label className="flex flex-col gap-1 text-sm">
        <span className="font-medium text-gray-600">To</span>
        <input
          type="date"
          value={filters.to ?? ""}
          onChange={(e) => update({ to: e.target.value || undefined })}
          className="rounded-md border border-gray-300 px-2 py-1.5 text-sm"
        />
      </label>

      <button
        type="button"
        onClick={() => onChange({ page: 1, pageSize: filters.pageSize })}
        className="rounded-md border border-gray-300 px-3 py-1.5 text-sm font-medium text-gray-600 hover:bg-gray-50"
      >
        Clear
      </button>
    </div>
  );
}
