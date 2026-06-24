"use client";

import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import useSWR from "swr";
import { api, type AuthContext } from "./api";
import type { User } from "./types";

const STORAGE_KEY = "mh.currentUserId";

interface CurrentUserContextValue {
  users: User[];
  currentUser: User | null;
  auth: AuthContext | null;
  setCurrentUserId: (id: string) => void;
  isLoading: boolean;
}

const CurrentUserContext = createContext<CurrentUserContextValue | undefined>(undefined);

export function CurrentUserProvider({ children }: { children: ReactNode }) {
  const { data: users, isLoading } = useSWR<User[]>("users", () => api.getUsers());
  const [currentUserId, setCurrentUserId] = useState<string | null>(null);

  useEffect(() => {
    if (!users || users.length === 0) return;
    const stored = typeof window !== "undefined" ? window.localStorage.getItem(STORAGE_KEY) : null;
    const valid = stored && users.some((u) => u.id === stored);
    setCurrentUserId(valid ? stored : users[0].id);
  }, [users]);

  const selectUser = (id: string) => {
    setCurrentUserId(id);
    if (typeof window !== "undefined") window.localStorage.setItem(STORAGE_KEY, id);
  };

  const value = useMemo<CurrentUserContextValue>(() => {
    const currentUser = users?.find((u) => u.id === currentUserId) ?? null;
    return {
      users: users ?? [],
      currentUser,
      auth: currentUser ? { id: currentUser.id, role: currentUser.role } : null,
      setCurrentUserId: selectUser,
      // True until the current user resolves; the length check lets it settle
      // when there are genuinely no users to select.
      isLoading: isLoading || (currentUser === null && (users?.length ?? 0) > 0),
    };
  }, [users, currentUserId, isLoading]);

  return <CurrentUserContext.Provider value={value}>{children}</CurrentUserContext.Provider>;
}

export function useCurrentUser(): CurrentUserContextValue {
  const ctx = useContext(CurrentUserContext);
  if (!ctx) throw new Error("useCurrentUser must be used within a CurrentUserProvider");
  return ctx;
}
