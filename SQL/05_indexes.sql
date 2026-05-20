-- ------------------------------------------------------------
-- LISTAR TODOS LOS INDICES DEL ESQUEMA
-- ------------------------------------------------------------
SELECT
    i.index_name,
    i.table_name,
    i.index_type,          -- NORMAL, BITMAP, FUNCTION-BASED NORMAL, etc.
    i.uniqueness,          -- UNIQUE / NONUNIQUE
    i.status,              -- VALID / UNUSABLE
    i.partitioned,
    LISTAGG(ic.column_name, ', ')
        WITHIN GROUP (ORDER BY ic.column_position) AS columns
FROM ALL_INDEXES     i
INNER JOIN ALL_IND_COLUMNS ic
    ON  i.index_name  = ic.index_name
    AND i.owner       = ic.index_owner
WHERE i.owner = UPPER(:owner)
GROUP BY
    i.index_name,
    i.table_name,
    i.index_type,
    i.uniqueness,
    i.status,
    i.partitioned
ORDER BY i.table_name, i.index_name;


-- ------------------------------------------------------------
-- DETALLE DE COLUMNAS DE UN INDICE ESPECIFICO
-- ------------------------------------------------------------
SELECT
    ic.column_name,
    ic.column_position,
    ic.descend,            -- ASC / DESC
    ic.column_length
FROM ALL_IND_COLUMNS ic
WHERE ic.index_owner = UPPER(:owner)
  AND ic.index_name  = UPPER(:index_name)
ORDER BY ic.column_position;


-- ------------------------------------------------------------
-- INDICES DE UNA TABLA ESPECIFICA
-- ------------------------------------------------------------
SELECT
    i.index_name,
    i.index_type,
    i.uniqueness,
    i.status,
    LISTAGG(ic.column_name, ', ')
        WITHIN GROUP (ORDER BY ic.column_position) AS columns
FROM ALL_INDEXES     i
INNER JOIN ALL_IND_COLUMNS ic
    ON  i.index_name = ic.index_name
    AND i.owner      = ic.index_owner
WHERE i.owner      = UPPER(:owner)
  AND i.table_name = UPPER(:table_name)
GROUP BY
    i.index_name, i.index_type, i.uniqueness, i.status
ORDER BY i.uniqueness DESC, i.index_name;

