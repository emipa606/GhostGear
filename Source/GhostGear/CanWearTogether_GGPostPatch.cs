using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GhostGear
{
	// Token: 0x02000006 RID: 6
	[HarmonyPatch(typeof(ApparelUtility), "CanWearTogether")]
	public class CanWearTogether_GGPostPatch
	{
		// Token: 0x0600000D RID: 13 RVA: 0x0000241B File Offset: 0x0000061B
		[HarmonyPostfix]
		public static void PostFix(ref bool __result, ThingDef A, ThingDef B, BodyDef body)
		{
			if (__result && GhostGearUtility.GetIsGGApparel(A) && GhostGearUtility.GetIsGGApparel(B))
			{
				__result = false;
			}
		}
	}
}
