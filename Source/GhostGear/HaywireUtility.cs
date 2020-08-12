using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace GhostGear
{
	// Token: 0x02000017 RID: 23
	public static class HaywireUtility
	{
		// Token: 0x06000072 RID: 114 RVA: 0x00004C28 File Offset: 0x00002E28
		public static bool IsHaywireJobDef(JobDef def)
		{
			return def == HaywireUtility.HWJobDef.HWAttackPawn || def == HaywireUtility.HWJobDef.HWAttackThing || def == HaywireUtility.HWJobDef.HWBreakDown || def == HaywireUtility.HWJobDef.HWExplosion || def == HaywireUtility.HWJobDef.HWWander;
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00004C58 File Offset: 0x00002E58
		public static void DoHaywireEffect(Pawn pawn)
		{
			if (pawn != null && pawn.RaceProps.IsMechanoid && HaywireData.TrySetHaywireTicks(pawn, 1.5f, 2.5f) && ((pawn != null) ? pawn.Map : null) != null)
			{
				MoteMaker.ThrowText(new Vector3((float)pawn.Position.x + 1f, (float)pawn.Position.y, (float)pawn.Position.z + 1f), pawn.Map, "GhostGear.HayWired".Translate(), Color.yellow, -1f);
			}
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00004CF1 File Offset: 0x00002EF1
		public static int Rnd100()
		{
			return Rand.Range(1, 100);
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00004CFB File Offset: 0x00002EFB
		public static int RndTicks(int min, int max)
		{
			return (int)((float)Rand.Range(min, max) * 1.2f);
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00004D0C File Offset: 0x00002F0C
		public static void TryStartHaywireJob(Pawn pawn, int Ticks)
		{
			Job HaywireJob = HaywireUtility.TryGetHaywireJob(pawn, Ticks);
			if (HaywireJob != null)
			{
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
				if (flag)
				{
					pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false, true);
				}
				bool flag2;
				if (pawn == null)
				{
					flag2 = (null != null);
				}
				else
				{
					Pawn_JobTracker jobs2 = pawn.jobs;
					flag2 = (((jobs2 != null) ? jobs2.jobQueue : null) != null);
				}
				if (flag2 && pawn != null)
				{
					Pawn_JobTracker jobs3 = pawn.jobs;
					int? num = (jobs3 != null) ? new int?(jobs3.jobQueue.Count) : null;
					int num2 = 0;
					if (num.GetValueOrDefault() > num2 & num != null)
					{
						pawn.jobs.ClearQueuedJobs(true);
					}
				}
				pawn.jobs.jobQueue.EnqueueFirst(HaywireJob, null);
			}
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00004DD0 File Offset: 0x00002FD0
		public static Job TryGetHaywireJob(Pawn pawn, int HWTicks)
		{
			Job haywireJob = null;
			if (pawn.Map == null || pawn.Downed || pawn.Dead || !pawn.Spawned)
			{
				return null;
			}
			if (!HaywireData.IsHaywired(pawn))
			{
				return null;
			}
			if (((pawn != null) ? pawn.CurJob : null) != null && HaywireUtility.IsHaywireJobDef(pawn.CurJobDef))
			{
				return null;
			}
			if (pawn.jobs.jobQueue.Count > 0 && HaywireUtility.IsHaywireJobDef(pawn.jobs.jobQueue.Peek().job.def))
			{
				return null;
			}
			int minTicks = 0;
			int maxTicks = 0;
			int tickBlock = 5;
			Thing targ;
			JobDef haywireJobDef = HaywireUtility.TryGetHWJobDef(pawn, out targ);
			if (haywireJobDef != null)
			{
				if (haywireJobDef == HaywireUtility.HWJobDef.HWAttackPawn || haywireJobDef == HaywireUtility.HWJobDef.HWAttackThing)
				{
					if (targ != null)
					{
						haywireJob = new Job(haywireJobDef, targ);
						haywireJob.locomotionUrgency = LocomotionUrgency.Jog;
						minTicks = HWTicks * (tickBlock * 4);
						maxTicks = HWTicks * (tickBlock * 6);
					}
					else
					{
						haywireJobDef = DefDatabase<JobDef>.GetNamed("HWWander", false);
					}
				}
				if (haywireJobDef == HaywireUtility.HWJobDef.HWBreakDown || haywireJobDef == HaywireUtility.HWJobDef.HWExplosion || haywireJobDef == HaywireUtility.HWJobDef.HWWander)
				{
					haywireJob = new Job(haywireJobDef);
					if (haywireJob.def == HaywireUtility.HWJobDef.HWWander)
					{
						IntVec3 wanderCell = HaywireUtility.TryGetHWWanderCell(pawn);
						minTicks = HWTicks * tickBlock;
						maxTicks = HWTicks * (tickBlock * 3);
						haywireJob = new Job(haywireJobDef, wanderCell);
						haywireJob.locomotionUrgency = LocomotionUrgency.Walk;
					}
					else
					{
						minTicks = HWTicks * tickBlock;
						maxTicks = HWTicks * (tickBlock * 3);
						haywireJob = new Job(haywireJobDef);
						haywireJob.locomotionUrgency = LocomotionUrgency.None;
					}
				}
				if (haywireJob != null)
				{
					haywireJob.expiryInterval = HaywireUtility.RndTicks(minTicks, maxTicks);
				}
			}
			return haywireJob;
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00004F40 File Offset: 0x00003140
		public static JobDef TryGetHWJobDef(Pawn pawn, out Thing target)
		{
			int rnd = HaywireUtility.Rnd100();
			target = null;
			string haywireJobDefName;
			if (rnd <= 15)
			{
				haywireJobDefName = "HWExplosion";
			}
			else if (rnd <= 40)
			{
				haywireJobDefName = "HWBreakDown";
			}
			else if (rnd <= 65)
			{
				Thing targetA;
				haywireJobDefName = HaywireUtility.GetHWAttackJobDefName(pawn, out targetA);
				target = targetA;
			}
			else
			{
				haywireJobDefName = "HWWander";
			}
			return DefDatabase<JobDef>.GetNamed(haywireJobDefName, false);
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00004F98 File Offset: 0x00003198
		public static string GetHWAttackJobDefName(Pawn pawn, out Thing target)
		{
			string haywireJobDefName = "HWAttackPawn";
			target = null;
			if (HaywireUtility.Rnd100() < 60)
			{
				target = HaywireUtility.TryGetHWTargetPawn(pawn);
			}
			else
			{
				haywireJobDefName = "HWAttackThing";
				target = HaywireUtility.TryGetHWTargetStatic(pawn);
			}
			return haywireJobDefName;
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00004FD0 File Offset: 0x000031D0
		public static IntVec3 TryGetHWWanderCell(Pawn pawn)
		{
			return RCellFinder.RandomWanderDestFor(pawn, pawn.Position, HaywireUtility.RndHWWanderRadius(), null, Danger.Deadly);
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00004FF2 File Offset: 0x000031F2
		public static float RndHWWanderRadius()
		{
			return Rand.Range(7f, 10f);
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00005004 File Offset: 0x00003204
		public static Thing TryGetHWTargetPawn(Pawn pawn)
		{
			List<Thing> candidates = new List<Thing>();
			Thing target = null;
			if (((pawn != null) ? pawn.Map : null) != null)
			{
				List<Thing> pawnlist = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn);
				if (pawnlist.Count > 0)
				{
					float closestdist = 999999f;
					foreach (Thing potential in pawnlist)
					{
						if (potential != pawn && pawn.Position.InHorDistOf(potential.Position, 50f) && HaywireUtility.IsValidHWPawnAtk(pawn, potential as Pawn) && HaywireUtility.IsPathableHWThing(pawn, potential, true))
						{
							float dist = pawn.Position.DistanceTo(potential.Position);
							if (dist < closestdist)
							{
								closestdist = dist;
								target = potential;
							}
							candidates.Add(potential);
						}
					}
				}
			}
			if (candidates.Count > 0 && HaywireUtility.Rnd100() < 50)
			{
				target = HaywireUtility.GetRandomThing(candidates);
				candidates.Clear();
			}
			return target;
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00005110 File Offset: 0x00003310
		public static Thing TryGetHWTargetStatic(Pawn pawn)
		{
			Thing target = null;
			if (((pawn != null) ? pawn.Map : null) != null)
			{
				List<Thing> list = new List<Thing>();
				List<Thing> maplist = pawn.Map.listerThings.AllThings;
				if (maplist.Count > 0)
				{
					foreach (Thing thing in maplist)
					{
						if (pawn.Position.InHorDistOf(thing.Position, 50f) && HaywireUtility.IsValidHWThingStatic(pawn, thing) && thing.def.useHitPoints && HaywireUtility.IsPathableHWThing(pawn, thing, false))
						{
							list.AddDistinct(thing);
						}
					}
					if (list.Count > 0)
					{
						target = HaywireUtility.GetRandomThing(list);
						list.Clear();
					}
				}
			}
			return target;
		}

		// Token: 0x0600007E RID: 126 RVA: 0x000051F0 File Offset: 0x000033F0
		public static Thing GetRandomThing(List<Thing> list)
		{
			return list.RandomElement<Thing>();
		}

		// Token: 0x0600007F RID: 127 RVA: 0x000051F8 File Offset: 0x000033F8
		public static bool IsPathableHWThing(Pawn pawn, Thing thing, bool isPawn)
		{
			if (pawn != null && thing != null && ((pawn != null) ? pawn.Map : null) != null)
			{
				LocalTargetInfo target = new LocalTargetInfo(thing);
				if (pawn.CanSee(target.Thing, null))
				{
					return true;
				}
				PathEndMode PEMode = PathEndMode.Touch;
				if (isPawn)
				{
					PEMode = PathEndMode.ClosestTouch;
				}
				if (pawn.CanReach(target, PEMode, Danger.Deadly, false, TraverseMode.NoPassClosedDoors))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000080 RID: 128 RVA: 0x0000524A File Offset: 0x0000344A
		public static bool IsValidHWPawnAtk(Pawn attacker, Pawn potential)
		{
			return potential != null && potential.Spawned && !potential.Downed && !potential.Dead && !potential.IsBurning() && attacker.CanReserve(potential, 1, -1, null, false);
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00005284 File Offset: 0x00003484
		public static bool IsValidHWThingStatic(Pawn attacker, Thing thing)
		{
			return attacker.CanReserve(thing, 1, -1, null, false) && !thing.DestroyedOrNull() && thing.def.useHitPoints && ((thing is Building && thing.def.IsBuildingArtificial) || (thing is Plant && thing.def.plant.IsTree));
		}

		// Token: 0x04000023 RID: 35
		public const float sensible = 50f;

		// Token: 0x0200002F RID: 47
		[DefOf]
		public static class HWJobDef
		{
			// Token: 0x0400004C RID: 76
			public static JobDef HWAttackPawn;

			// Token: 0x0400004D RID: 77
			public static JobDef HWAttackThing;

			// Token: 0x0400004E RID: 78
			public static JobDef HWWander;

			// Token: 0x0400004F RID: 79
			public static JobDef HWBreakDown;

			// Token: 0x04000050 RID: 80
			public static JobDef HWExplosion;
		}

		// Token: 0x02000030 RID: 48
		public static class CFJobDef
		{
			// Token: 0x04000051 RID: 81
			public static JobDef GGWaitConfused;

			// Token: 0x04000052 RID: 82
			public static JobDef GGFleeConfused;
		}
	}
}
