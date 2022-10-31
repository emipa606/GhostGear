using HarmonyLib;
using RimWorld;
using Verse;

namespace PelShield;

[HarmonyPatch(typeof(ApparelUtility), "CanWearTogether")]
public class CanWearTogether_PostPatch
{
    [HarmonyPostfix]
    public static void PostFix(ref bool __result, ThingDef A, ThingDef B, BodyDef body)
    {
        if (__result && A.statBases.StatListContains(StatDefOf.EnergyShieldEnergyMax) &&
            B.statBases.StatListContains(StatDefOf.EnergyShieldEnergyMax))
        {
            __result = false;
        }
    }
}