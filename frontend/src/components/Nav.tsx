"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useCurrentUser } from "@/lib/current-user";
import { UserSwitcher } from "./UserSwitcher";

export function Nav() {
  const pathname = usePathname();
  const { currentUser, isLoading } = useCurrentUser();
  const isManager = currentUser?.role === "Manager";

  // Employees submit + review their own; managers only review/track the team.
  const links = isLoading
    ? []
    : isManager
      ? [
          { href: "/checkins", label: "Check-ins" },
          { href: "/dashboard", label: "Dashboard" },
        ]
      : [
          { href: "/", label: "New check-in" },
          { href: "/checkins", label: "Check-ins" },
        ];

  const homeHref = isManager ? "/dashboard" : "/";

  return (
    <header className="bg-gray-900 text-white">
      <nav className="mx-auto flex max-w-4xl flex-wrap items-center justify-between gap-3 px-4 py-3">
        <div className="flex items-center gap-6">
          <Link href={homeHref} className="text-base font-semibold">
            🧠 Wellness Check-in
          </Link>
          <ul className="flex items-center gap-1">
            {links.map((link) => {
              const active = pathname === link.href;
              return (
                <li key={link.href}>
                  <Link
                    href={link.href}
                    className={`rounded-md px-3 py-1.5 text-sm transition ${
                      active ? "bg-gray-700 font-medium" : "text-gray-300 hover:bg-gray-800"
                    }`}
                  >
                    {link.label}
                  </Link>
                </li>
              );
            })}
          </ul>
        </div>
        <UserSwitcher />
      </nav>
    </header>
  );
}
