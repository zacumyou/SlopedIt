using System;
using Game;
using System.Reflection;
using Colossal.Serialization.Entities;
using Colossal.UI.Binding;
using Game.Tools;

namespace SlopedIt
{
    public partial class SlopeUI : Game.UI.UISystemBase
    {
        private ToolSystem tools;
        private SlopedDecalSystem alignment;
        private ValueBinding<bool> visible, enabled, decal;
        private int lastSettings = -1;
        // Reviewed against the exact Game.dll guarded in Mod.OnLoad.
        private static readonly FieldInfo forceUpdate = typeof(ToolBaseSystem).GetField("m_ForceUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
        public override GameMode gameMode => GameMode.Game | GameMode.Editor;
        protected override void OnCreate()
        {
            base.OnCreate();
            tools = World.GetOrCreateSystemManaged<ToolSystem>();
            alignment = World.GetOrCreateSystemManaged<SlopedDecalSystem>();
            AddBinding(visible = new ValueBinding<bool>("SlopedIt", "Visible", false));
            AddBinding(enabled = new ValueBinding<bool>("SlopedIt", "Enabled", false));
            AddBinding(decal = new ValueBinding<bool>("SlopedIt", "Decal", true));
            AddBinding(new TriggerBinding("SlopedIt", "Toggle", Toggle));
        }
        private bool Selected(out bool isDecal)
        {
            isDecal = false;
            var tool = tools.activeTool;
            return Mod.Active && tool != null && (tool is ObjectToolSystem || tool.toolID == "Line Tool") &&
                alignment.TrySelectedKind(tool.GetPrefab(), out isDecal);
        }
        private void Toggle()
        {
            if (Mod.Options == null || !Selected(out bool isDecal)) return;
            if (isDecal) Mod.Options.Enabled = !Mod.Options.Enabled;
            else Mod.Options.PropsEnabled = !Mod.Options.PropsEnabled;
            try { Mod.Options.ApplyAndSave(); }
            catch (Exception error) { Mod.Log.Error(error, "Could not save alignment settings."); }
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
            var settings = Mod.Options;
            bool show = Selected(out bool isDecal);
            visible.Update(show);
            decal.Update(isDecal);
            enabled.Update(settings != null && (isDecal ? settings.Enabled : settings.PropsEnabled));
            if (settings == null) return;
            int current = (settings.Enabled ? 1 : 0) | (settings.PropsEnabled ? 2 : 0) | (settings.ForceDecalAlignment ? 4 : 0);
            if (lastSettings != current && Mod.Active && tools.activeTool != null)
                forceUpdate?.SetValue(tools.activeTool, true);
            lastSettings = current;
        }
    }
}

