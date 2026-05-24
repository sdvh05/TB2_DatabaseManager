# SQL — Consultas a System Tables de Oracle XE

Contiene las consultas de referencia usadas por la herramienta para obtener metadata directamente desde las system tables de Oracle. **Sin uso de `information_schema`.**

---

## Archivos

| Archivo | Descripción | System tables usadas |
|---|---|---|
| `01_tables.sql` | Listar tablas, columnas, constraints y comentarios | `ALL_TABLES`, `ALL_TAB_COLUMNS`, `ALL_CONSTRAINTS`, `ALL_CONS_COLUMNS` |
| `02_views.sql` | Listar vistas, su código fuente y dependencias | `ALL_VIEWS`, `ALL_TAB_COLUMNS`, `ALL_DEPENDENCIES` |
| `03_procedures_functions.sql` | Procedimientos, funciones, paquetes y sus parámetros | `ALL_OBJECTS`, `ALL_SOURCE`, `ALL_ARGUMENTS`, `ALL_ERRORS` |
| `04_triggers.sql` | Listar triggers y ver su cuerpo PL/SQL | `ALL_TRIGGERS` |
| `05_indexes.sql` | Listar índices y sus columnas | `ALL_INDEXES`, `ALL_IND_COLUMNS` |
| `06_sequences.sql` | Listar secuencias del esquema | `ALL_SEQUENCES` |
| `07_tablespaces_users.sql` | Tablespaces con espacio usado/libre, usuarios y sus privilegios | `DBA_TABLESPACES`, `DBA_DATA_FILES`, `DBA_FREE_SPACE`, `DBA_USERS`, `DBA_ROLE_PRIVS`, `DBA_SYS_PRIVS` |
| `08_ddl_generator.sql` | Generación de DDL desde metadata con `DBMS_METADATA` | `DBMS_METADATA.GET_DDL`, `DUAL` |

---

## Notas técnicas

- Los parámetros se escriben con `:nombre` — sintaxis oficial de Oracle (bind variables). Visual Studio puede subrayarlos en rojo pero son 100% válidos en Oracle y ODP.NET.
- Las vistas `ALL_*` funcionan con cualquier usuario válido. Las vistas `DBA_*` requieren rol DBA.
- Estos archivos son **referencia de consultas**. La capa `Data/` los implementa en C# llamándolos mediante `OracleCommand` con parámetros tipados.
