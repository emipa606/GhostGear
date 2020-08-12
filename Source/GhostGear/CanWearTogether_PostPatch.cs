using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GhostGear
{
	// Token: 0x02000007 RID: 7
	[HarmonyPatch(typeof(ApparelUtility), "CanWearTogether")]
	public class CanWearTogether_PostPatch
	{
		// Token: 0x0600000F RID: 15 RVA: 0x0000243C File Offset: 0x0000063C
		[HarmonyPostfix]
		public static void PostFix(ref bool __result, ThingDef A, ThingDef B, BodyDef body)
		{
			if (__result && A.statBases.StatListContains(StatDefOf.EnergyShieldEnergyMax) && B.statBases.StatListContains(StatDefOf.EnergyShieldEnergyMax))
			{
				__result = false;
			}
		}
	}
}
