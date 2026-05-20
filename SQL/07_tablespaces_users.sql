-- ============================================================
-- SECCION A: TABLESPACES
-- ============================================================

-- ------------------------------------------------------------
--  LISTAR TODOS LOS TABLESPACES
-- ------------------------------------------------------------
SELECT
    ts.tablespace_name,
    ts.status,             -- ONLINE / OFFLINE / READ ONLY
    ts.contents,           -- PERMANENT / TEMPORARY / UNDO
    ts.logging,
    ts.block_size,
    ts.extent_management,  -- LOCAL / DICTIONARY
    ts.segment_space_management
FROM DBA_TABLESPACES ts
ORDER BY ts.contents, ts.tablespace_name;


-- ------------------------------------------------------------
--  TABLESPACES CON ESPACIO USADO Y LIBRE
-- ------------------------------------------------------------
SELECT
    ts.tablespace_name,
    ts.status,
    ts.contents,
    NVL(df.total_mb, 0)                                      AS total_mb,
    NVL(fs.free_mb,  0)                                      AS free_mb,
    NVL(df.total_mb, 0) - NVL(fs.free_mb, 0)                AS used_mb,
    ROUND(
        (NVL(df.total_mb, 0) - NVL(fs.free_mb, 0))
        / NULLIF(NVL(df.total_mb, 0), 0) * 100
    , 2)                                                     AS used_pct
FROM DBA_TABLESPACES ts
LEFT JOIN (
    SELECT tablespace_name,
           ROUND(SUM(bytes) / 1048576, 2) AS total_mb
    FROM DBA_DATA_FILES
    GROUP BY tablespace_name
) df ON ts.tablespace_name = df.tablespace_name
LEFT JOIN (
    SELECT tablespace_name,
           ROUND(SUM(bytes) / 1048576, 2) AS free_mb
    FROM DBA_FREE_SPACE
    GROUP BY tablespace_name
) fs ON ts.tablespace_name = fs.tablespace_name
ORDER BY ts.tablespace_name;


-- ============================================================
-- SECCION B: USUARIOS
-- ============================================================

-- ------------------------------------------------------------
-- 3. LISTAR TODOS LOS USUARIOS DE LA BASE DE DATOS
-- ------------------------------------------------------------
SELECT
    u.username,
    u.account_status,      -- OPEN / LOCKED / EXPIRED / etc.
    u.created,
    u.default_tablespace,
    u.temporary_tablespace,
    u.expiry_date,
    u.profile
FROM DBA_USERS u
ORDER BY u.username;


-- ------------------------------------------------------------
-- ROLES Y PRIVILEGIOS DE UN USUARIO
-- ------------------------------------------------------------
SELECT
    rp.granted_role,
    rp.admin_option,
    rp.default_role
FROM DBA_ROLE_PRIVS rp
WHERE rp.grantee = UPPER(:username)
ORDER BY rp.granted_role;


-- ------------------------------------------------------------
--  PRIVILEGIOS DE SISTEMA DE UN USUARIO
-- ------------------------------------------------------------
SELECT
    sp.privilege,
    sp.admin_option
FROM DBA_SYS_PRIVS sp
WHERE sp.grantee = UPPER(:username)
ORDER BY sp.privilege;


