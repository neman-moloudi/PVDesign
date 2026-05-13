using System.Collections.Generic;

namespace PVDesigner.Models
{
    public enum VesselOrientation { Vertical, Horizontal }

    public enum HeadType
    {
        Klopperboden,   // DIN 28011 – torispherical (r=0.1·Di, R=Di)
        Korbbogen,      // DIN 28013 – semi-ellipsoidal (a/b = 2)
        Conical,        // Konischer Boden
        Spherical,      // Halbkugelförmiger Boden
        Flat            // Flachboden
    }

    public enum MaterialGroup
    {
        W_I,    // Stähle (z.B. P265GH)
        W_II,   // Austenitische Stähle
        NE      // Nichteisenmetalle
    }

    public class HeadSpecification
    {
        public HeadType Type { get; set; } = HeadType.Klopperboden;
        /// <summary>Half apex angle for conical heads (degrees)</summary>
        public double ConicalAngle { get; set; } = 30.0;
    }

    public class VesselSpecification
    {
        // --- Orientation & Geometry ---
        public VesselOrientation Orientation { get; set; } = VesselOrientation.Vertical;

        /// <summary>Inner diameter Di [mm]</summary>
        public double InnerDiameter { get; set; } = 1000.0;

        /// <summary>Cylindrical shell length L [mm]</summary>
        public double ShellLength { get; set; } = 2000.0;

        // --- Heads ---
        public HeadSpecification TopHead { get; set; } = new HeadSpecification();
        public HeadSpecification BottomHead { get; set; } = new HeadSpecification();
        public bool SameHeadsBothEnds { get; set; } = true;

        // --- Design Loads (AD-2000 B0) ---
        /// <summary>Calculation pressure p [bar]</summary>
        public double DesignPressure { get; set; } = 10.0;

        /// <summary>Calculation temperature T [°C]</summary>
        public double DesignTemperature { get; set; } = 200.0;

        // --- Material ---
        public MaterialGroup Material { get; set; } = MaterialGroup.W_I;
        public string MaterialGrade { get; set; } = "P265GH";

        /// <summary>Nominal design stress f [N/mm²] at design temperature</summary>
        public double AllowableStress { get; set; } = 170.0;

        // --- Wall Thickness (calculated) ---
        /// <summary>Required shell wall thickness s_erf [mm]</summary>
        public double RequiredShellThickness { get; set; }

        /// <summary>Chosen nominal shell wall thickness s [mm]</summary>
        public double NominalShellThickness { get; set; }

        /// <summary>Corrosion allowance c1 [mm]</summary>
        public double CorrosionAllowance { get; set; } = 1.0;

        /// <summary>Minus tolerance c2 [mm]</summary>
        public double MinusTolerance { get; set; } = 0.8;
    }
}
