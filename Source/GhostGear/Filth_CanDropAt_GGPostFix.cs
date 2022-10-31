using HarmonyLib;
using RimWorld;
using Verse;

namespace GhostGear;

[HarmonyPatch(typeof(Filth), "CanDropAt")]
public class Filth_CanDropAt_GGPostFix
{
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

    public static bool IsGGFilth(Filth filth)
    {
        var defName = filth.def.defName;
        return defName == "Filth_GGCaltrops";
    }
}