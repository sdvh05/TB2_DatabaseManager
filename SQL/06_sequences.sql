-- ------------------------------------------------------------
--   ALL SECUENCIAS DEL ESQUEMA
-- ------------------------------------------------------------
SELECT
    s.sequence_name,
    s.min_value,
    s.max_value,
    s.increment_by,
    s.cycle_flag,         
    s.order_flag,
    s.cache_size,          
    s.last_number          
FROM ALL_SEQUENCES s
WHERE s.sequence_owner = UPPER(:owner)
ORDER BY s.sequence_name;


-- ------------------------------------------------------------
-- DETALLE DE UNA SECUENCIA ESPECIFICA
-- ------------------------------------------------------------
SELECT
    s.sequence_name,
    s.min_value,
    s.max_value,
    s.increment_by,
    s.cycle_flag,
    s.order_flag,
    s.cache_size,
    s.last_number
FROM ALL_SEQUENCES s
WHERE s.sequence_owner = UPPER(:owner)
  AND s.sequence_name  = UPPER(:sequence_name);

