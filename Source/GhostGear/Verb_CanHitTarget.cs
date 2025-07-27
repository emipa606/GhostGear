using HarmonyLib;
using Verse;

namespace GhostGear;

[HarmonyPatch(typeof(Verb), nameof(Verb.CanHitTarget))]
public class Verb_CanHitTarget
{
    [HarmonyPostfix]
    public static void PostFix(ref Verb __instance, ref bool __result, LocalTargetInfo targ)
    {
        if (!__result || __instance.IsMeleeAttack || !__instance.verbProps.requireLineOfSight || !targ.HasThing ||
            targ.Thing is not Pawn pawn || !GhostGearUtility.IsTargetGhosted(pawn, __instance.caster))
        {
            return;
        }

        if (Controller.Settings.ShowConfusion)
        {
            GhostGearUtility.DoConfusedMote(__instance.caster, pawn);
        }

        __result = false;
    }
}