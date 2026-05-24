# DatabaseManager Tool
**Clase:** Teoría de Base de Datos II  
**SGBD:** Oracle XE  
**Tecnología:** C# — Windows Forms — .NET Framework 4.7.2  
**Driver:** Oracle.ManagedDataAccess (ODP.NET) — sin ORM  
**Alumno:** Steve Valladares 22341344  
---

## Descripción

Herramienta administrativa de base de datos para Oracle XE que interactúa directamente con las **system tables** del SGBD. Permite gestionar conexiones, visualizar objetos de la base de datos, generar DDL desde metadata y ejecutar sentencias SQL.

---

## Estructura del proyecto

```
DatabaseManager/
├── SQL/                          # Consultas de referencia a system tables (Fase 1)
│   ├── 01_tables.sql
│   ├── 02_views.sql
│   ├── 03_procedures_functions.sql
│   ├── 04_triggers.sql
│   ├── 05_indexes.sql
│   ├── 06_sequences.sql
│   ├── 07_tablespaces_users.sql
│   └── 08_ddl_generator.sql
└── src/DatabaseManager/
    ├── Data/                     # Capa de acceso a datos
    │   ├── OracleConnectionManager.cs
    │   ├── ObjectRepository.cs
    │   ├── DataMapper.cs
    │   └── ConnectionStorage.cs
    ├── Models/                   # Clases tipadas de Oracle
    │   └── OracleModels.cs
    └── Forms/                    # Interfaz de usuario -> (UI diseñada con el Apoyo de Inteligencia Artiicial) -> Permitido por el Docente
        ├── FormLogin.cs
        ├── FormMain.cs
        ├── FormCreateTable.cs
        └── FormCreateView.cs
```

---

## System tables utilizadas

| Vista Oracle         | Uso en la herramienta                        |
|----------------------|----------------------------------------------|
| `ALL_TABLES`         | Listar tablas del esquema                    |
| `ALL_TAB_COLUMNS`    | Columnas de una tabla                        |
| `ALL_CONSTRAINTS`    | Constraints (PK, FK, UK, Check)              |
| `ALL_CONS_COLUMNS`   | Columnas de cada constraint                  |
| `ALL_VIEWS`          | Listar vistas y su código fuente             |
| `ALL_OBJECTS`        | Procedimientos, funciones, paquetes          |
| `ALL_SOURCE`         | Código fuente de objetos PL/SQL              |
| `ALL_TRIGGERS`       | Triggers del esquema                         |
| `ALL_INDEXES`        | Índices del esquema                          |
| `ALL_IND_COLUMNS`    | Columnas de cada índice                      |
| `ALL_SEQUENCES`      | Secuencias del esquema                       |
| `ALL_ARGUMENTS`      | Parámetros de procedimientos y funciones     |
| `ALL_DEPENDENCIES`   | Dependencias entre objetos                   |
| `DBA_TABLESPACES`    | Tablespaces (requiere rol DBA)               |
| `DBA_DATA_FILES`     | Archivos de datos de tablespaces             |
| `DBA_FREE_SPACE`     | Espacio libre por tablespace                 |
| `DBA_USERS`          | Usuarios de la base de datos                 |
| `DBA_ROLE_PRIVS`     | Roles asignados a usuarios                   |
| `DBA_SYS_PRIVS`      | Privilegios de sistema de usuarios           |
| `V$SESSION`          | Sesiones activas                             |
| `DBMS_METADATA`      | Generación de DDL desde metadata             |

> **Nota:** Se usa `ALL_*` en lugar de `DBA_*` donde es posible, para que la herramienta funcione con cualquier usuario válido, no solo con DBA. Las vistas `DBA_*` requieren el rol DBA o `SELECT ANY DICTIONARY`.

---

## Limitaciones conocidas de Oracle XE

### 1. `information_schema` no existe en Oracle
Oracle no implementa el esquema estándar `information_schema` de SQL estándar. Toda la metadata se obtiene exclusivamente a través de las vistas del diccionario de datos (`ALL_*`, `DBA_*`, `V$*`). Esta herramienta cumple este requisito por diseño.

### 2. Restricciones de Oracle XE (Express Edition)
Oracle XE tiene las siguientes limitaciones de recursos que no existen en Oracle Standard/Enterprise:
- Máximo **2 CPUs** utilizadas
- Máximo **2 GB de RAM**
- Máximo **12 GB** de datos de usuario en disco
- Solo **una base de datos** por instalación (service name `XE` fijo)

### 3. Tablespaces y Usuarios requieren privilegios DBA
Las vistas `DBA_TABLESPACES` y `DBA_USERS` solo son accesibles para usuarios con el rol `DBA` o el privilegio `SELECT ANY DICTIONARY`. Si el usuario conectado no tiene estos privilegios, la herramienta muestra un mensaje de error claro en lugar de fallar silenciosamente.

### 4. `PACKAGE BODY` y DDL
`DBMS_METADATA.GET_DDL` para `PACKAGE BODY` puede requerir privilegios adicionales dependiendo del esquema. Si el usuario no tiene acceso, la pestaña DDL mostrará el error correspondiente sin interrumpir el resto de la aplicación.

### 5. Tipos de columna LONG en ALL_VIEWS
La columna `TEXT` de `ALL_VIEWS` es de tipo `LONG` en Oracle, un tipo de dato legado. Se convierte a `CLOB` con `TO_CLOB(v.text)` para su correcta lectura desde ODP.NET en C#.

### 6. Parámetros con `:nombre` en ODP.NET
Oracle usa `:parametro` como sintaxis de bind variables, a diferencia de SQL Server (`@param`) o MySQL (`?`). Visual Studio puede subrayar en rojo los archivos `.sql` al no reconocer esta sintaxis, pero el código es 100% correcto para Oracle.

### 7. Punto y coma en ODP.NET
ODP.NET (el driver oficial de Oracle para .NET) **no acepta** punto y coma al final de sentencias SQL individuales. La herramienta elimina automáticamente el `;` final antes de ejecutar cualquier sentencia.

### 8. Generadores/Sequences
Oracle XE no tiene `GENERATORS` como Firebird. El equivalente son las `SEQUENCES`. Desde Oracle 12c también se pueden usar columnas `GENERATED AS IDENTITY`, pero Oracle XE 11g solo soporta sequences tradicionales.

### 9. Passwords no se persisten
Por razones de seguridad, la herramienta guarda los datos de conexión (host, puerto, service name, usuario) en un archivo `connections.json` local, pero **nunca guarda el password**. El usuario debe ingresarlo manualmente cada vez que conecta.

---

## Restricciones técnicas cumplidas

| Restricción                          | Cumplimiento          |
|--------------------------------------|-----------------------|
| Sin `information_schema`             | ✅ Solo `ALL_*`/`DBA_*` |
| Sin ORM (SQLAlchemy, EF, Hibernate)  | ✅ ODP.NET directo    |
| Sin librerías de administración      | ✅ Solo Oracle driver |
| Interfaz Web o Desktop               | ✅ Windows Forms      |
| Uso explícito de system tables       | ✅ Documentado arriba |
| Creación visual de tablas y vistas   | ✅ FormCreateTable / FormCreateView |
| Generación de DDL desde metadata     | ✅ DBMS_METADATA      |

---

## Cómo ejecutar

1. Tener Oracle XE instalado y corriendo en `localhost:1521/XE`
2. Abrir la solución en Visual Studio
3. Compilar (`Ctrl+Shift+B`)
4. Ejecutar (`F5`)
5. En el login ingresar: Host `localhost`, Puerto `1521`, Service `XE`, usuario y password de Oracle
