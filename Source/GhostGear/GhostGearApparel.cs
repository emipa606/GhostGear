using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace GhostGear
{
	// Token: 0x02000010 RID: 16
	[StaticConstructorOnStartup]
	public class GhostGearApparel : Apparel
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600002E RID: 46 RVA: 0x00002D20 File Offset: 0x00000F20
		public float EnergyMax
		{
			get
			{
				return this.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true);
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600002F RID: 47 RVA: 0x00002D2E File Offset: 0x00000F2E
		public float EnergyGainPerTick
		{
			get
			{
				return this.GetStatValue(StatDefOf.EnergyShieldRechargeRate, true) / 60f;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000030 RID: 48 RVA: 0x00002D42 File Offset: 0x00000F42
		public float Energy
		{
			get
			{
				return this.energy;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000031 RID: 49 RVA: 0x00002D4A File Offset: 0x00000F4A
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

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000032 RID: 50 RVA: 0x00002D58 File Offset: 0x00000F58
		private bool ShouldDisplay
		{
			get
			{
				Pawn wearer = base.Wearer;
				return wearer.Spawned && !wearer.Dead && !wearer.Downed && (wearer.InAggroMentalState || wearer.Drafted || (wearer.Faction.HostileTo(Faction.OfPlayer) && !wearer.IsPrisoner) || Find.TickManager.TicksGame < this.lastKeepDisplayTick + this.KeepDisplayingTicks);
			}
		}

		// Token: 0x06000033 RID: 51 RVA: 0x00002DD4 File Offset: 0x00000FD4
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.energy, "energy", 0f, false);
			Scribe_Values.Look<int>(ref this.ticksToReset, "ticksToReset", -1, false);
			Scribe_Values.Look<int>(ref this.lastKeepDisplayTick, "lastKeepDisplayTick", 0, false);
			Scribe_Values.Look<int>(ref this.CaltropsUses, "CaltropsUses", 0, false);
			Scribe_Values.Look<int>(ref this.CaltropsMax, "CaltropsMax", 1, false);
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00002E45 File Offset: 0x00001045
		public override IEnumerable<Gizmo> GetWornGizmos()
		{
			if (ModLister.HasActiveModWithName("RimPlas") && (Find.Selector.SingleSelectedThing == base.Wearer || Find.Selector.SingleSelectedThing == this))
			{
				if (Find.Selector.SingleSelectedThing == base.Wearer)
				{
					Pawn wearer = base.Wearer;
					if (((wearer != null) ? wearer.Map : null) != null)
					{
						if (base.Wearer.Drafted)
						{
							yield return new Command_GrappleHook
							{
								defaultLabel = "GhostGear.GrappleHook".Translate(),
								defaultDesc = "GhostGear.GrappleHookDesc".Translate(this.def.label.CapitalizeFirst()),
								icon = ContentFinder<Texture2D>.Get(this.GrappleHookIconPath, true),
								user = base.Wearer,
								action = delegate(IntVec3 cell)
								{
									SoundDefOf.Click.PlayOneShotOnCamera(null);
									GhostGearApparel.UseGrappleHook(base.Wearer, this, cell);
								}
							};
						}
						yield return new Command_Action
						{
							defaultLabel = "GhostGear.RepulseLabel".Translate(this.def.label.CapitalizeFirst()),
							defaultDesc = "GhostGear.RepulseDesc".Translate(this.def.label.CapitalizeFirst()),
							icon = ContentFinder<Texture2D>.Get(this.RepulseIconPath, true),
							action = delegate()
							{
								SoundDefOf.Click.PlayOneShotOnCamera(null);
								GhostGearApparel.DoRepulse(base.Wearer, this);
							}
						};
						if (ResearchProjectDef.Named("RimPlas_GGCaltrops").IsFinished)
						{
							yield return new Command_Action
							{
								defaultLabel = "GhostGear.CaltropsLabel".Translate(this.def.label.CapitalizeFirst(), this.CaltropsUses.ToString()),
								defaultDesc = "GhostGear.CaltropsDesc".Translate(),
								icon = ContentFinder<Texture2D>.Get(this.CaltropsIconPath, true),
								action = delegate()
								{
									SoundDefOf.Click.PlayOneShotOnCamera(null);
									GhostGearApparel.DoCaltrops(base.Wearer, this);
								}
							};
						}
					}
				}
				yield return new Gizmo_EnergyGGShieldStatus
				{
					shield = this
				};
				yield break;
			}
			yield break;
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00002E58 File Offset: 0x00001058
		public override void Tick()
		{
			base.Tick();
			if (base.Wearer == null)
			{
				this.energy = 0f;
				return;
			}
			if (this.ShieldState == ShieldState.Resetting)
			{
				this.ticksToReset--;
				if (this.ticksToReset <= 0)
				{
					this.Reset();
					return;
				}
			}
			else if (this.ShieldState == ShieldState.Active)
			{
				if (!this.ActiveCamo)
				{
					this.energy += this.EnergyGainPerTick;
				}
				else
				{
					this.energy -= this.EnergyGainPerTick / 15f;
				}
				if (this.energy > this.EnergyMax)
				{
					this.energy = this.EnergyMax;
					return;
				}
				if (this.energy <= 0f)
				{
					this.Break();
				}
			}
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00002F14 File Offset: 0x00001114
		public static void DoCaltrops(Pawn pawn, ThingWithComps GGArmour)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			string text = "GhostGear.DoNothing".Translate();
			list.Add(new FloatMenuOption(text, delegate()
			{
				GhostGearApparel.GGCaltropsUse(pawn, GGArmour, false, false);
			}, MenuOptionPriority.Default, null, null, 29f, null, null));
			Pawn pawn2 = pawn;
			if (((pawn2 != null) ? pawn2.Map : null) != null && pawn.Spawned && !pawn.Dead && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && (GGArmour as GhostGearApparel).CaltropsUses > 0)
			{
				text = "GhostGear.DoCaltrops".Translate();
				list.Add(new FloatMenuOption(text, delegate()
				{
					GhostGearApparel.GGCaltropsUse(pawn, GGArmour, true, false);
				}, MenuOptionPriority.Default, null, null, 29f, null, null));
			}
			Pawn pawn3 = pawn;
			if (((pawn3 != null) ? pawn3.Map : null) != null && pawn.Spawned && !pawn.Dead && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving) && (GGArmour as GhostGearApparel).CaltropsUses < (GGArmour as GhostGearApparel).CaltropsMax)
			{
				text = "GhostGear.CaltropsRearm".Translate();
				list.Add(new FloatMenuOption(text, delegate()
				{
					GhostGearApparel.GGCaltropsUse(pawn, GGArmour, false, true);
				}, MenuOptionPriority.Default, null, null, 29f, null, null));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		// Token: 0x06000037 RID: 55 RVA: 0x000030D0 File Offset: 0x000012D0
		public static void GGCaltropsUse(Pawn pawn, ThingWithComps GGArmour, bool usingCaltrops, bool rearming)
		{
			string Reason;
			if (!rearming)
			{
				if (usingCaltrops)
				{
					DamageDef dmdef = DefDatabase<DamageDef>.GetNamed("Damage_GGCaltrops", true);
					ThingDef postTD = DefDatabase<ThingDef>.GetNamed("Filth_GGCaltrops", true);
					GenExplosion.DoExplosion(pawn.Position, pawn.Map, 4.9f, dmdef, pawn, 0, -1f, null, null, null, null, postTD, 1f, 1, false, null, 0f, 0, 0f, true, null, null);
					(GGArmour as GhostGearApparel).CaltropsUses--;
					if ((GGArmour as GhostGearApparel).CaltropsUses < 0)
					{
						(GGArmour as GhostGearApparel).CaltropsUses = 0;
						return;
					}
				}
			}
			else if (GhostGearUtility.GGComposMentis(pawn, GGArmour, out Reason))
			{
				if ((GGArmour as GhostGearApparel).CaltropsUses >= (GGArmour as GhostGearApparel).CaltropsMax)
				{
					Messages.Message("GhostGear.IsFullyRearmed".Translate(GGArmour.Label.CapitalizeFirst()), pawn, MessageTypeDefOf.NeutralEvent, false);
					SoundDefOf.ClickReject.PlayOneShotOnCamera(null);
					return;
				}
				JobDef GGRearm = DefDatabase<JobDef>.GetNamed("GGRearmCaltrops", true);
				Thing targ;
				GhostGearApparel.FindBestGGRearm(pawn, GGArmour, out targ);
				if (targ != null)
				{
					Job job = new Job(GGRearm, targ);
					pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
					return;
				}
				Messages.Message("GhostGear.NoCaltropsFound".Translate(GGArmour.Label.CapitalizeFirst()), pawn, MessageTypeDefOf.NeutralEvent, false);
				SoundDefOf.ClickReject.PlayOneShotOnCamera(null);
				return;
			}
			else
			{
				Messages.Message("GhostGear.CantDo".Translate(pawn, Reason, GGArmour.Label.CapitalizeFirst()), pawn, MessageTypeDefOf.NeutralEvent, false);
				SoundDefOf.ClickReject.PlayOneShotOnCamera(null);
			}
		}

		// Token: 0x06000038 RID: 56 RVA: 0x00003294 File Offset: 0x00001494
		internal static void FindBestGGRearm(Pawn pilot, ThingWithComps GGArmour, out Thing targ)
		{
			targ = null;
			if (((pilot != null) ? pilot.Map : null) != null)
			{
				ThingDef CaltropsPod = DefDatabase<ThingDef>.GetNamed("GGCaltropsPod", true);
				List<Thing> listpods = (pilot != null) ? pilot.Map.listerThings.ThingsOfDef(CaltropsPod) : null;
				int fuelneeded = (GGArmour as GhostGearApparel).CaltropsMax - (GGArmour as GhostGearApparel).CaltropsUses;
				if (fuelneeded > CaltropsPod.stackLimit)
				{
					fuelneeded = CaltropsPod.stackLimit;
				}
				if (listpods.Count > 0)
				{
					Thing besttarg = null;
					float bestpoints = 0f;
					for (int i = 0; i < listpods.Count; i++)
					{
						Thing targchk = listpods[i];
						if (!targchk.IsForbidden(pilot) && (((targchk != null) ? targchk.Faction : null) == null || targchk.Faction.IsPlayer) && pilot.CanReserveAndReach(targchk, PathEndMode.ClosestTouch, Danger.None, 1, -1, null, false))
						{
							float targpoints;
							if (targchk.stackCount >= fuelneeded)
							{
								targpoints = (float)targchk.stackCount / pilot.Position.DistanceTo(targchk.Position);
							}
							else
							{
								targpoints = (float)targchk.stackCount / (pilot.Position.DistanceTo(targchk.Position) * 2f);
							}
							if (targpoints > bestpoints)
							{
								besttarg = targchk;
								bestpoints = targpoints;
							}
						}
					}
					if (besttarg != null)
					{
						targ = besttarg;
					}
				}
			}
		}

		// Token: 0x06000039 RID: 57 RVA: 0x000033E4 File Offset: 0x000015E4
		public static void DoRepulse(Pawn pawn, ThingWithComps GGArmour)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			string text = "GhostGear.DoNothing".Translate();
			list.Add(new FloatMenuOption(text, delegate()
			{
				GhostGearApparel.GGRepulse(pawn, GGArmour, 0f);
			}, MenuOptionPriority.Default, null, null, 29f, null, null));
			Pawn pawn2 = pawn;
			if (((pawn2 != null) ? pawn2.Map : null) != null && pawn.Spawned && !pawn.Dead && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && (GGArmour as GhostGearApparel).energy > 0f)
			{
				text = "GhostGear.DoRepulse".Translate();
				list.Add(new FloatMenuOption(text, delegate()
				{
					GhostGearApparel.GGRepulse(pawn, GGArmour, (GGArmour as GhostGearApparel).energy);
				}, MenuOptionPriority.Default, null, null, 29f, null, null));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		// Token: 0x0600003A RID: 58 RVA: 0x000034E0 File Offset: 0x000016E0
		public static void GGRepulse(Pawn p, ThingWithComps a, float e)
		{
			if (e > 0f)
			{
				(a as GhostGearApparel).Break();
				for (int i = 0; i < 3; i++)
				{
					ThingDef postTD = ThingDefOf.Mote_Smoke;
					float radius = 1.9f;
					DamageDef dmdef = null;
					int dmg = GhostGearApparel.RepulseDmg(50, a, e);
					bool repulsing = false;
					switch (i)
					{
					case 0:
						dmdef = DamageDefOf.EMP;
						repulsing = false;
						radius = 2.9f;
						break;
					case 1:
						dmdef = DamageDefOf.Stun;
						repulsing = true;
						break;
					case 2:
						dmdef = DamageDefOf.Smoke;
						repulsing = false;
						radius = 2.9f;
						dmg = -1;
						break;
					}
					if (!repulsing)
					{
						GenExplosion.DoExplosion(p.Position, p.Map, radius, dmdef, p, dmg, -1f, null, null, null, null, postTD, 1f, 1, false, null, 0f, 0, 0f, true, null, null);
					}
					else
					{
						GhostGearApparel.RepulseEffect(p, dmdef, dmg);
					}
				}
				HaywireEffect.DoHWMiniEffect(p);
			}
		}

		// Token: 0x0600003B RID: 59 RVA: 0x000035C8 File Offset: 0x000017C8
		public static void RepulseEffect(Pawn p, DamageDef def, int dmg)
		{
			if (p != null && def != null && ((p != null) ? p.Map : null) != null)
			{
				List<IntVec3> cellList = GenAdj.CellsAdjacent8Way(p).ToList<IntVec3>();
				if (cellList.Count > 0)
				{
					foreach (IntVec3 c in cellList)
					{
						List<Thing> cellThings = c.GetThingList(p.Map);
						if (cellThings.Count > 0)
						{
							foreach (Thing thing in cellThings)
							{
								if (thing is Pawn)
								{
									DamageInfo dinfo = default(DamageInfo);
									dinfo.Def = def;
									dinfo.SetAmount((float)dmg);
									(thing as Pawn).TakeDamage(dinfo);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0600003C RID: 60 RVA: 0x000036C4 File Offset: 0x000018C4
		public static int RepulseDmg(int baseDmg, ThingWithComps a, float e)
		{
			int dmg = 0;
			if (baseDmg > 0)
			{
				float eFactor = 0f;
				if ((a as GhostGearApparel).EnergyMax > 0f)
				{
					eFactor = e / (a as GhostGearApparel).EnergyMax;
				}
				dmg = (int)Mathf.Lerp(0f, (float)baseDmg, eFactor);
			}
			return dmg;
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00003710 File Offset: 0x00001910
		public static void UseGrappleHook(Pawn Wearer, ThingWithComps thing, IntVec3 cell)
		{
			Job GrappleJob = new Job(DefDatabase<JobDef>.GetNamed("GGGrappleHook", true), cell, thing);
			if (GrappleJob != null)
			{
				if (Wearer.CurJob != null)
				{
					Wearer.jobs.EndCurrentJob(JobCondition.InterruptForced, false, true);
				}
				GrappleJob.expiryInterval = (int)(300f * (Controller.Settings.GHSpeed / 100f));
				Wearer.jobs.TryTakeOrderedJob(GrappleJob, JobTag.DraftedOrder);
			}
		}

		// Token: 0x0600003E RID: 62 RVA: 0x0000377E File Offset: 0x0000197E
		public override float GetSpecialApparelScoreOffset()
		{
			return this.EnergyMax * this.ApparelScorePerEnergyMax;
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00003790 File Offset: 0x00001990
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

		// Token: 0x06000040 RID: 64 RVA: 0x0000384A File Offset: 0x00001A4A
		public void KeepDisplaying()
		{
			this.lastKeepDisplayTick = Find.TickManager.TicksGame;
		}

		// Token: 0x06000041 RID: 65 RVA: 0x0000385C File Offset: 0x00001A5C
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

		// Token: 0x06000042 RID: 66 RVA: 0x00003940 File Offset: 0x00001B40
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
			this.ActiveCamo = false;
		}

		// Token: 0x06000043 RID: 67 RVA: 0x00003A08 File Offset: 0x00001C08
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

		// Token: 0x06000044 RID: 68 RVA: 0x00003A70 File Offset: 0x00001C70
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
				Graphics.DrawMesh(MeshPool.plane10, matrix, GhostGearApparel.BubbleMat, 0);
			}
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00003B53 File Offset: 0x00001D53
		public override bool AllowVerbCast(IntVec3 root, Map map, LocalTargetInfo targ, Verb v)
		{
			return true;
		}

		// Token: 0x04000007 RID: 7
		public float energy;

		// Token: 0x04000008 RID: 8
		public int ticksToReset = -1;

		// Token: 0x04000009 RID: 9
		public int lastKeepDisplayTick = -9999;

		// Token: 0x0400000A RID: 10
		public Vector3 impactAngleVect;

		// Token: 0x0400000B RID: 11
		public int lastAbsorbDamageTick = -9999;

		// Token: 0x0400000C RID: 12
		public const float MinDrawSize = 1.2f;

		// Token: 0x0400000D RID: 13
		public const float MaxDrawSize = 1.55f;

		// Token: 0x0400000E RID: 14
		public const float MaxDamagedJitterDist = 0.05f;

		// Token: 0x0400000F RID: 15
		public const int JitterDurationTicks = 8;

		// Token: 0x04000010 RID: 16
		public int StartingTicksToReset = 2500;

		// Token: 0x04000011 RID: 17
		public float EnergyOnReset = 0.2f;

		// Token: 0x04000012 RID: 18
		public float EnergyLossPerDamage = 0.03f;

		// Token: 0x04000013 RID: 19
		public int KeepDisplayingTicks = 1000;

		// Token: 0x04000014 RID: 20
		public float ApparelScorePerEnergyMax = 0.25f;

		// Token: 0x04000015 RID: 21
		public static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent, Color.green);

		// Token: 0x04000016 RID: 22
		public int CaltropsUses;

		// Token: 0x04000017 RID: 23
		public int CaltropsMax = 1;

		// Token: 0x04000018 RID: 24
		public bool ActiveCamo;

		// Token: 0x04000019 RID: 25
		[NoTranslate]
		private string RepulseIconPath = "Things/Special/GGRepulseIcon";

		// Token: 0x0400001A RID: 26
		[NoTranslate]
		private string GrappleHookIconPath = "Things/Special/GGGrappleHookIcon";

		// Token: 0x0400001B RID: 27
		[NoTranslate]
		private string CaltropsIconPath = "Things/Special/GGCaltropsIcon";
	}
}
