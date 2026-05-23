using DatabaseManager.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public class FormCreateTable : Form
    {
        // --------------------------------------------------------
        // Dependencias
        // --------------------------------------------------------
        private readonly OracleConnectionManager _manager;

        // --------------------------------------------------------
        // Controles
        // --------------------------------------------------------
        private TextBox      txtTableName;
        private DataGridView gridColumns;
        private RichTextBox  txtDDLPreview;
        private Button       btnCreate;
        private Button       btnCancel;
        private Button       btnAddColumn;
        private Label        lblStatus;

        // Colores
        private readonly Color BG     = Color.FromArgb(30, 30, 30);
        private readonly Color BG2    = Color.FromArgb(42, 42, 42);
        private readonly Color BG3    = Color.FromArgb(55, 55, 55);
        private readonly Color ACCENT = Color.FromArgb(255, 87, 34);
        private readonly Color TEXT   = Color.White;
        private readonly Color TEXT2  = Color.FromArgb(180, 180, 180);

        // Tipos de datos Oracle disponibles en el combo
        private readonly string[] ORACLE_TYPES = {
            "NUMBER", "VARCHAR2", "CHAR", "NVARCHAR2",
            "DATE", "TIMESTAMP", "CLOB", "BLOB",
            "INTEGER", "FLOAT", "BINARY_FLOAT", "BINARY_DOUBLE"
        };

        public FormCreateTable(OracleConnectionManager manager)
        {
            _manager = manager;
            BuildUI();
            AgregarFilaColumna(); // Empezar con una fila vacia
        }

        // ====================================================
        // CONSTRUCCION DE LA INTERFAZ
        // ====================================================
        private void BuildUI()
        {
            this.Text            = "Crear Tabla — " + _manager.CurrentUser;
            this.Size            = new Size(860, 620);
            this.MinimumSize     = new Size(760, 540);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor       = BG;
            this.ForeColor       = TEXT;

            // Barra naranja lateral
            Panel panelAccent = new Panel();
            panelAccent.Dock      = DockStyle.Left;
            panelAccent.Width     = 4;
            panelAccent.BackColor = ACCENT;
            this.Controls.Add(panelAccent);

            // Panel principal con padding
            Panel panelMain = new Panel();
            panelMain.Dock        = DockStyle.Fill;
            panelMain.BackColor   = BG;
            panelMain.Padding     = new Padding(16, 12, 16, 8);
            this.Controls.Add(panelMain);

            // ── Seccion: nombre de la tabla ────────────────
            Label lblSeccion1 = CrearLabelSeccion("Nombre de la tabla");
            lblSeccion1.Dock = DockStyle.Top;

            Panel panelNombre = new Panel();
            panelNombre.Dock      = DockStyle.Top;
            panelNombre.Height    = 48;
            panelNombre.BackColor = BG;

            Label lblNombre = new Label();
            lblNombre.Text      = "Nombre:";
            lblNombre.Font      = new Font("Segoe UI", 9);
            lblNombre.ForeColor = TEXT2;
            lblNombre.AutoSize  = true;
            lblNombre.Location  = new Point(0, 14);

            txtTableName           = new TextBox();
            txtTableName.Font      = new Font("Segoe UI", 10, FontStyle.Bold);
            txtTableName.BackColor = BG2;
            txtTableName.ForeColor = TEXT;
            txtTableName.BorderStyle = BorderStyle.FixedSingle;
            txtTableName.Size      = new Size(280, 28);
            txtTableName.Location  = new Point(70, 10);
            txtTableName.CharacterCasing = CharacterCasing.Upper;
            txtTableName.TextChanged += (s, e) => ActualizarPreviewDDL();

            Label lblSchema = new Label();
            lblSchema.Text      = "Schema: " + _manager.CurrentUser;
            lblSchema.Font      = new Font("Segoe UI", 9);
            lblSchema.ForeColor = Color.FromArgb(255, 87, 34);
            lblSchema.AutoSize  = true;
            lblSchema.Location  = new Point(370, 14);

            panelNombre.Controls.Add(lblSchema);
            panelNombre.Controls.Add(txtTableName);
            panelNombre.Controls.Add(lblNombre);

            // ── Seccion: columnas ──────────────────────────
            Panel panelColHeader = new Panel();
            panelColHeader.Dock      = DockStyle.Top;
            panelColHeader.Height    = 36;
            panelColHeader.BackColor = Color.FromArgb(25, 25, 25);

            Label lblSeccion2 = new Label();
            lblSeccion2.Text      = "Columnas";
            lblSeccion2.Font      = new Font("Segoe UI", 9, FontStyle.Bold);
            lblSeccion2.ForeColor = ACCENT;
            lblSeccion2.AutoSize  = true;
            lblSeccion2.Location  = new Point(0, 10);

            btnAddColumn           = new Button();
            btnAddColumn.Text      = "+ Agregar columna";
            btnAddColumn.Font      = new Font("Segoe UI", 9);
            btnAddColumn.BackColor = BG3;
            btnAddColumn.ForeColor = TEXT;
            btnAddColumn.FlatStyle = FlatStyle.Flat;
            btnAddColumn.FlatAppearance.BorderSize = 0;
            btnAddColumn.Size      = new Size(140, 26);
            btnAddColumn.Anchor    = AnchorStyles.Right | AnchorStyles.Top;
            btnAddColumn.Cursor    = Cursors.Hand;
            btnAddColumn.Click    += (s, e) => AgregarFilaColumna();

            panelColHeader.Controls.Add(btnAddColumn);
            panelColHeader.Controls.Add(lblSeccion2);
            panelColHeader.Resize += (s, e) =>
            {
                btnAddColumn.Location = new Point(
                    panelColHeader.Width - btnAddColumn.Width - 4, 5);
            };

            // Grid de columnas
            gridColumns = CrearGridColumnas();
            gridColumns.Dock = DockStyle.Top;
            gridColumns.Height = 180;
            gridColumns.CellEndEdit          += (s, e) => ActualizarPreviewDDL();
            gridColumns.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (gridColumns.IsCurrentCellDirty)
                    gridColumns.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            gridColumns.DataError += (s, e) => e.Cancel = true;

            // ── Seccion: preview DDL ───────────────────────
            Label lblSeccion3 = CrearLabelSeccion("Preview DDL generado");
            lblSeccion3.Dock = DockStyle.Top;

            txtDDLPreview            = new RichTextBox();
            txtDDLPreview.Dock       = DockStyle.Fill;
            txtDDLPreview.BackColor  = BG2;
            txtDDLPreview.ForeColor  = Color.FromArgb(200, 220, 150);
            txtDDLPreview.Font       = new Font("Consolas", 10);
            txtDDLPreview.ReadOnly   = true;
            txtDDLPreview.BorderStyle = BorderStyle.None;
            txtDDLPreview.ScrollBars = RichTextBoxScrollBars.Both;

            // ── Barra inferior: botones + status ──────────
            Panel panelBottom = new Panel();
            panelBottom.Dock      = DockStyle.Bottom;
            panelBottom.Height    = 50;
            panelBottom.BackColor = Color.FromArgb(25, 25, 25);

            lblStatus           = new Label();
            lblStatus.Text      = "";
            lblStatus.Font      = new Font("Segoe UI", 9);
            lblStatus.ForeColor = TEXT2;
            lblStatus.AutoSize  = false;
            lblStatus.Size      = new Size(500, 24);
            lblStatus.Location  = new Point(10, 14);
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;

            btnCreate           = new Button();
            btnCreate.Text      = "Crear tabla";
            btnCreate.Font      = new Font("Segoe UI", 10, FontStyle.Bold);
            btnCreate.BackColor = ACCENT;
            btnCreate.ForeColor = TEXT;
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.Size      = new Size(120, 34);
            btnCreate.Anchor    = AnchorStyles.Right | AnchorStyles.Top;
            btnCreate.Cursor    = Cursors.Hand;
            btnCreate.Click    += BtnCreate_Click;

            btnCancel           = new Button();
            btnCancel.Text      = "Cancelar";
            btnCancel.Font      = new Font("Segoe UI", 10);
            btnCancel.BackColor = BG3;
            btnCancel.ForeColor = TEXT;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Size      = new Size(100, 34);
            btnCancel.Anchor    = AnchorStyles.Right | AnchorStyles.Top;
            btnCancel.Cursor    = Cursors.Hand;
            btnCancel.Click    += (s, e) => this.Close();

            panelBottom.Controls.Add(lblStatus);
            panelBottom.Controls.Add(btnCreate);
            panelBottom.Controls.Add(btnCancel);
            panelBottom.Resize += (s, e) =>
            {
                btnCancel.Location = new Point(panelBottom.Width - 110, 8);
                btnCreate.Location = new Point(panelBottom.Width - 240, 8);
            };

            // Ensamblar en orden (DockStyle.Top se apila de abajo hacia arriba)
            panelMain.Controls.Add(txtDDLPreview);
            panelMain.Controls.Add(lblSeccion3);
            panelMain.Controls.Add(gridColumns);
            panelMain.Controls.Add(panelColHeader);
            panelMain.Controls.Add(panelNombre);
            panelMain.Controls.Add(lblSeccion1);

            this.Controls.Add(panelBottom);
        }

        // ====================================================
        // CREAR EL GRID DE COLUMNAS
        // ====================================================
        private DataGridView CrearGridColumnas()
        {
            DataGridView g = new DataGridView();
            g.BackgroundColor    = BG2;
            g.GridColor          = BG3;
            g.ForeColor          = TEXT;
            g.RowHeadersVisible  = false;
            g.AllowUserToAddRows = false;
            g.BorderStyle        = BorderStyle.None;
            g.Font               = new Font("Consolas", 9);
            g.SelectionMode      = DataGridViewSelectionMode.FullRowSelect;
            g.EditMode           = DataGridViewEditMode.EditOnEnter;
            g.ScrollBars         = ScrollBars.Both;
            g.AllowUserToResizeColumns = true;
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(25, 25, 25);
            g.ColumnHeadersDefaultCellStyle.ForeColor = ACCENT;
            g.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9, FontStyle.Bold);
            g.ColumnHeadersHeightSizeMode             = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight                     = 30;
            g.DefaultCellStyle.BackColor              = BG2;
            g.DefaultCellStyle.ForeColor              = TEXT;
            g.DefaultCellStyle.SelectionBackColor     = Color.FromArgb(80, 255, 87, 34);
            g.DefaultCellStyle.SelectionForeColor     = TEXT;
            g.AlternatingRowsDefaultCellStyle.BackColor = BG3;
            g.RowTemplate.Height = 28;

            // Columna: Nombre
            DataGridViewTextBoxColumn colNombre = new DataGridViewTextBoxColumn();
            colNombre.Name       = "ColNombre";
            colNombre.HeaderText = "Nombre";
            colNombre.Width      = 160;

            // Columna: Tipo (combo)
            DataGridViewComboBoxColumn colTipo = new DataGridViewComboBoxColumn();
            colTipo.Name       = "ColTipo";
            colTipo.HeaderText = "Tipo";
            colTipo.Width      = 130;
            colTipo.FlatStyle  = FlatStyle.Flat;
            foreach (string t in ORACLE_TYPES)
                colTipo.Items.Add(t);

            // Columna: Longitud
            DataGridViewTextBoxColumn colLong = new DataGridViewTextBoxColumn();
            colLong.Name       = "ColLongitud";
            colLong.HeaderText = "Longitud";
            colLong.Width      = 80;

            // Columna: Nullable (checkbox)
            DataGridViewCheckBoxColumn colNull = new DataGridViewCheckBoxColumn();
            colNull.Name       = "ColNullable";
            colNull.HeaderText = "Nullable";
            colNull.Width      = 70;
            colNull.TrueValue  = true;
            colNull.FalseValue = false;

            // Columna: PK (checkbox)
            DataGridViewCheckBoxColumn colPK = new DataGridViewCheckBoxColumn();
            colPK.Name       = "ColPK";
            colPK.HeaderText = "PK";
            colPK.Width      = 50;
            colPK.TrueValue  = true;
            colPK.FalseValue = false;

            // Columna: Default
            DataGridViewTextBoxColumn colDefault = new DataGridViewTextBoxColumn();
            colDefault.Name       = "ColDefault";
            colDefault.HeaderText = "Default";
            colDefault.Width      = 120;

            // Columna: Eliminar (boton)
            DataGridViewButtonColumn colDel = new DataGridViewButtonColumn();
            colDel.Name       = "ColEliminar";
            colDel.HeaderText = "";
            colDel.Text       = "✕";
            colDel.Width      = 40;
            colDel.UseColumnTextForButtonValue = true;

            g.Columns.AddRange(new DataGridViewColumn[] {
                colNombre, colTipo, colLong, colNull, colPK, colDefault, colDel
            });

            // Click en boton eliminar fila
            g.CellClick += (s, e) =>
            {
                if (e.ColumnIndex == g.Columns["ColEliminar"].Index && e.RowIndex >= 0)
                {
                    g.Rows.RemoveAt(e.RowIndex);
                    ActualizarPreviewDDL();
                }
            };

            // DoubleBuffer via reflection
            typeof(DataGridView)
                .GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)
                .SetValue(g, true, null);

            return g;
        }

        // ====================================================
        // AGREGAR FILA AL GRID
        // ====================================================
        private void AgregarFilaColumna()
        {
            int idx = gridColumns.Rows.Add();
            DataGridViewRow row = gridColumns.Rows[idx];
            row.Cells["ColTipo"].Value     = "VARCHAR2";
            row.Cells["ColNullable"].Value = true;
            row.Cells["ColPK"].Value       = false;
            ActualizarPreviewDDL();
        }

        // ====================================================
        // GENERAR Y MOSTRAR PREVIEW DEL DDL
        // ====================================================
        private void ActualizarPreviewDDL()
        {
            txtDDLPreview.Text = GenerarDDL();
        }

        private string GenerarDDL()
        {
            string tableName = txtTableName.Text.Trim();
            if (string.IsNullOrEmpty(tableName))
                return "-- Escribe el nombre de la tabla para ver el DDL";

            List<string> columnas   = new List<string>();
            List<string> pkColumnas = new List<string>();

            foreach (DataGridViewRow row in gridColumns.Rows)
            {
                string nombre = row.Cells["ColNombre"].Value?.ToString().Trim().ToUpper();
                string tipo   = row.Cells["ColTipo"].Value?.ToString();
                string long_  = row.Cells["ColLongitud"].Value?.ToString().Trim();
                bool nullable = row.Cells["ColNullable"].Value is bool nb && nb;
                bool esPK     = row.Cells["ColPK"].Value     is bool pk && pk;
                string def_   = row.Cells["ColDefault"].Value?.ToString().Trim();

                if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(tipo))
                    continue;

                // Construir tipo con longitud si aplica
                string tipoCompleto = tipo;
                if (!string.IsNullOrEmpty(long_) &&
                    (tipo == "VARCHAR2" || tipo == "CHAR" ||
                     tipo == "NVARCHAR2" || tipo == "NUMBER" || tipo == "FLOAT"))
                    tipoCompleto = tipo + "(" + long_ + ")";

                StringBuilder col = new StringBuilder();
                col.Append("    ");
                col.Append(nombre.PadRight(20));
                col.Append(tipoCompleto.PadRight(20));

                if (!string.IsNullOrEmpty(def_))
                    col.Append("DEFAULT " + def_ + " ");

                col.Append(nullable ? "NULL" : "NOT NULL");

                columnas.Add(col.ToString());

                if (esPK) pkColumnas.Add(nombre);
            }

            if (columnas.Count == 0)
                return "-- Agrega al menos una columna";

            // Constraint PK
            if (pkColumnas.Count > 0)
                columnas.Add("    CONSTRAINT PK_" + tableName +
                             " PRIMARY KEY (" + string.Join(", ", pkColumnas) + ")");

            StringBuilder ddl = new StringBuilder();
            ddl.AppendLine("CREATE TABLE " + _manager.CurrentUser + "." + tableName);
            ddl.AppendLine("(");
            ddl.AppendLine(string.Join(",\r\n", columnas));
            ddl.AppendLine(");");

            return ddl.ToString();
        }

        // ====================================================
        // EVENTO — Crear tabla
        // ====================================================
        private void BtnCreate_Click(object sender, EventArgs e)
        {
            string ddl = GenerarDDL();

            if (ddl.StartsWith("--"))
            {
                MostrarError("Completa el nombre de la tabla y al menos una columna.");
                return;
            }

            // Confirmacion
            DialogResult confirm = MessageBox.Show(
                "Se ejecutara el siguiente DDL:\r\n\r\n" + ddl +
                "\r\n\r\n¿Confirmar?",
                "Crear tabla",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                // Quitar el punto y coma final para ODP.NET
                string sqlEjecutar = ddl.TrimEnd().TrimEnd(';');
                _manager.ExecuteNonQuery(sqlEjecutar);

                lblStatus.ForeColor = Color.LimeGreen;
                lblStatus.Text      = "Tabla '" + txtTableName.Text.Trim() + "' creada exitosamente.";

                MessageBox.Show(
                    "Tabla creada exitosamente.",
                    "Exito",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MostrarError("Error: " + ex.Message);
            }
        }

        // ====================================================
        // HELPERS
        // ====================================================
        private Label CrearLabelSeccion(string texto)
        {
            Label lbl     = new Label();
            lbl.Text      = texto;
            lbl.Font      = new Font("Segoe UI", 9, FontStyle.Bold);
            lbl.ForeColor = ACCENT;
            lbl.BackColor = Color.FromArgb(25, 25, 25);
            lbl.AutoSize  = false;
            lbl.Height    = 28;
            lbl.TextAlign = ContentAlignment.MiddleLeft;
            return lbl;
        }

        private void MostrarError(string msg)
        {
            lblStatus.ForeColor = Color.FromArgb(255, 80, 80);
            lblStatus.Text      = msg;
        }
    }
}
