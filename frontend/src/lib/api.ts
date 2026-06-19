import type {
  CheckIn,
  CheckInFilters,
  CheckInInput,
  DashboardStats,
  PagedResult,
  Role,
  User,
} from "./types";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5080";

export interface AuthContext {
  id: string;
  role: Role;
}

interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  status: number;
  errors?: Record<string, string[]>;

  constructor(message: string, status: number, errors?: Record<string, string[]>) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.errors = errors;
  }
}

interface RequestOptions {
  method?: string;
  body?: unknown;
  auth?: AuthContext | null;
}

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers: Record<string, string> = { "Content-Type": "application/json" };

  if (options.auth) {
    headers["X-User-Id"] = options.auth.id;
    headers["X-User-Role"] = options.auth.role;
  }

  const response = await fetch(`${API_URL}${path}`, {
    method: options.method ?? "GET",
    headers,
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
    cache: "no-store",
  });

  if (!response.ok) {
    throw await toApiError(response);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

async function toApiError(response: Response): Promise<ApiError> {
  let problem: ProblemDetails | null = null;
  try {
    problem = (await response.json()) as ProblemDetails;
  } catch {
    // non-JSON body; fall back to status text below
  }

  const firstValidationMessage = problem?.errors
    ? Object.values(problem.errors).flat()[0]
    : undefined;

  const message =
    firstValidationMessage ??
    problem?.detail ??
    problem?.title ??
    `Request failed with status ${response.status}`;

  return new ApiError(message, response.status, problem?.errors);
}

function buildQuery(filters: CheckInFilters): string {
  const params = new URLSearchParams();
  if (filters.userId) params.set("userId", filters.userId);
  if (filters.from) params.set("from", filters.from);
  if (filters.to) params.set("to", filters.to);
  if (filters.page) params.set("page", String(filters.page));
  if (filters.pageSize) params.set("pageSize", String(filters.pageSize));
  const qs = params.toString();
  return qs ? `?${qs}` : "";
}

export const api = {
  getUsers: (auth?: AuthContext | null) => request<User[]>("/users", { auth }),

  getCheckIns: (filters: CheckInFilters, auth?: AuthContext | null) =>
    request<PagedResult<CheckIn>>(`/checkins${buildQuery(filters)}`, { auth }),

  getCheckIn: (id: string, auth?: AuthContext | null) =>
    request<CheckIn>(`/checkins/${id}`, { auth }),

  createCheckIn: (input: CheckInInput, auth?: AuthContext | null) =>
    request<CheckIn>("/checkins", { method: "POST", body: input, auth }),

  updateCheckIn: (id: string, input: CheckInInput, auth?: AuthContext | null) =>
    request<CheckIn>(`/checkins/${id}`, { method: "PUT", body: input, auth }),

  getDashboardStats: (
    filters: CheckInFilters,
    tzOffsetMinutes: number,
    auth?: AuthContext | null,
  ) => {
    const qs = buildQuery(filters);
    const sep = qs ? "&" : "?";
    return request<DashboardStats>(
      `/dashboard/stats${qs}${sep}tzOffsetMinutes=${tzOffsetMinutes}`,
      { auth },
    );
  },
};
