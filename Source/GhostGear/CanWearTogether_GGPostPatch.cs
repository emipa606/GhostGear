using HarmonyLib;
using RimWorld;
using Verse;

namespace GhostGear;

[HarmonyPatch(typeof(ApparelUtility), nameof(ApparelUtility.CanWearTogether))]
public class CanWearTogether_GGPostPatch
{
    [HarmonyPostfix]
    public static void PostFix(ref bool __result, ThingDef A, ThingDef B)
    {
        if (__result && GhostGearUtility.GetIsGGApparel(A) && GhostGearUtility.GetIsGGApparel(B))
        {
            __result = false;
        }
    }
}