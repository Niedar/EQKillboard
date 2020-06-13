-- Deploy eqkillboard-database:0003_ranked_kill_stats to pg
-- requires: @v2.0.0

BEGIN;

-- XXX Add DDLs here.


CREATE VIEW character_ranked_kill_death AS
    WITH session_killmail AS (
        SELECT 
            *,
            victim_id || '_' || SUM(new_session) OVER (PARTITION BY victim_id ORDER BY killed_at) AS session_id
        FROM (
            SELECT 
                *,
                CASE
                    WHEN EXTRACT(EPOCH FROM killed_at) - LAG(EXTRACT(EPOCH FROM killed_at)) OVER (PARTITION BY victim_id ORDER BY killed_at) >= 30 * 60
                    THEN 1
                    ELSE 0
                END AS new_session
            FROM killmail
        ) killmail_lag
    )
    SELECT
        c.id,
        c.name,
        c.level,
        c.class_id,
        COALESCE(character_ranked_kills.ranked_kills, 0) AS ranked_kills,
        COALESCE(character_ranked_deaths.ranked_deaths, 0) AS ranked_deaths,
        c.season
    FROM character c
    LEFT JOIN (
        SELECT
            distinct_character_session_kills.id,
            CAST(COUNT(*) AS INTEGER) AS ranked_kills
        FROM (
            SELECT
                DISTINCT ON (character.id, session_killmail.session_id)
                character.id,
                session_killmail.session_id
            FROM character
            INNER JOIN session_killmail
                ON session_killmail.attacker_id = character.id
        ) distinct_character_session_kills
        GROUP BY distinct_character_session_kills.id        
    ) character_ranked_kills ON character_ranked_kills.id = c.id
    LEFT JOIN (
        SELECT
            distinct_character_session_deaths.id,
            CAST(COUNT(*) AS INTEGER) AS ranked_deaths
        FROM (
            SELECT
                DISTINCT ON (character.id, session_killmail.session_id)
                character.id,
                session_killmail.session_id
            FROM character
            INNER JOIN session_killmail
                ON session_killmail.victim_id = character.id
        ) distinct_character_session_deaths
        GROUP BY distinct_character_session_deaths.id        
    ) character_ranked_deaths ON character_ranked_deaths.id = c.id;


CREATE VIEW guild_ranked_kill_death AS
    WITH session_killmail AS (
        SELECT 
            *,
            victim_id || '_' || SUM(new_session) OVER (PARTITION BY victim_id ORDER BY killed_at) AS session_id
        FROM (
            SELECT 
                *,
                CASE
                    WHEN EXTRACT(EPOCH FROM killed_at) - LAG(EXTRACT(EPOCH FROM killed_at)) OVER (PARTITION BY victim_id ORDER BY killed_at) >= 30 * 60
                    THEN 1
                    ELSE 0
                END AS new_session
            FROM killmail
        ) killmail_lag
    )
    SELECT
        g.id,
        g.name,
        COALESCE(guild_ranked_kills.ranked_kills, 0) AS ranked_kills,
        COALESCE(guild_ranked_deaths.ranked_deaths, 0) AS ranked_deaths,
        g.season
    FROM guild g
    LEFT JOIN (
        SELECT
            distinct_guild_session_kills.id,
            CAST(COUNT(*) AS INTEGER) AS ranked_kills
        FROM (
            SELECT
                DISTINCT ON (guild.id, session_killmail.session_id)
                guild.id,
                session_killmail.session_id
            FROM guild
            INNER JOIN session_killmail
                ON session_killmail.attacker_guild_id = guild.id
        ) distinct_guild_session_kills
        GROUP BY distinct_guild_session_kills.id        
    ) guild_ranked_kills ON guild_ranked_kills.id = g.id
    LEFT JOIN (
        SELECT
            distinct_guild_session_deaths.id,
            CAST(COUNT(*) AS INTEGER) AS ranked_deaths
        FROM (
            SELECT
                DISTINCT ON (guild.id, session_killmail.session_id)
                guild.id,
                session_killmail.session_id
            FROM guild
            INNER JOIN session_killmail
                ON session_killmail.victim_guild_id = guild.id
        ) distinct_guild_session_deaths
        GROUP BY distinct_guild_session_deaths.id        
    ) guild_ranked_deaths ON guild_ranked_deaths.id = g.id;

COMMIT;
