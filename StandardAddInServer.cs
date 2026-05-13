using System;
using System.Runtime.InteropServices;
using Inventor;
using PVDesigner.Forms;

namespace PVDesigner
{
    /// <summary>
    /// Entry point for the PV Designer Inventor add-in.
    /// CLSID must match the ClassId in PVDesigner.addin.
    /// </summary>
    [GuidAttribute("B4F8A3C2-1E5D-4A6F-9B2C-7D3E8F4A1B9C")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class StandardAddInServer : ApplicationAddInServer
    {
        private Application _inventorApp;
        private ButtonDefinition _buttonDef;

        // ApplicationAddInServer
        public void Activate(ApplicationAddInSite addInSiteObject, bool firstTime)
        {
            _inventorApp = addInSiteObject.Application;

            CreateButton(firstTime);
        }

        public void Deactivate()
        {
            _buttonDef?.Delete();
            _buttonDef = null;
            _inventorApp = null;
        }

        public void ExecuteCommand(int commandID) { }

        public object Automation => null;

        // ── Ribbon setup ─────────────────────────────────────────────────────
        private void CreateButton(bool firstTime)
        {
            var ctrlDefs = _inventorApp.CommandManager.ControlDefinitions;

            const string internalName = "PVDesigner.OpenDesigner";
            const string clientId = "B4F8A3C2-1E5D-4A6F-9B2C-7D3E8F4A1B9C";

            // Avoid re-creating if already exists (e.g. add-in reload)
            try
            {
                _buttonDef = (ButtonDefinition)ctrlDefs[internalName];
            }
            catch
            {
                _buttonDef = ctrlDefs.AddButtonDefinition(
                    DisplayName:  "PV Designer",
                    InternalName: internalName,
                    Classification: CommandTypesEnum.kNonShapeEditCmdType,
                    ClientId:     clientId,
                    DescriptionText: "Open the AD-2000 Pressure Vessel Designer",
                    ToolTipText:  "Design pressure vessels per AD-2000 Regelwerk");
            }

            _buttonDef.OnExecute += OnButtonClicked;

            if (firstTime)
                AddToRibbon();
        }

        private void AddToRibbon()
        {
            // Add button to the "Tools" tab, "Add-Ins" panel (present in all document types and zero-document state)
            var uiMgr = _inventorApp.UserInterfaceManager;

            // We add it to the "ZeroDoc" ribbon (shown when no document is open)
            // and also to the Part/Assembly/Drawing environment ribbons.
            foreach (Ribbon ribbon in uiMgr.Ribbons)
            {
                try
                {
                    RibbonTab toolsTab = null;
                    foreach (RibbonTab tab in ribbon.RibbonTabs)
                    {
                        if (tab.InternalName == "id_TabTools" || tab.InternalName == "id_ZeroDocTabTools")
                        {
                            toolsTab = tab;
                            break;
                        }
                    }

                    if (toolsTab == null) continue;

                    // Find or create our panel
                    RibbonPanel panel = null;
                    foreach (RibbonPanel p in toolsTab.RibbonPanels)
                    {
                        if (p.InternalName == "PVDesigner.Panel")
                        {
                            panel = p;
                            break;
                        }
                    }

                    if (panel == null)
                        panel = toolsTab.RibbonPanels.Add("PV Designer", "PVDesigner.Panel", "{B4F8A3C2-1E5D-4A6F-9B2C-7D3E8F4A1B9C}");

                    if (panel.CommandControls.Count == 0)
                        panel.CommandControls.AddButton(_buttonDef, true);
                }
                catch { /* skip ribbons that don't support modification */ }
            }
        }

        private void OnButtonClicked(NameValueMap context)
        {
            using (var form = new VesselDesignerForm(_inventorApp))
            {
                form.ShowDialog();
            }
        }
    }
}
