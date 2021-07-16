using HarmonyLib;
using RimWorld;
using Verse;

namespace PelShield
{
    // Token: 0x02000004 RID: 4
    [HarmonyPatch(typeof(ApparelUtility), "CanWearTogether")]
    public class CanWearTogether_PostPatch
    {
        // Token: 0x06000006 RID: 6 RVA: 0x0000211D File Offset: 0x0000031D
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
}