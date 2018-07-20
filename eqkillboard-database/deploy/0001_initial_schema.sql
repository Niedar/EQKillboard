-- Deploy eqkillboard-database:0001_initial_schema to pg

BEGIN;

CREATE TABLE guild (
    id SERIAL PRIMARY KEY,
    name TEXT UNIQUE
);

CREATE TABLE zone (
    id SERIAL PRIMARY KEY,
    name TEXT UNIQUE
);

CREATE TABLE class (
    id SERIAL PRIMARY KEY,
    name TEXT UNIQUE
);

CREATE TABLE status_type (
    id SERIAL PRIMARY KEY,
    name TEXT UNIQUE
);

CREATE TABLE character (
    id SERIAL PRIMARY KEY,
    name TEXT UNIQUE,
    guild_id INTEGER REFERENCES guild,
    class_id INTEGER REFERENCES class,
    level INTEGER
);

CREATE TABLE killmail_raw (
    id SERIAL PRIMARY KEY,
    discord_message_id NUMERIC UNIQUE NOT NULL,
    message TEXT,
    status_type_id INTEGER REFERENCES status_type
);
CREATE INDEX killmail_raw_discord_message_id_index ON killmail_raw(discord_message_id);
CREATE INDEX killmail_raw_status_type_id_index ON killmail_raw(status_type_id);

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

COMMIT;

