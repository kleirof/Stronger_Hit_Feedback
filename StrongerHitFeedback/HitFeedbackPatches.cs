using System;
using System.Reflection;
using Random = System.Random;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Linq;

namespace StrongerHitFeedback
{
    public static class HitFeedbackPatches
    {
        public static void EmitCall<T>(this ILCursor iLCursor, string methodName, Type[] parameters = null, Type[] generics = null)
        {
            MethodInfo methodInfo = AccessTools.Method(typeof(T), methodName, parameters, generics);
            iLCursor.Emit(OpCodes.Call, methodInfo);
        }

        public static T GetFieldInEnumerator<T>(object instance, string fieldNamePattern)
        {
            return (T)instance.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(f => f.Name.Contains("$" + fieldNamePattern) || f.Name.Contains("<" + fieldNamePattern + ">"))
                .GetValue(instance);
        }

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.HandleKnockback))]
        public class HandleKnockbackPatchClass
        {
            [HarmonyILManipulator]
            public static void HandleKnockbackPatch(ILContext ctx)
            {
                ILCursor crs = new ILCursor(ctx);

                if (crs.TryGotoNext(MoveType.After, x => x.MatchMul()))
                {
                    crs.EmitCall<HandleKnockbackPatchClass>(nameof(HandleKnockbackPatchClass.HandleKnockbackPatchCall));
                }
            }

            private static float HandleKnockbackPatchCall(float orig)
            {
                return orig * 2f;
            }
        }

        [HarmonyPatch(typeof(HealthHaver), nameof(HealthHaver.FlashOnHit), MethodType.Enumerator)]
        public class FlashOnHitPatchClass
        {
            [HarmonyILManipulator]
            public static void FlashOnHitPatch(ILContext ctx)
            {
                ILCursor crs = new ILCursor(ctx);
                if (crs.TryGotoNext(MoveType.Before, x => x.MatchLdstr("Play_ENM_hurt")))
                {
                    FieldInfo fi = AccessTools.Field(Type.GetType("HealthHaver+<FlashOnHit>c__Iterator0, Assembly-CSharp"), "$this");

                    crs.Emit(OpCodes.Ldarg_0);
                    crs.EmitCall<FlashOnHitPatchClass>(nameof(FlashOnHitPatchClass.FlashOnHitPatchCall_1));
                }
                crs.Index = 0;

                for (int i = 0; i < 2; ++i)
                {
                    if (crs.TryGotoNext(MoveType.After, x => x.MatchLdcR4(0.04f)))
                    {
                        crs.EmitCall<FlashOnHitPatchClass>(nameof(FlashOnHitPatchClass.FlashOnHitPatchCall_2));
                    }
                }
                crs.Index = 0;

                if (crs.TryGotoNext(MoveType.After, x => x.MatchLdcR4(0.2f)))
                {
                    crs.EmitCall<FlashOnHitPatchClass>(nameof(FlashOnHitPatchClass.FlashOnHitPatchCall_3));
                }
            }

            private static void FlashOnHitPatchCall_1(object selfObject)
            {
                HealthHaver self = GetFieldInEnumerator<HealthHaver>(selfObject, "this");
                Random ran1 = new Random();
                int n1 = ran1.Next(1, 7);
                AkSoundEngine.PostEvent(string.Format("Play_hit_{0}", n1), self.gameObject);
            }

            private static float FlashOnHitPatchCall_2(float orig)
            {
                return orig * 9f;
            }

            private static float FlashOnHitPatchCall_3(float orig)
            {
                return 0f;
            }
        }

        [HarmonyPatch(typeof(HealthHaver), nameof(HealthHaver.ApplyDamageDirectional))]
        public class ApplyDamageDirectionalPatchClass
        {
            [HarmonyILManipulator]
            public static void ApplyDamageDirectionalPatch(ILContext ctx)
            {
                ILCursor crs = new ILCursor(ctx);

                if (crs.TryGotoNext(MoveType.Before, x => x.MatchLdstr("Play_ENM_death")))
                {
                    crs.Emit(OpCodes.Ldarg_0);
                    crs.EmitCall<ApplyDamageDirectionalPatchClass>(nameof(ApplyDamageDirectionalPatchClass.ApplyDamageDirectionalPatchCall));
                }
            }

            private static void ApplyDamageDirectionalPatchCall(HealthHaver self)
            {
                Random ran = new Random();
                int n = ran.Next(1, 5);
                AkSoundEngine.PostEvent(string.Format("Play_killed_{0}", n), self.gameObject);
            }
        }
    }
}
