using HarmonyLib;
using RimWorld;
using Verse;

namespace GhostGear
{
    // Token: 0x0200000D RID: 13
    [HarmonyPatch(typeof(Filth), "CanDropAt")]
    public class Filth_CanDropAt_GGPostFix
    {
        // Token: 0x06000026 RID: 38 RVA: 0x0000297C File Offset: 0x00000B7C
        [HarmonyPostfix]
        public static void Postfix(ref Filth __instance, ref bool __result, ref IntVec3 c, ref Map map)
        {
            if (__result)
            {
                return;
            }

            BuildableDef buildableDef = map.terrainGrid.TerrainAt(c);
            var chkfilth = __instance;
            if (buildableDef.fertility > 0f && IsGGFilth(chkfilth))
            {
                __result = true;
            }
        }

        // Token: 0x06000027 RID: 39 RVA: 0x000029BC File Offset: 0x00000BBC
        public static bool IsGGFilth(Filth filth)
        {
            var defName = filth.def.defName;
            return defName == "Filth_GGCaltrops";
        }
    }
}