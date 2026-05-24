using DatabaseManager.Data;
using System.Reflection;
using DatabaseManager.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public class FormMain : Form
    {
        private readonly OracleConnectionManager _manager;
        private readonly ObjectRepository        _repo;

        private TreeView     treeObjects;
        private DataGridView gridDetails;
        private RichTextBox  txtDDL;
        private RichTextBox  txtSQL;
        private DataGridView gridResults;
        private Label        lblStatus;
        private TabControl   tabDetails;
        private Button       btnExecute;
        private Label        lblObjectTitle;
        private Label        lblRowCount;

        private readonly Color BG     = Color.FromArgb(30, 30, 30);
        private readonly Color BG2    = Color.FromArgb(42, 42, 42);
        private readonly Color BG3    = Color.FromArgb(55, 55, 55);
        private readonly Color ACCENT = Color.FromArgb(255, 87, 34);
        private readonly Color TEXT   = Color.White;
        private readonly Color TEXT2  = Color.FromArgb(180, 180, 180);

        public FormMain(OracleConnectionManager manager)
        {
            _manager = manager;
            _repo    = new ObjectRepository(manager);

            this.Text          = "DatabaseManager  —  " +
                                 manager.CurrentUser + " @ " + manager.CurrentDataSource;
            this.Size          = new Size(1200, 750);
            this.MinimumSize   = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor     = BG;
            this.ForeColor     = TEXT;

            BuildUI();
            LoadTree();
        }

        // ====================================================
        // CONSTRUCCION DE LA INTERFAZ
        // ====================================================
        private void BuildUI()
        {
            // ── Panel izquierdo: TreeView ──────────────────
            Panel panelLeft = new Panel();
            panelLeft.Dock      = DockStyle.Left;
            panelLeft.Width     = 220;
            panelLeft.BackColor = BG2;

            Label lblTree = new Label();
            lblTree.Text      = "Objetos  —  " + _manager.CurrentUser;
            lblTree.Font      = new Font("Segoe UI", 9, FontStyle.Bold);
            lblTree.ForeColor = ACCENT;
            lblTree.Dock      = DockStyle.Top;
            lblTree.Height    = 36;
            lblTree.TextAlign = ContentAlignment.MiddleCenter;
            lblTree.BackColor = Color.FromArgb(25, 25, 25);

            Button btnNewTable = new Button();
            btnNewTable.Text = "+ Nueva tabla";
            btnNewTable.Font = new Font("Segoe UI", 8);
            btnNewTable.BackColor = Color.FromArgb(255, 87, 34);
            btnNewTable.ForeColor = Color.White;
            btnNewTable.FlatStyle = FlatStyle.Flat;
            btnNewTable.FlatAppearance.BorderSize = 0;
            btnNewTable.Dock = DockStyle.Bottom;
            btnNewTable.Height = 30;
            btnNewTable.Cursor = Cursors.Hand;
            btnNewTable.Click += (s, e) =>
            {
                FormCreateTable f = new FormCreateTable(_manager);
                if (f.ShowDialog() == DialogResult.OK)
                    LoadTree(); // refrescar el arbol
            };
            panelLeft.Controls.Add(btnNewTable);

            Button btnNewView = new Button();
            btnNewView.Text = "+ Nueva vista";
            btnNewView.Font = new Font("Segoe UI", 8);
            btnNewView.BackColor = Color.FromArgb(33, 150, 243);  
            btnNewView.ForeColor = Color.White;
            btnNewView.FlatStyle = FlatStyle.Flat;
            btnNewView.FlatAppearance.BorderSize = 0;
            btnNewView.Dock = DockStyle.Bottom;
            btnNewView.Height = 30;
            btnNewView.Cursor = Cursors.Hand;
            btnNewView.Click += (s, e) =>
            {
                FormCreateView f = new FormCreateView(_manager);
                if (f.ShowDialog() == DialogResult.OK)
                    LoadTree();
            };
            panelLeft.Controls.Add(btnNewView);

            treeObjects = new TreeView();
            treeObjects.Dock         = DockStyle.Fill;
            treeObjects.BackColor    = BG2;
            treeObjects.ForeColor    = TEXT;
            treeObjects.BorderStyle  = BorderStyle.None;
            treeObjects.Font         = new Font("Segoe UI", 9);
            treeObjects.ImageList    = BuildImageList();
            treeObjects.AfterExpand += Tree_AfterExpand;
            treeObjects.AfterSelect += Tree_AfterSelect;

            panelLeft.Controls.Add(treeObjects);
            panelLeft.Controls.Add(lblTree);

            // ── Separador naranja ──────────────────────────
            Panel panelAccent = new Panel();
            panelAccent.Dock      = DockStyle.Left;
            panelAccent.Width     = 3;
            panelAccent.BackColor = ACCENT;

            // ── Panel derecho ──────────────────────────────
            Panel panelRight = new Panel();
            panelRight.Dock      = DockStyle.Fill;
            panelRight.BackColor = BG;

            // Titulo del objeto seleccionado
            lblObjectTitle           = new Label();
            lblObjectTitle.Dock      = DockStyle.Top;
            lblObjectTitle.Height    = 36;
            lblObjectTitle.Font      = new Font("Segoe UI", 10, FontStyle.Bold);
            lblObjectTitle.ForeColor = TEXT;
            lblObjectTitle.BackColor = Color.FromArgb(25, 25, 25);
            lblObjectTitle.Text      = "Selecciona un objeto del arbol";
            lblObjectTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblObjectTitle.Padding   = new Padding(10, 0, 0, 0);

            // SplitContainer: detalles arriba / SQL abajo
            SplitContainer split    = new SplitContainer();
            split.Dock              = DockStyle.Fill;
            split.Orientation       = Orientation.Horizontal;
            split.SplitterDistance  = 320;
            split.BackColor         = Color.FromArgb(60, 60, 60);
            split.Panel1.BackColor  = BG;
            split.Panel2.BackColor  = BG;

            // ── Panel superior: tabs de detalles ──────────
            tabDetails               = new TabControl();
            tabDetails.Dock          = DockStyle.Fill;
            tabDetails.BackColor     = BG;
            tabDetails.Font          = new Font("Segoe UI", 9);
            tabDetails.DrawMode      = TabDrawMode.OwnerDrawFixed;
            tabDetails.ItemSize      = new Size(110, 28);
            tabDetails.DrawItem     += TabDetails_DrawItem;

            TabPage tabCols = new TabPage("Columnas / Info");
            TabPage tabDDL  = new TabPage("DDL");
            tabCols.BackColor = BG;
            tabDDL.BackColor  = BG;

            // Grid de detalles (columnas, info)
            gridDetails = CrearGrid();
            gridDetails.Dock = DockStyle.Fill;
            tabCols.Controls.Add(gridDetails);

            // Visor DDL
            txtDDL             = new RichTextBox();
            txtDDL.Dock        = DockStyle.Fill;
            txtDDL.BackColor   = BG2;
            txtDDL.ForeColor   = Color.FromArgb(200, 220, 255);
            txtDDL.Font        = new Font("Consolas", 10);
            txtDDL.ReadOnly    = true;
            txtDDL.BorderStyle = BorderStyle.None;
            txtDDL.ScrollBars  = RichTextBoxScrollBars.Both;
            tabDDL.Controls.Add(txtDDL);

            tabDetails.TabPages.Add(tabCols);
            tabDetails.TabPages.Add(tabDDL);
            split.Panel1.Controls.Add(tabDetails);

            // ── Panel inferior: Editor SQL ─────────────────
            Panel panelSQL = new Panel();
            panelSQL.Dock      = DockStyle.Fill;
            panelSQL.BackColor = BG;

            // Barra del editor
            Panel barSQL = new Panel();
            barSQL.Dock      = DockStyle.Top;
            barSQL.Height    = 36;
            barSQL.BackColor = Color.FromArgb(25, 25, 25);

            Label lblSQLTitle = new Label();
            lblSQLTitle.Text      = "Editor SQL  (F5 para ejecutar)";
            lblSQLTitle.Font      = new Font("Segoe UI", 9, FontStyle.Bold);
            lblSQLTitle.ForeColor = ACCENT;
            lblSQLTitle.AutoSize  = true;
            lblSQLTitle.Location  = new Point(10, 10);

            btnExecute            = new Button();
            btnExecute.Text       = "▶  Ejecutar  (F5)";
            btnExecute.Font       = new Font("Segoe UI", 9, FontStyle.Bold);
            btnExecute.BackColor  = ACCENT;
            btnExecute.ForeColor  = TEXT;
            btnExecute.FlatStyle  = FlatStyle.Flat;
            btnExecute.FlatAppearance.BorderSize = 0;
            btnExecute.Size       = new Size(150, 26);
            btnExecute.Anchor     = AnchorStyles.Top | AnchorStyles.Right;
            btnExecute.Cursor     = Cursors.Hand;
            btnExecute.Click     += BtnExecute_Click;

            barSQL.Controls.Add(lblSQLTitle);
            barSQL.Controls.Add(btnExecute);
            barSQL.Resize += (s, e) =>
            {
                btnExecute.Location = new Point(barSQL.Width - 160, 5);
            };

            // SplitContainer: texto SQL / resultados
            SplitContainer splitSQL   = new SplitContainer();
            splitSQL.Dock             = DockStyle.Fill;
            splitSQL.Orientation      = Orientation.Horizontal;
            splitSQL.SplitterDistance = 110;
            splitSQL.BackColor        = Color.FromArgb(60, 60, 60);
            splitSQL.Panel1.BackColor = BG;
            splitSQL.Panel2.BackColor = BG;

            // Editor de texto SQL
            txtSQL            = new RichTextBox();
            txtSQL.Dock       = DockStyle.Fill;
            txtSQL.BackColor  = BG2;
            txtSQL.ForeColor  = Color.FromArgb(200, 220, 150);
            txtSQL.Font       = new Font("Consolas", 10);
            txtSQL.BorderStyle = BorderStyle.None;
            txtSQL.Text       = "SELECT * FROM USUARIO";
            txtSQL.KeyDown   += TxtSQL_KeyDown;
            splitSQL.Panel1.Controls.Add(txtSQL);

            // ── Grid de RESULTADOS con scroll horizontal ───
            Panel panelResults = new Panel();
            panelResults.Dock      = DockStyle.Fill;
            panelResults.BackColor = BG;

            // Barra de info de resultados
            lblRowCount           = new Label();
            lblRowCount.Dock      = DockStyle.Top;
            lblRowCount.Height    = 22;
            lblRowCount.BackColor = Color.FromArgb(25, 25, 25);
            lblRowCount.ForeColor = TEXT2;
            lblRowCount.Font      = new Font("Segoe UI", 8);
            lblRowCount.Text      = "Resultados";
            lblRowCount.TextAlign = ContentAlignment.MiddleLeft;
            lblRowCount.Padding   = new Padding(6, 0, 0, 0);

            // Grid con scroll horizontal habilitado
            gridResults = new DataGridView();
            gridResults.Dock               = DockStyle.Fill;
            gridResults.BackgroundColor    = BG2;
            gridResults.GridColor          = BG3;
            gridResults.ForeColor          = TEXT;
            gridResults.RowHeadersVisible  = false;
            gridResults.AllowUserToAddRows = false;
            gridResults.ReadOnly           = true;
            gridResults.BorderStyle        = BorderStyle.None;
            gridResults.Font               = new Font("Consolas", 9);
            gridResults.SelectionMode      = DataGridViewSelectionMode.FullRowSelect;

            // ── SCROLL HORIZONTAL: clave esta aqui ─────────
            // AutoSizeColumnsMode = None permite ancho fijo por columna
            gridResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            // ScrollBars Both = vertical + horizontal automatico
            gridResults.ScrollBars          = ScrollBars.Both;
            // Ancho por defecto de cada columna (el usuario puede redimensionar)
            gridResults.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            gridResults.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            gridResults.AllowUserToResizeColumns  = true;

            // Estilos
            gridResults.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(25, 25, 25);
            gridResults.ColumnHeadersDefaultCellStyle.ForeColor = ACCENT;
            gridResults.ColumnHeadersDefaultCellStyle.Font      = new Font("Segoe UI", 9, FontStyle.Bold);
            gridResults.ColumnHeadersHeightSizeMode             = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            gridResults.ColumnHeadersHeight                     = 30;
            gridResults.DefaultCellStyle.BackColor              = BG2;
            gridResults.DefaultCellStyle.ForeColor              = TEXT;
            gridResults.DefaultCellStyle.SelectionBackColor     = Color.FromArgb(80, 255, 87, 34);
            gridResults.DefaultCellStyle.SelectionForeColor     = TEXT;
            gridResults.AlternatingRowsDefaultCellStyle.BackColor = BG3;

            // Al cargar datos, ajustar columnas a un ancho fijo legible
            gridResults.DataSourceChanged += GridResults_DataSourceChanged;

            // Eliminar ghosting al hacer scroll
            ActivarDoubleBuffer(gridResults);

            panelResults.Controls.Add(gridResults);
            panelResults.Controls.Add(lblRowCount);
            splitSQL.Panel2.Controls.Add(panelResults);

            panelSQL.Controls.Add(splitSQL);
            panelSQL.Controls.Add(barSQL);
            split.Panel2.Controls.Add(panelSQL);

            // Barra de estado inferior
            lblStatus           = new Label();
            lblStatus.Dock      = DockStyle.Bottom;
            lblStatus.Height    = 24;
            lblStatus.BackColor = Color.FromArgb(25, 25, 25);
            lblStatus.ForeColor = TEXT2;
            lblStatus.Font      = new Font("Segoe UI", 8);
            lblStatus.Text      = "  Conectado como " + _manager.CurrentUser;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.Padding   = new Padding(8, 0, 0, 0);

            this.KeyPreview = true;
            this.KeyDown   += (s, e) =>
            {
                if (e.KeyCode == Keys.F5) BtnExecute_Click(null, null);
            };

            panelRight.Controls.Add(split);
            panelRight.Controls.Add(lblObjectTitle);

            this.Controls.Add(panelRight);
            this.Controls.Add(panelAccent);
            this.Controls.Add(panelLeft);
            this.Controls.Add(lblStatus);
        }

        // ====================================================
        // EVENTO: cuando el DataSource del grid cambia,
        // pone un ancho fijo legible a cada columna
        // ====================================================
        private void GridResults_DataSourceChanged(object sender, EventArgs e)
        {
            if (gridResults.Columns.Count == 0) return;

            foreach (DataGridViewColumn col in gridResults.Columns)
            {
                // Ancho base: 150px. Si el header es mas largo, se ajusta
                int anchoHeader = TextRenderer.MeasureText(
                    col.HeaderText,
                    new Font("Segoe UI", 9, FontStyle.Bold)).Width + 20;

                col.Width    = Math.Max(150, anchoHeader);
                col.MinimumWidth = 80;
            }
        }

        // ====================================================
        // HELPER: crea un DataGridView con tema oscuro comun
        // ====================================================
        private DataGridView CrearGrid()
        {
            DataGridView g = new DataGridView();
            g.BackgroundColor    = BG2;
            g.GridColor          = BG3;
            g.ForeColor          = TEXT;
            g.RowHeadersVisible  = false;
            g.AllowUserToAddRows = false;
            g.ReadOnly           = true;
            g.SelectionMode      = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.BorderStyle        = BorderStyle.None;
            g.Font               = new Font("Consolas", 9);
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
            ActivarDoubleBuffer(g);
            return g;
        }

        // ====================================================
        // DOUBLE BUFFER — elimina ghosting al hacer scroll
        // DoubleBuffered es protected en Control, se activa
        // via reflection en .NET Framework 4.7.2
        // ====================================================
        private void ActivarDoubleBuffer(DataGridView grid)
        {
            typeof(DataGridView)
                .GetProperty("DoubleBuffered",
                             BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(grid, true, null);
        }

        // ====================================================
        // IMAGELIST del TreeView
        // ====================================================
        private ImageList BuildImageList()
        {
            ImageList il = new ImageList();
            il.ImageSize  = new Size(16, 16);
            il.ColorDepth = ColorDepth.Depth32Bit;

            string[] tipos = { "Tables","Views","Procedures","Functions",
                               "Packages","Triggers","Indexes","Sequences",
                               "Tablespaces","Users","Item" };
            Color[] cols = {
                Color.FromArgb(255,152,0),
                Color.FromArgb(33,150,243),
                Color.FromArgb(76,175,80),
                Color.FromArgb(0,188,212),
                Color.FromArgb(156,39,176),
                Color.FromArgb(244,67,54),
                Color.FromArgb(255,235,59),
                Color.FromArgb(121,85,72),
                Color.FromArgb(96,125,139),
                Color.FromArgb(233,30,99),
                Color.FromArgb(158,158,158)
            };

            for (int i = 0; i < tipos.Length; i++)
            {
                Bitmap bmp = new Bitmap(16, 16);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.FillRectangle(new SolidBrush(cols[i]), 2, 2, 12, 12);
                }
                il.Images.Add(tipos[i], bmp);
            }
            return il;
        }

        // ====================================================
        // CARGAR ARBOL
        // ====================================================
        private void LoadTree()
        {
            treeObjects.Nodes.Clear();

            var nodos = new[]
            {
                new { Text = "Tables",      Tag = "Tables"      },
                new { Text = "Views",       Tag = "Views"       },
                new { Text = "Procedures",  Tag = "Procedures"  },
                new { Text = "Functions",   Tag = "Functions"   },
                new { Text = "Packages",    Tag = "Packages"    },
                new { Text = "Triggers",    Tag = "Triggers"    },
                new { Text = "Indexes",     Tag = "Indexes"     },
                new { Text = "Sequences",   Tag = "Sequences"   },
                new { Text = "Tablespaces", Tag = "Tablespaces" },
                new { Text = "Users",       Tag = "Users"       },
            };

            foreach (var n in nodos)
            {
                TreeNode nodo = new TreeNode(n.Text);
                nodo.Tag              = n.Tag;
                nodo.ImageKey         = n.Tag;
                nodo.SelectedImageKey = n.Tag;
                nodo.Nodes.Add(new TreeNode("Cargando..."));
                treeObjects.Nodes.Add(nodo);
            }
        }

        // ====================================================
        // EXPANDIR NODO — carga hijos lazy
        // ====================================================
        private void Tree_AfterExpand(object sender, TreeViewEventArgs e)
        {
            TreeNode nodo = e.Node;
            if (nodo.Tag == null) return;
            if (nodo.Nodes.Count != 1 || nodo.Nodes[0].Text != "Cargando...") return;

            nodo.Nodes.Clear();
            string tipo  = nodo.Tag.ToString();
            string owner = _manager.CurrentUser;

            try
            {
                DataTable dt        = ObtenerListaObjetos(tipo, owner);
                string    colNombre = ObtenerColumnaNombre(tipo);

                if (dt == null || dt.Rows.Count == 0)
                {
                    nodo.Nodes.Add(new TreeNode("(Sin objetos)"));
                    return;
                }

                foreach (DataRow row in dt.Rows)
                {
                    string   nombre = row[colNombre].ToString();
                    TreeNode hijo   = new TreeNode(nombre);
                    hijo.Tag              = tipo + ":" + nombre;
                    hijo.ImageKey         = "Item";
                    hijo.SelectedImageKey = "Item";
                    nodo.Nodes.Add(hijo);
                }

                SetStatus(dt.Rows.Count + " objeto(s) — " + tipo);
            }
            catch (Exception ex)
            {
                nodo.Nodes.Add(new TreeNode("Error: " + ex.Message));
                SetStatus("Error al cargar " + tipo + ": " + ex.Message, true);
            }
        }

        // ====================================================
        // SELECCIONAR NODO — muestra detalles
        // ====================================================
        private void Tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode nodo = e.Node;
            if (nodo.Tag == null) return;

            string tag = nodo.Tag.ToString();
            if (!tag.Contains(":")) return;

            string[] partes = tag.Split(new char[] { ':' }, 2);
            string tipo     = partes[0];
            string nombre   = partes[1];
            string owner    = _manager.CurrentUser;

            lblObjectTitle.Text = tipo.TrimEnd('s') + ":  " + nombre;

            MostrarDetalles(tipo, nombre, owner);
            MostrarDDL(tipo, nombre, owner);
        }

        // ====================================================
        // MOSTRAR DETALLES EN EL GRID
        // ====================================================
        private void MostrarDetalles(string tipo, string nombre, string owner)
        {
            try
            {
                DataTable dt = null;

                switch (tipo)
                {
                    case "Tables":
                        dt = _repo.GetTableColumns(owner, nombre);
                        break;
                    case "Views":
                        dt = _repo.GetViews(owner);
                        break;
                    case "Procedures":
                    case "Functions":
                    case "Packages":
                        dt = _repo.GetProgrammingObjects(owner);
                        break;
                    case "Triggers":
                        dt = _repo.GetTriggerDetail(owner, nombre);
                        break;
                    case "Indexes":
                        dt = _repo.GetIndexes(owner);
                        break;
                    case "Sequences":
                        dt = _repo.GetSequences(owner);
                        break;
                    case "Tablespaces":
                        dt = _repo.GetTablespaces();
                        break;
                    case "Users":
                        dt = _repo.GetUsers();
                        break;
                }

                if (dt != null)
                {
                    gridDetails.DataSource = dt;
                    SetStatus("Detalles cargados — " + nombre +
                              "  (" + dt.Rows.Count + " filas)");
                }
            }
            catch (Exception ex)
            {
                SetStatus("Error al cargar detalles: " + ex.Message, true);
            }
        }

        // ====================================================
        // MOSTRAR DDL
        // ====================================================
        private void MostrarDDL(string tipo, string nombre, string owner)
        {
            try
            {
                string oracleTipo = MapearTipoOracle(tipo);
                if (string.IsNullOrEmpty(oracleTipo))
                {
                    txtDDL.Text = "-- DDL no disponible para este tipo.";
                    return;
                }

                string ddl = _repo.GetDDL(oracleTipo, nombre, owner);
                txtDDL.Text = string.IsNullOrEmpty(ddl)
                                  ? "-- Sin DDL disponible"
                                  : ddl;
            }
            catch (Exception ex)
            {
                txtDDL.Text = "-- Error al obtener DDL:\r\n-- " + ex.Message;
            }
        }

        // ====================================================
        // EJECUTAR SQL
        // ====================================================
        private void BtnExecute_Click(object sender, EventArgs e)
        {
            string sql = txtSQL.SelectedText.Length > 0
                             ? txtSQL.SelectedText
                             : txtSQL.Text.Trim();

            if (sql.EndsWith(";"))
                sql = sql.Substring(0, sql.Length - 1).Trim();

            if (string.IsNullOrWhiteSpace(sql)) return;

            SetStatus("Ejecutando...");
            gridResults.DataSource = null;
            lblRowCount.Text       = "Ejecutando...";

            try
            {
                string sqlUpper = sql.TrimStart().ToUpper();

                if (sqlUpper.StartsWith("SELECT") || sqlUpper.StartsWith("WITH"))
                {
                    DataTable dt = _manager.ExecuteQuery(sql);
                    gridResults.DataSource = dt;
                    lblRowCount.Text       = "  " + dt.Rows.Count + " fila(s)  |  " +
                                            dt.Columns.Count + " columna(s)";
                    SetStatus(dt.Rows.Count + " fila(s) retornada(s).");
                }
                else
                {
                    int afectadas = _manager.ExecuteNonQuery(sql);
                    lblRowCount.Text = "  " + afectadas + " fila(s) afectada(s)";
                    SetStatus(afectadas + " fila(s) afectada(s).");
                    LoadTree();
                }
            }
            catch (Exception ex)
            {
                lblRowCount.Text = "  Error al ejecutar";
                SetStatus("Error SQL: " + ex.Message, true);
                MessageBox.Show(ex.Message, "Error SQL",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TxtSQL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                BtnExecute_Click(null, null);
            }
        }

        // ====================================================
        // HELPERS
        // ====================================================
        private DataTable ObtenerListaObjetos(string tipo, string owner)
        {
            switch (tipo)
            {
                case "Tables":      return _repo.GetTables(owner);
                case "Views":       return _repo.GetViews(owner);
                case "Procedures":
                case "Functions":
                case "Packages":    return _repo.GetProgrammingObjects(owner);
                case "Triggers":    return _repo.GetTriggers(owner);
                case "Indexes":     return _repo.GetIndexes(owner);
                case "Sequences":   return _repo.GetSequences(owner);
                case "Tablespaces": return _repo.GetTablespaces();
                case "Users":       return _repo.GetUsers();
                default:            return null;
            }
        }

        private string ObtenerColumnaNombre(string tipo)
        {
            switch (tipo)
            {
                case "Tables":      return "TABLE_NAME";
                case "Views":       return "VIEW_NAME";
                case "Procedures":
                case "Functions":
                case "Packages":    return "OBJECT_NAME";
                case "Triggers":    return "TRIGGER_NAME";
                case "Indexes":     return "INDEX_NAME";
                case "Sequences":   return "SEQUENCE_NAME";
                case "Tablespaces": return "TABLESPACE_NAME";
                case "Users":       return "USERNAME";
                default:            return "OBJECT_NAME";
            }
        }

        private string MapearTipoOracle(string tipo)
        {
            switch (tipo)
            {
                case "Tables":     return "TABLE";
                case "Views":      return "VIEW";
                case "Procedures": return "PROCEDURE";
                case "Functions":  return "FUNCTION";
                case "Packages":   return "PACKAGE";
                case "Triggers":   return "TRIGGER";
                case "Indexes":    return "INDEX";
                case "Sequences":  return "SEQUENCE";
                default:           return "";
            }
        }

        private void SetStatus(string msg, bool esError = false)
        {
            lblStatus.ForeColor = esError ? Color.FromArgb(255, 80, 80) : TEXT2;
            lblStatus.Text      = "  " + msg;
        }

        // ====================================================
        // CUSTOM DRAW TABS
        // ====================================================
        private void TabDetails_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage  page = tabDetails.TabPages[e.Index];
            Graphics g    = e.Graphics;
            bool sel      = e.Index == tabDetails.SelectedIndex;

            g.FillRectangle(new SolidBrush(sel ? BG2 : Color.FromArgb(25, 25, 25)), e.Bounds);

            if (sel)
                g.FillRectangle(new SolidBrush(ACCENT),
                                e.Bounds.Left, e.Bounds.Bottom - 2, e.Bounds.Width, 2);

            StringFormat sf = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(page.Text,
                         new Font("Segoe UI", 9, FontStyle.Bold),
                         new SolidBrush(sel ? ACCENT : TEXT2),
                         e.Bounds, sf);
        }
    }
}
