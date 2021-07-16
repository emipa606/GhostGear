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
        // Token: 0x04000023 RID: 35
        public const float sensible = 50f;

        // Token: 0x06000072 RID: 114 RVA: 0x00004C28 File Offset: 0x00002E28
        public static bool IsHaywireJobDef(JobDef def)
        {
            return def == HWJobDef.HWAttackPawn || def == HWJobDef.HWAttackThing || def == HWJobDef.HWBreakDown ||
                   def == HWJobDef.HWExplosion || def == HWJobDef.HWWander;
        }

        // Token: 0x06000073 RID: 115 RVA: 0x00004C58 File Offset: 0x00002E58
        public static void DoHaywireEffect(Pawn pawn)
        {
            if (pawn != null && pawn.RaceProps.IsMechanoid && HaywireData.TrySetHaywireTicks(pawn, 1.5f, 2.5f) &&
                pawn.Map != null)
            {
                MoteMaker.ThrowText(new Vector3(pawn.Position.x + 1f, pawn.Position.y, pawn.Position.z + 1f), pawn.Map,
                    "GhostGear.HayWired".Translate(), Color.yellow);
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
            return (int) (Rand.Range(min, max) * 1.2f);
        }

        // Token: 0x06000076 RID: 118 RVA: 0x00004D0C File Offset: 0x00002F0C
        public static void TryStartHaywireJob(Pawn pawn, int Ticks)
        {
            var HaywireJob = TryGetHaywireJob(pawn, Ticks);
            if (HaywireJob == null)
            {
                return;
            }

            bool hasJob;
            if (pawn == null)
            {
                hasJob = false;
            }
            else
            {
                var jobs = pawn.jobs;
                hasJob = jobs?.curJob != null;
            }

            if (hasJob)
            {
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
            }

            bool hasJobQueue;
            if (pawn == null)
            {
                hasJobQueue = false;
            }
            else
            {
                var jobs2 = pawn.jobs;
                hasJobQueue = jobs2?.jobQueue != null;
            }

            if (hasJobQueue)
            {
                var jobs3 = pawn.jobs;
                var num = jobs3 != null ? new int?(jobs3.jobQueue.Count) : null;
                var num2 = 0;
                if ((num.GetValueOrDefault() > num2) & (num != null))
                {
                    pawn.jobs.ClearQueuedJobs();
                }
            }

            pawn?.jobs.jobQueue.EnqueueFirst(HaywireJob);
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

            if (pawn.CurJob != null && IsHaywireJobDef(pawn.CurJobDef))
            {
                return null;
            }

            if (pawn.jobs.jobQueue.Count > 0 && IsHaywireJobDef(pawn.jobs.jobQueue.Peek().job.def))
            {
                return null;
            }

            var minTicks = 0;
            var maxTicks = 0;
            var tickBlock = 5;
            var haywireJobDef = TryGetHWJobDef(pawn, out var targ);
            if (haywireJobDef == null)
            {
                return null;
            }

            if (haywireJobDef == HWJobDef.HWAttackPawn || haywireJobDef == HWJobDef.HWAttackThing)
            {
                if (targ != null)
                {
                    haywireJob = new Job(haywireJobDef, targ) {locomotionUrgency = LocomotionUrgency.Jog};
                    minTicks = HWTicks * tickBlock * 4;
                    maxTicks = HWTicks * tickBlock * 6;
                }
                else
                {
                    haywireJobDef = DefDatabase<JobDef>.GetNamed("HWWander", false);
                }
            }

            if (haywireJobDef == HWJobDef.HWBreakDown || haywireJobDef == HWJobDef.HWExplosion ||
                haywireJobDef == HWJobDef.HWWander)
            {
                haywireJob = new Job(haywireJobDef);
                if (haywireJob.def == HWJobDef.HWWander)
                {
                    var wanderCell = TryGetHWWanderCell(pawn);
                    minTicks = HWTicks * tickBlock;
                    maxTicks = HWTicks * tickBlock * 3;
                    haywireJob = new Job(haywireJobDef, wanderCell) {locomotionUrgency = LocomotionUrgency.Walk};
                }
                else
                {
                    minTicks = HWTicks * tickBlock;
                    maxTicks = HWTicks * tickBlock * 3;
                    haywireJob = new Job(haywireJobDef) {locomotionUrgency = LocomotionUrgency.None};
                }
            }

            if (haywireJob != null)
            {
                haywireJob.expiryInterval = RndTicks(minTicks, maxTicks);
            }

            return haywireJob;
        }

        // Token: 0x06000078 RID: 120 RVA: 0x00004F40 File Offset: 0x00003140
        public static JobDef TryGetHWJobDef(Pawn pawn, out Thing target)
        {
            var rnd = Rnd100();
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
                haywireJobDefName = GetHWAttackJobDefName(pawn, out var targetA);
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
            var haywireJobDefName = "HWAttackPawn";
            target = null;
            if (Rnd100() < 60)
            {
                target = TryGetHWTargetPawn(pawn);
            }
            else
            {
                haywireJobDefName = "HWAttackThing";
                target = TryGetHWTargetStatic(pawn);
            }

            return haywireJobDefName;
        }

        // Token: 0x0600007A RID: 122 RVA: 0x00004FD0 File Offset: 0x000031D0
        public static IntVec3 TryGetHWWanderCell(Pawn pawn)
        {
            return RCellFinder.RandomWanderDestFor(pawn, pawn.Position, RndHWWanderRadius(), null, Danger.Deadly);
        }

        // Token: 0x0600007B RID: 123 RVA: 0x00004FF2 File Offset: 0x000031F2
        public static float RndHWWanderRadius()
        {
            return Rand.Range(7f, 10f);
        }

        // Token: 0x0600007C RID: 124 RVA: 0x00005004 File Offset: 0x00003204
        public static Thing TryGetHWTargetPawn(Pawn pawn)
        {
            var candidates = new List<Thing>();
            Thing target = null;
            if (pawn?.Map != null)
            {
                var pawnlist = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn);
                if (pawnlist.Count > 0)
                {
                    var closestdist = 999999f;
                    foreach (var potential in pawnlist)
                    {
                        if (potential == pawn || !pawn.Position.InHorDistOf(potential.Position, 50f) ||
                            !IsValidHWPawnAtk(pawn, potential as Pawn) || !IsPathableHWThing(pawn, potential, true))
                        {
                            continue;
                        }

                        var dist = pawn.Position.DistanceTo(potential.Position);
                        if (dist < closestdist)
                        {
                            closestdist = dist;
                            target = potential;
                        }

                        candidates.Add(potential);
                    }
                }
            }

            if (candidates.Count <= 0 || Rnd100() >= 50)
            {
                return target;
            }

            target = GetRandomThing(candidates);
            candidates.Clear();

            return target;
        }

        // Token: 0x0600007D RID: 125 RVA: 0x00005110 File Offset: 0x00003310
        public static Thing TryGetHWTargetStatic(Pawn pawn)
        {
            if (pawn?.Map == null)
            {
                return null;
            }

            var list = new List<Thing>();
            var maplist = pawn.Map.listerThings.AllThings;
            if (maplist.Count <= 0)
            {
                return null;
            }

            foreach (var thing in maplist)
            {
                if (pawn.Position.InHorDistOf(thing.Position, 50f) && IsValidHWThingStatic(pawn, thing) &&
                    thing.def.useHitPoints && IsPathableHWThing(pawn, thing, false))
                {
                    list.AddDistinct(thing);
                }
            }

            if (list.Count <= 0)
            {
                return null;
            }

            var target = GetRandomThing(list);
            list.Clear();

            return target;
        }

        // Token: 0x0600007E RID: 126 RVA: 0x000051F0 File Offset: 0x000033F0
        public static Thing GetRandomThing(List<Thing> list)
        {
            return list.RandomElement();
        }

        // Token: 0x0600007F RID: 127 RVA: 0x000051F8 File Offset: 0x000033F8
        public static bool IsPathableHWThing(Pawn pawn, Thing thing, bool isPawn)
        {
            if (pawn == null || thing == null || pawn.Map == null)
            {
                return false;
            }

            var target = new LocalTargetInfo(thing);
            if (pawn.CanSee(target.Thing))
            {
                return true;
            }

            var PEMode = PathEndMode.Touch;
            if (isPawn)
            {
                PEMode = PathEndMode.ClosestTouch;
            }

            if (pawn.CanReach(target, PEMode, Danger.Deadly, false, false, TraverseMode.NoPassClosedDoors))
            {
                return true;
            }

            return false;
        }

        // Token: 0x06000080 RID: 128 RVA: 0x0000524A File Offset: 0x0000344A
        public static bool IsValidHWPawnAtk(Pawn attacker, Pawn potential)
        {
            return potential is {Spawned: true, Downed: false, Dead: false} && !potential.IsBurning() &&
                   attacker.CanReserve(potential);
        }

        // Token: 0x06000081 RID: 129 RVA: 0x00005284 File Offset: 0x00003484
        public static bool IsValidHWThingStatic(Pawn attacker, Thing thing)
        {
            return attacker.CanReserve(thing) && !thing.DestroyedOrNull() && thing.def.useHitPoints &&
                   (thing is Building && thing.def.IsBuildingArtificial || thing is Plant && thing.def.plant.IsTree);
        }

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