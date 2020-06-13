-- Revert eqkillboard-database:0003_ranked_kill_stats from pg

BEGIN;

-- XXX Add DDLs here.
DROP VIEW IF EXISTS character_ranked_kill_death;
DROP VIEW IF EXISTS guild_ranked_kill_death;

COMMIT;
