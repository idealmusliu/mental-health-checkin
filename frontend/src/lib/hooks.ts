"use client";

import useSWR, { type ScopedMutator } from "swr";
import { api } from "./api";
import { useCurrentUser } from "./current-user";
import type { CheckIn, CheckInFilters, DashboardStats, PagedResult } from "./types";

// Keys include the current user id so data refetches when you switch user.
const CHECK_IN_KEYS = ["checkins", "checkin", "dashboard"];

// Minutes to add to UTC for the browser's local time (UTC+2 => 120).
function localTzOffsetMinutes() {
  return -new Date().getTimezoneOffset();
}

export function useCheckIns(filters: CheckInFilters) {
  const { auth } = useCurrentUser();
  const key = auth ? ["checkins", auth.id, JSON.stringify(filters)] : null;
  return useSWR<PagedResult<CheckIn>>(key, () => api.getCheckIns(filters, auth));
}

export function useCheckIn(id: string | undefined) {
  const { auth } = useCurrentUser();
  const key = auth && id ? ["checkin", auth.id, id] : null;
  return useSWR<CheckIn>(key, () => api.getCheckIn(id as string, auth));
}

export function useDashboardStats(filters: CheckInFilters = {}) {
  const { auth } = useCurrentUser();
  const tz = localTzOffsetMinutes();
  const key = auth ? ["dashboard", auth.id, JSON.stringify(filters), tz] : null;
  return useSWR<DashboardStats>(key, () => api.getDashboardStats(filters, tz, auth));
}

// Revalidates all check-in/dashboard queries so changes show up across every view.
export function revalidateCheckInData(mutate: ScopedMutator) {
  return mutate((key) => Array.isArray(key) && CHECK_IN_KEYS.includes(key[0] as string));
}
