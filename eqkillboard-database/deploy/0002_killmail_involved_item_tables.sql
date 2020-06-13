-- Deploy eqkillboard-database:0002_killmail_involed_item_tables to pg
-- requires: @v1.0.0

BEGIN;

CREATE TABLE killmail_involved (
    id SERIAL PRIMARY KEY,
    killmail_id INTEGER REFERENCES killmail NOT NULL,
    attacker_id INTEGER REFERENCES character NOT NULL,
    attacker_guild_id INTEGER REFERENCES guild,
    attacker_level INTEGER,
    melee_damage INTEGER,
    melee_hits INTEGER,
    spell_damage INTEGER,
    spell_hits INTEGER
);
CREATE INDEX killmail_involved_killmail_id_index ON killmail_involved(killmail_id);
CREATE INDEX killmail_involved_attacker_id_index ON killmail_involved(attacker_id);
CREATE INDEX killmail_involved_attacker_guild_id_index ON killmail_involved(attacker_guild_id);

-- Insert involved records for the killing blow data that already exists in the killmail table. Missing damage statistics
INSERT INTO killmail_involved (killmail_id, attacker_id, attacker_guild_id, attacker_level)
SELECT
    km.id AS killmail_id,
    km.attacker_id,
    km.attacker_guild_id,
    km.attacker_level
FROM killmail km
ORDER BY km.id;

CREATE TABLE item (
    id SERIAL PRIMARY KEY,
    name TEXT UNIQUE
);

CREATE TABLE item_sale (
    id SERIAL PRIMARY KEY,
    item_id INTEGER REFERENCES item NOT NULL,
    price INTEGER NOT NULL,
    season INTEGER NOT NULL
);
CREATE INDEX item_sale_item_id_index ON item_sale(item_id);
CREATE INDEX item_sale_season_index ON item_sale(season);

CREATE VIEW item_stats AS
SELECT
    item_id,
    season,
    AVG(price) AS average_price,
    MAX(price) AS max_price,
    CAST(COUNT(item_id) AS INTEGER) AS total_sales
FROM item_sale
GROUP BY item_id, season;

ALTER TABLE killmail ADD looted_item INTEGER REFERENCES item;
ALTER TABLE killmail ADD looted_by INTEGER REFERENCES character;
ALTER TABLE character ADD is_npc BOOLEAN NOT NULL DEFAULT false;

-- Add season discriminator to tables
ALTER TABLE character ADD season INTEGER NULL;
ALTER TABLE guild ADD season INTEGER NULL;
ALTER TABLE killmail ADD season INTEGER NULL;
ALTER TABLE killmail_raw ADD season INTEGER NULL;

UPDATE character SET season = 1;
UPDATE guild SET season = 1;
UPDATE killmail SET season = 1;
UPDATE killmail_raw SET season = 1;

ALTER TABLE character ALTER COLUMN season SET NOT NULL;
ALTER TABLE guild ALTER COLUMN season SET NOT NULL;
ALTER TABLE killmail ALTER COLUMN season SET NOT NULL;
ALTER TABLE killmail_raw ALTER COLUMN season SET NOT NULL;

ALTER TABLE character DROP CONSTRAINT character_name_key;
ALTER TABLE guild DROP CONSTRAINT guild_name_key;
ALTER TABLE character ADD CONSTRAINT character_name_season_uniq UNIQUE (name, season);
ALTER TABLE guild ADD CONSTRAINT guild_name_season_uniq UNIQUE (name, season);

CREATE INDEX character_season_index ON character(season);
CREATE INDEX guild_season_index ON guild(season);
CREATE INDEX killmail_season_index ON killmail(season);
CREATE INDEX killmail_raw_season_index ON killmail_raw(season);


DROP FUNCTION search_guilds(search TEXT);
CREATE FUNCTION search_guilds(season INTEGER, search TEXT) RETURNS SETOF guild AS $$
    SELECT set_limit(0.1);

    SELECT guild.*
    FROM guild
    WHERE guild.season = season AND guild.name % search
    ORDER BY similarity(guild.name, search) DESC
    LIMIT 10;
$$ LANGUAGE SQL STABLE;

DROP FUNCTION search_characters(search TEXT);
CREATE FUNCTION search_characters(season INTEGER, search TEXT) RETURNS SETOF public.character AS $$
    SELECT set_limit(0.1);

    SELECT character.*
    FROM character
    WHERE character.season = season AND character.name % search
    ORDER BY similarity(character.name, search) DESC
    LIMIT 5;
$$ LANGUAGE SQL STABLE;



COMMIT;
