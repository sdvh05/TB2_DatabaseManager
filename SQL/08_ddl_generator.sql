
-- ------------------------------------------------------------
BEGIN
    DBMS_METADATA.SET_TRANSFORM_PARAM(
        DBMS_METADATA.SESSION_TRANSFORM, 'SQLTERMINATOR', TRUE);
    DBMS_METADATA.SET_TRANSFORM_PARAM(
        DBMS_METADATA.SESSION_TRANSFORM, 'PRETTY',        TRUE);
    DBMS_METADATA.SET_TRANSFORM_PARAM(
        DBMS_METADATA.SESSION_TRANSFORM, 'STORAGE',       FALSE);
    DBMS_METADATA.SET_TRANSFORM_PARAM(
        DBMS_METADATA.SESSION_TRANSFORM, 'TABLESPACE',    FALSE);
END;
/


-- ------------------------------------------------------------
-- 1. DDL PARA CUALQUIER OBJETO
-- ------------------------------------------------------------
SELECT DBMS_METADATA.GET_DDL(
    UPPER(:object_type),
    UPPER(:object_name),
    UPPER(:owner)
) AS ddl_text
FROM DUAL;


-- ------------------------------------------------------------
-- 2. DDL DE UNA TABLA CON SUS CONSTRAINTS E INDICES
-- ------------------------------------------------------------
SELECT DBMS_METADATA.GET_DDL('TABLE', UPPER(:table_name), UPPER(:owner))
    || CHR(10)
    || NVL(
        (SELECT LISTAGG(
                    DBMS_METADATA.GET_DDL('INDEX', index_name, UPPER(:owner)),
                    CHR(10)
                ) WITHIN GROUP (ORDER BY index_name)
         FROM ALL_INDEXES
         WHERE owner      = UPPER(:owner)
           AND table_name = UPPER(:table_name)
           AND index_name NOT IN (
               SELECT constraint_name
               FROM ALL_CONSTRAINTS
               WHERE owner      = UPPER(:owner)
                 AND table_name = UPPER(:table_name)
           )
        ),
        '-- (sin indices adicionales)'
    ) AS ddl_completo
FROM DUAL;


-- ------------------------------------------------------------
-- 3. DDL DE TODOS LOS OBJETOS DE UN ESQUEMA (por tipo)
-- ------------------------------------------------------------
SELECT
    o.object_name,
    DBMS_METADATA.GET_DDL(
        UPPER(:object_type),
        o.object_name,
        o.owner
    ) AS ddl_text
FROM ALL_OBJECTS o
WHERE o.owner       = UPPER(:owner)
  AND o.object_type = UPPER(:object_type)
  AND o.status      = 'VALID'
ORDER BY o.object_name;


-- ------------------------------------------------------------
-- 4. DDL DE UNA VISTA 
-- ------------------------------------------------------------
SELECT
    'CREATE OR REPLACE VIEW '
    || UPPER(:owner) || '.' || v.view_name
    || ' AS' || CHR(10)
    || TO_CLOB(v.text)
    || ';' AS ddl_text
FROM ALL_VIEWS v
WHERE v.owner     = UPPER(:owner)
  AND v.view_name = UPPER(:view_name);


-- ------------------------------------------------------------
-- 5. DDL DE UNA SECUENCIA 
-- ------------------------------------------------------------
SELECT
    'CREATE SEQUENCE '
    || UPPER(:owner) || '.' || s.sequence_name || CHR(10)
    || '    START WITH '    || s.last_number   || CHR(10)
    || '    INCREMENT BY '  || s.increment_by  || CHR(10)
    || '    MINVALUE '      || s.min_value     || CHR(10)
    || '    MAXVALUE '      || s.max_value     || CHR(10)
    || CASE s.cycle_flag
           WHEN 'Y' THEN '    CYCLE'
           ELSE '    NOCYCLE'
       END || CHR(10)
    || '    CACHE '         || s.cache_size    || ';' AS ddl_text
FROM ALL_SEQUENCES s
WHERE s.sequence_owner = UPPER(:owner)
  AND s.sequence_name  = UPPER(:sequence_name);
