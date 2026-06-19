import type { Metadata } from "next";
import "./globals.css";
import { CurrentUserProvider } from "@/lib/current-user";
import { Nav } from "@/components/Nav";

export const metadata: Metadata = {
  title: "Wellness Check-in",
  description: "Submit and review mental health check-ins.",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>
        <CurrentUserProvider>
          <Nav />
          <main className="mx-auto max-w-4xl px-4 py-8">{children}</main>
        </CurrentUserProvider>
      </body>
    </html>
  );
}
