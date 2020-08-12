using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GhostGear
{
	// Token: 0x02000021 RID: 33
	[HarmonyPatch(typeof(StunHandler), "Notify_DamageApplied", new Type[]
	{
		typeof(DamageInfo),
		typeof(bool)
	})]
	public class Notify_DamageApplied_PostPatch
	{
		// Token: 0x060000A9 RID: 169 RVA: 0x00005E84 File Offset: 0x00004084
		[HarmonyPostfix]
		public static void PostFix(ref StunHandler __instance, ref int ___EMPAdaptedTicksLeft, DamageInfo dinfo, bool affectedByEMP)
		{
			Pawn pawn = __instance.parent as Pawn;
			if (pawn != null && (pawn.Downed || pawn.Dead))
			{
				return;
			}
			DamageDef Haywire = DefDatabase<DamageDef>.GetNamed("GGHaywireEMP", true);
			if (dinfo.Def == Haywire && Notify_DamageApplied_PostPatch.EMPAffects(pawn))
			{
				if (___EMPAdaptedTicksLeft <= 0 || HaywireUtility.Rnd100() < (int)Controller.Settings.HWChance)
				{
					__instance.StunFor(Mathf.RoundToInt(dinfo.Amount * 15f), dinfo.Instigator, true);
					if (pawn != null && pawn.RaceProps.IsMechanoid)
					{
						___EMPAdaptedTicksLeft = 2000;
					}
					HaywireUtility.DoHaywireEffect(pawn);
					return;
				}
				IntVec3 position = __instance.parent.Position;
				MoteMaker.ThrowText(new Vector3((float)position.x + 1f, (float)position.y, (float)position.z + 1f), __instance.parent.Map, "Adapted".Translate(), Color.white, -1f);
			}
		}

		// Token: 0x060000AA RID: 170 RVA: 0x00005F89 File Offset: 0x00004189
		internal static bool EMPAffects(Pawn pawn)
		{
			return !pawn.RaceProps.IsFlesh;
		}
	}
}
