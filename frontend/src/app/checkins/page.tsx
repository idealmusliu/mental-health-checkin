"use client";

import { useState } from "react";
import { CheckInList } from "@/components/CheckInList";
import { FilterBar } from "@/components/FilterBar";
import { Pagination } from "@/components/Pagination";
import { useCurrentUser } from "@/lib/current-user";
import { useCheckIns } from "@/lib/hooks";
import { toQueryFilters } from "@/lib/filters";
import type { CheckInFilters } from "@/lib/types";

export default function CheckInsPage() {
  const { users, currentUser } = useCurrentUser();
  const isManager = currentUser?.role === "Manager";

  const [filters, setFilters] = useState<CheckInFilters>({ page: 1, pageSize: 10 });
  const { data, isLoading, error } = useCheckIns(toQueryFilters(filters));

  return (
    <div className="space-y-5">
      <header>
        <h1 className="text-2xl font-bold">Check-ins</h1>
        <p className="text-sm text-gray-500">
          {isManager
            ? "Reviewing check-ins across the team."
            : "Your personal check-in history."}
        </p>
      </header>

      <FilterBar
        users={users}
        filters={filters}
        onChange={setFilters}
        showUserFilter={isManager}
      />

      {isLoading ? (
        <p className="text-sm text-gray-500">Loading…</p>
      ) : error ? (
        <p className="text-sm text-red-600">Could not load check-ins. Is the API running?</p>
      ) : (
        <>
          <CheckInList items={data?.items ?? []} showUser={isManager} />
          <Pagination
            page={data?.page ?? 1}
            totalPages={data?.totalPages ?? 1}
            onPageChange={(page) => setFilters((f) => ({ ...f, page }))}
          />
        </>
      )}
    </div>
  );
}
