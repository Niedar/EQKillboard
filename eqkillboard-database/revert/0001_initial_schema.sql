-- Revert eqkillboard-database:0001_initial_schema from pg

BEGIN;

DROP TABLE killmail;
DROP TABLE killmail_raw;
DROP FUNCTION search_characters(TEXT);
DROP TABLE character;
DROP FUNCTION search_guilds(TEXT);
DROP TABLE guild;
DROP FUNCTION search_zones(TEXT);
DROP TABLE zone;
DROP FUNCTION search_classes(TEXT);
DROP TABLE class;
DROP TABLE status_type;
DROP EXTENSION pg_trgm;

COMMIT;
