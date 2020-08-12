using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GhostGear
{
	// Token: 0x02000009 RID: 9
	[HarmonyPatch(typeof(ShieldBelt), "CheckPreAbsorbDamage")]
	public class CheckPreAbsorbDamage_PostPatch
	{
		// Token: 0x06000012 RID: 18 RVA: 0x000024A4 File Offset: 0x000006A4
		[HarmonyPostfix]
		public static void PostFix(ref ShieldBelt __instance, ref bool __result, ref float ___energy, DamageInfo dinfo)
		{
			if (__instance.ShieldState != ShieldState.Active)
			{
				__result = false;
				return;
			}
			DamageDef haywire = DefDatabase<DamageDef>.GetNamed("GGHaywireEMP", true);
			if (dinfo.Def == haywire)
			{
				___energy = 0f;
				NonPublicMethods.ShieldBelt_Break(__instance);
				__result = false;
			}
		}
	}
}
