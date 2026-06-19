import Link from "next/link";
import { MoodBadge } from "./MoodBadge";
import { formatDateTime } from "@/lib/mood";
import type { CheckIn } from "@/lib/types";

interface CheckInListProps {
  items: CheckIn[];
  showUser?: boolean;
}

export function CheckInList({ items, showUser = false }: CheckInListProps) {
  if (items.length === 0) {
    return (
      <p className="rounded-lg border border-dashed border-gray-300 bg-gray-50 px-4 py-10 text-center text-sm text-gray-500">
        No check-ins found.
      </p>
    );
  }

  return (
    <ul className="divide-y divide-gray-100 overflow-hidden rounded-lg border border-gray-200 bg-white">
      {items.map((checkIn) => (
        <li key={checkIn.id}>
          <Link
            href={`/checkins/${checkIn.id}`}
            className="flex items-center justify-between gap-4 px-4 py-3 transition hover:bg-gray-50"
          >
            <div className="min-w-0">
              <div className="flex items-center gap-2">
                <MoodBadge mood={checkIn.mood} />
                {showUser && (
                  <span className="truncate text-sm font-medium text-gray-700">
                    {checkIn.userName}
                  </span>
                )}
              </div>
              {checkIn.notes && (
                <p className="mt-1 truncate text-sm text-gray-500">{checkIn.notes}</p>
              )}
            </div>
            <time className="shrink-0 text-xs text-gray-400" dateTime={checkIn.createdAt}>
              {formatDateTime(checkIn.createdAt)}
            </time>
          </Link>
        </li>
      ))}
    </ul>
  );
}
