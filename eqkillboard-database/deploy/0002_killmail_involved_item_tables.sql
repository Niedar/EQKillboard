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
    price INTEGER NOT NULL
);

-- VIEW
CREATE TABLE item_stats (
    item_id INTEGER REFERENCES item PRIMARY KEY,
    average_price INTEGER,
    max_price INTEGER,
    total_sales INTEGER
);

ALTER TABLE killmail ADD looted_item INTEGER REFERENCES item;
ALTER TABLE killmail ADD looted_by INTEGER REFERENCES character;

COMMIT;
