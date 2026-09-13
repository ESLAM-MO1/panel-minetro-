export type LeaderboardCategory =
  | "money"
  | "kills"
  | "deaths"
  | "playtime"
  | "shards"
  | "placedBlocks"
  | "brokenBlocks"
  | "mobKills";

export const categories: { key: LeaderboardCategory; label: string }[] = [
  { key: "money", label: "Money" },
  { key: "kills", label: "Kills" },
  { key: "deaths", label: "Deaths" },
  { key: "playtime", label: "Playtime" },
  { key: "shards", label: "Shards" },
  { key: "placedBlocks", label: "Blocks Placed" },
  { key: "brokenBlocks", label: "Blocks Broken" },
  { key: "mobKills", label: "Mob Kills" },
];

export interface LeaderboardEntry {
  rank: number;
  username: string;
  value: string;
}

export const leaderboards: Record<LeaderboardCategory, LeaderboardEntry[]> = {
  money: [
    { rank: 1, username: "BakerBoyy", value: "$2.4T" },
    { rank: 2, username: "SugarRushh", value: "$2.1T" },
    { rank: 3, username: "Choc_Chip99", value: "$1.9T" },
    { rank: 4, username: "GingerSnapz", value: "$1.4T" },
    { rank: 5, username: "OvenMitt_", value: "$1.3T" },
  ],
  kills: [
    { rank: 1, username: "PvP_Crumbles", value: "121,286" },
    { rank: 2, username: "WaferKing", value: "121,171" },
    { rank: 3, username: "Golbary", value: "119,770" },
    { rank: 4, username: "SweetTooth7", value: "59,298" },
    { rank: 5, username: "Frostyy", value: "59,148" },
  ],
  deaths: [
    { rank: 1, username: "itsCookieDR", value: "3,743,938" },
    { rank: 2, username: "nikkyson", value: "3,378,117" },
    { rank: 3, username: "Willen", value: "3,285,492" },
    { rank: 4, username: "Handovers12", value: "2,613,445" },
    { rank: 5, username: "SkyKidd", value: "1,069,268" },
  ],
  playtime: [
    { rank: 1, username: "Hulpic", value: "502d 13h" },
    { rank: 2, username: "kostbar_troll", value: "401d 7h" },
    { rank: 3, username: "CoolProgrammer", value: "399d 8h" },
    { rank: 4, username: "YukonCharlie", value: "395d 13h" },
    { rank: 5, username: "DarkBob_", value: "394d 5h" },
  ],
  shards: [
    { rank: 1, username: "Test_of_Fate", value: "1,017,296" },
    { rank: 2, username: "cesgk", value: "867,627" },
    { rank: 3, username: "CoolProgrammer", value: "719,876" },
    { rank: 4, username: "Itszdeath", value: "549,858" },
    { rank: 5, username: "Quirkers", value: "525,432" },
  ],
  placedBlocks: [
    { rank: 1, username: "Squintom", value: "19,991,900" },
    { rank: 2, username: "TailsMC", value: "15,339,552" },
    { rank: 3, username: "SolarDecay", value: "15,295,192" },
    { rank: 4, username: "ItsLOLMaster", value: "13,723,719" },
    { rank: 5, username: "varo_15gr", value: "13,460,783" },
  ],
  brokenBlocks: [
    { rank: 1, username: "Munkerlich", value: "19,015,460" },
    { rank: 2, username: "LazerCivan", value: "15,272,275" },
    { rank: 3, username: "ItsLOLMaster", value: "13,561,069" },
    { rank: 4, username: "GioRobit", value: "9,718,314" },
    { rank: 5, username: "Squintom", value: "9,573,546" },
  ],
  mobKills: [
    { rank: 1, username: "ilovecookies", value: "60,875,361" },
    { rank: 2, username: "Nottis1", value: "37,806,175" },
    { rank: 3, username: "R1kuo_", value: "37,128,908" },
    { rank: 4, username: "FauxCube4345", value: "23,081,099" },
    { rank: 5, username: "TanSquare2762", value: "20,291,681" },
  ],
};

export interface OnlinePlayer {
  username: string;
  ping: number;
  playtimeToday: string;
  world: string;
}

export const onlinePlayers: OnlinePlayer[] = [
  { username: "BakerBoyy", ping: 24, playtimeToday: "3h 12m", world: "world" },
  { username: "SugarRushh", ping: 41, playtimeToday: "1h 45m", world: "world_nether" },
  { username: "Choc_Chip99", ping: 18, playtimeToday: "5h 02m", world: "world" },
  { username: "GingerSnapz", ping: 63, playtimeToday: "0h 20m", world: "world_the_end" },
  { username: "OvenMitt_", ping: 29, playtimeToday: "2h 51m", world: "world" },
  { username: "Frostyy", ping: 55, playtimeToday: "4h 08m", world: "world" },
];

export const serverInfo = {
  name: "Cookie SMP",
  ip: "cookie-smp.minetro.net",
  playersOnline: onlinePlayers.length,
  maxPlayers: 500,
  version: "Paper 1.21.11",
};
