using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace DatabaseManager.Data
{
    // --------------------------------------------------------
    // OracleConnectionManager
    // Responsabilidad: abrir, cerrar y almacenar conexiones
    // a instancias de Oracle XE.
    // NO usa ORM ni information_schema.
    // --------------------------------------------------------
    public class OracleConnectionManager
    {
        // ----------------------------------------------------
        // Conexion activa en este momento
        // ----------------------------------------------------
        private OracleConnection _connection;

        // ----------------------------------------------------
        // Lista de conexiones guardadas (nombre -> cadena)
        // ----------------------------------------------------
        private Dictionary<string, string> _savedConnections
            = new Dictionary<string, string>();

        // ----------------------------------------------------
        // Datos de la conexion activa (para mostrar en la UI)
        // ----------------------------------------------------
        public string CurrentUser       { get; private set; }
        public string CurrentDataSource { get; private set; }

        public bool IsConnected
        {
            get
            {
                return _connection != null
                    && _connection.State == ConnectionState.Open;
            }
        }

        // ====================================================
        // CONECTAR
        // Parametros:
        //   host        ej: "localhost"
        //   port        ej: 1521
        //   serviceName ej: "XE"
        //   user        ej: "HR"
        //   password    ej: "hr_password"
        // ====================================================
        public void Connect(string host, int port, string serviceName,
                            string user, string password)
        {
            string connectionString =
                "User Id="     + user     + ";" +
                "Password="    + password + ";" +
                "Data Source=" + host + ":" + port + "/" + serviceName + ";";

            Disconnect();

            _connection = new OracleConnection(connectionString);
            _connection.Open();

            CurrentUser       = user.ToUpper();
            CurrentDataSource = host + ":" + port + "/" + serviceName;
        }

        // ====================================================
        // DESCONECTAR
        // ====================================================
        public void Disconnect()
        {
            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();

                _connection.Dispose();
                _connection = null;
            }

            CurrentUser       = null;
            CurrentDataSource = null;
        }

        // ====================================================
        // OBTENER LA CONEXION ACTIVA
        // Otras clases la piden para ejecutar queries
        // ====================================================
        public OracleConnection GetConnection()
        {
            if (!IsConnected)
                throw new InvalidOperationException(
                    "No hay una conexion activa. Conectate primero.");

            return _connection;
        }

        // ====================================================
        // GUARDAR CONEXION CON UN NOMBRE
        // ====================================================
        public void SaveConnection(string name, string host, int port,
                                   string serviceName, string user, string password)
        {
            string connectionString =
                "User Id="     + user     + ";" +
                "Password="    + password + ";" +
                "Data Source=" + host + ":" + port + "/" + serviceName + ";";

            _savedConnections[name] = connectionString;
        }

        // ====================================================
        // OBTENER LISTA DE CONEXIONES GUARDADAS
        // ====================================================
        public IEnumerable<string> GetSavedConnectionNames()
        {
            return _savedConnections.Keys;
        }

        // ====================================================
        // CONECTAR DESDE UNA CONEXION GUARDADA POR NOMBRE
        // ====================================================
        public void ConnectFromSaved(string name)
        {
            if (!_savedConnections.ContainsKey(name))
                throw new KeyNotFoundException(
                    "No existe una conexion guardada con el nombre: " + name);

            Disconnect();

            _connection = new OracleConnection(_savedConnections[name]);
            _connection.Open();

            foreach (string part in _savedConnections[name].Split(';'))
            {
                if (part.Trim().StartsWith("User Id=",
                        StringComparison.OrdinalIgnoreCase))
                    CurrentUser = part.Split('=')[1].ToUpper();

                if (part.Trim().StartsWith("Data Source=",
                        StringComparison.OrdinalIgnoreCase))
                    CurrentDataSource = part.Split('=')[1];
            }
        }

        // ====================================================
        // EJECUTAR UN SELECT Y RETORNAR UN DataTable
        // Uso: var dt = manager.ExecuteQuery("SELECT...", params)
        // ====================================================
        public DataTable ExecuteQuery(string sql,
                          Dictionary<string, object> parameters = null)
        {
            OracleConnection conn = GetConnection();

            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> p in parameters)
                        cmd.Parameters.Add(p.Key, p.Value ?? DBNull.Value);
                }

                using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }

        // ====================================================
        // EJECUTAR UN SCRIPT DDL/DML (sin retorno de filas)
        // Uso: manager.ExecuteNonQuery("CREATE TABLE ...")
        // ====================================================
        public int ExecuteNonQuery(string sql,
                       Dictionary<string, object> parameters = null)
        {
            OracleConnection conn = GetConnection();

            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> p in parameters)
                        cmd.Parameters.Add(p.Key, p.Value ?? DBNull.Value);
                }

                return cmd.ExecuteNonQuery();
            }
        }
    }
}
