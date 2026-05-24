using DatabaseManager.Data;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public class FormCreateView : Form
    {
        // --------------------------------------------------------
        // Dependencias
        // --------------------------------------------------------
        private readonly OracleConnectionManager _manager;

        // --------------------------------------------------------
        // Controles
        // --------------------------------------------------------
        private TextBox     txtViewName;
        private RichTextBox txtSelectBody;
        private RichTextBox txtDDLPreview;
        private Button      btnCreate;
        private Button      btnCancel;
        private Label       lblStatus;
        private CheckBox    chkOrReplace;
        private CheckBox    chkForce;

        // Colores
        private readonly Color BG     = Color.FromArgb(30, 30, 30);
        private readonly Color BG2    = Color.FromArgb(42, 42, 42);
        private readonly Color BG3    = Color.FromArgb(55, 55, 55);
        private readonly Color ACCENT = Color.FromArgb(255, 87, 34);
        private readonly Color TEXT   = Color.White;
        private readonly Color TEXT2  = Color.FromArgb(180, 180, 180);

        public FormCreateView(OracleConnectionManager manager)
        {
            _manager = manager;
            BuildUI();
        }

        // ====================================================
        // CONSTRUCCION DE LA INTERFAZ
        // ====================================================
        private void BuildUI()
        {
            this.Text            = "Crear Vista — " + _manager.CurrentUser;
            this.Size            = new Size(820, 620);
            this.MinimumSize     = new Size(700, 520);
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

            // Panel principal
            Panel panelMain = new Panel();
            panelMain.Dock      = DockStyle.Fill;
            panelMain.BackColor = BG;
            panelMain.Padding   = new Padding(16, 12, 16, 8);
            this.Controls.Add(panelMain);

            // ── Seccion: nombre de la vista ────────────────
            Label lblSec1 = CrearLabelSeccion("Nombre de la vista");
            lblSec1.Dock = DockStyle.Top;

            Panel panelNombre = new Panel();
            panelNombre.Dock      = DockStyle.Top;
            panelNombre.Height    = 52;
            panelNombre.BackColor = BG;

            Label lblNombre     = new Label();
            lblNombre.Text      = "Nombre:";
            lblNombre.Font      = new Font("Segoe UI", 9);
            lblNombre.ForeColor = TEXT2;
            lblNombre.AutoSize  = true;
            lblNombre.Location  = new Point(0, 14);

            txtViewName                  = new TextBox();
            txtViewName.Font             = new Font("Segoe UI", 10, FontStyle.Bold);
            txtViewName.BackColor        = BG2;
            txtViewName.ForeColor        = TEXT;
            txtViewName.BorderStyle      = BorderStyle.FixedSingle;
            txtViewName.Size             = new Size(260, 28);
            txtViewName.Location         = new Point(70, 10);
            txtViewName.CharacterCasing  = CharacterCasing.Upper;
            txtViewName.TextChanged     += (s, e) => ActualizarPreviewDDL();

            Label lblSchema     = new Label();
            lblSchema.Text      = "Schema: " + _manager.CurrentUser;
            lblSchema.Font      = new Font("Segoe UI", 9);
            lblSchema.ForeColor = ACCENT;
            lblSchema.AutoSize  = true;
            lblSchema.Location  = new Point(344, 14);

            // Opciones OR REPLACE y FORCE
            chkOrReplace           = new CheckBox();
            chkOrReplace.Text      = "OR REPLACE";
            chkOrReplace.Font      = new Font("Segoe UI", 9);
            chkOrReplace.ForeColor = TEXT2;
            chkOrReplace.Checked   = true;
            chkOrReplace.AutoSize  = true;
            chkOrReplace.Location  = new Point(490, 12);
            chkOrReplace.CheckedChanged += (s, e) => ActualizarPreviewDDL();

            chkForce           = new CheckBox();
            chkForce.Text      = "FORCE";
            chkForce.Font      = new Font("Segoe UI", 9);
            chkForce.ForeColor = TEXT2;
            chkForce.Checked   = false;
            chkForce.AutoSize  = true;
            chkForce.Location  = new Point(600, 12);
            chkForce.CheckedChanged += (s, e) => ActualizarPreviewDDL();

            panelNombre.Controls.Add(chkForce);
            panelNombre.Controls.Add(chkOrReplace);
            panelNombre.Controls.Add(lblSchema);
            panelNombre.Controls.Add(txtViewName);
            panelNombre.Controls.Add(lblNombre);

            // ── Seccion: cuerpo SELECT ─────────────────────
            Label lblSec2 = CrearLabelSeccion("Cuerpo de la vista  (sentencia SELECT)");
            lblSec2.Dock = DockStyle.Top;

            txtSelectBody            = new RichTextBox();
            txtSelectBody.Dock       = DockStyle.Top;
            txtSelectBody.Height     = 180;
            txtSelectBody.BackColor  = BG2;
            txtSelectBody.ForeColor  = Color.FromArgb(200, 220, 150);
            txtSelectBody.Font       = new Font("Consolas", 10);
            txtSelectBody.BorderStyle = BorderStyle.None;
            txtSelectBody.ScrollBars = RichTextBoxScrollBars.Both;
            txtSelectBody.Text       = "SELECT *\r\nFROM    \r\nWHERE   1 = 1";
            txtSelectBody.TextChanged += (s, e) => ActualizarPreviewDDL();

            // ── Seccion: preview DDL ───────────────────────
            Label lblSec3 = CrearLabelSeccion("Preview DDL generado");
            lblSec3.Dock = DockStyle.Top;

            txtDDLPreview            = new RichTextBox();
            txtDDLPreview.Dock       = DockStyle.Fill;
            txtDDLPreview.BackColor  = BG2;
            txtDDLPreview.ForeColor  = Color.FromArgb(200, 220, 255);
            txtDDLPreview.Font       = new Font("Consolas", 10);
            txtDDLPreview.ReadOnly   = true;
            txtDDLPreview.BorderStyle = BorderStyle.None;
            txtDDLPreview.ScrollBars = RichTextBoxScrollBars.Both;

            // ── Barra inferior ─────────────────────────────
            Panel panelBottom = new Panel();
            panelBottom.Dock      = DockStyle.Bottom;
            panelBottom.Height    = 50;
            panelBottom.BackColor = Color.FromArgb(25, 25, 25);

            lblStatus           = new Label();
            lblStatus.Text      = "";
            lblStatus.Font      = new Font("Segoe UI", 9);
            lblStatus.ForeColor = TEXT2;
            lblStatus.AutoSize  = false;
            lblStatus.Size      = new Size(480, 24);
            lblStatus.Location  = new Point(10, 14);
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;

            btnCreate           = new Button();
            btnCreate.Text      = "Crear vista";
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

            // Ensamblar (orden inverso por DockStyle.Top)
            panelMain.Controls.Add(txtDDLPreview);
            panelMain.Controls.Add(lblSec3);
            panelMain.Controls.Add(txtSelectBody);
            panelMain.Controls.Add(lblSec2);
            panelMain.Controls.Add(panelNombre);
            panelMain.Controls.Add(lblSec1);

            this.Controls.Add(panelBottom);

            // Preview inicial
            ActualizarPreviewDDL();
        }

        // ====================================================
        // GENERAR Y MOSTRAR PREVIEW DDL
        // ====================================================
        private void ActualizarPreviewDDL()
        {
            txtDDLPreview.Text = GenerarDDL();
        }

        private string GenerarDDL()
        {
            string viewName = txtViewName.Text.Trim();
            string body     = txtSelectBody.Text.Trim();

            if (string.IsNullOrEmpty(viewName))
                return "-- Escribe el nombre de la vista para ver el DDL";

            if (string.IsNullOrEmpty(body))
                return "-- Escribe el SELECT que define la vista";

            // Quitar punto y coma final del SELECT si lo tiene
            if (body.EndsWith(";"))
                body = body.Substring(0, body.Length - 1).TrimEnd();

            string orReplace = chkOrReplace.Checked ? "OR REPLACE " : "";
            string force     = chkForce.Checked     ? "FORCE "      : "";

            return "CREATE " + orReplace + force + "VIEW " +
                   _manager.CurrentUser + "." + viewName +
                   " AS\r\n" + body + ";";
        }

        // ====================================================
        // EVENTO — Crear vista
        // ====================================================
        private void BtnCreate_Click(object sender, EventArgs e)
        {
            string ddl = GenerarDDL();

            if (ddl.StartsWith("--"))
            {
                MostrarError("Completa el nombre de la vista y el SELECT.");
                return;
            }

            // Validacion minima: el body debe tener SELECT
            string body = txtSelectBody.Text.Trim().ToUpper();
            if (!body.StartsWith("SELECT") && !body.StartsWith("WITH"))
            {
                MostrarError("El cuerpo debe comenzar con SELECT o WITH.");
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Se ejecutara el siguiente DDL:\r\n\r\n" + ddl +
                "\r\n\r\n¿Confirmar?",
                "Crear vista",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                // Quitar punto y coma final para ODP.NET
                string sqlEjecutar = ddl.TrimEnd().TrimEnd(';');
                _manager.ExecuteNonQuery(sqlEjecutar);

                lblStatus.ForeColor = Color.LimeGreen;
                lblStatus.Text      = "Vista '" + txtViewName.Text.Trim() +
                                      "' creada exitosamente.";

                MessageBox.Show(
                    "Vista creada exitosamente.",
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
