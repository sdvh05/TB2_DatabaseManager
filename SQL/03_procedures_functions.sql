-- ------------------------------------------------------------
-- LISTAR TODOS LOS OBJETOS PL/SQL DEL ESQUEMA
-- ------------------------------------------------------------
SELECT
    o.object_name,
    o.object_type,
    o.status,          
    o.last_ddl_time,
    o.created
FROM ALL_OBJECTS o
WHERE o.owner       = UPPER(:owner)
  AND o.object_type IN ('PROCEDURE', 'FUNCTION', 'PACKAGE', 'PACKAGE BODY')
ORDER BY o.object_type, o.object_name;


-- ------------------------------------------------------------
-- CODIGO FUENTE DE UN OBJETO PL/SQL
-- ------------------------------------------------------------
SELECT
    s.line,
    s.text
FROM ALL_SOURCE s
WHERE s.owner = UPPER(:owner)
  AND s.name  = UPPER(:object_name)
  AND s.type  = UPPER(:object_type)
ORDER BY s.line;


-- ------------------------------------------------------------
-- PARAMETROS DE UN PROCEDIMIENTO O FUNCION
-- ------------------------------------------------------------
SELECT
    a.argument_name,
    a.position,
    a.sequence,
    a.data_type,
    a.in_out,          
    a.defaulted,
    a.default_value
FROM ALL_ARGUMENTS a
WHERE a.owner       = UPPER(:owner)
  AND a.object_name = UPPER(:object_name)
  AND a.package_name IS NULL    
ORDER BY a.sequence;


-- ------------------------------------------------------------
-- PARAMETROS DE UN PROCEDIMIENTO DENTRO DE UN PAQUETE
-- ------------------------------------------------------------
SELECT
    a.argument_name,
    a.position,
    a.data_type,
    a.in_out,
    a.defaulted
FROM ALL_ARGUMENTS a
WHERE a.owner        = UPPER(:owner)
  AND a.package_name = UPPER(:package_name)
  AND a.object_name  = UPPER(:procedure_name)
ORDER BY a.sequence;


-- ------------------------------------------------------------
-- ERRORES DE COMPILACION DE UN OBJETO INVALIDO
-- ------------------------------------------------------------
SELECT
    e.line,
    e.position,
    e.text AS error_message,
    e.attribute        
FROM ALL_ERRORS e
WHERE e.owner = UPPER(:owner)
  AND e.name  = UPPER(:object_name)
  AND e.type  = UPPER(:object_type)
ORDER BY e.sequence;


