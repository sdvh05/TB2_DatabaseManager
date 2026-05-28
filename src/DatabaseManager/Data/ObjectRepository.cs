using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DatabaseManager.Data
{

    public class ObjectRepository
    {
        private readonly OracleConnectionManager _manager;

        public ObjectRepository(OracleConnectionManager manager)
        {
            _manager = manager;
        }

        // ====================================================
        // TABLAS
        // ====================================================
        public DataTable GetTables(string owner)
        {
            string sql =
                "SELECT " +
                "    t.table_name, " +
                "    t.tablespace_name, " +
                "    t.num_rows, " +
                "    t.status, " +
                "    t.last_analyzed " +
                "FROM ALL_TABLES t " +
                "WHERE t.owner = UPPER(:owner) " +
                "ORDER BY t.table_name";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { ":owner", owner }
            };

            return _manager.ExecuteQuery(sql, parameters);
        }

        public DataTable GetTableColumns(string owner, string tableName)
        {
            string sql =
                "SELECT " +
                "    c.column_id, " +
                "    c.column_name, " +
                "    c.data_type, " +
                "    c.data_length, " +
                "    c.data_precision, " +
                "    c.data_scale, " +
                "    c.nullable, " +
                "    c.data_default " +
                "FROM ALL_TAB_COLUMNS c " +
                "WHERE c.owner      = UPPER(:owner) " +
                "  AND c.table_name = UPPER(:table_name) " +
                "ORDER BY c.column_id";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { ":owner",      owner     },
                { ":table_name", tableName }
            };

            return _manager.ExecuteQuery(sql, parameters);
        }

        public DataTable GetTableConstraints(string owner, string tableName)
        {
            string sql =
                "SELECT " +
                "    con.constraint_name, " +
                "    con.constraint_type, " +
                "    col.column_name, " +
                "    col.position, " +
                "    con.r_constraint_name, " +
                "    con.status " +
                "FROM ALL_CONSTRAINTS  con " +
                "JOIN ALL_CONS_COLUMNS col " +
                "    ON  con.constraint_name = col.constraint_name " +
                "    AND con.owner           = col.owner " +
                "WHERE con.owner      = UPPER(:owner) " +
                "  AND con.table_name = UPPER(:table_name) " +
                "ORDER BY con.constraint_type, col.position";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { ":owner",      owner     },
                { ":table_name", tableName }
            };

            return _manager.ExecuteQuery(sql, parameters);
        }

        // ====================================================
        // VISTAS
        // ====================================================
        public DataTable GetViews(string owner)
        {
            string sql =
                "SELECT " +
                "    v.view_name, " +
                "    v.text_length, " +
                "    v.read_only " +
                "FROM ALL_VIEWS v " +
                "WHERE v.owner = UPPER(:owner) " +
                "ORDER BY v.view_name";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { ":owner", owner }
            };

            return _manager.ExecuteQuery(sql, parameters);
        }

        public string GetViewSource(string owner, string viewName)
        {
            string sql =
                "SELECT TO_CLOB(v.text) AS view_text " +
                "FROM ALL_VIEWS v " +
                "WHERE v.owner     = UPPER(:owner) " +
                "  AND v.view_name = UPPER(:view_name)";

            OracleConnection conn = _manager.GetConnection();

            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.Parameters.Add(":owner",     owner);
                cmd.Parameters.Add(":view_name", viewName);

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                        return reader.IsDBNull(0) ? "" : reader.GetString(0);
                }
            }

            return "";
        }

        // ====================================================
        // PROCEDIMIENTOS, FUNCIONES Y PAQUETES
        // ====================================================
        public DataTable GetProgrammingObjects(string owner)
        {
            string sql =
                "SELECT " +
                "    o.object_name, " +
                "    o.object_type, " +
                "    o.status, " +
                "    o.last_ddl_time, " +
                "    o.created " +
                "FROM ALL_OBJECTS o " +
                "WHERE o.owner       = UPPER(:owner) " +
                "  AND o.object_type IN (" +
                "      'PROCEDURE','FUNCTION','PACKAGE','PACKAGE BODY') " +
                "ORDER BY o.object_type, o.object_name";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { ":owner", owner }
            };

            return _manager.ExecuteQuery(sql, parameters);
        }

        public string GetObjectSource(string owner, string objectName, string objectType)
        {
            string sql =
                "SELECT s.line, s.text " +
                "FROM ALL_SOURCE s " +
                "WHERE s.owner = UPPER(:owner) " +
                "  AND s.name  = UPPER(:object_name) " +
                "  AND s.type  = UPPER(:object_type) " +
                "ORDER BY s.line";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { ":owner",       owner      },
                { ":object_name", objectName },
                { ":object_type", objectType }
            };

            DataTable dt = _manager.ExecuteQuery(sql, parameters);

            StringBuilder sb = new StringBuilder();
            foreach (DataRow row in dt.Rows)
                sb.Append(row["TEXT"].ToString());

            return sb.ToString();
        }

        // ====================================================
        // TRIGGERS
        // ====================================================
        public DataTable GetTriggers(string owner)
        {
            string sql =
                "SELECT " +
                "    t.trigger_name, " +
                "    t.trigger_type, " +
                "    t.triggering_event, " +
                "    t.table_name, " +
                "    t.status " +
                "FROM ALL_TRIGGERS t " +
                "WHERE t.owner = UPPER(:owner) " +
                "ORDER BY t.table_name, t.trigger_name";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { ":owner", owner }
            };

            return _manager.ExecuteQuery(sql, parameters);
        }

        public DataTable GetTriggerDetail(string owner, string triggerName)
        {
            string sql =
                "SELECT " +
                "    t.trigger_name, " +
                "    t.trigger_type, " +
                "    t.triggering_event, " +
                "    t.table_name, " +
                "    t.when_clause, " +
                "    t.status, " +
                "    t.trigger_body " +
                "FROM ALL_TRIGGERS t " +
                "WHERE t.owner        = UPPER(:owner) " +
                "  AND t.trigger_name = UPPER(:trigger_name)";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { ":owner",        owner       },
                { ":trigger_name", triggerName }
            };

            return _manager.ExecuteQuery(sql, parameters);
        }

        // ====================================================
        // INDICES
        // ====================================================
        public DataTable GetIndexes(string owner)
        {
            string sql =
                "SELECT " +
                "    i.index_name, " +
                "    i.table_name, " +
                "    i.index_type, " +
                "    i.uniqueness, " +
                "    i.status, " +
                "    LISTAGG(ic.column_name, ', ') " +
                "        WITHIN GROUP (ORDER BY ic.column_position) AS columns " +
                "FROM ALL_INDEXES     i " +
                "JOIN ALL_IND_COLUMNS ic " +
                "    ON  i.index_name = ic.index_name " +
                "    AND i.owner      = ic.index_owner " +
                "WHERE i.owner = UPPER(:owner) " +
                "GROUP BY " +
                "    i.index_name, i.table_name, " +
                "    i.index_type, i.uniqueness, i.status " +
                "ORDER BY i.table_name, i.index_name";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { ":owner", owner }
            };

            return _manager.ExecuteQuery(sql, parameters);
        }

        // ====================================================
        // SECUENCIAS
        // ====================================================
        public DataTable GetSequences(string owner)
        {
            string sql =
                "SELECT " +
                "    s.sequence_name, " +
                "    s.min_value, " +
                "    s.max_value, " +
                "    s.increment_by, " +
                "    s.cycle_flag, " +
                "    s.cache_size, " +
                "    s.last_number " +
                "FROM ALL_SEQUENCES s " +
                "WHERE s.sequence_owner = UPPER(:owner) " +
                "ORDER BY s.sequence_name";

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { ":owner", owner }
            };

            return _manager.ExecuteQuery(sql, parameters);
        }

        // ====================================================
        // TABLESPACES (requiere privilegios DBA)
        // ====================================================
        public DataTable GetTablespaces()
        {
            string sql =
                "SELECT " +
                "    ts.tablespace_name, " +
                "    ts.status, " +
                "    ts.contents, " +
                "    ts.block_size, " +
                "    NVL(df.total_mb, 0) AS total_mb, " +
                "    NVL(fs.free_mb,  0) AS free_mb, " +
                "    NVL(df.total_mb, 0) - NVL(fs.free_mb, 0) AS used_mb " +
                "FROM DBA_TABLESPACES ts " +
                "LEFT JOIN ( " +
                "    SELECT tablespace_name, " +
                "           ROUND(SUM(bytes)/1048576, 2) AS total_mb " +
                "    FROM DBA_DATA_FILES " +
                "    GROUP BY tablespace_name " +
                ") df ON ts.tablespace_name = df.tablespace_name " +
                "LEFT JOIN ( " +
                "    SELECT tablespace_name, " +
                "           ROUND(SUM(bytes)/1048576, 2) AS free_mb " +
                "    FROM DBA_FREE_SPACE " +
                "    GROUP BY tablespace_name " +
                ") fs ON ts.tablespace_name = fs.tablespace_name " +
                "ORDER BY ts.tablespace_name";

            return _manager.ExecuteQuery(sql);
        }

        // ====================================================
        // USUARIOS (requiere privilegios DBA)
        // ====================================================
        public DataTable GetUsers()
        {
            string sql =
                "SELECT " +
                "    u.username, " +
                "    u.account_status, " +
                "    u.created, " +
                "    u.default_tablespace, " +
                "    u.temporary_tablespace, " +
                "    u.expiry_date " +
                "FROM DBA_USERS u " +
                "ORDER BY u.username";

            return _manager.ExecuteQuery(sql);
        }

        // ====================================================
        // DDL GENERATOR usando DBMS_METADATA
        // objectType: 'TABLE','VIEW','PROCEDURE','FUNCTION',
        //             'PACKAGE','TRIGGER','INDEX','SEQUENCE'
        // ====================================================
        public string GetDDL(string objectType, string objectName, string owner)
        {
            // Configurar formato del DDL primero
            string setupSql =
                "BEGIN " +
                "    DBMS_METADATA.SET_TRANSFORM_PARAM(" +
                "        DBMS_METADATA.SESSION_TRANSFORM,'SQLTERMINATOR',TRUE); " +
                "    DBMS_METADATA.SET_TRANSFORM_PARAM(" +
                "        DBMS_METADATA.SESSION_TRANSFORM,'PRETTY',TRUE); " +
                "    DBMS_METADATA.SET_TRANSFORM_PARAM(" +
                "        DBMS_METADATA.SESSION_TRANSFORM,'STORAGE',FALSE); " +
                "    DBMS_METADATA.SET_TRANSFORM_PARAM(" +
                "        DBMS_METADATA.SESSION_TRANSFORM,'TABLESPACE',FALSE); " +
                "END;";

            _manager.ExecuteNonQuery(setupSql);

            // Obtener el DDL — retorna CLOB
            string sql =
                "SELECT DBMS_METADATA.GET_DDL( " +
                "    UPPER(:object_type), " +
                "    UPPER(:object_name), " +
                "    UPPER(:owner) " +
                ") AS ddl_text FROM DUAL";

            OracleConnection conn = _manager.GetConnection();

            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                cmd.Parameters.Add(":object_type", objectType);
                cmd.Parameters.Add(":object_name", objectName);
                cmd.Parameters.Add(":owner",       owner);

                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // GET_DDL retorna CLOB, lo leemos con OracleClob
                        OracleClob clob = reader.GetOracleClob(0);
                        return clob.IsNull ? "" : clob.Value;
                    }
                }
            }

            return "";
        }
    }
}
