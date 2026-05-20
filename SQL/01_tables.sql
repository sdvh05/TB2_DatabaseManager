
-- ------------------------------------------------------------
-- All TABLES de un Eschema (owner)
-- ------------------------------------------------------------
SELECT
    t.table_name,
    t.tablespace_name,
    t.num_rows,
    t.blocks,
    t.status,
    t.last_analyzed
FROM ALL_TABLES t
WHERE t.owner = UPPER(:owner)
ORDER BY t.table_name;


-- ------------------------------------------------------------
-- COLUMNAS DE UNA TABLA ESPECIFICA
-- ------------------------------------------------------------
SELECT
    c.column_id,
    c.column_name,
    c.data_type,
    c.data_length,
    c.data_precision,
    c.data_scale,
    c.nullable,
    c.data_default
FROM ALL_TAB_COLUMNS c
WHERE c.owner      = UPPER(:owner)
  AND c.table_name = UPPER(:table_name)
ORDER BY c.column_id;


-- ------------------------------------------------------------
-- CONSTRAINTS DE UNA TABLA
-- ------------------------------------------------------------
SELECT
    con.constraint_name,
    con.constraint_type,
    col.column_name,
    col.position,
    con.r_owner,
    con.r_constraint_name,
    con.status,
    con.search_condition
FROM ALL_CONSTRAINTS  con
INNER JOIN ALL_CONS_COLUMNS col
    ON  con.constraint_name = col.constraint_name
    AND con.owner           = col.owner
WHERE con.owner      = UPPER(:owner)
  AND con.table_name = UPPER(:table_name)
ORDER BY con.constraint_type, col.position;


-- ------------------------------------------------------------
-- COMENTARIOS DE TABLA Y COLUMNAS
-- ------------------------------------------------------------

-- Comentario de la tabla
SELECT comments
FROM ALL_TAB_COMMENTS
WHERE owner      = UPPER(:owner)
  AND table_name = UPPER(:table_name);

-- Comentarios de columnas
SELECT
    column_name,
    comments
FROM ALL_COL_COMMENTS
WHERE owner      = UPPER(:owner)
  AND table_name = UPPER(:table_name)
ORDER BY column_name;


-- ------------------------------------------------------------
-- ESTADISTICAS DE UNA TABLA
-- ------------------------------------------------------------
SELECT
    t.table_name,
    t.num_rows,
    t.blocks,
    t.avg_row_len,
    t.last_analyzed,
    t.partitioned,
    t.row_movement
FROM ALL_TABLES t
WHERE t.owner      = UPPER(:owner)
  AND t.table_name = UPPER(:table_name);


