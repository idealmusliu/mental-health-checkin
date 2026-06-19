export type Role = "Employee" | "Manager";

export interface User {
  id: string;
  name: string;
  email: string;
  role: Role;
}

export interface CheckIn {
  id: string;
  userId: string;
  userName: string;
  mood: number;
  notes: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface MoodTrendPoint {
  date: string;
  averageMood: number;
  count: number;
}

export interface DashboardStats {
  totalCheckIns: number;
  averageMood: number;
  moodOverTime: MoodTrendPoint[];
}

export interface CheckInFilters {
  userId?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}

export interface CheckInInput {
  mood: number;
  notes: string | null;
}
