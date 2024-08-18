using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;
using MonoMod.RuntimeDetour;
using System.Reflection;
using Dungeonator;
using Pathfinding;
using System.Threading;
using Random = System.Random;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace StrongerHitFeedback
{
    [BepInDependency("etgmodding.etg.mtgapi")]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class HitFeedbackModule : BaseUnityPlugin
    {
        public const string GUID = "kleirof.etg.strongerhitfeedback";
        public const string NAME = "Stronger Hit Feedback";
        public const string VERSION = "1.1.1";
        public const string TEXT_COLOR = "#92A1E6";

		public static HitFeedbackModule instance;

		public void Start()
        {
			instance = this;
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
		}

        public class HitFeedbackPatches
        {
            [HarmonyILManipulator, HarmonyPatch(typeof(Projectile), nameof(Projectile.HandleKnockback))]
            public static void HandleKnockbackPatch(ILContext ctx)
            {
                ILCursor crs = new ILCursor(ctx);

                if (crs.TryGotoNext(MoveType.After, x => x.MatchMul()))
                {
                    crs.EmitDelegate<Func<float, float>>(orig => {
                        return orig * 2f;
                    });
                }
            }

            [HarmonyILManipulator, HarmonyPatch(typeof(HealthHaver), nameof(HealthHaver.FlashOnHit), MethodType.Enumerator)]
            public static void FlashOnHitPatch(ILContext ctx)
            {
                ILCursor crs = new ILCursor(ctx);
                if (crs.TryGotoNext(MoveType.Before, x => x.MatchLdstr("Play_ENM_hurt")))
                {
                    FieldInfo fi = AccessTools.Field(Type.GetType("HealthHaver+<FlashOnHit>c__Iterator0, Assembly-CSharp"), "$this");

                    crs.Emit(OpCodes.Ldarg_0);
                    crs.Emit(OpCodes.Ldfld, fi);
                    crs.EmitDelegate<Action<HealthHaver>>(self => {
                        Random ran1 = new Random();
                        int n1 = ran1.Next(1, 7);
                        AkSoundEngine.PostEvent(string.Format("Play_hit_{0}", n1), self.gameObject);
                    });
                }
                crs.Index = 0;
                while (crs.TryGotoNext(MoveType.After, x => x.MatchLdcR4(0.04f)))
                {
                    crs.EmitDelegate<Func<float, float>>(orig => {
                        return orig * 9f;
                    });
                }
                crs.Index = 0;
                if (crs.TryGotoNext(MoveType.After, x => x.MatchLdcR4(0.2f)))
                {
                    crs.EmitDelegate<Func<float, float>>(orig => {
                        return 0f;
                    });
                }
            }

            [HarmonyILManipulator, HarmonyPatch(typeof(HealthHaver), nameof(HealthHaver.ApplyDamageDirectional))]
            public static void ApplyDamageDirectionalPatch(ILContext ctx)
            {
                ILCursor crs = new ILCursor(ctx);

                if (crs.TryGotoNext(MoveType.Before, x => x.MatchLdstr("Play_ENM_death")))
                {
                    crs.Emit(OpCodes.Ldarg_0);
                    crs.EmitDelegate<Action<HealthHaver>>(self => {
                        Random ran = new Random();
                        int n = ran.Next(1, 5);
                        AkSoundEngine.PostEvent(string.Format("Play_killed_{0}", n), self.gameObject);
                    });
                }
            }
        }

        public void GMStart(GameManager g)
        {
            Log($"{NAME} v{VERSION} started successfully.", TEXT_COLOR);

			AudioResourceLoader.loadFromFolder(NAME);

            Harmony.CreateAndPatchAll(typeof(HitFeedbackPatches));
        }

        public static void Log(string text, string color = "FFFFFF")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }
	}
}
