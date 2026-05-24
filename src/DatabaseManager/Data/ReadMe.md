# Data — Capa de acceso a Oracle XE

Contiene todas las clases que interactúan directamente con Oracle XE usando **ODP.NET** (`Oracle.ManagedDataAccess`). Sin ORM, sin `information_schema`, conexión directa via SQL y system tables.

---

## Archivos

### `OracleConnectionManager.cs`
Gestiona la conexión activa y las conexiones guardadas.

| Método | Descripción |
|---|---|
| `Connect(host, port, service, user, pass)` | Abre una conexión a Oracle XE |
| `Disconnect()` | Cierra y libera la conexión activa |
| `GetConnection()` | Retorna la `OracleConnection` activa |
| `SaveConnection(name, ...)` | Guarda una conexión con nombre (en memoria) |
| `ConnectFromSaved(name)` | Conecta usando una conexión guardada |
| `ExecuteQuery(sql, params)` | Ejecuta un SELECT y retorna `DataTable` |
| `ExecuteNonQuery(sql, params)` | Ejecuta DDL/DML sin retorno de filas |

---

### `ObjectRepository.cs`
Consulta las system tables de Oracle para obtener metadata de todos los objetos. Cada método corresponde a un tipo de objeto.

| Método | System table usada |
|---|---|
| `GetTables(owner)` | `ALL_TABLES` |
| `GetTableColumns(owner, table)` | `ALL_TAB_COLUMNS` |
| `GetTableConstraints(owner, table)` | `ALL_CONSTRAINTS`, `ALL_CONS_COLUMNS` |
| `GetViews(owner)` | `ALL_VIEWS` |
| `GetViewSource(owner, view)` | `ALL_VIEWS` |
| `GetProgrammingObjects(owner)` | `ALL_OBJECTS` |
| `GetObjectSource(owner, name, type)` | `ALL_SOURCE` |
| `GetTriggers(owner)` | `ALL_TRIGGERS` |
| `GetTriggerDetail(owner, trigger)` | `ALL_TRIGGERS` |
| `GetIndexes(owner)` | `ALL_INDEXES`, `ALL_IND_COLUMNS` |
| `GetSequences(owner)` | `ALL_SEQUENCES` |
| `GetTablespaces()` | `DBA_TABLESPACES`, `DBA_DATA_FILES`, `DBA_FREE_SPACE` |
| `GetUsers()` | `DBA_USERS` |
| `GetDDL(type, name, owner)` | `DBMS_METADATA.GET_DDL` |

---

### `DataMapper.cs`
Convierte `DataTable` (resultado crudo de Oracle) en listas de Models tipados. Centraliza todo el mapeo de columnas en un solo lugar.

| Método | Retorna |
|---|---|
| `ToTableList(dt)` | `List<TableInfo>` |
| `ToColumnList(dt)` | `List<ColumnInfo>` |
| `ToConstraintList(dt)` | `List<ConstraintInfo>` |
| `ToViewList(dt)` | `List<ViewInfo>` |
| `ToProgrammingObjectList(dt)` | `List<ProgrammingObjectInfo>` |
| `ToTriggerList(dt)` | `List<TriggerInfo>` |
| `ToIndexList(dt)` | `List<IndexInfo>` |
| `ToSequenceList(dt)` | `List<SequenceInfo>` |
| `ToTablespaceList(dt)` | `List<TablespaceInfo>` |
| `ToUserList(dt)` | `List<UserInfo>` |

---

### `ConnectionStorage.cs`
Persiste las conexiones guardadas en `connections.json` en la carpeta del ejecutable. **No guarda passwords.**

| Método | Descripción |
|---|---|
| `Save(List<ConnectionInfo>)` | Escribe el archivo JSON en disco |
| `Load()` | Lee y parsea el archivo JSON |

El archivo `connections.json` se genera automáticamente en la primera conexión guardada:
```json
[
  {
    "Name":        "Local XE",
    "Host":        "localhost",
    "Port":         1521,
    "ServiceName": "XE",
    "Username":    "SYSTEM"
  }
]
```
