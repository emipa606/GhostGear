using HarmonyLib;
using RimWorld;
using Verse;

namespace GhostGear
{
    // Token: 0x0200000E RID: 14
    [HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
    public class GetValueUnfinalized_GGPostPatch
    {
        // Token: 0x06000029 RID: 41 RVA: 0x000029F0 File Offset: 0x00000BF0
        [HarmonyPostfix]
        public static void PostFix(ref float __result, StatWorker __instance, StatDef ___stat, StatRequest req)
        {
            if (!req.HasThing)
            {
                return;
            }

            var thing = req.Thing;
            if (!(thing is Pawn pawn))
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
}