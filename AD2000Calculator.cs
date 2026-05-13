using System;

namespace PVDesigner.Models
{
    /// <summary>
    /// AD-2000 Merkblatt B1 (Zylinderschalen) and B2 (Böden) thickness calculations.
    /// All dimensions in mm, pressure in bar.
    /// </summary>
    public static class AD2000Calculator
    {
        private const double BarToNmm2 = 0.1; // 1 bar = 0.1 N/mm²

        // AD-2000 B1 §3.1 – required wall thickness for cylindrical shell under internal pressure
        public static double ShellThickness(double Di, double p_bar, double f, double v = 1.0)
        {
            double p = p_bar * BarToNmm2;
            // s_erf = (p * Di) / (2 * f * v - p)
            return (p * Di) / (2.0 * f * v - p);
        }

        // AD-2000 B3 §3 – Klöpperboden (torispherical, DIN 28011: R=Di, r=0.1·Di)
        public static double KlopperbodenThickness(double Di, double p_bar, double f, double v = 1.0)
        {
            double p = p_bar * BarToNmm2;
            double R = Di;           // crown radius
            double r = 0.1 * Di;    // knuckle radius
            // β factor for Klöpperboden per AD-2000 B3 Table 1 (approx. for standard geometry)
            double beta = 1.35;
            return beta * p * Di / (2.0 * f * v);
        }

        // AD-2000 B3 §3 – Korbbogen (semi-ellipsoidal, DIN 28013: a/b=2 → equivalent sphere R=0.9·Di)
        public static double KorbbogenThickness(double Di, double p_bar, double f, double v = 1.0)
        {
            double p = p_bar * BarToNmm2;
            // Korbbogen with a/b=2: β ≈ 1.15
            double beta = 1.15;
            return beta * p * Di / (2.0 * f * v);
        }

        // AD-2000 B3 – Spherical head (R = Di/2)
        public static double SphericalHeadThickness(double Di, double p_bar, double f, double v = 1.0)
        {
            double p = p_bar * BarToNmm2;
            double R = Di / 2.0;
            return (p * R) / (2.0 * f * v - 0.5 * p);
        }

        // AD-2000 B2 §3 – Conical head
        public static double ConicalHeadThickness(double Di, double p_bar, double f, double halfApexDeg, double v = 1.0)
        {
            double p = p_bar * BarToNmm2;
            double alpha = halfApexDeg * Math.PI / 180.0;
            return (p * Di) / (2.0 * Math.Cos(alpha) * (f * v - 0.5 * p));
        }

        // AD-2000 B5 – Flat head (simplified, bolted flat cover β=0.41)
        public static double FlatHeadThickness(double Di, double p_bar, double f, double beta = 0.41)
        {
            double p = p_bar * BarToNmm2;
            return Di * Math.Sqrt(beta * p / f);
        }

        public static double HeadThickness(HeadSpecification head, double Di, double p_bar, double f, double v = 1.0)
        {
            switch (head.Type)
            {
                case HeadType.Klopperboden: return KlopperbodenThickness(Di, p_bar, f, v);
                case HeadType.Korbbogen:    return KorbbogenThickness(Di, p_bar, f, v);
                case HeadType.Spherical:    return SphericalHeadThickness(Di, p_bar, f, v);
                case HeadType.Conical:      return ConicalHeadThickness(Di, p_bar, f, head.ConicalAngle, v);
                case HeadType.Flat:         return FlatHeadThickness(Di, p_bar, f);
                default:                    return KlopperbodenThickness(Di, p_bar, f, v);
            }
        }

        /// <summary>Nominal thickness = s_erf + c1 + c2, rounded up to next 0.5 mm step.</summary>
        public static double NominalThickness(double s_erf, double c1, double c2)
        {
            double s_min = s_erf + c1 + c2;
            return Math.Ceiling(s_min * 2.0) / 2.0;  // round up to 0.5 mm
        }
    }
}
