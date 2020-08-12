using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PelShield
{
	// Token: 0x02000005 RID: 5
	[StaticConstructorOnStartup]
	public class PelShieldApparel : Apparel
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000008 RID: 8 RVA: 0x00002152 File Offset: 0x00000352
		public float EnergyMax
		{
			get
			{
				return this.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true);
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000009 RID: 9 RVA: 0x00002160 File Offset: 0x00000360
		public float EnergyGainPerTick
		{
			get
			{
				return this.GetStatValue(StatDefOf.EnergyShieldRechargeRate, true) / 60f;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600000A RID: 10 RVA: 0x00002174 File Offset: 0x00000374
		public float Energy
		{
			get
			{
				return this.energy;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600000B RID: 11 RVA: 0x0000217C File Offset: 0x0000037C
		public ShieldState ShieldState
		{
			get
			{
				if (this.ticksToReset > 0)
				{
					return ShieldState.Resetting;
				}
				return ShieldState.Active;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600000C RID: 12 RVA: 0x0000218C File Offset: 0x0000038C
		private bool ShouldDisplay
		{
			get
			{
				Pawn wearer = base.Wearer;
				return wearer.Spawned && !wearer.Dead && !wearer.Downed && (wearer.InAggroMentalState || wearer.Drafted || (wearer.Faction.HostileTo(Faction.OfPlayer) && !wearer.IsPrisoner) || Find.TickManager.TicksGame < this.lastKeepDisplayTick + this.KeepDisplayingTicks);
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002208 File Offset: 0x00000408
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.energy, "energy", 0f, false);
			Scribe_Values.Look<int>(ref this.ticksToReset, "ticksToReset", -1, false);
			Scribe_Values.Look<int>(ref this.lastKeepDisplayTick, "lastKeepDisplayTick", 0, false);
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002255 File Offset: 0x00000455
		public override IEnumerable<Gizmo> GetWornGizmos()
		{
			if (Find.Selector.SingleSelectedThing == base.Wearer)
			{
				yield return new Gizmo_EnergyPelShieldStatus
				{
					shield = this
				};
			}
			yield break;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002265 File Offset: 0x00000465
		public override float GetSpecialApparelScoreOffset()
		{
			return this.EnergyMax * this.ApparelScorePerEnergyMax;
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002274 File Offset: 0x00000474
		public override void Tick()
		{
			base.Tick();
			Pawn wearer = base.Wearer;
			if (wearer == null)
			{
				this.energy = 0f;
			}
			else if (this.ShieldState == ShieldState.Resetting)
			{
				this.ticksToReset--;
				if (this.ticksToReset <= 0)
				{
					this.Reset();
				}
			}
			else if (this.ShieldState == ShieldState.Active)
			{
				this.energy += this.EnergyGainPerTick;
				if (this.energy > this.EnergyMax)
				{
					this.energy = this.EnergyMax;
				}
			}
			if (wearer != null && wearer.IsHashIntervalTick(2500))
			{
				float regenEnergy = 0.1f;
				if (this.energy > regenEnergy && !wearer.Drafted && this.isAutoRepair(this))
				{
					this.DoAutoRepair(this, regenEnergy);
				}
			}
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002334 File Offset: 0x00000534
		public void DoAutoRepair(Apparel apparel, float regenEnergy)
		{
			if ((apparel as PelShieldApparel).energy > regenEnergy)
			{
				int regenAmount = 5;
				if (apparel.def.useHitPoints)
				{
					int maxhp = apparel.MaxHitPoints;
					int hp = apparel.HitPoints;
					if (hp < maxhp)
					{
						if (hp + regenAmount <= maxhp)
						{
							hp += regenAmount;
						}
						(apparel as PelShieldApparel).energy -= regenEnergy;
					}
				}
			}
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002394 File Offset: 0x00000594
		public bool isAutoRepair(Apparel apparel)
		{
			if (apparel != null && apparel.def.useHitPoints)
			{
				ApparelProperties apparel2 = apparel.def.apparel;
				if (apparel2 != null && apparel2.tags.Count > 0)
				{
					ApparelProperties apparel3 = apparel.def.apparel;
					List<string> tags = (apparel3 != null) ? apparel3.tags : null;
					if (tags.Count > 0)
					{
						using (List<string>.Enumerator enumerator = tags.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								if (enumerator.Current == "PelRegenFromShield")
								{
									return true;
								}
							}
						}
						return false;
					}
				}
			}
			return false;
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002444 File Offset: 0x00000644
		public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
		{
			if (this.ShieldState != ShieldState.Active)
			{
				return false;
			}
			if (dinfo.Def == DamageDefOf.EMP)
			{
				this.energy = 0f;
				this.Break();
				return false;
			}
			DamageDef haywire = DefDatabase<DamageDef>.GetNamed("GGHaywireEMP", false);
			if (haywire != null && dinfo.Def == haywire)
			{
				this.energy = 0f;
				this.Break();
				return false;
			}
			if (dinfo.Def.isRanged || dinfo.Def.isExplosive)
			{
				this.energy -= dinfo.Amount * this.EnergyLossPerDamage;
				if (this.energy < 0f)
				{
					this.Break();
				}
				else
				{
					this.AbsorbedDamage(dinfo);
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000014 RID: 20 RVA: 0x000024FE File Offset: 0x000006FE
		public void KeepDisplaying()
		{
			this.lastKeepDisplayTick = Find.TickManager.TicksGame;
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002510 File Offset: 0x00000710
		public void AbsorbedDamage(DamageInfo dinfo)
		{
			Pawn wearer = base.Wearer;
			SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(wearer.Position, wearer.Map, false));
			this.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
			Vector3 loc = wearer.TrueCenter() + this.impactAngleVect.RotatedBy(180f) * 0.5f;
			float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
			MoteMaker.MakeStaticMote(loc, wearer.Map, ThingDefOf.Mote_ExplosionFlash, num);
			int num2 = (int)num;
			for (int i = 0; i < num2; i++)
			{
				MoteMaker.ThrowDustPuff(loc, wearer.Map, Rand.Range(0.8f, 1.2f));
			}
			this.lastAbsorbDamageTick = Find.TickManager.TicksGame;
			this.KeepDisplaying();
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000025F4 File Offset: 0x000007F4
		public void Break()
		{
			Pawn wearer = base.Wearer;
			SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(wearer.Position, wearer.Map, false));
			MoteMaker.MakeStaticMote(wearer.TrueCenter(), wearer.Map, ThingDefOf.Mote_ExplosionFlash, 12f);
			for (int i = 0; i < 6; i++)
			{
				MoteMaker.ThrowDustPuff(wearer.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), wearer.Map, Rand.Range(0.8f, 1.2f));
			}
			this.energy = 0f;
			this.ticksToReset = this.StartingTicksToReset;
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000026B4 File Offset: 0x000008B4
		public void Reset()
		{
			Pawn wearer = base.Wearer;
			if (wearer.Spawned)
			{
				SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(wearer.Position, wearer.Map, false));
				MoteMaker.ThrowLightningGlow(wearer.TrueCenter(), wearer.Map, 3f);
			}
			this.ticksToReset = -1;
			this.energy = this.EnergyOnReset;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x0000271C File Offset: 0x0000091C
		public override void DrawWornExtras()
		{
			if (this.ShieldState == ShieldState.Active && this.ShouldDisplay)
			{
				Pawn wearer = base.Wearer;
				float num = Mathf.Lerp(1.2f, 1.55f, this.energy);
				Vector3 vector = wearer.Drawer.DrawPos;
				vector.y = AltitudeLayer.Blueprint.AltitudeFor();
				int num2 = Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
				if (num2 < 8)
				{
					float num3 = (float)(8 - num2) / 8f * 0.05f;
					vector += this.impactAngleVect * num3;
					num -= num3;
				}
				float angle = (float)Rand.Range(0, 360);
				Vector3 s = new Vector3(num, 1f, num);
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, PelShieldApparel.BubbleMat, 0);
			}
		}

		// Token: 0x06000019 RID: 25 RVA: 0x000027FF File Offset: 0x000009FF
		public override bool AllowVerbCast(IntVec3 root, Map map, LocalTargetInfo targ, Verb v)
		{
			return true;
		}

		// Token: 0x04000004 RID: 4
		public float energy;

		// Token: 0x04000005 RID: 5
		public int ticksToReset = -1;

		// Token: 0x04000006 RID: 6
		public int lastKeepDisplayTick = -9999;

		// Token: 0x04000007 RID: 7
		public Vector3 impactAngleVect;

		// Token: 0x04000008 RID: 8
		public int lastAbsorbDamageTick = -9999;

		// Token: 0x04000009 RID: 9
		public const float MinDrawSize = 1.2f;

		// Token: 0x0400000A RID: 10
		public const float MaxDrawSize = 1.55f;

		// Token: 0x0400000B RID: 11
		public const float MaxDamagedJitterDist = 0.05f;

		// Token: 0x0400000C RID: 12
		public const int JitterDurationTicks = 8;

		// Token: 0x0400000D RID: 13
		public int StartingTicksToReset = 2500;

		// Token: 0x0400000E RID: 14
		public float EnergyOnReset = 0.2f;

		// Token: 0x0400000F RID: 15
		public float EnergyLossPerDamage = 0.03f;

		// Token: 0x04000010 RID: 16
		public int KeepDisplayingTicks = 1000;

		// Token: 0x04000011 RID: 17
		public float ApparelScorePerEnergyMax = 0.25f;

		// Token: 0x04000012 RID: 18
		public static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);
	}
}
