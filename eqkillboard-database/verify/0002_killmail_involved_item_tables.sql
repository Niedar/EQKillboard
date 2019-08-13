-- Verify eqkillboard-database:0002_killmail_involed_item_tables on pg

BEGIN;

SELECT * FROM killmail_involved;
SELECT * FROM item;
SELECT * FROM item_sale;
SELECT * FROM item_stats;
SELECT looted_item, looted_by FROM killmail;

ROLLBACK;
