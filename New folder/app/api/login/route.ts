import { NextRequest, NextResponse } from "next/server";

export async function POST(req: NextRequest) {
  const { username, password } = await req.json().catch(() => ({}));

  const validUsername = process.env.SITE_USERNAME;
  const validPassword = process.env.SITE_PASSWORD;
  const secret = process.env.SITE_SESSION_SECRET;

  if (!validUsername || !validPassword || !secret) {
    return NextResponse.json(
      { error: "Login is not configured on the server." },
      { status: 500 }
    );
  }

  if (username !== validUsername || password !== validPassword) {
    return NextResponse.json(
      { error: "Incorrect username or password." },
      { status: 401 }
    );
  }

  const res = NextResponse.json({ ok: true });
  res.cookies.set("session", secret, {
    httpOnly: true,
    sameSite: "lax",
    path: "/",
    maxAge: 60 * 60 * 24 * 30,
  });
  return res;
}