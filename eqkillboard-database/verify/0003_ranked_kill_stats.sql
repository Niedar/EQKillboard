-- Verify eqkillboard-database:0003_ranked_kill_stats on pg

BEGIN;

-- XXX Add verifications here.
SELECT * FROM character_ranked_kill_death;
SELECT * FROM guild_ranked_kill_death;

ROLLBACK;
