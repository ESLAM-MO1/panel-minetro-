-- Local dev seed data only. This mimics the table shape we EXPECT
-- ajLeaderboards to produce in MySQL mode (one table per tracked
-- placeholder, prefixed "ajlb_", with a player-name column and a numeric
-- value column) so the full pipeline (MySQL -> backend -> frontend) can be
-- proven end-to-end before the real server is pointed at any database.
--
-- Once the real production database is available, run:
--   SHOW TABLES LIKE 'ajlb_%';
--   DESCRIBE <table_name>;
-- and update Leaderboards:Sources in appsettings.json to match the real
-- table/column names if they differ from these guesses.

CREATE TABLE IF NOT EXISTS ajlb_statistic_player_kills (
  uuid  VARCHAR(36) NOT NULL PRIMARY KEY,
  name  VARCHAR(32) NOT NULL,
  value DOUBLE NOT NULL
);

CREATE TABLE IF NOT EXISTS ajlb_statistic_deaths (
  uuid  VARCHAR(36) NOT NULL PRIMARY KEY,
  name  VARCHAR(32) NOT NULL,
  value DOUBLE NOT NULL
);

-- Stored in ticks (20 ticks/second), matching Minecraft's own played-time
-- statistic unit -- see the "Divisor": 20 note in appsettings.json.
CREATE TABLE IF NOT EXISTS ajlb_statistic_time_played (
  uuid  VARCHAR(36) NOT NULL PRIMARY KEY,
  name  VARCHAR(32) NOT NULL,
  value DOUBLE NOT NULL
);

CREATE TABLE IF NOT EXISTS ajlb_statistic_mob_kills (
  uuid  VARCHAR(36) NOT NULL PRIMARY KEY,
  name  VARCHAR(32) NOT NULL,
  value DOUBLE NOT NULL
);

CREATE TABLE IF NOT EXISTS ajlb_statistic_mine_block (
  uuid  VARCHAR(36) NOT NULL PRIMARY KEY,
  name  VARCHAR(32) NOT NULL,
  value DOUBLE NOT NULL
);

INSERT INTO ajlb_statistic_player_kills (uuid, name, value) VALUES
  (UUID(), 'PvP_Crumbles', 121286),
  (UUID(), 'WaferKing',    121171),
  (UUID(), 'Golbary',      119770),
  (UUID(), 'SweetTooth7',   59298),
  (UUID(), 'Frostyy',       59148),
  (UUID(), 'BakerBoyy',     41220),
  (UUID(), 'Choc_Chip99',   38010),
  (UUID(), 'GingerSnapz',   30500),
  (UUID(), 'OvenMitt_',     22190),
  (UUID(), 'SugarRushh',    18040);

INSERT INTO ajlb_statistic_deaths (uuid, name, value) VALUES
  (UUID(), 'itsCookieDR',   3743938),
  (UUID(), 'nikkyson',      3378117),
  (UUID(), 'Willen',        3285492),
  (UUID(), 'Handovers12',   2613445),
  (UUID(), 'SkyKidd',       1069268),
  (UUID(), 'BakerBoyy',      845210),
  (UUID(), 'Frostyy',        712043),
  (UUID(), 'GingerSnapz',    533019),
  (UUID(), 'PvP_Crumbles',   402188),
  (UUID(), 'Choc_Chip99',    215760);

INSERT INTO ajlb_statistic_time_played (uuid, name, value) VALUES
  (UUID(), 'Hulpic',          868320000),
  (UUID(), 'kostbar_troll',   693561600),
  (UUID(), 'CoolProgrammer',  689990400),
  (UUID(), 'YukonCharlie',    682732800),
  (UUID(), 'DarkBob_',        681264000),
  (UUID(), 'itsCookieDR',     512640000),
  (UUID(), 'Willen',          401760000),
  (UUID(), 'SkyKidd',         298080000),
  (UUID(), 'nikkyson',        184320000),
  (UUID(), 'Handovers12',      92160000);

INSERT INTO ajlb_statistic_mob_kills (uuid, name, value) VALUES
  (UUID(), 'ilovecookies',  60875361),
  (UUID(), 'Nottis1',       37806175),
  (UUID(), 'R1kuo_',        37128908),
  (UUID(), 'FauxCube4345',  23081099),
  (UUID(), 'TanSquare2762', 20291681),
  (UUID(), 'Hulpic',        15230044),
  (UUID(), 'CoolProgrammer',12904012),
  (UUID(), 'YukonCharlie',   9873001),
  (UUID(), 'DarkBob_',       7213980),
  (UUID(), 'kostbar_troll',  5019233);

INSERT INTO ajlb_statistic_mine_block (uuid, name, value) VALUES
  (UUID(), 'Munkerlich',    19015460),
  (UUID(), 'LazerCivan',    15272275),
  (UUID(), 'ItsLOLMaster',  13561069),
  (UUID(), 'GioRobit',       9718314),
  (UUID(), 'Squintom',       9573546),
  (UUID(), 'ilovecookies',   6432100),
  (UUID(), 'Nottis1',        4321099),
  (UUID(), 'R1kuo_',         3219044),
  (UUID(), 'FauxCube4345',   2103988),
  (UUID(), 'TanSquare2762',  1503221);
