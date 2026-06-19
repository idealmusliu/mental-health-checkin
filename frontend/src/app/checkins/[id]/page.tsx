"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { useState } from "react";
import { useSWRConfig } from "swr";
import { CheckInForm } from "@/components/CheckInForm";
import { MoodBadge } from "@/components/MoodBadge";
import { api, ApiError } from "@/lib/api";
import { useCurrentUser } from "@/lib/current-user";
import { useCheckIn, revalidateCheckInData } from "@/lib/hooks";
import { formatDateTime } from "@/lib/mood";
import type { CheckInInput } from "@/lib/types";

export default function CheckInDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { auth, currentUser } = useCurrentUser();
  const { mutate: globalMutate } = useSWRConfig();
  const { data: checkIn, isLoading, error, mutate } = useCheckIn(id);

  const [editing, setEditing] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);

  // Only the employee who owns the check-in may edit it; managers are read-only.
  const canEdit = !!currentUser && !!checkIn && currentUser.id === checkIn.userId;

  const handleUpdate = async (input: CheckInInput) => {
    setSubmitting(true);
    setSaveError(null);
    try {
      const updated = await api.updateCheckIn(id, input, auth);
      await mutate(updated, { revalidate: false });
      await revalidateCheckInData(globalMutate);
      setEditing(false);
    } catch (e) {
      setSaveError(e instanceof ApiError ? e.message : "Could not save changes.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      <Link href="/checkins" className="text-sm text-blue-600 hover:underline">
        ← Back to check-ins
      </Link>

      {isLoading ? (
        <p className="text-sm text-gray-500">Loading…</p>
      ) : error ? (
        <p className="text-sm text-red-600">Check-in not found.</p>
      ) : checkIn ? (
        <article className="space-y-6 rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
          <div className="flex items-start justify-between gap-4">
            <div>
              <h1 className="text-xl font-bold">{checkIn.userName}</h1>
              <p className="text-sm text-gray-400">
                {formatDateTime(checkIn.createdAt)}
                {checkIn.updatedAt && ` · edited ${formatDateTime(checkIn.updatedAt)}`}
              </p>
            </div>
            {!editing && <MoodBadge mood={checkIn.mood} />}
          </div>

          {editing ? (
            <CheckInForm
              initialMood={checkIn.mood}
              initialNotes={checkIn.notes ?? ""}
              onSubmit={handleUpdate}
              submitting={submitting}
              submitLabel="Save changes"
              error={saveError}
            />
          ) : (
            <>
              <p className="whitespace-pre-wrap text-gray-700">
                {checkIn.notes ?? <span className="italic text-gray-400">No notes.</span>}
              </p>
              {canEdit && (
                <button
                  type="button"
                  onClick={() => setEditing(true)}
                  className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
                >
                  Edit
                </button>
              )}
            </>
          )}
        </article>
      ) : null}
    </div>
  );
}
