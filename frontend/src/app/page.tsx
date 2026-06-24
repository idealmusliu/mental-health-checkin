"use client";

import Link from "next/link";
import { useState } from "react";
import { useSWRConfig } from "swr";
import { CheckInForm } from "@/components/CheckInForm";
import { CheckInList } from "@/components/CheckInList";
import { Loading } from "@/components/Loading";
import { api, ApiError } from "@/lib/api";
import { useCurrentUser } from "@/lib/current-user";
import { useCheckIns, revalidateCheckInData } from "@/lib/hooks";
import type { CheckInInput } from "@/lib/types";

export default function HomePage() {
  const { currentUser, auth, isLoading } = useCurrentUser();
  const { mutate } = useSWRConfig();
  const recent = useCheckIns({ userId: currentUser?.id, page: 1, pageSize: 5 });

  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const handleSubmit = async (input: CheckInInput) => {
    if (!currentUser) return;
    setSubmitting(true);
    setError(null);
    setSuccess(false);
    try {
      await api.createCheckIn(input, auth);
      setSuccess(true);
      await revalidateCheckInData(mutate);
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Something went wrong. Please try again.");
    } finally {
      setSubmitting(false);
    }
  };

  if (isLoading) {
    return <Loading />;
  }

  if (currentUser?.role === "Manager") {
    return (
      <div className="space-y-6">
        <header>
          <h1 className="text-2xl font-bold">Welcome, {currentUser.name}</h1>
          <p className="text-sm text-gray-500">
            As a manager you review and track the team&apos;s check-ins.
          </p>
        </header>
        <div className="grid gap-4 sm:grid-cols-2">
          <Link
            href="/dashboard"
            className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm transition hover:shadow-md"
          >
            <p className="text-lg font-semibold">📊 Team dashboard</p>
            <p className="mt-1 text-sm text-gray-500">Average mood over time and team totals.</p>
          </Link>
          <Link
            href="/checkins"
            className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm transition hover:shadow-md"
          >
            <p className="text-lg font-semibold">📋 All check-ins</p>
            <p className="mt-1 text-sm text-gray-500">Browse and filter everyone&apos;s check-ins.</p>
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-10">
      <section>
        <h1 className="mb-1 text-2xl font-bold">New check-in</h1>
        <p className="mb-6 text-sm text-gray-500">
          Submitting as <span className="font-medium text-gray-700">{currentUser?.name ?? "…"}</span>
        </p>

        <div className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
          {/* key forces a fresh form after submit */}
          <CheckInForm key={success ? "submitted" : "form"} onSubmit={handleSubmit} submitting={submitting} error={error} />
          {success && (
            <p role="status" className="mt-4 rounded-lg bg-green-50 px-3 py-2 text-sm text-green-700">
              ✅ Your check-in has been recorded.
            </p>
          )}
        </div>
      </section>

      <section>
        <h2 className="mb-3 text-lg font-semibold">Your recent check-ins</h2>
        {recent.isLoading ? (
          <p className="text-sm text-gray-500">Loading…</p>
        ) : recent.error ? (
          <p className="text-sm text-red-600">Could not load check-ins. Is the API running?</p>
        ) : (
          <CheckInList items={recent.data?.items ?? []} />
        )}
      </section>
    </div>
  );
}
