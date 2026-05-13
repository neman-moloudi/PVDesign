using System;
using System.Drawing;
using System.Windows.Forms;
using PVDesigner.Models;

namespace PVDesigner.Forms
{
    public partial class VesselDesignerForm : Form
    {
        public VesselSpecification Specification { get; private set; } = new VesselSpecification();

        public VesselDesignerForm()
        {
            InitializeComponent();
            PopulateMaterialGrades();
            SyncUIFromSpec();
            UpdateResults();
        }

        // ── Populate material grades based on group ──────────────────────────
        private void PopulateMaterialGrades()
        {
            cmbMaterialGrade.Items.Clear();
            switch (Specification.Material)
            {
                case MaterialGroup.W_I:
                    cmbMaterialGrade.Items.AddRange(new[] { "P265GH", "P355GH", "16Mo3", "13CrMo4-5" });
                    break;
                case MaterialGroup.W_II:
                    cmbMaterialGrade.Items.AddRange(new[] { "X5CrNi18-10", "X6CrNiMoTi17-12-2", "X2CrNi19-11" });
                    break;
                case MaterialGroup.NE:
                    cmbMaterialGrade.Items.AddRange(new[] { "Al99.5", "CuZn28Sn1As" });
                    break;
            }
            cmbMaterialGrade.SelectedIndex = 0;
        }

        // ── Sync all UI controls from the current Specification object ────────
        private void SyncUIFromSpec()
        {
            // Tab 1 – Vessel Type
            if (Specification.Orientation == VesselOrientation.Vertical)
                rbVertical.Checked = true;
            else
                rbHorizontal.Checked = true;

            chkSameHeads.Checked = Specification.SameHeadsBothEnds;
            SetHeadCombo(cmbTopHead, Specification.TopHead.Type);
            SetHeadCombo(cmbBottomHead, Specification.BottomHead.Type);
            nudConicalAngleTop.Value = (decimal)Specification.TopHead.ConicalAngle;
            nudConicalAngleBottom.Value = (decimal)Specification.BottomHead.ConicalAngle;
            lblBottomHead.Text = Specification.Orientation == VesselOrientation.Vertical ? "Bottom Head:" : "Right Head:";
            lblTopHead.Text = Specification.Orientation == VesselOrientation.Vertical ? "Top Head:" : "Left Head:";

            UpdateBottomHeadVisibility();

            // Tab 2 – Geometry
            nudDi.Value = (decimal)Specification.InnerDiameter;
            nudShellLength.Value = (decimal)Specification.ShellLength;

            // Tab 3 – Loads & Material
            nudPressure.Value = (decimal)Specification.DesignPressure;
            nudTemperature.Value = (decimal)Specification.DesignTemperature;
            nudAllowableStress.Value = (decimal)Specification.AllowableStress;
            nudC1.Value = (decimal)Specification.CorrosionAllowance;
            nudC2.Value = (decimal)Specification.MinusTolerance;

            cmbMaterialGroup.SelectedIndex = (int)Specification.Material;
        }

        private void SetHeadCombo(ComboBox cmb, HeadType type)
        {
            cmb.SelectedIndex = (int)type;
        }

        private void UpdateBottomHeadVisibility()
        {
            bool sameHeads = chkSameHeads.Checked;
            grpBottomHead.Visible = !sameHeads;
            if (sameHeads)
            {
                cmbBottomHead.SelectedIndex = cmbTopHead.SelectedIndex;
                nudConicalAngleBottom.Value = nudConicalAngleTop.Value;
            }
        }

        // ── Read UI → Specification ───────────────────────────────────────────
        private void ReadUIToSpec()
        {
            Specification.Orientation = rbVertical.Checked ? VesselOrientation.Vertical : VesselOrientation.Horizontal;

            Specification.TopHead.Type = (HeadType)cmbTopHead.SelectedIndex;
            Specification.TopHead.ConicalAngle = (double)nudConicalAngleTop.Value;

            Specification.SameHeadsBothEnds = chkSameHeads.Checked;
            if (chkSameHeads.Checked)
            {
                Specification.BottomHead.Type = Specification.TopHead.Type;
                Specification.BottomHead.ConicalAngle = Specification.TopHead.ConicalAngle;
            }
            else
            {
                Specification.BottomHead.Type = (HeadType)cmbBottomHead.SelectedIndex;
                Specification.BottomHead.ConicalAngle = (double)nudConicalAngleBottom.Value;
            }

            Specification.InnerDiameter = (double)nudDi.Value;
            Specification.ShellLength = (double)nudShellLength.Value;

            Specification.DesignPressure = (double)nudPressure.Value;
            Specification.DesignTemperature = (double)nudTemperature.Value;
            Specification.AllowableStress = (double)nudAllowableStress.Value;
            Specification.CorrosionAllowance = (double)nudC1.Value;
            Specification.MinusTolerance = (double)nudC2.Value;

            Specification.Material = (MaterialGroup)cmbMaterialGroup.SelectedIndex;
            Specification.MaterialGrade = cmbMaterialGrade.SelectedItem?.ToString() ?? "";
        }

        // ── AD-2000 calculation & result display ──────────────────────────────
        private void UpdateResults()
        {
            ReadUIToSpec();

            double Di  = Specification.InnerDiameter;
            double p   = Specification.DesignPressure;
            double f   = Specification.AllowableStress;
            double c1  = Specification.CorrosionAllowance;
            double c2  = Specification.MinusTolerance;

            double s_shell = AD2000Calculator.ShellThickness(Di, p, f);
            double s_top   = AD2000Calculator.HeadThickness(Specification.TopHead, Di, p, f);
            double s_bot   = AD2000Calculator.HeadThickness(Specification.BottomHead, Di, p, f);

            double sn_shell = AD2000Calculator.NominalThickness(s_shell, c1, c2);
            double sn_top   = AD2000Calculator.NominalThickness(s_top, c1, c2);
            double sn_bot   = AD2000Calculator.NominalThickness(s_bot, c1, c2);

            Specification.RequiredShellThickness = s_shell;
            Specification.NominalShellThickness  = sn_shell;

            string topLabel = Specification.Orientation == VesselOrientation.Vertical ? "Top Head" : "Left Head";
            string botLabel = Specification.Orientation == VesselOrientation.Vertical ? "Bottom Head" : "Right Head";

            lblResultShell.Text = $"Shell:      s_erf = {s_shell:F2} mm  →  s_nom = {sn_shell:F1} mm";
            lblResultTopHead.Text  = $"{topLabel}: s_erf = {s_top:F2} mm  →  s_nom = {sn_top:F1} mm";
            lblResultBotHead.Text  = $"{botLabel}: s_erf = {s_bot:F2} mm  →  s_nom = {sn_bot:F1} mm";

            // Summary box
            txtSummary.Text =
                $"=== AD-2000 Vessel Summary ==={Environment.NewLine}" +
                $"Orientation     : {Specification.Orientation}{Environment.NewLine}" +
                $"Inner Diameter  : {Di} mm{Environment.NewLine}" +
                $"Shell Length    : {Specification.ShellLength} mm{Environment.NewLine}" +
                $"Design Pressure : {p} bar{Environment.NewLine}" +
                $"Design Temp.    : {Specification.DesignTemperature} °C{Environment.NewLine}" +
                $"Material        : {Specification.MaterialGrade} (f = {f} N/mm²){Environment.NewLine}" +
                $"c1 (corrosion)  : {c1} mm,  c2 (tolerance): {c2} mm{Environment.NewLine}" +
                $"{Environment.NewLine}" +
                $"--- Required Thicknesses ---{Environment.NewLine}" +
                $"Shell   : s_erf = {s_shell:F2} mm  →  s_nom = {sn_shell:F1} mm{Environment.NewLine}" +
                $"{topLabel}: s_erf = {s_top:F2} mm  →  s_nom = {sn_top:F1} mm{Environment.NewLine}" +
                $"{botLabel}: s_erf = {s_bot:F2} mm  →  s_nom = {sn_bot:F1} mm{Environment.NewLine}";
        }

        // ── Event handlers ────────────────────────────────────────────────────
        private void rbVertical_CheckedChanged(object sender, EventArgs e)
        {
            lblTopHead.Text    = rbVertical.Checked ? "Top Head:" : "Left Head:";
            lblBottomHead.Text = rbVertical.Checked ? "Bottom Head:" : "Right Head:";
            UpdateResults();
        }

        private void chkSameHeads_CheckedChanged(object sender, EventArgs e)
        {
            UpdateBottomHeadVisibility();
            UpdateResults();
        }

        private void cmbTopHead_SelectedIndexChanged(object sender, EventArgs e)
        {
            var type = (HeadType)cmbTopHead.SelectedIndex;
            nudConicalAngleTop.Visible = (type == HeadType.Conical);
            lblConicalTop.Visible = (type == HeadType.Conical);
            if (chkSameHeads.Checked)
            {
                cmbBottomHead.SelectedIndex = cmbTopHead.SelectedIndex;
                nudConicalAngleBottom.Value = nudConicalAngleTop.Value;
            }
            UpdateResults();
        }

        private void cmbBottomHead_SelectedIndexChanged(object sender, EventArgs e)
        {
            var type = (HeadType)cmbBottomHead.SelectedIndex;
            nudConicalAngleBottom.Visible = (type == HeadType.Conical);
            lblConicalBottom.Visible = (type == HeadType.Conical);
            UpdateResults();
        }

        private void AnyValueChanged(object sender, EventArgs e) => UpdateResults();

        private void cmbMaterialGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            Specification.Material = (MaterialGroup)cmbMaterialGroup.SelectedIndex;
            PopulateMaterialGrades();
            UpdateResults();
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            UpdateResults();
            tabMain.SelectedTab = tabResults;
        }

        private void btnClose_Click(object sender, EventArgs e) => this.Close();
    }
}
