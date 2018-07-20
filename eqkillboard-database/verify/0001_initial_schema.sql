-- Verify eqkillboard-database:0001_initial_schema on pg

BEGIN;

SELECT * FROM guild;
SELECT * FROM zone;
SELECT * FROM class;
SELECT * FROM status_type;
SELECT * FROM character;
SELECT * FROM killmail;
SELECT * FROM killmail_raw;

ROLLBACK;
