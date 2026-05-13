namespace PVDesigner.Forms
{
    partial class VesselDesignerForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tabMain     = new System.Windows.Forms.TabControl();
            this.tabType     = new System.Windows.Forms.TabPage();
            this.tabGeometry = new System.Windows.Forms.TabPage();
            this.tabLoads    = new System.Windows.Forms.TabPage();
            this.tabResults  = new System.Windows.Forms.TabPage();

            // ── Shared helpers ─────────────────────────────────────────────
            System.Func<string, int, System.Windows.Forms.Label> MakeLabel = (txt, y) =>
                new System.Windows.Forms.Label
                {
                    Text = txt, Left = 15, Top = y, Width = 200,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft
                };

            // ── TAB 1: Vessel Type ─────────────────────────────────────────

            // Vessel identification
            var grpName = new System.Windows.Forms.GroupBox
                { Text = "Vessel Identification", Left = 12, Top = 12, Width = 440, Height = 60 };
            var lblName = new System.Windows.Forms.Label
                { Text = "Vessel Name / Tag:", Left = 10, Top = 26, Width = 130 };
            this.txtVesselName = new System.Windows.Forms.TextBox
                { Left = 145, Top = 23, Width = 180, Text = "V-101" };
            grpName.Controls.AddRange(new System.Windows.Forms.Control[] { lblName, txtVesselName });
            txtVesselName.TextChanged += AnyValueChanged;

            // Orientation
            var grpOrientation = new System.Windows.Forms.GroupBox
                { Text = "Vessel Orientation", Left = 12, Top = 82, Width = 440, Height = 60 };
            this.rbVertical   = new System.Windows.Forms.RadioButton
                { Text = "Vertical",   Left = 20, Top = 25, Width = 120, Checked = true };
            this.rbHorizontal = new System.Windows.Forms.RadioButton
                { Text = "Horizontal", Left = 160, Top = 25, Width = 120 };
            grpOrientation.Controls.AddRange(new System.Windows.Forms.Control[] { rbVertical, rbHorizontal });
            rbVertical.CheckedChanged   += rbVertical_CheckedChanged;
            rbHorizontal.CheckedChanged += rbVertical_CheckedChanged;

            // Same heads
            this.chkSameHeads = new System.Windows.Forms.CheckBox
                { Text = "Same head type on both ends", Left = 12, Top = 154, Width = 280, Checked = true };
            chkSameHeads.CheckedChanged += chkSameHeads_CheckedChanged;

            // Top head group
            var grpTopHead = new System.Windows.Forms.GroupBox
                { Text = "Top / Left Head", Left = 12, Top = 180, Width = 440, Height = 100 };
            this.lblTopHead = new System.Windows.Forms.Label
                { Text = "Top Head:", Left = 10, Top = 26, Width = 110 };
            this.cmbTopHead = new System.Windows.Forms.ComboBox
                { Left = 120, Top = 22, Width = 220, DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList };
            cmbTopHead.Items.AddRange(new string[]
                { "Klöpperboden (DIN 28011)", "Korbbogen (DIN 28013)", "Conical", "Spherical", "Flat" });
            cmbTopHead.SelectedIndex = 0;
            this.lblConicalTop = new System.Windows.Forms.Label
                { Text = "Apex angle α [°]:", Left = 10, Top = 62, Width = 130, Visible = false };
            this.nudConicalAngleTop = new System.Windows.Forms.NumericUpDown
                { Left = 145, Top = 58, Width = 70, Minimum = 10, Maximum = 75, Value = 30,
                  DecimalPlaces = 1, Increment = 1M, Visible = false };
            grpTopHead.Controls.AddRange(new System.Windows.Forms.Control[]
                { lblTopHead, cmbTopHead, lblConicalTop, nudConicalAngleTop });
            cmbTopHead.SelectedIndexChanged += cmbTopHead_SelectedIndexChanged;

            // Bottom head group
            this.grpBottomHead = new System.Windows.Forms.GroupBox
                { Text = "Bottom / Right Head", Left = 12, Top = 292, Width = 440, Height = 100, Visible = false };
            this.lblBottomHead = new System.Windows.Forms.Label
                { Text = "Bottom Head:", Left = 10, Top = 26, Width = 110 };
            this.cmbBottomHead = new System.Windows.Forms.ComboBox
                { Left = 120, Top = 22, Width = 220, DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList };
            cmbBottomHead.Items.AddRange(new string[]
                { "Klöpperboden (DIN 28011)", "Korbbogen (DIN 28013)", "Conical", "Spherical", "Flat" });
            cmbBottomHead.SelectedIndex = 0;
            this.lblConicalBottom = new System.Windows.Forms.Label
                { Text = "Apex angle α [°]:", Left = 10, Top = 62, Width = 130, Visible = false };
            this.nudConicalAngleBottom = new System.Windows.Forms.NumericUpDown
                { Left = 145, Top = 58, Width = 70, Minimum = 10, Maximum = 75, Value = 30,
                  DecimalPlaces = 1, Increment = 1M, Visible = false };
            grpBottomHead.Controls.AddRange(new System.Windows.Forms.Control[]
                { lblBottomHead, cmbBottomHead, lblConicalBottom, nudConicalAngleBottom });
            cmbBottomHead.SelectedIndexChanged += cmbBottomHead_SelectedIndexChanged;

            tabType.Controls.AddRange(new System.Windows.Forms.Control[]
                { grpName, grpOrientation, chkSameHeads, grpTopHead, grpBottomHead });

            // ── TAB 2: Geometry ────────────────────────────────────────────
            int row = 20;
            var lblDiLabel = MakeLabel("Inner Diameter Di [mm]:", row);
            this.nudDi = new System.Windows.Forms.NumericUpDown
                { Left = 220, Top = row, Width = 120, Minimum = 100, Maximum = 10000,
                  Value = 1000, DecimalPlaces = 1, Increment = 10M };
            nudDi.ValueChanged += AnyValueChanged; row += 45;

            var lblShellLabel = MakeLabel("Shell Length L [mm]:", row);
            this.nudShellLength = new System.Windows.Forms.NumericUpDown
                { Left = 220, Top = row, Width = 120, Minimum = 100, Maximum = 50000,
                  Value = 2000, DecimalPlaces = 1, Increment = 50M };
            nudShellLength.ValueChanged += AnyValueChanged; row += 45;

            var lblUnit = new System.Windows.Forms.Label
                { Text = "All dimensions in millimetres.", Left = 15, Top = row + 10,
                  Width = 400, ForeColor = System.Drawing.Color.Gray };

            tabGeometry.Controls.AddRange(new System.Windows.Forms.Control[]
                { lblDiLabel, nudDi, lblShellLabel, nudShellLength, lblUnit });

            // ── TAB 3: Loads & Material ────────────────────────────────────
            row = 20;
            var lblPres = MakeLabel("Design Pressure p [bar]:", row);
            this.nudPressure = new System.Windows.Forms.NumericUpDown
                { Left = 220, Top = row, Width = 120, Minimum = 0.1M, Maximum = 400,
                  Value = 10, DecimalPlaces = 2, Increment = 0.5M };
            nudPressure.ValueChanged += AnyValueChanged; row += 45;

            var lblTemp = MakeLabel("Design Temperature T [°C]:", row);
            this.nudTemperature = new System.Windows.Forms.NumericUpDown
                { Left = 220, Top = row, Width = 120, Minimum = -200, Maximum = 700,
                  Value = 200, DecimalPlaces = 1, Increment = 5M };
            nudTemperature.ValueChanged += AnyValueChanged; row += 45;

            var lblMatGrp = MakeLabel("Material Group (AD-2000 W):", row);
            this.cmbMaterialGroup = new System.Windows.Forms.ComboBox
                { Left = 220, Top = row, Width = 170, DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList };
            cmbMaterialGroup.Items.AddRange(new string[]
                { "W I – Ferritic steels", "W II – Austenitic steels", "NE – Non-ferrous" });
            cmbMaterialGroup.SelectedIndex = 0;
            cmbMaterialGroup.SelectedIndexChanged += cmbMaterialGroup_SelectedIndexChanged; row += 45;

            var lblMatGrade = MakeLabel("Material Grade:", row);
            this.cmbMaterialGrade = new System.Windows.Forms.ComboBox
                { Left = 220, Top = row, Width = 170, DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList };
            cmbMaterialGrade.SelectedIndexChanged += AnyValueChanged; row += 45;

            var lblStress = MakeLabel("Allowable stress f [N/mm²]:", row);
            this.nudAllowableStress = new System.Windows.Forms.NumericUpDown
                { Left = 220, Top = row, Width = 120, Minimum = 50, Maximum = 500,
                  Value = 170, DecimalPlaces = 1, Increment = 5M };
            nudAllowableStress.ValueChanged += AnyValueChanged; row += 45;

            var lblC1 = MakeLabel("c1 – Corrosion allowance [mm]:", row);
            this.nudC1 = new System.Windows.Forms.NumericUpDown
                { Left = 220, Top = row, Width = 120, Minimum = 0, Maximum = 10,
                  Value = 1, DecimalPlaces = 1, Increment = 0.5M };
            nudC1.ValueChanged += AnyValueChanged; row += 45;

            var lblC2 = MakeLabel("c2 – Minus tolerance [mm]:", row);
            this.nudC2 = new System.Windows.Forms.NumericUpDown
                { Left = 220, Top = row, Width = 120, Minimum = 0, Maximum = 5,
                  Value = 0.8M, DecimalPlaces = 2, Increment = 0.1M };
            nudC2.ValueChanged += AnyValueChanged;

            tabLoads.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblPres, nudPressure, lblTemp, nudTemperature,
                lblMatGrp, cmbMaterialGroup, lblMatGrade, cmbMaterialGrade,
                lblStress, nudAllowableStress, lblC1, nudC1, lblC2, nudC2
            });

            // ── TAB 4: Results ─────────────────────────────────────────────
            var grpThick = new System.Windows.Forms.GroupBox
                { Text = "Required Wall Thicknesses (AD-2000)", Left = 10, Top = 10, Width = 455, Height = 110 };
            this.lblResultShell = new System.Windows.Forms.Label
                { Left = 10, Top = 25, Width = 435, Font = new System.Drawing.Font("Consolas", 9f) };
            this.lblResultTopHead = new System.Windows.Forms.Label
                { Left = 10, Top = 50, Width = 435, Font = new System.Drawing.Font("Consolas", 9f) };
            this.lblResultBotHead = new System.Windows.Forms.Label
                { Left = 10, Top = 75, Width = 435, Font = new System.Drawing.Font("Consolas", 9f) };
            grpThick.Controls.AddRange(new System.Windows.Forms.Control[]
                { lblResultShell, lblResultTopHead, lblResultBotHead });

            var lblSumTitle = new System.Windows.Forms.Label
            {
                Text = "Full Summary:", Left = 10, Top = 130, Width = 200,
                Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold)
            };
            this.txtSummary = new System.Windows.Forms.TextBox
            {
                Left = 10, Top = 150, Width = 455, Height = 210,
                Multiline = true, ReadOnly = true,
                ScrollBars = System.Windows.Forms.ScrollBars.Vertical,
                Font = new System.Drawing.Font("Consolas", 9f),
                BackColor = System.Drawing.SystemColors.Window
            };

            tabResults.Controls.AddRange(new System.Windows.Forms.Control[]
                { grpThick, lblSumTitle, txtSummary });

            // ── Buttons ────────────────────────────────────────────────────
            this.btnCalculate = new System.Windows.Forms.Button
                { Text = "Calculate",   Left = 100, Top = 480, Width = 110, Height = 30 };
            this.btnBuildModel = new System.Windows.Forms.Button
                { Text = "Build Model", Left = 220, Top = 480, Width = 110, Height = 30 };
            this.btnClose = new System.Windows.Forms.Button
                { Text = "Close",       Left = 340, Top = 480, Width = 80,  Height = 30 };

            btnCalculate.Click  += btnCalculate_Click;
            btnBuildModel.Click += btnBuildModel_Click;
            btnClose.Click      += btnClose_Click;

            // ── TabControl ─────────────────────────────────────────────────
            tabType.Text     = "1 – Vessel Type";
            tabGeometry.Text = "2 – Geometry";
            tabLoads.Text    = "3 – Loads & Material";
            tabResults.Text  = "4 – Results";

            tabType.Padding     = new System.Windows.Forms.Padding(8);
            tabGeometry.Padding = new System.Windows.Forms.Padding(8);
            tabLoads.Padding    = new System.Windows.Forms.Padding(8);
            tabResults.Padding  = new System.Windows.Forms.Padding(8);

            this.tabMain.Controls.AddRange(new System.Windows.Forms.TabPage[]
                { tabType, tabGeometry, tabLoads, tabResults });
            this.tabMain.Location = new System.Drawing.Point(0, 0);
            this.tabMain.Size     = new System.Drawing.Size(480, 472);

            // ── Form ──────────────────────────────────────────────────────
            this.Text            = "PV Designer – AD-2000 Pressure Vessel";
            this.ClientSize      = new System.Drawing.Size(480, 522);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.StartPosition   = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Controls.AddRange(new System.Windows.Forms.Control[]
                { tabMain, btnCalculate, btnBuildModel, btnClose });
        }

        // ── Control declarations ───────────────────────────────────────────────
        private System.Windows.Forms.TabControl    tabMain;
        private System.Windows.Forms.TabPage       tabType, tabGeometry, tabLoads, tabResults;

        private System.Windows.Forms.TextBox       txtVesselName;

        private System.Windows.Forms.RadioButton   rbVertical, rbHorizontal;
        private System.Windows.Forms.CheckBox      chkSameHeads;
        private System.Windows.Forms.Label         lblTopHead, lblBottomHead;
        private System.Windows.Forms.ComboBox      cmbTopHead, cmbBottomHead;
        private System.Windows.Forms.Label         lblConicalTop, lblConicalBottom;
        private System.Windows.Forms.NumericUpDown nudConicalAngleTop, nudConicalAngleBottom;
        private System.Windows.Forms.GroupBox      grpBottomHead;

        private System.Windows.Forms.NumericUpDown nudDi, nudShellLength;
        private System.Windows.Forms.NumericUpDown nudPressure, nudTemperature;
        private System.Windows.Forms.NumericUpDown nudAllowableStress, nudC1, nudC2;
        private System.Windows.Forms.ComboBox      cmbMaterialGroup, cmbMaterialGrade;

        private System.Windows.Forms.Label         lblResultShell, lblResultTopHead, lblResultBotHead;
        private System.Windows.Forms.TextBox       txtSummary;

        private System.Windows.Forms.Button        btnCalculate, btnBuildModel, btnClose;
    }
}
