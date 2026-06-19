import type { CheckInFilters } from "./types";

// Push "to" to end-of-day so same-day check-ins are included in the range.
export function toQueryFilters(filters: CheckInFilters): CheckInFilters {
  return {
    ...filters,
    to: filters.to ? `${filters.to}T23:59:59.999` : undefined,
  };
}
