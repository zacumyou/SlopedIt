using System;
using System.IO;
using System.Security.Cryptography;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Game.Tools;
using Unity.Entities;

namespace SlopedIt
{
    public sealed class Mod : IMod
    {
        public const string DisplayName = "Sloped It";
        internal static readonly ILog Log = LogManager.GetLogger("SlopedIt").SetShowsErrorsInUI(true);
        internal static bool Active;
        internal static string Status = "Not loaded / 로드 전";
        internal static Setting Options;
        private SlopedDecalSystem m_System;

        public void OnLoad(UpdateSystem updateSystem)
        {
            Options = new Setting(this);
            AssetDatabase.global.LoadSettings("SlopedIt", Options, new Setting(this));
            GameManager.instance.localizationManager.AddSource("en-US", new Locale(Options, false));
            GameManager.instance.localizationManager.AddSource("ko-KR", new Locale(Options, true));
            Options.RegisterInOptionsUI();
            try
            {
                string hash;
                using (var sha = SHA256.Create())
                using (var stream = File.OpenRead(typeof(ObjectToolSystem).Assembly.Location))
                    hash = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
                if (hash != "AAEE15C4FA41C130ABAA1183840667E4FAAE531D67E8A9FA7536618EFFA2F86A")
                    throw new InvalidOperationException("Game update requires compatibility review. SHA256=" + hash);
                m_System = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SlopedDecalSystem>();
                updateSystem.UpdateBefore<SlopedDecalSystem, GenerateObjectsSystem>(SystemUpdatePhase.Modification1);
                Active = true;
                Status = "Ready / 준비됨";
                Log.Info("Sloped It 1.0.1 loaded. 5x5 footprint plane fit, 3 passes; force decal alignment=" + Options.ForceDecalAlignment + "; before GenerateObjectsSystem. Game SHA256=" + hash);
            }
            catch (Exception error)
            {
                Active = false;
                Status = "Inactive / 비활성: " + error.Message;
                Log.Error(error, Status);
            }
        }

        public void OnDispose()
        {
            Active = false;
            m_System?.Stop();
            Options?.UnregisterInOptionsUI();
            Options = null;
        }
    }
}
