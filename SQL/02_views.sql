
-- ------------------------------------------------------------
--  Todas las Views de un Schema
-- ------------------------------------------------------------
SELECT
    v.view_name,
    v.text_length,
    v.read_only
FROM ALL_VIEWS v
WHERE v.owner = UPPER(:owner)
ORDER BY v.view_name;


-- ------------------------------------------------------------
-- Codigo de una Vista
-- ------------------------------------------------------------
SELECT
    v.view_name,
    TO_CLOB(v.text) AS view_text
FROM ALL_VIEWS v
WHERE v.owner     = UPPER(:owner)
  AND v.view_name = UPPER(:view_name);


-- ------------------------------------------------------------
-- CoLumnas expuestas por una View
-- ------------------------------------------------------------
SELECT
    c.column_id,
    c.column_name,
    c.data_type,
    c.data_length,
    c.nullable
FROM ALL_TAB_COLUMNS c
WHERE c.owner      = UPPER(:owner)
  AND c.table_name = UPPER(:view_name)
ORDER BY c.column_id;


-- ------------------------------------------------------------
-- DEPENDENCIAS DE UNA VISTA
-- ------------------------------------------------------------
SELECT
    d.referenced_owner,
    d.referenced_name,
    d.referenced_type
FROM ALL_DEPENDENCIES d
WHERE d.owner = UPPER(:owner)
  AND d.name  = UPPER(:view_name)
  AND d.type  = 'VIEW'
ORDER BY d.referenced_type, d.referenced_name;


