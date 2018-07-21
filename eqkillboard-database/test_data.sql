BEGIN;

TRUNCATE guild RESTART IDENTITY;
TRUNCATE zone RESTART IDENTITY;
TRUNCATE class RESTART IDENTITY;
TRUNCATE character RESTART IDENTITY;
TRUNCATE killmail RESRAER IDENTITY;


INSERT INTO guild (name) VALUES
('Catalyst'), -- 1
('Relentless'), -- 2
('Versus the World'), -- 3
('Casual Scum'), -- 4
('Domingueros de Norrath'); --5

INSERT INTO zone (name) VALUES
('Innothule Swamp'), --1
('The Ruins of Old Guk'), --2
('Rivervale'), --3
('Plane of Hate'), -- 4
('Ocean of Tears'); -- 5

INSERT INTO class (name) VALUES
('Monk'), -- 1
('Warrior'), -- 2
('Cleric'), -- 3
('Mage'), -- 4
('Druid'); --5

INSERT INTO character(name, guild_id, class_id, level) VALUES
('Niedar', 1, 4, 50), -- 1
('Hoid', NULL, NULL, NULL), --2
('Darksinga', 2, NULL, NULL), --3
('Fatbella', 4, 5, NULL), --4
('Mcnasty ', 3, 3, 23); --5

INSERT INTO killmail (victim_id, victim_guild_id, victim_level, attacker_id, attacker_guild_id, attacker_level, zone_id, killed_at) VALUES
(1, 1, 45, 2, NULL, NULL, 5, '2018-07-19 23:08:10+02'),
(4, 3, NULL, 2, 5, 50, 2, '2018-07-19 10:08:10+02'),
(5, 3, 50, 2, 5, 50, 2, '2018-07-16 14:45:00+02'),
(4, 2, 40, 1, 4, 50, 3, '2018-07-01 14:45:00+02'),
(3, 2, NULL, 1, 4, NULL, 1, '2018-07-01 14:47:00+02');




COMMIT;