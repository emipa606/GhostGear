using HarmonyLib;
using RimWorld;
using Verse;

namespace GhostGear;

[HarmonyPatch(typeof(ApparelUtility), nameof(ApparelUtility.CanWearTogether))]
public class ApparelUtility_CanWearTogether
{
    public static void Postfix(ref bool __result, ThingDef A, ThingDef B)
    {
        if (__result && A.statBases.StatListContains(StatDefOf.EnergyShieldEnergyMax) &&
            B.statBases.StatListContains(StatDefOf.EnergyShieldEnergyMax))
        {
            __result = false;
        }

        if (__result && GhostGearUtility.GetIsGGApparel(A) && GhostGearUtility.GetIsGGApparel(B))
        {
            __result = false;
        }
    }
}