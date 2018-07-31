-- Deploy eqkillboard-database:0001_initial_schema to pg

BEGIN;

CREATE EXTENSION pg_trgm;

-- guild
CREATE TABLE guild (
    id SERIAL PRIMARY KEY,
    name TEXT UNIQUE
);
CREATE INDEX guild_name_trgm_index ON guild USING gin (name gin_trgm_ops);

CREATE FUNCTION search_guilds(search TEXT) RETURNS SETOF guild AS $$
    SELECT set_limit(0.1);

    SELECT guild.*
    FROM guild
    WHERE guild.name % search
    ORDER BY similarity(guild.name, search) DESC
    LIMIT 10;
$$ LANGUAGE SQL STABLE;


-- zone
CREATE TABLE zone (
    id SERIAL PRIMARY KEY,
    name TEXT UNIQUE
);
CREATE INDEX zone_name_trgm_index ON zone USING gin (name gin_trgm_ops);

CREATE FUNCTION search_zones(search TEXT) RETURNS SETOF zone AS $$
    SELECT set_limit(0.1);

    SELECT zone.*
    FROM zone
    WHERE zone.name % search
    ORDER BY similarity(zone.name, search) DESC
    LIMIT 10;
$$ LANGUAGE SQL STABLE;


-- class
CREATE TABLE class (
    id SERIAL PRIMARY KEY,
    name TEXT UNIQUE
);
CREATE INDEX class_name_trgm_index ON class USING gin (name gin_trgm_ops);

CREATE FUNCTION search_classes(search TEXT) RETURNS SETOF class AS $$
    SELECT set_limit(0.1);

    SELECT class.*
    FROM class
    WHERE class.name % search
    ORDER BY similarity(class.name, search) DESC
    LIMIT 5;
$$ LANGUAGE SQL STABLE;


-- status_type
CREATE TABLE status_type (
    id SERIAL PRIMARY KEY,
    name TEXT UNIQUE
);


-- character
CREATE TABLE character (
    id SERIAL PRIMARY KEY,
    name TEXT UNIQUE,
    guild_id INTEGER REFERENCES guild,
    class_id INTEGER REFERENCES class,
    level INTEGER
);
CREATE INDEX character_name_trgm_index ON character USING gin (name gin_trgm_ops);

CREATE FUNCTION search_characters(search TEXT) RETURNS SETOF public.character AS $$
    SELECT set_limit(0.1);

    SELECT character.*
    FROM character
    WHERE character.name % search
    ORDER BY similarity(character.name, search) DESC
    LIMIT 5;
$$ LANGUAGE SQL STABLE;


-- killmail_raw
CREATE TABLE killmail_raw (
    id SERIAL PRIMARY KEY,
    discord_message_id NUMERIC UNIQUE NOT NULL,
    message TEXT,
    status_type_id INTEGER REFERENCES status_type
);
CREATE INDEX killmail_raw_discord_message_id_index ON killmail_raw(discord_message_id);
CREATE INDEX killmail_raw_status_type_id_index ON killmail_raw(status_type_id);


-- killmails
CREATE TABLE killmail (
    id SERIAL PRIMARY KEY,
    victim_id INTEGER REFERENCES character NOT NULL,
    victim_guild_id INTEGER REFERENCES guild,
    victim_level INTEGER,
    attacker_id INTEGER REFERENCES character NOT NULL,
    attacker_guild_id INTEGER REFERENCES guild,
    attacker_level INTEGER,
    zone_id INTEGER REFERENCES zone NOT NULL,
    killed_at TIMESTAMP WITH TIME ZONE,
    killmail_raw_id INTEGER REFERENCES killmail_raw
);
CREATE INDEX killmail_victim_id_index ON killmail(victim_id);
CREATE INDEX killmail_victim_guild_id_index ON killmail(victim_guild_id);
CREATE INDEX killmail_attacker_id_index ON killmail(attacker_id);
CREATE INDEX killmail_attacker_guild_id_index ON killmail(attacker_guild_id);
CREATE INDEX killmail_zone_id_index ON killmail(zone_id);


DO $$
BEGIN
   execute 'ALTER DATABASE '||current_database()||' SET timezone TO ''UTC''';
END;
$$;

SELECT pg_reload_conf();

COMMIT;

