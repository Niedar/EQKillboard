-- Revert eqkillboard-database:0002_killmail_involed_item_tables from pg

BEGIN;

ALTER TABLE killmail DROP COLUMN looted_item;
ALTER TABLE killmail DROP COLUMN looted_by;
ALTER TABLE character DROP COLUMN is_npc;
DROP TABLE killmail_involved;
DROP VIEW item_stats;
DROP TABLE item_sale;
DROP TABLE item;


DROP INDEX character_season_index;
DROP INDEX guild_season_index;
DROP INDEX killmail_season_index;
DROP INDEX killmail_raw_season_index;

DELETE FROM killmail WHERE season != 1;
DELETE FROM killmail_raw WHERE season != 1;
DELETE FROM character WHERE season != 1;
DELETE FROM guild WHERE season != 1;

ALTER TABLE character DROP CONSTRAINT character_name_season_uniq;
ALTER TABLE guild DROP CONSTRAINT guild_name_season_uniq;
ALTER TABLE character ADD CONSTRAINT character_name_key UNIQUE (name);
ALTER TABLE guild ADD CONSTRAINT guild_name_key UNIQUE (name);

DROP FUNCTION search_characters(season INTEGER, search TEXT);
CREATE FUNCTION search_characters(search TEXT) RETURNS SETOF public.character AS $$
    SELECT set_limit(0.1);

    SELECT character.*
    FROM character
    WHERE character.name % search
    ORDER BY similarity(character.name, search) DESC
    LIMIT 5;
$$ LANGUAGE SQL STABLE;

DROP FUNCTION search_guilds(season INTEGER, search TEXT);
CREATE FUNCTION search_guilds(search TEXT) RETURNS SETOF guild AS $$
    SELECT set_limit(0.1);

    SELECT guild.*
    FROM guild
    WHERE guild.name % search
    ORDER BY similarity(guild.name, search) DESC
    LIMIT 10;
$$ LANGUAGE SQL STABLE;

ALTER TABLE character DROP COLUMN season;
ALTER TABLE guild DROP COLUMN season;
ALTER TABLE killmail DROP COLUMN season;
ALTER TABLE killmail_raw DROP COLUMN season;

COMMIT;
