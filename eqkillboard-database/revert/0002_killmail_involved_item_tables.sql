-- Revert eqkillboard-database:0002_killmail_involed_item_tables from pg

BEGIN;

ALTER TABLE killmail DROP COLUMN looted_item;
ALTER TABLE killmail DROP COLUMN looted_by;
DROP TABLE killmail_involved;
DROP TABLE item_sale;
DROP TABLE item_stats;
DROP TABLE item;



COMMIT;
