-- Revert eqkillboard-database:0002_killmail_involed_item_tables from pg

BEGIN;

ALTER TABLE killmail DROP COLUMN looted_item;
ALTER TABLE killmail DROP COLUMN looted_by;
ALTER TABLE character DROP COLUMN is_npc;
DROP TABLE killmail_involved;
DROP VIEW item_stats;
DROP TABLE item_sale;
DROP TABLE item;

COMMIT;
