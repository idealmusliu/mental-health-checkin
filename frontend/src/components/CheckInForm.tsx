"use client";

import { useState, type FormEvent } from "react";
import { MoodSelect } from "./MoodSelect";
import type { CheckInInput } from "@/lib/types";

const MAX_NOTES = 1000;

interface CheckInFormProps {
  onSubmit: (input: CheckInInput) => void | Promise<void>;
  initialMood?: number | null;
  initialNotes?: string;
  submitting?: boolean;
  submitLabel?: string;
  error?: string | null;
}

export function CheckInForm({
  onSubmit,
  initialMood = null,
  initialNotes = "",
  submitting = false,
  submitLabel = "Submit check-in",
  error = null,
}: CheckInFormProps) {
  const [mood, setMood] = useState<number | null>(initialMood);
  const [notes, setNotes] = useState(initialNotes);
  const [validationError, setValidationError] = useState<string | null>(null);

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();

    if (mood === null) {
      setValidationError("Please select a mood from 1 to 5.");
      return;
    }
    if (notes.length > MAX_NOTES) {
      setValidationError(`Notes cannot exceed ${MAX_NOTES} characters.`);
      return;
    }

    setValidationError(null);
    void onSubmit({ mood, notes: notes.trim() === "" ? null : notes.trim() });
  };

  const message = validationError ?? error;

  return (
    <form onSubmit={handleSubmit} className="space-y-5" noValidate>
      <div>
        <label className="mb-2 block text-sm font-medium text-gray-700">
          How are you feeling today?
        </label>
        <MoodSelect value={mood} onChange={setMood} />
      </div>

      <div>
        <label htmlFor="notes" className="mb-2 block text-sm font-medium text-gray-700">
          Notes <span className="font-normal text-gray-400">(optional)</span>
        </label>
        <textarea
          id="notes"
          value={notes}
          onChange={(e) => setNotes(e.target.value)}
          rows={4}
          maxLength={MAX_NOTES}
          placeholder="Anything you'd like to add…"
          className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200"
        />
        <p className="mt-1 text-right text-xs text-gray-400">
          {notes.length}/{MAX_NOTES}
        </p>
      </div>

      {message && (
        <p role="alert" className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">
          {message}
        </p>
      )}

      <button
        type="submit"
        disabled={submitting}
        className="w-full rounded-lg bg-blue-600 px-4 py-2.5 font-medium text-white transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-60"
      >
        {submitting ? "Saving…" : submitLabel}
      </button>
    </form>
  );
}
