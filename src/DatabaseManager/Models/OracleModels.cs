using System;

namespace DatabaseManager.Models
{
    // ============================================================
    // OracleConnection Info
    // ============================================================
    public class ConnectionInfo
    {
        public string Name           { get; set; }   
        public string Host           { get; set; }   
        public int    Port           { get; set; }   
        public string ServiceName    { get; set; }   
        public string Username       { get; set; }   

        public override string ToString()
        {
            return Name + " (" + Username + "@" + Host + ":" + Port + "/" + ServiceName + ")";
        }
    }

    // ============================================================
    // TABLA
    // ============================================================
    public class TableInfo
    {
        public string   TableName      { get; set; }
        public string   TablespaceName { get; set; }
        public long     NumRows        { get; set; }
        public string   Status         { get; set; }
        public DateTime? LastAnalyzed  { get; set; }

        public override string ToString()
        {
            return TableName;
        }
    }

    // ============================================================
    // COLUMNA DE TABLA
    // ============================================================
    public class ColumnInfo
    {
        public int     ColumnId       { get; set; }
        public string  ColumnName     { get; set; }
        public string  DataType       { get; set; }
        public int     DataLength     { get; set; }
        public int?    DataPrecision  { get; set; }
        public int?    DataScale      { get; set; }
        public bool    IsNullable     { get; set; }   
        public string  DataDefault    { get; set; }

        // Muestra el tipo completo ej: "VARCHAR2(100)", "NUMBER(10,2)"
        public string FullDataType
        {
            get
            {
                if (DataType == "NUMBER" && DataPrecision.HasValue)
                    return "NUMBER(" + DataPrecision + "," + DataScale + ")";

                if (DataType == "VARCHAR2" || DataType == "CHAR" || DataType == "NVARCHAR2")
                    return DataType + "(" + DataLength + ")";

                return DataType;
            }
        }

        public override string ToString()
        {
            return ColumnName + " " + FullDataType;
        }
    }

    // ============================================================
    // ALL_CONSTRAINTS + ALL_CONS_COLUMNS
    // ============================================================
    public class ConstraintInfo
    {
        public string ConstraintName    { get; set; }
        public string ConstraintType    { get; set; }  // P, R, U, C
        public string ColumnName        { get; set; }
        public int    Position          { get; set; }
        public string RConstraintName   { get; set; }  // FK referencia
        public string Status            { get; set; }

        public string TypeDescription
        {
            get
            {
                switch (ConstraintType)
                {
                    case "P": return "Primary Key";
                    case "R": return "Foreign Key";
                    case "U": return "Unique";
                    case "C": return "Check";
                    default:  return ConstraintType;
                }
            }
        }
    }

    // ============================================================
    // ALL_VIEWS
    // ============================================================
    public class ViewInfo
    {
        public string ViewName   { get; set; }
        public long   TextLength { get; set; }
        public bool   IsReadOnly { get; set; }  // 'Y' -> true

        public override string ToString()
        {
            return ViewName;
        }
    }

    // ============================================================
    // OBJETO PL/SQL (Procedimiento, Funcion, 
    // ============================================================
    public class ProgrammingObjectInfo
    {
        public string   ObjectName  { get; set; }
        public string   ObjectType  { get; set; }  // PROCEDURE, FUNCTION, PACKAGE, PACKAGE BODY
        public string   Status      { get; set; }  
        public DateTime? LastDdlTime { get; set; }
        public DateTime? Created     { get; set; }

        public bool IsValid
        {
            get { return Status == "VALID"; }
        }

        public override string ToString()
        {
            return ObjectType + ": " + ObjectName;
        }
    }

    // ============================================================
    // ALL_TRIGGERS
    // ============================================================
    public class TriggerInfo
    {
        public string TriggerName      { get; set; }
        public string TriggerType      { get; set; }   // BEFORE/AFTER 
        public string TriggeringEvent  { get; set; }   // INSERT, UPDATE, DELETE
        public string TableName        { get; set; }
        public string Status           { get; set; }  
        public string WhenClause       { get; set; }
        public string TriggerBody      { get; set; }

        public bool IsEnabled
        {
            get { return Status == "ENABLED"; }
        }

        public override string ToString()
        {
            return TriggerName + " ON " + TableName;
        }
    }

    // ============================================================
    // ALL_INDEXES
    // ============================================================
    public class IndexInfo
    {
        public string IndexName  { get; set; }
        public string TableName  { get; set; }
        public string IndexType  { get; set; }   
        public string Uniqueness { get; set; }   
        public string Status     { get; set; }   
        public string Columns    { get; set; }   

        public bool IsUnique
        {
            get { return Uniqueness == "UNIQUE"; }
        }

        public override string ToString()
        {
            return IndexName + " (" + Columns + ")";
        }
    }

    // ============================================================
    // ALL_SEQUENCES
    // ============================================================
    public class SequenceInfo
    {
        public string SequenceName { get; set; }
        public long   MinValue     { get; set; }
        public long   MaxValue     { get; set; }
        public long   IncrementBy  { get; set; }
        public bool   IsCyclic     { get; set; }  
        public long   CacheSize    { get; set; }
        public long   LastNumber   { get; set; }

        public override string ToString()
        {
            return SequenceName;
        }
    }

    // ============================================================
    // DBA_TABLESPACES 
    // ============================================================
    public class TablespaceInfo
    {
        public string  TablespaceName { get; set; }
        public string  Status         { get; set; }   
        public string  Contents       { get; set; }   
        public int     BlockSize      { get; set; }
        public decimal TotalMb        { get; set; }
        public decimal FreeMb         { get; set; }
        public decimal UsedMb         { get; set; }

        public decimal UsedPercent
        {
            get
            {
                if (TotalMb == 0) return 0;
                return Math.Round((UsedMb / TotalMb) * 100, 1);
            }
        }

        public override string ToString()
        {
            return TablespaceName + " (" + UsedPercent + "% usado)";
        }
    }

    // ============================================================
    // DBA_USERS
    // ============================================================
    public class UserInfo
    {
        public string   Username            { get; set; }
        public string   AccountStatus       { get; set; }  
        public DateTime? Created            { get; set; }
        public string   DefaultTablespace   { get; set; }
        public string   TemporaryTablespace { get; set; }
        public DateTime? ExpiryDate         { get; set; }

        public bool IsOpen
        {
            get { return AccountStatus == "OPEN"; }
        }

        public override string ToString()
        {
            return Username + " [" + AccountStatus + "]";
        }
    }
}
