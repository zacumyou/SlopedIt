using System.Collections.Generic;
using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;

namespace SlopedIt
{
    [FileLocation("SlopedIt")]
    public sealed class Setting : ModSetting
    {
        public Setting(IMod mod) : base(mod) { }
        [SettingsUISection("Main", "General")]
        public bool Enabled { get; set; } = true;
        [SettingsUISection("Main", "General")]
        public bool ForceDecalAlignment { get; set; } = true;
        [SettingsUISection("Main", "General")]
        public string Status => Mod.Status;
        public override void SetDefaults() { Enabled = true; ForceDecalAlignment = true; }
    }

    internal sealed class Locale : IDictionarySource
    {
        private readonly Setting m_Setting;
        private readonly bool m_Korean;
        public Locale(Setting setting, bool korean) { m_Setting = setting; m_Korean = korean; }
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                [m_Setting.GetSettingsLocaleID()] = Mod.DisplayName,
                [m_Setting.GetOptionTabLocaleID("Main")] = m_Korean ? "데칼" : "Decals",
                [m_Setting.GetOptionGroupLocaleID("General")] = m_Korean ? "지형 경사 정렬" : "Terrain slope alignment",
                [m_Setting.GetOptionLabelLocaleID(nameof(Setting.Enabled))] = m_Korean ? "자동 경사 정렬" : "Automatic slope alignment",
                [m_Setting.GetOptionDescLocaleID(nameof(Setting.Enabled))] = m_Korean ? "새로 배치하는 지형 데칼의 크기를 기준으로 경사를 계산하여 미리보기와 배치에 적용합니다. 변경 후 커서를 움직이거나 데칼을 다시 선택하세요. 기존 데칼은 변경하지 않습니다." : "Fits terrain across each new decal's footprint for preview and placement. Move the cursor or reselect the decal after changing this setting. Existing decals are unchanged.",
                [m_Setting.GetOptionLabelLocaleID(nameof(Setting.Status))] = m_Korean ? "모드 상태" : "Mod status",
                [m_Setting.GetOptionLabelLocaleID(nameof(Setting.ForceDecalAlignment))] = m_Korean ? "외부 데칼 강제 경사 정렬" : "Force slope alignment for imported decals",
                [m_Setting.GetOptionDescLocaleID(nameof(Setting.ForceDecalAlignment))] = m_Korean ? "지형 투영 레이어가 없는 데칼도 독립 배치 시 지형 경사에 맞춥니다. 벽면·부착형 데칼은 제외하며, 데칼의 표시 레이어는 바꾸지 않습니다. 변경 후 데칼을 다시 선택하세요." : "Aligns standalone decals even when their receiver mask excludes terrain. Wall and attached decals remain excluded. Rendering layers are unchanged. Reselect the decal after changing this option.",
                [m_Setting.GetOptionDescLocaleID(nameof(Setting.Status))] = m_Korean ? "준비됨은 모드 로드를 의미합니다. 실제 배치 처리 여부는 SlopedIt.log에서 확인할 수 있습니다." : "Ready means the mod loaded. SlopedIt.log records the first aligned preview."
            };
        }
        public void Unload() { }
    }
}
