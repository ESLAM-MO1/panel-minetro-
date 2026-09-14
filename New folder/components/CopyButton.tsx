"use client";

import { useState } from "react";

export default function CopyButton({ text }: { text: string }) {
  const [copied, setCopied] = useState(false);

  const copy = async () => {
    try {
      await navigator.clipboard.writeText(text);
      setCopied(true);
      setTimeout(() => setCopied(false), 1500);
    } catch {
      // clipboard API unavailable -- fail silently
    }
  };

  return (
    <button
      onClick={copy}
      className="rounded-lg bg-primary px-3 py-1.5 text-xs font-semibold hover:bg-accentText"
    >
      {copied ? "Copied!" : "Copy"}
    </button>
  );
}