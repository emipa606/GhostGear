using System;
using HarmonyLib;
using Verse;

namespace GhostGear
{
	// Token: 0x02000005 RID: 5
	[HarmonyPatch(typeof(Verb), "CanHitTarget")]
	public class CanHitTarget_PostPatch
	{
		// Token: 0x0600000B RID: 11 RVA: 0x00002398 File Offset: 0x00000598
		[HarmonyPostfix]
		public static void PostFix(ref Verb __instance, ref bool __result, LocalTargetInfo targ)
		{
			if (__result && !__instance.IsMeleeAttack && __instance.verbProps.requireLineOfSight && targ.HasThing && targ.Thing is Pawn && GhostGearUtility.IsTargetGhosted(targ.Thing as Pawn, __instance.caster))
			{
				if (Controller.Settings.ShowConfusion)
				{
					GhostGearUtility.DoConfusedMote(__instance.caster, targ.Thing);
				}
				__result = false;
			}
		}
	}
}
