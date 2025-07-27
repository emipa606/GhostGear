using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GhostGear;

[HarmonyPatch(typeof(StunHandler), nameof(StunHandler.Notify_DamageApplied), typeof(DamageInfo))]
public class StunHandler_Notify_DamageApplied
{
    [HarmonyPostfix]
    public static void PostFix(ref StunHandler __instance, ref int ___stunTicksLeft, DamageInfo dinfo)
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

        if (___stunTicksLeft <= 0 || HaywireUtility.Rnd100() < (int)Controller.Settings.HWChance)
        {
            __instance.StunFor(Mathf.RoundToInt(dinfo.Amount * 15f), dinfo.Instigator);
            if (pawn != null && pawn.RaceProps.IsMechanoid)
            {
                ___stunTicksLeft = 2000;
            }

            HaywireUtility.DoHaywireEffect(pawn);
            return;
        }

        var position = __instance.parent.Position;
        MoteMaker.ThrowText(new Vector3(position.x + 1f, position.y, position.z + 1f), __instance.parent.Map,
            "Adapted".Translate(), Color.white);
    }

    private static bool EMPAffects(Pawn pawn)
    {
        return !pawn.RaceProps.IsFlesh;
    }
}