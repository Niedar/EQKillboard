-- Revert eqkillboard-database:0004_dispel_slots_involved_stats from pg

BEGIN;

-- XXX Add DDLs here.
DROP VIEW IF EXISTS character_ranked_kill_death_involved;
DROP VIEW IF EXISTS guild_ranked_kill_death_involved;
DROP FUNCTION IF EXISTS guild_kills;
ALTER TABLE killmail_involved DROP CONSTRAINT killmail_id_attacker_id_uniq;
ALTER TABLE killmail_involved DROP COLUMN dispel_slots;

COMMIT;
