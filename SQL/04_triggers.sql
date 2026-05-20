-- ------------------------------------------------------------
-- ALL TRIGGERS SCHEEMA
-- ------------------------------------------------------------
SELECT
    t.trigger_name,
    t.trigger_type,        -- 'BEFORE EACH ROW', 'AFTER STATEMENT', etc.
    t.triggering_event,    -- 'INSERT', 'UPDATE', 'DELETE', combinaciones
    t.table_owner,
    t.table_name,
    t.status,              -- ENABLED / DISABLED
    t.action_type          -- 'PL/SQL' (siempre en Oracle XE)
FROM ALL_TRIGGERS t
WHERE t.owner = UPPER(:owner)
ORDER BY t.table_name, t.trigger_name;


-- ------------------------------------------------------------
-- DETALLE COMPLETO + CUERPO DE UN TRIGGER
-- ------------------------------------------------------------
SELECT
    t.trigger_name,
    t.trigger_type,
    t.triggering_event,
    t.table_owner,
    t.table_name,
    t.column_name,         -- si es un column-level trigger
    t.referencing_names,   -- alias OLD y NEW
    t.when_clause,         -- condicion WHEN (si aplica)
    t.status,
    t.description,
    t.action_type,
    t.trigger_body         -- cuerpo PL/SQL del trigger
FROM ALL_TRIGGERS t
WHERE t.owner        = UPPER(:owner)
  AND t.trigger_name = UPPER(:trigger_name);


-- ------------------------------------------------------------
-- TRIGGERS ASOCIADOS A UNA TABLA ESPECIFICA
-- ------------------------------------------------------------
SELECT
    t.trigger_name,
    t.trigger_type,
    t.triggering_event,
    t.status
FROM ALL_TRIGGERS t
WHERE t.table_owner = UPPER(:owner)
  AND t.table_name  = UPPER(:table_name)
ORDER BY t.trigger_type, t.trigger_name;


