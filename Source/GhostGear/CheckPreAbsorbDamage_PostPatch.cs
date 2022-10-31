using HarmonyLib;
using RimWorld;
using Verse;

namespace GhostGear;

[HarmonyPatch(typeof(Apparel), "CheckPreAbsorbDamage")]
public class CheckPreAbsorbDamage_PostPatch
{
    [HarmonyPostfix]
    public static void PostFix(ref Apparel __instance, ref bool __result, DamageInfo dinfo)
    {
        if (!__instance.def.HasComp(typeof(CompShield)))
        {
            return;
        }

        var shieldComp = __instance.GetComp<CompShield>();
        if (shieldComp.ShieldState != ShieldState.Active)
        {
            __result = false;
            return;
        }

        var haywire = DefDatabase<DamageDef>.GetNamed("GGHaywireEMP");
        if (dinfo.Def != haywire)
        {
            return;
        }

        NonPublicMethods.ShieldBelt_Break(shieldComp);
        __result = false;
    }
}