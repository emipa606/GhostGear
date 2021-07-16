using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GhostGear
{
    // Token: 0x02000021 RID: 33
    [HarmonyPatch(typeof(StunHandler), "Notify_DamageApplied", typeof(DamageInfo))]
    public class Notify_DamageApplied_PostPatch
    {
        // Token: 0x060000A9 RID: 169 RVA: 0x00005E84 File Offset: 0x00004084
        [HarmonyPostfix]
        public static void PostFix(ref StunHandler __instance, ref int ___EMPAdaptedTicksLeft, DamageInfo dinfo)
        {
            var pawn = __instance.parent as Pawn;
            if (pawn != null && (pawn.Downed || pawn.Dead))
            {
                return;
            }

            var Haywire = DefDatabase<DamageDef>.GetNamed("GGHaywireEMP");
            if (dinfo.Def != Haywire || !EMPAffects(pawn))
            {
                return;
            }

            if (___EMPAdaptedTicksLeft <= 0 || HaywireUtility.Rnd100() < (int) Controller.Settings.HWChance)
            {
                __instance.StunFor(Mathf.RoundToInt(dinfo.Amount * 15f), dinfo.Instigator);
                if (pawn != null && pawn.RaceProps.IsMechanoid)
                {
                    ___EMPAdaptedTicksLeft = 2000;
                }

                HaywireUtility.DoHaywireEffect(pawn);
                return;
            }

            var position = __instance.parent.Position;
            MoteMaker.ThrowText(new Vector3(position.x + 1f, position.y, position.z + 1f), __instance.parent.Map,
                "Adapted".Translate(), Color.white);
        }

        // Token: 0x060000AA RID: 170 RVA: 0x00005F89 File Offset: 0x00004189
        internal static bool EMPAffects(Pawn pawn)
        {
            return !pawn.RaceProps.IsFlesh;
        }
    }
}