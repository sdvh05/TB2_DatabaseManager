using DatabaseManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DatabaseManager.Data
{
    // --------------------------------------------------------
    // ConnectionStorage
    // Guarda y carga conexiones desde un archivo JSON simple
    // ubicado en la misma carpeta del ejecutable.
    // NO guarda passwords por seguridad.
    // --------------------------------------------------------
    public static class ConnectionStorage
    {
        private static readonly string FilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "connections.json");

        // ====================================================
        // GUARDAR lista de conexiones en disco
        // ====================================================
        public static void Save(List<ConnectionInfo> conexiones)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[");

            for (int i = 0; i < conexiones.Count; i++)
            {
                ConnectionInfo c = conexiones[i];
                sb.AppendLine("  {");
                sb.AppendLine("    \"Name\":        \"" + Escape(c.Name)        + "\",");
                sb.AppendLine("    \"Host\":        \"" + Escape(c.Host)        + "\",");
                sb.AppendLine("    \"Port\":         "  + c.Port                + ",");
                sb.AppendLine("    \"ServiceName\": \"" + Escape(c.ServiceName) + "\",");
                sb.AppendLine("    \"Username\":    \"" + Escape(c.Username)    + "\"");
                sb.Append    ("  }");
                sb.AppendLine(i < conexiones.Count - 1 ? "," : "");
            }

            sb.AppendLine("]");
            File.WriteAllText(FilePath, sb.ToString(), Encoding.UTF8);
        }

        // ====================================================
        // CARGAR lista de conexiones desde disco
        // ====================================================
        public static List<ConnectionInfo> Load()
        {
            List<ConnectionInfo> lista = new List<ConnectionInfo>();

            if (!File.Exists(FilePath))
                return lista;

            try
            {
                string json = File.ReadAllText(FilePath, Encoding.UTF8);
                lista = ParseJson(json);
            }
            catch
            {
                // Si el archivo esta corrupto, devolvemos lista vacia
            }

            return lista;
        }

        // ====================================================
        // Parser JSON minimalista (sin dependencias externas)
        // Solo soporta el formato exacto que Save() genera
        // ====================================================
        private static List<ConnectionInfo> ParseJson(string json)
        {
            List<ConnectionInfo> lista = new List<ConnectionInfo>();

            // Dividir por objetos { ... }
            int i = 0;
            while (i < json.Length)
            {
                int inicio = json.IndexOf('{', i);
                if (inicio < 0) break;
                int fin = json.IndexOf('}', inicio);
                if (fin < 0) break;

                string bloque = json.Substring(inicio + 1, fin - inicio - 1);
                ConnectionInfo c = ParseBloque(bloque);
                if (c != null)
                    lista.Add(c);

                i = fin + 1;
            }

            return lista;
        }

        private static ConnectionInfo ParseBloque(string bloque)
        {
            try
            {
                ConnectionInfo c = new ConnectionInfo();
                string[] lineas = bloque.Split(new char[] { '\n', '\r' },
                                               StringSplitOptions.RemoveEmptyEntries);

                foreach (string linea in lineas)
                {
                    string l = linea.Trim().TrimEnd(',');
                    if (l.Length == 0) continue;

                    int colon = l.IndexOf(':');
                    if (colon < 0) continue;

                    string clave = l.Substring(0, colon).Trim().Trim('"');
                    string valor = l.Substring(colon + 1).Trim().Trim('"');

                    switch (clave)
                    {
                        case "Name":        c.Name        = valor; break;
                        case "Host":        c.Host        = valor; break;
                        case "Port":        int.TryParse(valor, out int p);
                                            c.Port        = p;     break;
                        case "ServiceName": c.ServiceName = valor; break;
                        case "Username":    c.Username    = valor; break;
                    }
                }

                // Solo valida si tiene los campos minimos
                if (string.IsNullOrEmpty(c.Name) || string.IsNullOrEmpty(c.Host))
                    return null;

                return c;
            }
            catch
            {
                return null;
            }
        }

        // Escapa comillas dobles en strings para el JSON
        private static string Escape(string s)
        {
            return s == null ? "" : s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
