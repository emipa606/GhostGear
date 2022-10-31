using HarmonyLib;
using RimWorld;
using Verse;

namespace GhostGear;

[HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
public class GetValueUnfinalized_GGPostPatch
{
    [HarmonyPostfix]
    public static void PostFix(ref float __result, StatWorker __instance, StatDef ___stat, StatRequest req)
    {
        if (!req.HasThing)
        {
            return;
        }

        var thing = req.Thing;
        if (thing is not Pawn pawn)
        {
            return;
        }

        var confusedDef = DefDatabase<ThoughtDef>.GetNamed("GGConfused");
        bool isConfused;
        var needs = pawn.needs;
        if (needs == null)
        {
            isConfused = false;
        }
        else
        {
            var mood = needs.mood;
            if (mood == null)
            {
                isConfused = false;
            }
            else
            {
                var thoughts = mood.thoughts;
                isConfused = thoughts?.memories.GetFirstMemoryOfDef(confusedDef) != null;
            }
        }

        if (!isConfused)
        {
            return;
        }

        var offset = pawn.needs.mood.thoughts.memories.GetFirstMemoryOfDef(confusedDef).MoodOffset();
        if (___stat == StatDefOf.AimingDelayFactor)
        {
            __result += (0f - offset) / 10f;
        }
    }
}