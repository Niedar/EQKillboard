-- Verify eqkillboard-database:0004_dispel_slots_involved_stats on pg

BEGIN;

-- XXX Add verifications here.

SELECT dispel_slots, * FROM killmail_involved;
SELECT * FROM character_ranked_kill_death;
SELECT * FROM guild_ranked_kill_death;

ROLLBACK;
