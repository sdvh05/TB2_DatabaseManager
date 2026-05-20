using DatabaseManager.Data;
using DatabaseManager.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public class FormLogin : Form
    {
        private OracleConnectionManager _connectionManager;
        private List<ConnectionInfo>    _savedConnections;

        private TextBox txtHost;
        private TextBox txtPort;
        private TextBox txtService;
        private TextBox txtUser;
        private TextBox txtPassword;
        private TextBox txtConnectionName;
        private Button  btnConnect;
        private Button  btnSaved;
        private Label   lblStatus;

        // Colores
        private readonly Color BG     = Color.FromArgb(30, 30, 30);
        private readonly Color BG2    = Color.FromArgb(50, 50, 50);
        private readonly Color ACCENT = Color.FromArgb(255, 87, 34);
        private readonly Color TEXT   = Color.White;
        private readonly Color TEXT2  = Color.FromArgb(180, 180, 180);

        public FormLogin()
        {
            _connectionManager = new OracleConnectionManager();

            // Cargar conexiones guardadas desde disco al arrancar
            _savedConnections = ConnectionStorage.Load();

            SetupForm();
        }

        // ====================================================
        // CONSTRUCCION VISUAL
        // ====================================================
        private void SetupForm()
        {
            this.Text            = "DatabaseManager — Conectar";
            this.Size            = new Size(480, 590);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.BackColor       = BG;
            this.ForeColor       = TEXT;

            // Barra lateral naranja
            Panel panelLeft = new Panel();
            panelLeft.BackColor = ACCENT;
            panelLeft.Size      = new Size(6, 590);
            panelLeft.Location  = new Point(0, 0);
            this.Controls.Add(panelLeft);

            // Titulo
            Label lblTitle = new Label();
            lblTitle.Text      = "DatabaseManager";
            lblTitle.Font      = new Font("Segoe UI", 18, FontStyle.Bold);
            lblTitle.ForeColor = TEXT;
            lblTitle.AutoSize  = true;
            lblTitle.Location  = new Point(30, 28);
            this.Controls.Add(lblTitle);

            Label lblSub = new Label();
            lblSub.Text      = "Oracle XE — Iniciar Conexion";
            lblSub.Font      = new Font("Segoe UI", 9);
            lblSub.ForeColor = TEXT2;
            lblSub.AutoSize  = true;
            lblSub.Location  = new Point(33, 62);
            this.Controls.Add(lblSub);

            // Separador
            Panel sep = new Panel();
            sep.BackColor = ACCENT;
            sep.Size      = new Size(420, 1);
            sep.Location  = new Point(30, 90);
            this.Controls.Add(sep);

            // Campos
            int startY = 108;
            int gap    = 56;

            txtHost     = CrearCampo("Host",         "localhost", startY);
            txtPort     = CrearCampo("Puerto",        "1521",     startY + gap);
            txtService  = CrearCampo("Service Name",  "XE",      startY + gap * 2);
            txtUser     = CrearCampo("Usuario",       "SYSTEM",  startY + gap * 3);
            txtPassword = CrearCampo("Password",      "",        startY + gap * 4);
            txtPassword.PasswordChar         = '●';
            txtPassword.UseSystemPasswordChar = false;

            // Campo nombre conexion (con placeholder simulado)
            Label lblNombre = new Label();
            lblNombre.Text      = "Guardar como (opcional)";
            lblNombre.Font      = new Font("Segoe UI", 8);
            lblNombre.ForeColor = TEXT2;
            lblNombre.AutoSize  = true;
            lblNombre.Location  = new Point(30, startY + gap * 5);
            this.Controls.Add(lblNombre);

            txtConnectionName           = new TextBox();
            txtConnectionName.Font      = new Font("Segoe UI", 10);
            txtConnectionName.BackColor = BG2;
            txtConnectionName.ForeColor = Color.FromArgb(130, 130, 130);
            txtConnectionName.BorderStyle = BorderStyle.FixedSingle;
            txtConnectionName.Size      = new Size(420, 28);
            txtConnectionName.Location  = new Point(30, startY + gap * 5 + 18);
            txtConnectionName.Text      = "ej: Local XE - SYSTEM";
            txtConnectionName.GotFocus += (s, e) =>
            {
                if (txtConnectionName.Text == "ej: Local XE - SYSTEM")
                {
                    txtConnectionName.Text      = "";
                    txtConnectionName.ForeColor = TEXT;
                }
            };
            txtConnectionName.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtConnectionName.Text))
                {
                    txtConnectionName.Text      = "ej: Local XE - SYSTEM";
                    txtConnectionName.ForeColor = Color.FromArgb(130, 130, 130);
                }
            };
            this.Controls.Add(txtConnectionName);

            // Boton Conectar
            btnConnect           = new Button();
            btnConnect.Text      = "CONECTAR";
            btnConnect.Font      = new Font("Segoe UI", 10, FontStyle.Bold);
            btnConnect.BackColor = ACCENT;
            btnConnect.ForeColor = TEXT;
            btnConnect.FlatStyle = FlatStyle.Flat;
            btnConnect.FlatAppearance.BorderSize = 0;
            btnConnect.Size      = new Size(200, 40);
            btnConnect.Location  = new Point(30, startY + gap * 6 + 8);
            btnConnect.Cursor    = Cursors.Hand;
            btnConnect.Click    += BtnConnect_Click;
            this.Controls.Add(btnConnect);

            // Boton Guardadas
            btnSaved           = new Button();
            btnSaved.Text      = "GUARDADAS  ▼";
            btnSaved.Font      = new Font("Segoe UI", 10);
            btnSaved.BackColor = Color.FromArgb(60, 60, 60);
            btnSaved.ForeColor = TEXT;
            btnSaved.FlatStyle = FlatStyle.Flat;
            btnSaved.FlatAppearance.BorderSize = 0;
            btnSaved.Size      = new Size(200, 40);
            btnSaved.Location  = new Point(248, startY + gap * 6 + 8);
            btnSaved.Cursor    = Cursors.Hand;
            btnSaved.Click    += BtnSaved_Click;
            this.Controls.Add(btnSaved);

            // Label estado
            lblStatus           = new Label();
            lblStatus.Text      = _savedConnections.Count > 0
                                      ? _savedConnections.Count + " conexion(es) guardada(s) cargadas."
                                      : "";
            lblStatus.Font      = new Font("Segoe UI", 9);
            lblStatus.ForeColor = TEXT2;
            lblStatus.AutoSize  = false;
            lblStatus.Size      = new Size(420, 40);
            lblStatus.Location  = new Point(30, startY + gap * 6 + 55);
            this.Controls.Add(lblStatus);
        }

        private TextBox CrearCampo(string labelText, string defaultValue, int y)
        {
            Label lbl     = new Label();
            lbl.Text      = labelText;
            lbl.Font      = new Font("Segoe UI", 8);
            lbl.ForeColor = TEXT2;
            lbl.AutoSize  = true;
            lbl.Location  = new Point(30, y);
            this.Controls.Add(lbl);

            TextBox txt     = new TextBox();
            txt.Font        = new Font("Segoe UI", 10);
            txt.BackColor   = BG2;
            txt.ForeColor   = TEXT;
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.Size        = new Size(420, 28);
            txt.Location    = new Point(30, y + 18);
            txt.Text        = defaultValue;
            this.Controls.Add(txt);

            return txt;
        }

        // ====================================================
        // EVENTO — Conectar
        // ====================================================
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            lblStatus.ForeColor = ACCENT;
            lblStatus.Text      = "Conectando...";
            btnConnect.Enabled  = false;
            Application.DoEvents();

            try
            {
                if (string.IsNullOrWhiteSpace(txtHost.Text)    ||
                    string.IsNullOrWhiteSpace(txtPort.Text)    ||
                    string.IsNullOrWhiteSpace(txtService.Text) ||
                    string.IsNullOrWhiteSpace(txtUser.Text)    ||
                    string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MostrarError("Completa todos los campos obligatorios.");
                    return;
                }

                int port;
                if (!int.TryParse(txtPort.Text.Trim(), out port))
                {
                    MostrarError("El puerto debe ser un numero (ej: 1521).");
                    return;
                }

                string host     = txtHost.Text.Trim();
                string service  = txtService.Text.Trim();
                string user     = txtUser.Text.Trim();
                string password = txtPassword.Text;

                _connectionManager.Connect(host, port, service, user, password);

                // Guardar conexion si tiene nombre
                string nombre = txtConnectionName.Text.Trim();
                if (!string.IsNullOrWhiteSpace(nombre) &&
                    nombre != "ej: Local XE - SYSTEM")
                {
                    // Evitar duplicados por nombre
                    _savedConnections.RemoveAll(c =>
                        c.Name.Equals(nombre, StringComparison.OrdinalIgnoreCase));

                    _savedConnections.Add(new ConnectionInfo
                    {
                        Name        = nombre,
                        Host        = host,
                        Port        = port,
                        ServiceName = service,
                        Username    = user
                        // Password NO se guarda
                    });

                    // Persistir en disco
                    ConnectionStorage.Save(_savedConnections);

                    _connectionManager.SaveConnection(
                        nombre, host, port, service, user, password);
                }

                lblStatus.ForeColor = Color.LimeGreen;
                lblStatus.Text      = "Conexion exitosa. Abriendo...";
                Application.DoEvents();

                FormMain formMain = new FormMain(_connectionManager);
                formMain.FormClosed += (s, ev) =>
                {
                    lblStatus.Text = "";
                    this.Show();
                };
                formMain.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MostrarError("Error: " + ex.Message);
            }
        }

        // ====================================================
        // EVENTO — Conexiones Guardadas
        // ====================================================
        private void BtnSaved_Click(object sender, EventArgs e)
        {
            if (_savedConnections.Count == 0)
            {
                MessageBox.Show(
                    "No hay conexiones guardadas todavia.\n" +
                    "Conectate y escribe un nombre en 'Guardar como'.",
                    "Sin conexiones guardadas",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.BackColor = Color.FromArgb(50, 50, 50);
            menu.ForeColor = TEXT;
            menu.Font      = new Font("Segoe UI", 9);

            foreach (ConnectionInfo conn in _savedConnections)
            {
                ConnectionInfo capturada = conn;

                ToolStripMenuItem item = new ToolStripMenuItem(conn.ToString());
                item.Click += (s, ev) => CargarConexion(capturada);
                menu.Items.Add(item);
            }

            // Separador + opcion para eliminar
            menu.Items.Add(new ToolStripSeparator());
            ToolStripMenuItem itemEliminar = new ToolStripMenuItem("Eliminar una conexion...");
            itemEliminar.ForeColor = Color.FromArgb(255, 80, 80);
            itemEliminar.Click    += MostrarMenuEliminar;
            menu.Items.Add(itemEliminar);

            menu.Show(btnSaved, new Point(0, btnSaved.Height));
        }

        private void CargarConexion(ConnectionInfo conn)
        {
            txtHost.Text     = conn.Host;
            txtPort.Text     = conn.Port.ToString();
            txtService.Text  = conn.ServiceName;
            txtUser.Text     = conn.Username;
            txtPassword.Text = "";
            txtPassword.Focus();

            lblStatus.ForeColor = TEXT2;
            lblStatus.Text      = "Conexion cargada. Ingresa el password para conectar.";
        }

        private void MostrarMenuEliminar(object sender, EventArgs e)
        {
            ContextMenuStrip menuDel = new ContextMenuStrip();
            menuDel.BackColor = Color.FromArgb(50, 50, 50);
            menuDel.ForeColor = TEXT;
            menuDel.Font      = new Font("Segoe UI", 9);

            foreach (ConnectionInfo conn in _savedConnections)
            {
                ConnectionInfo capturada = conn;
                ToolStripMenuItem item   = new ToolStripMenuItem(conn.Name);
                item.ForeColor = Color.FromArgb(255, 80, 80);
                item.Click    += (s, ev) =>
                {
                    _savedConnections.Remove(capturada);
                    ConnectionStorage.Save(_savedConnections);
                    lblStatus.ForeColor = TEXT2;
                    lblStatus.Text      = "Conexion '" + capturada.Name + "' eliminada.";
                };
                menuDel.Items.Add(item);
            }

            menuDel.Show(btnSaved, new Point(0, btnSaved.Height));
        }

        private void MostrarError(string msg)
        {
            lblStatus.ForeColor = Color.FromArgb(255, 80, 80);
            lblStatus.Text      = msg;
            btnConnect.Enabled  = true;
        }
    }
}
