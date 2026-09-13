"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

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
          <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-gradient-to-br from-primary to-tierRuby text-lg">
            🍪
          </span>
          <span className="flex flex-col leading-none">
            <span className="text-sm font-bold">Cookie SMP</span>
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

        <div className="flex items-center gap-2 text-xs text-mutedForeground">
          <span className="flex h-2 w-2 rounded-full bg-online" />
          cookie-smp.minetro.net
        </div>
      </div>
    </header>
  );
}
