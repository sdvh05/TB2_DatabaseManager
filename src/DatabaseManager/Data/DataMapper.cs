using DatabaseManager.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace DatabaseManager.Data
{
    // --------------------------------------------------------
    // DataMapper
    // Responsabilidad: convertir DataTable (resultado crudo
    // de Oracle) en listas de Models tipados.
    // Centraliza toda la logica de mapeo en un solo lugar.
    // --------------------------------------------------------
    public static class DataMapper
    {
        // ====================================================
        // TABLAS
        // ====================================================
        public static List<TableInfo> ToTableList(DataTable dt)
        {
            List<TableInfo> list = new List<TableInfo>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new TableInfo
                {
                    TableName      = row["TABLE_NAME"].ToString(),
                    TablespaceName = row["TABLESPACE_NAME"].ToString(),
                    NumRows        = row["NUM_ROWS"] == DBNull.Value
                                        ? 0
                                        : Convert.ToInt64(row["NUM_ROWS"]),
                    Status         = row["STATUS"].ToString(),
                    LastAnalyzed   = row["LAST_ANALYZED"] == DBNull.Value
                                        ? (DateTime?)null
                                        : Convert.ToDateTime(row["LAST_ANALYZED"])
                });
            }

            return list;
        }

        // ====================================================
        // COLUMNAS
        // ====================================================
        public static List<ColumnInfo> ToColumnList(DataTable dt)
        {
            List<ColumnInfo> list = new List<ColumnInfo>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ColumnInfo
                {
                    ColumnId      = Convert.ToInt32(row["COLUMN_ID"]),
                    ColumnName    = row["COLUMN_NAME"].ToString(),
                    DataType      = row["DATA_TYPE"].ToString(),
                    DataLength    = row["DATA_LENGTH"] == DBNull.Value
                                        ? 0
                                        : Convert.ToInt32(row["DATA_LENGTH"]),
                    DataPrecision = row["DATA_PRECISION"] == DBNull.Value
                                        ? (int?)null
                                        : Convert.ToInt32(row["DATA_PRECISION"]),
                    DataScale     = row["DATA_SCALE"] == DBNull.Value
                                        ? (int?)null
                                        : Convert.ToInt32(row["DATA_SCALE"]),
                    IsNullable    = row["NULLABLE"].ToString() == "Y",
                    DataDefault   = row["DATA_DEFAULT"] == DBNull.Value
                                        ? ""
                                        : row["DATA_DEFAULT"].ToString().Trim()
                });
            }

            return list;
        }

        // ====================================================
        // CONSTRAINTS
        // ====================================================
        public static List<ConstraintInfo> ToConstraintList(DataTable dt)
        {
            List<ConstraintInfo> list = new List<ConstraintInfo>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ConstraintInfo
                {
                    ConstraintName  = row["CONSTRAINT_NAME"].ToString(),
                    ConstraintType  = row["CONSTRAINT_TYPE"].ToString(),
                    ColumnName      = row["COLUMN_NAME"].ToString(),
                    Position        = Convert.ToInt32(row["POSITION"]),
                    RConstraintName = row["R_CONSTRAINT_NAME"] == DBNull.Value
                                          ? ""
                                          : row["R_CONSTRAINT_NAME"].ToString(),
                    Status          = row["STATUS"].ToString()
                });
            }

            return list;
        }

        // ====================================================
        // VISTAS
        // ====================================================
        public static List<ViewInfo> ToViewList(DataTable dt)
        {
            List<ViewInfo> list = new List<ViewInfo>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ViewInfo
                {
                    ViewName   = row["VIEW_NAME"].ToString(),
                    TextLength = row["TEXT_LENGTH"] == DBNull.Value
                                     ? 0
                                     : Convert.ToInt64(row["TEXT_LENGTH"]),
                    IsReadOnly = row["READ_ONLY"].ToString() == "Y"
                });
            }

            return list;
        }

        // ====================================================
        // OBJETOS PL/SQL
        // ====================================================
        public static List<ProgrammingObjectInfo> ToProgrammingObjectList(DataTable dt)
        {
            List<ProgrammingObjectInfo> list = new List<ProgrammingObjectInfo>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ProgrammingObjectInfo
                {
                    ObjectName   = row["OBJECT_NAME"].ToString(),
                    ObjectType   = row["OBJECT_TYPE"].ToString(),
                    Status       = row["STATUS"].ToString(),
                    LastDdlTime  = row["LAST_DDL_TIME"] == DBNull.Value
                                       ? (DateTime?)null
                                       : Convert.ToDateTime(row["LAST_DDL_TIME"]),
                    Created      = row["CREATED"] == DBNull.Value
                                       ? (DateTime?)null
                                       : Convert.ToDateTime(row["CREATED"])
                });
            }

            return list;
        }

        // ====================================================
        // TRIGGERS
        // ====================================================
        public static List<TriggerInfo> ToTriggerList(DataTable dt)
        {
            List<TriggerInfo> list = new List<TriggerInfo>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new TriggerInfo
                {
                    TriggerName     = row["TRIGGER_NAME"].ToString(),
                    TriggerType     = row["TRIGGER_TYPE"].ToString(),
                    TriggeringEvent = row["TRIGGERING_EVENT"].ToString(),
                    TableName       = row["TABLE_NAME"].ToString(),
                    Status          = row["STATUS"].ToString()
                });
            }

            return list;
        }

        // ====================================================
        // INDICES
        // ====================================================
        public static List<IndexInfo> ToIndexList(DataTable dt)
        {
            List<IndexInfo> list = new List<IndexInfo>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new IndexInfo
                {
                    IndexName  = row["INDEX_NAME"].ToString(),
                    TableName  = row["TABLE_NAME"].ToString(),
                    IndexType  = row["INDEX_TYPE"].ToString(),
                    Uniqueness = row["UNIQUENESS"].ToString(),
                    Status     = row["STATUS"].ToString(),
                    Columns    = row["COLUMNS"].ToString()
                });
            }

            return list;
        }

        // ====================================================
        // SECUENCIAS
        // ====================================================
        public static List<SequenceInfo> ToSequenceList(DataTable dt)
        {
            List<SequenceInfo> list = new List<SequenceInfo>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SequenceInfo
                {
                    SequenceName = row["SEQUENCE_NAME"].ToString(),
                    MinValue     = Convert.ToInt64(row["MIN_VALUE"]),
                    MaxValue     = Convert.ToInt64(row["MAX_VALUE"]),
                    IncrementBy  = Convert.ToInt64(row["INCREMENT_BY"]),
                    IsCyclic     = row["CYCLE_FLAG"].ToString() == "Y",
                    CacheSize    = Convert.ToInt64(row["CACHE_SIZE"]),
                    LastNumber   = Convert.ToInt64(row["LAST_NUMBER"])
                });
            }

            return list;
        }

        // ====================================================
        // TABLESPACES
        // ====================================================
        public static List<TablespaceInfo> ToTablespaceList(DataTable dt)
        {
            List<TablespaceInfo> list = new List<TablespaceInfo>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new TablespaceInfo
                {
                    TablespaceName = row["TABLESPACE_NAME"].ToString(),
                    Status         = row["STATUS"].ToString(),
                    Contents       = row["CONTENTS"].ToString(),
                    BlockSize      = Convert.ToInt32(row["BLOCK_SIZE"]),
                    TotalMb        = Convert.ToDecimal(row["TOTAL_MB"]),
                    FreeMb         = Convert.ToDecimal(row["FREE_MB"]),
                    UsedMb         = Convert.ToDecimal(row["USED_MB"])
                });
            }

            return list;
        }

        // ====================================================
        // USUARIOS
        // ====================================================
        public static List<UserInfo> ToUserList(DataTable dt)
        {
            List<UserInfo> list = new List<UserInfo>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new UserInfo
                {
                    Username            = row["USERNAME"].ToString(),
                    AccountStatus       = row["ACCOUNT_STATUS"].ToString(),
                    Created             = row["CREATED"] == DBNull.Value
                                              ? (DateTime?)null
                                              : Convert.ToDateTime(row["CREATED"]),
                    DefaultTablespace   = row["DEFAULT_TABLESPACE"].ToString(),
                    TemporaryTablespace = row["TEMPORARY_TABLESPACE"].ToString(),
                    ExpiryDate          = row["EXPIRY_DATE"] == DBNull.Value
                                              ? (DateTime?)null
                                              : Convert.ToDateTime(row["EXPIRY_DATE"])
                });
            }

            return list;
        }
    }
}
