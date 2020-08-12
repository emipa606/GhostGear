using System;
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
			Thing thing = req.Thing;
			if (!(thing is Pawn))
			{
				return;
			}
			ThoughtDef confusedDef = DefDatabase<ThoughtDef>.GetNamed("GGConfused", true);
			Pawn pawn = thing as Pawn;
			bool flag;
			if (pawn == null)
			{
				flag = (null != null);
			}
			else
			{
				Pawn_NeedsTracker needs = pawn.needs;
				if (needs == null)
				{
					flag = (null != null);
				}
				else
				{
					Need_Mood mood = needs.mood;
					if (mood == null)
					{
						flag = (null != null);
					}
					else
					{
						ThoughtHandler thoughts = mood.thoughts;
						flag = (((thoughts != null) ? thoughts.memories.GetFirstMemoryOfDef(confusedDef) : null) != null);
					}
				}
			}
			if (flag)
			{
				float offset = (thing as Pawn).needs.mood.thoughts.memories.GetFirstMemoryOfDef(confusedDef).MoodOffset();
				if (___stat == StatDefOf.AimingDelayFactor)
				{
					__result += (0f - offset) / 10f;
				}
			}
		}
	}
}
