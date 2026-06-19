"use client";

import { useCurrentUser } from "@/lib/current-user";

// Auth stand-in: pick which seeded user you're "logged in" as; drives the API headers.
export function UserSwitcher() {
  const { users, currentUser, setCurrentUserId } = useCurrentUser();

  return (
    <label className="flex items-center gap-2 text-sm">
      <span className="hidden text-gray-300 sm:inline">Logged in as</span>
      <select
        aria-label="Current user"
        value={currentUser?.id ?? ""}
        onChange={(e) => setCurrentUserId(e.target.value)}
        className="rounded-md border border-gray-600 bg-gray-800 px-2 py-1.5 text-sm text-white"
      >
        {users.map((u) => (
          <option key={u.id} value={u.id}>
            {u.name} · {u.role}
          </option>
        ))}
      </select>
    </label>
  );
}
