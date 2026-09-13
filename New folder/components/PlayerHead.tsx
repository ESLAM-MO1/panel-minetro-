import Image from "next/image";

export default function PlayerHead({
  username,
  size = 32,
}: {
  username: string;
  size?: number;
}) {
  return (
    <Image
      src={`https://mc-heads.net/avatar/${encodeURIComponent(username)}/${size}`}
      alt={`${username}'s Minecraft head`}
      width={size}
      height={size}
      className="rounded-md"
      unoptimized
    />
  );
}
