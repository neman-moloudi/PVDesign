using System;
using Inventor;
using PVDesigner.Models;

namespace PVDesigner.Modeling
{
    /// <summary>
    /// Builds a parametric 3D solid model of a pressure vessel in Autodesk Inventor.
    /// All components are named after the user-supplied vessel tag (e.g. "V-101"):
    ///   Shell   → "V-101_Shell"
    ///   TopHead → "V-101_TopHead"   (or "V-101_LeftHead" for horizontal vessels)
    ///   BotHead → "V-101_BottomHead" (or "V-101_RightHead")
    /// </summary>
    public class VesselModelBuilder
    {
        private readonly Application _app;

        private const double Mm = 0.1;   // Inventor internal unit: 1 mm = 0.1 cm

        public VesselModelBuilder(Application app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        // ── Public entry point ────────────────────────────────────────────────
        public void Build(VesselSpecification spec)
        {
            string name     = SanitizeName(spec.VesselName);
            bool   vertical = spec.Orientation == VesselOrientation.Vertical;

            double Di  = spec.InnerDiameter;
            double s   = spec.NominalShellThickness > 0 ? spec.NominalShellThickness
                         : AD2000Calculator.NominalThickness(
                               AD2000Calculator.ShellThickness(Di, spec.DesignPressure, spec.AllowableStress),
                               spec.CorrosionAllowance, spec.MinusTolerance);

            double Do  = Di + 2.0 * s;
            double L   = spec.ShellLength;

            double s_top = AD2000Calculator.NominalThickness(
                AD2000Calculator.HeadThickness(spec.TopHead, Di, spec.DesignPressure, spec.AllowableStress),
                spec.CorrosionAllowance, spec.MinusTolerance);

            double s_bot = AD2000Calculator.NominalThickness(
                AD2000Calculator.HeadThickness(spec.BottomHead, Di, spec.DesignPressure, spec.AllowableStress),
                spec.CorrosionAllowance, spec.MinusTolerance);

            string topPartName = vertical ? $"{name}_TopHead"    : $"{name}_LeftHead";
            string botPartName = vertical ? $"{name}_BottomHead" : $"{name}_RightHead";

            // Create assembly
            PartDocument shellDoc  = CreateShell(name, Di, Do, L, vertical);
            PartDocument topDoc    = CreateHead(topPartName, spec.TopHead, Di, s_top, vertical, isTop: true);
            PartDocument botDoc    = CreateHead(botPartName, spec.BottomHead, Di, s_bot, vertical, isTop: false);

            AssembleVessel(name, shellDoc, topDoc, botDoc, Di, s, L, spec, vertical);
        }

        // ── Shell ─────────────────────────────────────────────────────────────
        private PartDocument CreateShell(string vesselName, double Di, double Do, double L, bool vertical)
        {
            PartDocument doc = (PartDocument)_app.Documents.Add(
                DocumentTypeEnum.kPartDocumentObject,
                _app.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject));

            doc.DisplayName = $"{vesselName}_Shell";

            PartComponentDefinition def = doc.ComponentDefinition;
            TransientGeometry       tg  = _app.TransientGeometry;

            // Sketch on XZ-plane (horizontal axis) or XY-plane (vertical)
            PlanarSketch sketch = def.Sketches.Add(vertical
                ? def.WorkPlanes[2]  // XY plane
                : def.WorkPlanes[3]  // XZ plane
            );

            // Draw a rectangle that revolves to form the cylindrical shell wall
            double ri = (Di / 2.0) * Mm;
            double ro = (Do / 2.0) * Mm;
            double hl = (L  / 2.0) * Mm;

            SketchPoint p1 = sketch.SketchPoints.Add(tg.CreatePoint2d(ri,  hl));
            SketchPoint p2 = sketch.SketchPoints.Add(tg.CreatePoint2d(ro,  hl));
            SketchPoint p3 = sketch.SketchPoints.Add(tg.CreatePoint2d(ro, -hl));
            SketchPoint p4 = sketch.SketchPoints.Add(tg.CreatePoint2d(ri, -hl));

            sketch.SketchLines.AddByTwoPoints(p1, p2);
            sketch.SketchLines.AddByTwoPoints(p2, p3);
            sketch.SketchLines.AddByTwoPoints(p3, p4);
            sketch.SketchLines.AddByTwoPoints(p4, p1);

            Profile profile = sketch.Profiles.AddForSolid();

            // Revolution axis = Y-axis of the sketch (vertical axis)
            WorkAxis axis = vertical ? def.WorkAxes[2] : def.WorkAxes[3]; // Y or Z axis

            RevolveFeature revolve = def.Features.RevolveFeatures.AddFull(profile, axis,
                PartFeatureOperationEnum.kNewBodyOperation);
            revolve.Name = $"{vesselName}_Shell_Revolve";

            doc.Save2(false);
            return doc;
        }

        // ── Head ──────────────────────────────────────────────────────────────
        private PartDocument CreateHead(string partName, HeadSpecification head,
            double Di, double s, bool vertical, bool isTop)
        {
            PartDocument doc = (PartDocument)_app.Documents.Add(
                DocumentTypeEnum.kPartDocumentObject,
                _app.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject));

            doc.DisplayName = partName;

            PartComponentDefinition def = doc.ComponentDefinition;
            TransientGeometry       tg  = _app.TransientGeometry;

            WorkPlane sketchPlane = vertical ? def.WorkPlanes[2] : def.WorkPlanes[3];
            PlanarSketch sketch   = def.Sketches.Add(sketchPlane);
            WorkAxis     axis     = vertical ? def.WorkAxes[2] : def.WorkAxes[3];

            switch (head.Type)
            {
                case HeadType.Klopperboden:
                    BuildKlopperProfile(sketch, tg, Di, s);
                    break;
                case HeadType.Korbbogen:
                    BuildKorbbogenProfile(sketch, tg, Di, s);
                    break;
                case HeadType.Spherical:
                    BuildSphericalProfile(sketch, tg, Di, s);
                    break;
                case HeadType.Conical:
                    BuildConicalProfile(sketch, tg, Di, s, head.ConicalAngle);
                    break;
                case HeadType.Flat:
                    BuildFlatProfile(sketch, tg, Di, s);
                    break;
                default:
                    BuildKlopperProfile(sketch, tg, Di, s);
                    break;
            }

            Profile profile = sketch.Profiles.AddForSolid();

            RevolveFeature revolve = def.Features.RevolveFeatures.AddFull(profile, axis,
                PartFeatureOperationEnum.kNewBodyOperation);
            revolve.Name = $"{partName}_Revolve";

            // Flip orientation for bottom/right head
            if (!isTop)
            {
                WorkPlane midPlane = def.WorkPlanes[3]; // XZ
                MirrorFeature mirror = def.Features.MirrorFeatures.Add(
                    revolve as ObjectCollection ?? SingletonCollection(revolve, doc), midPlane);
                mirror.Name = $"{partName}_Mirror";
            }

            doc.Save2(false);
            return doc;
        }

        // ── Head profile helpers ─────────────────────────────────────────────

        // Klöpperboden (DIN 28011): torispherical  R=Di, r=0.1·Di
        private void BuildKlopperProfile(PlanarSketch sketch, TransientGeometry tg, double Di, double s)
        {
            double ri   = (Di / 2.0) * Mm;
            double ro   = ((Di / 2.0) + s) * Mm;
            double R_i  = Di * Mm;           // crown radius inner
            double r_k  = 0.1 * Di * Mm;    // knuckle radius inner
            // Height of Klöpperboden ≈ 0.255·Di (DIN 28011)
            double h    = 0.255 * Di * Mm;

            // Simplified revolution profile: outer arc → straight cylinder stub → inner arc
            // Crown centre is above the rim
            double arcCx = 0;
            double arcCy = -(R_i - h);

            SketchArc outerCrown = sketch.SketchArcs.AddByCenterStartEndPoint(
                tg.CreatePoint2d(arcCx, arcCy + s * Mm),
                tg.CreatePoint2d(0, h + s * Mm),
                tg.CreatePoint2d(ro, 0),
                true);

            SketchArc innerCrown = sketch.SketchArcs.AddByCenterStartEndPoint(
                tg.CreatePoint2d(arcCx, arcCy),
                tg.CreatePoint2d(ri, 0),
                tg.CreatePoint2d(0, h),
                false);

            // Close the profile with straight lines on axis and rim
            sketch.SketchLines.AddByTwoPoints(
                tg.CreatePoint2d(0, h),
                tg.CreatePoint2d(0, h + s * Mm));
            sketch.SketchLines.AddByTwoPoints(
                tg.CreatePoint2d(ri, 0),
                tg.CreatePoint2d(ro, 0));
        }

        // Korbbogen (DIN 28013): semi-ellipsoidal a/b=2, height = Di/4
        private void BuildKorbbogenProfile(PlanarSketch sketch, TransientGeometry tg, double Di, double s)
        {
            double ri = (Di / 2.0) * Mm;
            double ro = ((Di / 2.0) + s) * Mm;
            double h  = (Di / 4.0) * Mm;   // inner height of ellipsoidal head

            // Use ellipse arcs (quarter ellipse revolved = semi-ellipsoid)
            SketchEllipticalArc innerArc = sketch.SketchEllipticalArcs.Add(
                tg.CreatePoint2d(0, 0),
                tg.CreatePoint2d(ri, 0),
                tg.CreatePoint2d(0, h),
                0.0, Math.PI / 2.0);

            SketchEllipticalArc outerArc = sketch.SketchEllipticalArcs.Add(
                tg.CreatePoint2d(0, 0),
                tg.CreatePoint2d(ro, 0),
                tg.CreatePoint2d(0, h + s * Mm),
                Math.PI / 2.0, 0.0);

            sketch.SketchLines.AddByTwoPoints(
                tg.CreatePoint2d(0, h),
                tg.CreatePoint2d(0, h + s * Mm));
            sketch.SketchLines.AddByTwoPoints(
                tg.CreatePoint2d(ri, 0),
                tg.CreatePoint2d(ro, 0));
        }

        // Spherical head: hemisphere R = Di/2
        private void BuildSphericalProfile(PlanarSketch sketch, TransientGeometry tg, double Di, double s)
        {
            double ri = (Di / 2.0) * Mm;
            double ro = ((Di / 2.0) + s) * Mm;

            sketch.SketchArcs.AddByCenterStartEndPoint(
                tg.CreatePoint2d(0, 0),
                tg.CreatePoint2d(ri, 0),
                tg.CreatePoint2d(0, ri),
                false);

            sketch.SketchArcs.AddByCenterStartEndPoint(
                tg.CreatePoint2d(0, 0),
                tg.CreatePoint2d(0, ro),
                tg.CreatePoint2d(ro, 0),
                false);

            sketch.SketchLines.AddByTwoPoints(
                tg.CreatePoint2d(0, ri),
                tg.CreatePoint2d(0, ro));
            sketch.SketchLines.AddByTwoPoints(
                tg.CreatePoint2d(ri, 0),
                tg.CreatePoint2d(ro, 0));
        }

        // Conical head
        private void BuildConicalProfile(PlanarSketch sketch, TransientGeometry tg,
            double Di, double s, double halfApexDeg)
        {
            double ri   = (Di / 2.0) * Mm;
            double ro   = ((Di / 2.0) + s) * Mm;
            double alpha = halfApexDeg * Math.PI / 180.0;
            double h    = ri / Math.Tan(alpha);            // inner cone height

            // Outer cone is slightly taller due to wall thickness
            double ho   = ro / Math.Tan(alpha);

            // Profile: triangle with wall
            sketch.SketchLines.AddByTwoPoints(tg.CreatePoint2d(0, h),   tg.CreatePoint2d(ri, 0));
            sketch.SketchLines.AddByTwoPoints(tg.CreatePoint2d(ri, 0),  tg.CreatePoint2d(ro, 0));
            sketch.SketchLines.AddByTwoPoints(tg.CreatePoint2d(ro, 0),  tg.CreatePoint2d(0, ho));
            sketch.SketchLines.AddByTwoPoints(tg.CreatePoint2d(0, ho),  tg.CreatePoint2d(0, h));
        }

        // Flat head: simple disc
        private void BuildFlatProfile(PlanarSketch sketch, TransientGeometry tg, double Di, double s)
        {
            double ri = (Di / 2.0) * Mm;
            double ro = ((Di / 2.0) + s) * Mm;  // outer radius same as shell OD

            sketch.SketchLines.AddByTwoPoints(tg.CreatePoint2d(0, 0),  tg.CreatePoint2d(ri, 0));
            sketch.SketchLines.AddByTwoPoints(tg.CreatePoint2d(ri, 0), tg.CreatePoint2d(ro, 0));
            sketch.SketchLines.AddByTwoPoints(tg.CreatePoint2d(ro, 0), tg.CreatePoint2d(ro, s * Mm));
            sketch.SketchLines.AddByTwoPoints(tg.CreatePoint2d(ro, s * Mm), tg.CreatePoint2d(0, s * Mm));
            sketch.SketchLines.AddByTwoPoints(tg.CreatePoint2d(0, s * Mm), tg.CreatePoint2d(0, 0));
        }

        // ── Assembly ──────────────────────────────────────────────────────────
        private void AssembleVessel(string name,
            PartDocument shellDoc, PartDocument topDoc, PartDocument botDoc,
            double Di, double s, double L,
            VesselSpecification spec, bool vertical)
        {
            AssemblyDocument asmDoc = (AssemblyDocument)_app.Documents.Add(
                DocumentTypeEnum.kAssemblyDocumentObject,
                _app.FileManager.GetTemplateFile(DocumentTypeEnum.kAssemblyDocumentObject));

            asmDoc.DisplayName = name;

            AssemblyComponentDefinition asmDef = asmDoc.ComponentDefinition;
            TransientGeometry tg = _app.TransientGeometry;

            Matrix identity = tg.CreateMatrix();

            // Place shell at origin
            ComponentOccurrence shellOcc = asmDef.Occurrences.Add(shellDoc.FullFileName, identity);
            shellOcc.Name = $"{name}_Shell:1";

            // Top head: offset by L/2 + head height along the vessel axis
            double topHeadH   = HeadHeight(spec.TopHead, Di) * Mm;
            double botHeadH   = HeadHeight(spec.BottomHead, Di) * Mm;
            double halfL      = (L / 2.0) * Mm;

            Matrix topMat = tg.CreateMatrix();
            if (vertical)
                topMat.SetTranslation(tg.CreateVector(0, halfL, 0));
            else
                topMat.SetTranslation(tg.CreateVector(halfL, 0, 0));

            ComponentOccurrence topOcc = asmDef.Occurrences.Add(topDoc.FullFileName, topMat);
            topOcc.Name = vertical ? $"{name}_TopHead:1" : $"{name}_LeftHead:1";

            Matrix botMat = tg.CreateMatrix();
            if (vertical)
            {
                botMat.SetTranslation(tg.CreateVector(0, -halfL, 0));
                // Rotate 180° around X to flip the bottom head
                botMat.SetToRotation(Math.PI, tg.CreateUnitVector(1, 0, 0), tg.CreatePoint(0, 0, 0));
                botMat.SetTranslation(tg.CreateVector(0, -halfL, 0));
            }
            else
            {
                botMat.SetToRotation(Math.PI, tg.CreateUnitVector(0, 1, 0), tg.CreatePoint(0, 0, 0));
                botMat.SetTranslation(tg.CreateVector(-halfL, 0, 0));
            }

            ComponentOccurrence botOcc = asmDef.Occurrences.Add(botDoc.FullFileName, botMat);
            botOcc.Name = vertical ? $"{name}_BottomHead:1" : $"{name}_RightHead:1";

            asmDoc.Save2(false);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static double HeadHeight(HeadSpecification head, double Di)
        {
            switch (head.Type)
            {
                case HeadType.Klopperboden: return 0.255 * Di;
                case HeadType.Korbbogen:    return Di / 4.0;
                case HeadType.Spherical:    return Di / 2.0;
                case HeadType.Conical:
                    double alpha = head.ConicalAngle * Math.PI / 180.0;
                    return (Di / 2.0) / Math.Tan(alpha);
                case HeadType.Flat:         return 0;
                default:                    return 0.255 * Di;
            }
        }

        private static string SanitizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "V-101";
            // Replace characters invalid in Inventor part names
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                name = name.Replace(c.ToString(), "_");
            return name.Trim();
        }

        private static ObjectCollection SingletonCollection(object item, PartDocument doc)
        {
            ObjectCollection col = doc.ComponentDefinition.Features
                .RevolveFeatures.Application.TransientObjects.CreateObjectCollection();
            col.Add(item);
            return col;
        }
    }
}
