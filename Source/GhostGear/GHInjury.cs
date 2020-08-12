using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GhostGear
{
	// Token: 0x0200000F RID: 15
	public class GHInjury
	{
		// Token: 0x0600002B RID: 43 RVA: 0x00002AAC File Offset: 0x00000CAC
		public static void DoGHRelatedInjury(Thing victim, bool headSpace)
		{
			if (victim is Pawn)
			{
				BodyPartRecord candidate;
				DamageDef DamDef;
				float dmg;
				GHInjury.SetUpInjVars(victim as Pawn, headSpace, out candidate, out DamDef, out dmg);
				if (candidate != null && Rand.Range(1f, 100f) <= Controller.Settings.GHInj)
				{
					float armPen = 0f;
					DamageInfo dinfo = new DamageInfo(DamDef, dmg, armPen, -1f, null, candidate, null, DamageInfo.SourceCategory.ThingOrUnknown, null);
					(victim as Pawn).TakeDamage(dinfo);
				}
			}
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002B1C File Offset: 0x00000D1C
		public static void SetUpInjVars(Pawn Victim, bool headspace, out BodyPartRecord candidate, out DamageDef DamDef, out float dmg)
		{
			DamDef = null;
			int Rnd = Rand.Range(1, 100);
			if (Rnd < 33)
			{
				DamDef = DamageDefOf.Cut;
			}
			else if (Rnd >= 33 && Rnd < 60)
			{
				DamDef = DamageDefOf.Blunt;
			}
			else if (Rnd >= 60 && Rnd < 80)
			{
				DamDef = DamageDefOf.Stab;
			}
			else
			{
				DamDef = DamageDefOf.Stun;
			}
			dmg = Rand.Range(2f, 7f);
			if (DamDef == DamageDefOf.Stun)
			{
				dmg *= 2f;
			}
			candidate = null;
			List<BodyPartRecord> potentials = new List<BodyPartRecord>();
			if (headspace)
			{
				BodyDef body = Victim.RaceProps.body;
				if (((body != null) ? body.GetPartsWithDef(BodyPartDefOf.Head) : null) != null)
				{
					potentials.AddRange(Victim.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Head));
				}
				BodyDef body2 = Victim.RaceProps.body;
				if (((body2 != null) ? body2.GetPartsWithDef(BodyPartDefOf.Arm) : null) != null)
				{
					potentials.AddRange(Victim.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Arm));
				}
				BodyDef body3 = Victim.RaceProps.body;
				if (((body3 != null) ? body3.GetPartsWithDef(BodyPartDefOf.Torso) : null) != null)
				{
					potentials.AddRange(Victim.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Torso));
				}
				BodyDef body4 = Victim.RaceProps.body;
				if (((body4 != null) ? body4.GetPartsWithDef(BodyPartDefOf.InsectHead) : null) != null)
				{
					potentials.AddRange(Victim.RaceProps.body.GetPartsWithDef(BodyPartDefOf.InsectHead));
				}
			}
			else
			{
				BodyDef body5 = Victim.RaceProps.body;
				if (((body5 != null) ? body5.GetPartsWithDef(BodyPartDefOf.Leg) : null) != null)
				{
					potentials.AddRange(Victim.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Leg));
				}
			}
			if (potentials.Count <= 0)
			{
				BodyDef body6 = Victim.RaceProps.body;
				if (((body6 != null) ? body6.GetPartsWithDef(BodyPartDefOf.Body) : null) != null)
				{
					potentials.AddRange(Victim.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Body));
				}
			}
			if (potentials.Count > 0)
			{
				candidate = potentials.RandomElement<BodyPartRecord>();
			}
		}
	}
}
