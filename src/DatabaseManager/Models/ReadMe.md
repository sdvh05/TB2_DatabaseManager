
# Models — Clases tipadas de Oracle

Contiene las clases C# que representan cada objeto de Oracle como un tipo fuertemente tipado. Evitan trabajar directamente con `DataTable` y `DataRow["columna"]` en la UI.

---

## Archivo

### `OracleModels.cs`

| Clase | Representa | Origen |
|---|---|---|
| `ConnectionInfo` | Una conexión guardada (sin password) | Manual |
| `TableInfo` | Una fila de `ALL_TABLES` | `ALL_TABLES` |
| `ColumnInfo` | Una columna de tabla o vista | `ALL_TAB_COLUMNS` |
| `ConstraintInfo` | Un constraint (PK, FK, UK, Check) | `ALL_CONSTRAINTS` |
| `ViewInfo` | Una vista del esquema | `ALL_VIEWS` |
| `ProgrammingObjectInfo` | Procedimiento, función o paquete | `ALL_OBJECTS` |
| `TriggerInfo` | Un trigger | `ALL_TRIGGERS` |
| `IndexInfo` | Un índice con sus columnas | `ALL_INDEXES` |
| `SequenceInfo` | Una secuencia | `ALL_SEQUENCES` |
| `TablespaceInfo` | Un tablespace con espacio usado/libre | `DBA_TABLESPACES` |
| `UserInfo` | Un usuario de la base de datos | `DBA_USERS` |

---

## Propiedades calculadas destacadas

- `ColumnInfo.FullDataType` — construye el tipo completo legible, ej: `VARCHAR2(100)`, `NUMBER(10,2)`
- `ConstraintInfo.TypeDescription` — convierte `P/R/U/C` en `Primary Key / Foreign Key / Unique / Check`
- `TablespaceInfo.UsedPercent` — calcula el % de espacio usado
- `ProgrammingObjectInfo.IsValid` — `true` si `STATUS = 'VALID'`
- `TriggerInfo.IsEnabled` — `true` si `STATUS = 'ENABLED'`

---

## Cómo se usan

La capa `Data/DataMapper.cs` convierte los `DataTable` retornados por Oracle en listas de estos modelos:

```csharp
DataTable dt            = _repo.GetTables(owner);
List<TableInfo> tablas  = DataMapper.ToTableList(dt);
```
