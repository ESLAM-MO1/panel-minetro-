"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import ThemeToggle from "@/components/ThemeToggle";

const links = [
  { href: "/", label: "Home" },
  { href: "/leaderboards", label: "Leaderboards" },
  { href: "/players", label: "Players Online" },
];

export default function Navbar() {
  const pathname = usePathname();

  return (
    <header className="sticky top-0 z-40 border-b border-border bg-background/95 backdrop-blur">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-3">
        <Link href="/" className="flex items-center gap-2.5">
          <span className="flex h-9 w-9 items-center justify-center">
            <svg viewBox="0 0 80 80" xmlns="http://www.w3.org/2000/svg" className="h-9 w-9">
              <circle cx="40" cy="40" r="36" fill="#C08552" />
              <circle cx="40" cy="40" r="36" fill="none" stroke="#8F5C2E" strokeWidth="1" opacity="0.5" />
              <circle cx="26" cy="26" r="4.2" fill="#3E2412" />
              <circle cx="48" cy="22" r="3.2" fill="#3E2412" />
              <circle cx="54" cy="40" r="3.8" fill="#3E2412" />
              <circle cx="32" cy="52" r="3.4" fill="#3E2412" />
              <circle cx="48" cy="54" r="2.8" fill="#3E2412" />
              <circle cx="22" cy="44" r="2.6" fill="#3E2412" />
            </svg>
          </span>
          <span className="flex flex-col leading-none">
            <span className="text-sm font-bold">Koki Server</span>
            <span className="text-[11px] text-mutedForeground">
              Statistics Hub
            </span>
          </span>
        </Link>

        <nav className="flex items-center gap-1 rounded-xl bg-card p-1">
          {links.map((link) => {
            const active = pathname === link.href;
            return (
              <Link
                key={link.href}
                href={link.href}
                className={`rounded-lg px-3.5 py-1.5 text-sm font-medium transition-colors ${
                  active
                    ? "bg-primary text-white"
                    : "text-mutedForeground hover:bg-muted hover:text-foreground"
                }`}
              >
                {link.label}
              </Link>
            );
          })}
        </nav>

                <div className="flex items-center gap-3 text-xs text-mutedForeground">
          <span className="flex items-center gap-2">
            <span className="flex h-2 w-2 rounded-full bg-online" />
            cookie-smp.minetro.net
          </span>
          <ThemeToggle />
        </div>
      </div>
    </header>
  );
}
