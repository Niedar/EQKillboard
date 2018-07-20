-- Revert eqkillboard-database:0001_initial_schema from pg

BEGIN;

DROP TABLE killmail;
DROP TABLE killmail_raw;
DROP TABLE character;
DROP TABLE guild;
DROP TABLE zone;
DROP TABLE class;
DROP TABLE status_type;

COMMIT;
