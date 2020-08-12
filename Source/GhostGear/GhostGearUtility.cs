using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace GhostGear
{
	// Token: 0x02000011 RID: 17
	public static class GhostGearUtility
	{
		// Token: 0x0600004B RID: 75 RVA: 0x00003C50 File Offset: 0x00001E50
		public static bool IsTargetGhosted(Pawn target, Thing Caster)
		{
			Apparel GGItem;
			return !GhostGearUtility.isFFGhost(target, Caster) && target != null && target.Spawned && GhostGearUtility.IsWearingGhostGear(target, out GGItem) && GhostGearUtility.GhostGearIsActive(GGItem) && Caster != null && Caster.Spawned && ((target != null) ? target.Map : null) != null && ((Caster != null) ? Caster.Map : null) != null && target.Map == Caster.Map && GenSight.LineOfSight(Caster.Position, target.Position, Caster.Map, false, null, 0, 0) && GhostGearUtility.GhostEffectWorked(target, Caster, GGItem);
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00003CE2 File Offset: 0x00001EE2
		public static bool isFFGhost(Pawn target, Thing caster)
		{
			return ((target != null) ? target.Faction : null) != null && ((target != null) ? target.Faction : null) == ((caster != null) ? caster.Faction : null);
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00003D10 File Offset: 0x00001F10
		public static bool GhostEffectWorked(Pawn target, Thing caster, Apparel GGItem)
		{
			float distance = target.Position.DistanceTo(caster.Position);
			if (distance >= GhostGearUtility.minGGDist)
			{
				float scaler;
				if (distance > GhostGearUtility.maxGGDist)
				{
					scaler = 1f;
				}
				else
				{
					scaler = (distance - GhostGearUtility.minGGDist) / (GhostGearUtility.maxGGDist - GhostGearUtility.minGGDist);
				}
				if (scaler < 0f)
				{
					scaler = 0f;
				}
				if (scaler > 1f)
				{
					scaler = 1f;
				}
				int qualOffset = 0;
				int diffOffset = 0;
				QualityCategory qc;
				if (GGItem.TryGetQuality(out qc))
				{
					switch (qc)
					{
					case QualityCategory.Awful:
						qualOffset = -10;
						break;
					case QualityCategory.Poor:
						qualOffset = -5;
						break;
					case QualityCategory.Normal:
						qualOffset = 0;
						break;
					case QualityCategory.Good:
						qualOffset = 5;
						break;
					case QualityCategory.Excellent:
						qualOffset = 10;
						break;
					case QualityCategory.Masterwork:
						qualOffset = 15;
						break;
					case QualityCategory.Legendary:
						qualOffset = 20;
						break;
					}
				}
				int chance = (int)Mathf.Lerp(95f, 5f, scaler);
				if (caster is Pawn)
				{
					if ((caster as Pawn).health.capacities.CapableOf(PawnCapacityDefOf.Sight))
					{
						float sight = (caster as Pawn).health.capacities.GetLevel(PawnCapacityDefOf.Sight);
						chance = (int)((float)chance * sight);
						return chance < 100 && (chance < 5 || HaywireUtility.Rnd100() + (qualOffset + diffOffset) > chance);
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00003E51 File Offset: 0x00002051
		internal static bool GhostGearIsActive(Apparel GGItem)
		{
			return GGItem is GhostGearApparel && (GGItem as GhostGearApparel).ShieldState == ShieldState.Active;
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00003E6C File Offset: 0x0000206C
		internal static bool IsWearingGhostGear(Pawn pawn, out Apparel GGItem)
		{
			GGItem = null;
			if (((pawn != null) ? pawn.apparel : null) != null)
			{
				Pawn_ApparelTracker apparel = pawn.apparel;
				if (apparel != null && apparel.WornApparelCount > 0)
				{
					foreach (Apparel item in pawn.apparel.WornApparel)
					{
						if (item is GhostGearApparel && GhostGearUtility.GhostGearList().Contains(item.def.defName))
						{
							GGItem = item;
							return true;
						}
					}
					return false;
				}
			}
			return false;
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00003F10 File Offset: 0x00002110
		internal static List<string> GhostGearList()
		{
			List<string> list = new List<string>();
			list.AddDistinct("Apparel_PowerArmorGhostGear");
			return list;
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00003F22 File Offset: 0x00002122
		internal static bool GetIsGGApparel(ThingDef def)
		{
			return ((def != null) ? def.thingClass.FullName : null) == "GhostGear.GhostGearApparel";
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00003F44 File Offset: 0x00002144
		internal static Apparel GetWornGG(Thing pawn)
		{
			Apparel GG = null;
			Pawn pawn2 = pawn as Pawn;
			bool flag;
			if (pawn2 == null)
			{
				flag = false;
			}
			else
			{
				Pawn_ApparelTracker apparel = pawn2.apparel;
				int? num = (apparel != null) ? new int?(apparel.WornApparelCount) : null;
				int num2 = 0;
				flag = (num.GetValueOrDefault() > num2 & num != null);
			}
			if (flag)
			{
				Pawn pawn3 = pawn as Pawn;
				List<Apparel> list;
				if (pawn3 == null)
				{
					list = null;
				}
				else
				{
					Pawn_ApparelTracker apparel2 = pawn3.apparel;
					list = ((apparel2 != null) ? apparel2.WornApparel : null);
				}
				List<Apparel> apparels = list;
				if (apparels != null && apparels.Count > 0)
				{
					for (int i = 0; i < apparels.Count; i++)
					{
						if (apparels[i] is GhostGearApparel)
						{
							GG = apparels[i];
							break;
						}
					}
				}
			}
			return GG;
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00003FF8 File Offset: 0x000021F8
		internal static void DoConfusedMote(Thing thing, Thing ghost)
		{
			bool justPawns = true;
			bool doMote = false;
			if (thing != null && ((thing != null) ? thing.Map : null) != null && thing.Spawned)
			{
				if (justPawns)
				{
					if (thing is Pawn)
					{
						doMote = true;
					}
				}
				else
				{
					doMote = true;
				}
				if (doMote)
				{
					if (GhostGearUtility.isValidThingForConfusion(thing))
					{
						ThoughtDef confusedDef = DefDatabase<ThoughtDef>.GetNamed("GGConfused", true);
						Pawn pawn = thing as Pawn;
						if (pawn != null)
						{
							Pawn_NeedsTracker needs = pawn.needs;
							if (needs != null)
							{
								Need_Mood mood = needs.mood;
								if (mood != null)
								{
									ThoughtHandler thoughts = mood.thoughts;
									if (thoughts != null)
									{
										MemoryThoughtHandler memories = thoughts.memories;
										if (memories != null)
										{
											memories.TryGainMemory(confusedDef, null);
										}
									}
								}
							}
						}
						GhostGearUtility.DoConfusionJob(thing, false, ghost);
						return;
					}
					MoteMaker.ThrowText(thing.Position.ToVector3(), thing.Map, "GhostGear.ConfusedMsg".Translate(), Color.green, -1f);
					if (thing is Pawn)
					{
						GhostGearUtility.DoConfusionJob(thing, true, ghost);
					}
				}
			}
		}

		// Token: 0x06000054 RID: 84 RVA: 0x000040E0 File Offset: 0x000022E0
		internal static bool IsConfusionJob(JobDef jobdef, JobDef flee, JobDef wait)
		{
			return jobdef == flee || jobdef == wait;
		}

		// Token: 0x06000055 RID: 85 RVA: 0x000040F0 File Offset: 0x000022F0
		internal static void DoConfusionJob(Thing thing, bool isHiveMind, Thing ghost)
		{
			Pawn pawn = thing as Pawn;
			if (pawn != null)
			{
				JobDef fleeDef;
				JobDef waitDef;
				Job confusedJob = GhostGearUtility.GetConfusedJob(thing, isHiveMind, ghost, out fleeDef, out waitDef);
				if (confusedJob != null)
				{
					bool flag;
					if (pawn == null)
					{
						flag = (null != null);
					}
					else
					{
						Pawn_JobTracker jobs = pawn.jobs;
						flag = (((jobs != null) ? jobs.jobQueue : null) != null);
					}
					if (flag && pawn.jobs.jobQueue.Count > 0 && pawn != null)
					{
						pawn.jobs.ClearQueuedJobs(true);
					}
					bool flag2;
					if (pawn == null)
					{
						flag2 = (null != null);
					}
					else
					{
						Pawn_JobTracker jobs2 = pawn.jobs;
						flag2 = (((jobs2 != null) ? jobs2.curJob : null) != null);
					}
					if (flag2)
					{
						pawn.jobs.EndCurrentJob(JobCondition.Incompletable, false, true);
					}
					pawn.jobs.jobQueue.EnqueueFirst(confusedJob, null);
				}
			}
		}

		// Token: 0x06000056 RID: 86 RVA: 0x0000419C File Offset: 0x0000239C
		internal static Job GetConfusedJob(Thing thing, bool isHiveMind, Thing ghost, out JobDef ConfusionFleeJobDef, out JobDef ConfusionWaitJobDef)
		{
			int minTicks = 120;
			int maxTicks = 240;
			bool flee = false;
			IntVec3 confusionFleeCell = thing.Position;
			ConfusionFleeJobDef = DefDatabase<JobDef>.GetNamed("GGFleeConfused", true);
			ConfusionWaitJobDef = DefDatabase<JobDef>.GetNamed("GGWaitConfused", true);
			Pawn pawn = thing as Pawn;
			bool flag;
			if (pawn == null)
			{
				flag = (null != null);
			}
			else
			{
				Pawn_JobTracker jobs = pawn.jobs;
				flag = (((jobs != null) ? jobs.curJob : null) != null);
			}
			if (flag && GhostGearUtility.IsConfusionJob((thing as Pawn).CurJob.def, ConfusionFleeJobDef, ConfusionWaitJobDef))
			{
				return null;
			}
			Pawn pawn2 = thing as Pawn;
			bool flag2;
			if (pawn2 == null)
			{
				flag2 = (null != null);
			}
			else
			{
				Pawn_JobTracker jobs2 = pawn2.jobs;
				flag2 = (((jobs2 != null) ? jobs2.jobQueue : null) != null);
			}
			if (flag2 && (thing as Pawn).jobs.jobQueue.Count > 0 && GhostGearUtility.IsConfusionJob((thing as Pawn).jobs.jobQueue.Peek().job.def, ConfusionFleeJobDef, ConfusionWaitJobDef))
			{
				return null;
			}
			JobDef ConfusionJobDef;
			if (isHiveMind)
			{
				ConfusionJobDef = ConfusionWaitJobDef;
			}
			else if (HaywireUtility.Rnd100() > 33)
			{
				ConfusionJobDef = ConfusionWaitJobDef;
			}
			else
			{
				ConfusionJobDef = ConfusionFleeJobDef;
				minTicks *= 4;
				maxTicks *= 5;
				flee = true;
				IntVec3 result;
				if (!RCellFinder.TryFindDirectFleeDestination(ghost.Position, 10f, thing as Pawn, out result))
				{
					flee = false;
					ConfusionJobDef = ConfusionWaitJobDef;
				}
				else
				{
					confusionFleeCell = result;
				}
			}
			Job confusedJob;
			if (flee)
			{
				confusedJob = new Job(ConfusionJobDef, confusionFleeCell, ghost);
			}
			else
			{
				confusedJob = new Job(ConfusionJobDef);
			}
			confusedJob.expiryInterval = HaywireUtility.RndTicks(minTicks, maxTicks);
			if (flee)
			{
				confusedJob.locomotionUrgency = LocomotionUrgency.Sprint;
			}
			else
			{
				confusedJob.locomotionUrgency = LocomotionUrgency.None;
			}
			return confusedJob;
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00004314 File Offset: 0x00002514
		internal static bool isValidThingForConfusion(Thing thing)
		{
			return thing != null && ((thing != null) ? thing.Map : null) != null && thing is Pawn && (thing as Pawn).Spawned && !(thing as Pawn).RaceProps.IsMechanoid && (!(thing as Pawn).RaceProps.Animal || (thing as Pawn).RaceProps.FleshType != FleshTypeDefOf.Insectoid);
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00004388 File Offset: 0x00002588
		internal static bool GGComposMentis(Pawn p, Thing GGArmour, out string Reason)
		{
			Reason = "";
			if (p == null)
			{
				Reason = "GhostGear.ReasonNotFound".Translate();
				return false;
			}
			if (p.IsBurning())
			{
				Reason = "GhostGear.ReasonOnFire".Translate();
				return false;
			}
			if (p.Dead)
			{
				Reason = "GhostGear.ReasonDead".Translate();
				return false;
			}
			if (p.InMentalState)
			{
				Reason = "GhostGear.ReasonMental".Translate();
				return false;
			}
			if (p.Downed)
			{
				Reason = "GhostGear.ReasonIncap".Translate();
				return false;
			}
			if (!p.Awake() || !p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				Reason = "GhostGear.ReasonOperate".Translate() + GGArmour.Label.CapitalizeFirst();
				return false;
			}
			return true;
		}

		// Token: 0x0400001C RID: 28
		public static float minGGDist = Controller.Settings.MinGhostDist;

		// Token: 0x0400001D RID: 29
		public static float maxGGDist = Controller.Settings.MaxGhostDist;
	}
}
