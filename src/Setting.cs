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
        // Keep the serialized 1.0.x key so existing decal preferences survive.
        [SettingsUISection("Main", "Decals")]
        public bool Enabled { get; set; } = true;
        [SettingsUISection("Main", "Decals")]
        public bool ForceDecalAlignment { get; set; } = true;
        [SettingsUISection("Main", "Props")]
        public bool PropsEnabled { get; set; } = true;
        [SettingsUISection("Main", "General"), SettingsUIButton]
        public bool EnableAll { set { Enabled = PropsEnabled = ForceDecalAlignment = true; ApplyAndSave(); } }
        [SettingsUISection("Main", "General"), SettingsUIButton]
        public bool DisableAll { set { Enabled = PropsEnabled = false; ApplyAndSave(); } }
        [SettingsUISection("Main", "General")]
        public string Status => Mod.Status;
        public override void SetDefaults() { Enabled = PropsEnabled = ForceDecalAlignment = true; }
    }

    internal sealed class Locale : IDictionarySource
    {
        private readonly Setting settings;
        private readonly bool ko;
        public Locale(Setting setting, bool korean) { settings = setting; ko = korean; }
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            var entries = new Dictionary<string, string>
            {
                [settings.GetSettingsLocaleID()] = Mod.DisplayName,
                [settings.GetOptionTabLocaleID("Main")] = ko ? "붙이기" : "Ground alignment",
                [settings.GetOptionGroupLocaleID("Decals")] = ko ? "데칼" : "Decals",
                [settings.GetOptionGroupLocaleID("Props")] = ko ? "프롭" : "Props",
                [settings.GetOptionGroupLocaleID("General")] = ko ? "전체 설정" : "General",
                ["SlopedIt.Title"] = ko ? "붙이기" : "Ground alignment",
                ["SlopedIt.DecalDescription"] = ko ? "데칼을 지형 경사에 맞춥니다. 이 설정은 모든 새 데칼에 적용됩니다." : "Align decals with the terrain slope. Applies to all new decals.",
                ["SlopedIt.PropDescription"] = ko ? "프롭을 지형 경사에 맞추고 바닥 높이를 보정합니다. 이 설정은 모든 새 프롭에 적용됩니다." : "Align props with the terrain slope and seat their base on the ground. Applies to all new props."
            };
            Add(entries, nameof(Setting.Enabled), "데칼 자동 붙이기", "Align decals", "크기에 맞는 지형 경사를 미리보기와 배치에 적용합니다.", "Fits the terrain across each decal's footprint for preview and placement.");
            Add(entries, nameof(Setting.ForceDecalAlignment), "외부 데칼 강제 정렬", "Force imported decal alignment", "RealVision 등 지형 투영 레이어가 없는 데칼도 정렬합니다. 벽면·부착형은 제외합니다.", "Includes decals such as RealVision without a terrain receiver mask. Excludes wall and attached decals.");
            Add(entries, nameof(Setting.PropsEnabled), "프롭 자동 붙이기", "Align props", "독립된 정적 프롭의 크기로 경사와 바닥 높이를 보정합니다. 건물·나무·부착물·복합 객체는 제외합니다. 울퉁불퉁한 지형에서는 틈이 생길 수 있습니다.", "Fits standalone static props using their bounds and adjusts base height. Excludes buildings, trees, attached and composite objects. Uneven terrain may leave gaps.");
            Add(entries, nameof(Setting.EnableAll), "다 켜기", "Enable all", "데칼·외부 데칼·프롭 정렬을 모두 켭니다.", "Enable decal, imported decal and prop alignment.");
            Add(entries, nameof(Setting.DisableAll), "다 끄기", "Disable all", "정렬을 모두 끕니다. 기존에 배치한 객체는 바꾸지 않습니다.", "Disable all alignment. Previously placed objects are unchanged.");
            Add(entries, nameof(Setting.Status), "모드 상태", "Mod status", "준비됨은 모드 로드를 의미합니다.", "Ready means the mod loaded.");
            return entries;
        }
        private void Add(Dictionary<string,string> entries, string property, string labelKo, string labelEn, string descKo, string descEn)
        {
            entries[settings.GetOptionLabelLocaleID(property)] = ko ? labelKo : labelEn;
            entries[settings.GetOptionDescLocaleID(property)] = ko ? descKo : descEn;
        }
        public void Unload() { }
    }
}

