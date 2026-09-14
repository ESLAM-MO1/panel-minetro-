import type { Config } from "tailwindcss";

const config: Config = {
  darkMode: "class",
  content: [
    "./app/**/*.{ts,tsx}",
    "./components/**/*.{ts,tsx}",
  ],
  theme: {
    extend: {
        colors: {
        background: "var(--color-background)",
        foreground: "var(--color-foreground)",
        card: "var(--color-card)",
        muted: "var(--color-muted)",
        mutedForeground: "var(--color-mutedForeground)",
        border: "var(--color-border)",
        primary: "#A91955",
        accentText: "#F04C90",
        highlight: "#F99406",
        online: "#28AF60",
        tierDiamond: "#0A76A9",
        tierMedia: "#B52DCD",
        tierRuby: "#D9174E",
        tierEmerald: "#0B7F58",
      },
      borderRadius: {
        DEFAULT: "0.75rem",
        xl: "0.75rem",
        "2xl": "1.5rem",
      },
      fontFamily: {
        sans: ["var(--font-inter)", "Inter", "ui-sans-serif", "system-ui", "sans-serif"],
        mono: ["var(--font-jetbrains-mono)", "JetBrains Mono", "ui-monospace", "monospace"],
      },
    },
  },
  plugins: [],
};

export default config;
