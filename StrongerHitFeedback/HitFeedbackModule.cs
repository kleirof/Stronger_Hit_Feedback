using BepInEx;
using HarmonyLib;

namespace StrongerHitFeedback
{
    [BepInDependency("etgmodding.etg.mtgapi")]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class HitFeedbackModule : BaseUnityPlugin
    {
        public const string GUID = "kleirof.etg.strongerhitfeedback";
        public const string NAME = "Stronger Hit Feedback";
        public const string VERSION = "1.1.2";
        public const string TEXT_COLOR = "#92A1E6";

		public static HitFeedbackModule instance;

		public void Start()
        {
			instance = this;
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
		}

        public void GMStart(GameManager g)
        {
            Log($"{NAME} v{VERSION} started successfully.", TEXT_COLOR);

			AudioResourceLoader.loadFromFolder(NAME);

            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        public static void Log(string text, string color = "FFFFFF")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }
	}
}
