using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace GhostGear;

public static class HaywireUtility
{
    public const float sensible = 50f;

    public static bool IsHaywireJobDef(JobDef def)
    {
        return def == HWJobDef.HWAttackPawn || def == HWJobDef.HWAttackThing || def == HWJobDef.HWBreakDown ||
               def == HWJobDef.HWExplosion || def == HWJobDef.HWWander;
    }

    public static void DoHaywireEffect(Pawn pawn)
    {
        if (pawn != null && pawn.RaceProps.IsMechanoid && HaywireData.TrySetHaywireTicks(pawn, 1.5f, 2.5f) &&
            pawn.Map != null)
        {
            MoteMaker.ThrowText(new Vector3(pawn.Position.x + 1f, pawn.Position.y, pawn.Position.z + 1f), pawn.Map,
                "GhostGear.HayWired".Translate(), Color.yellow);
        }
    }

    public static int Rnd100()
    {
        return Rand.Range(1, 100);
    }

    public static int RndTicks(int min, int max)
    {
        return (int)(Rand.Range(min, max) * 1.2f);
    }

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
                haywireJob = new Job(haywireJobDef, targ) { locomotionUrgency = LocomotionUrgency.Jog };
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
                haywireJob = new Job(haywireJobDef, wanderCell) { locomotionUrgency = LocomotionUrgency.Walk };
            }
            else
            {
                minTicks = HWTicks * tickBlock;
                maxTicks = HWTicks * tickBlock * 3;
                haywireJob = new Job(haywireJobDef) { locomotionUrgency = LocomotionUrgency.None };
            }
        }

        if (haywireJob != null)
        {
            haywireJob.expiryInterval = RndTicks(minTicks, maxTicks);
        }

        return haywireJob;
    }

    public static JobDef TryGetHWJobDef(Pawn pawn, out Thing target)
    {
        var rnd = Rnd100();
        target = null;
        string haywireJobDefName;
        switch (rnd)
        {
            case <= 15:
                haywireJobDefName = "HWExplosion";
                break;
            case <= 40:
                haywireJobDefName = "HWBreakDown";
                break;
            case <= 65:
                haywireJobDefName = GetHWAttackJobDefName(pawn, out var targetA);
                target = targetA;
                break;
            default:
                haywireJobDefName = "HWWander";
                break;
        }

        return DefDatabase<JobDef>.GetNamed(haywireJobDefName, false);
    }

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

    public static IntVec3 TryGetHWWanderCell(Pawn pawn)
    {
        return RCellFinder.RandomWanderDestFor(pawn, pawn.Position, RndHWWanderRadius(), null, Danger.Deadly);
    }

    public static float RndHWWanderRadius()
    {
        return Rand.Range(7f, 10f);
    }

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

    public static Thing GetRandomThing(List<Thing> list)
    {
        return list.RandomElement();
    }

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

        return pawn.CanReach(target, PEMode, Danger.Deadly, false, false, TraverseMode.NoPassClosedDoors);
    }

    public static bool IsValidHWPawnAtk(Pawn attacker, Pawn potential)
    {
        return potential is { Spawned: true, Downed: false, Dead: false } && !potential.IsBurning() &&
               attacker.CanReserve(potential);
    }

    public static bool IsValidHWThingStatic(Pawn attacker, Thing thing)
    {
        return attacker.CanReserve(thing) && !thing.DestroyedOrNull() && thing.def.useHitPoints &&
               (thing is Building && thing.def.IsBuildingArtificial || thing is Plant && thing.def.plant.IsTree);
    }

    [DefOf]
    public static class HWJobDef
    {
        public static JobDef HWAttackPawn;

        public static JobDef HWAttackThing;

        public static JobDef HWWander;

        public static JobDef HWBreakDown;

        public static JobDef HWExplosion;
    }

    public static class CFJobDef
    {
        public static JobDef GGWaitConfused;

        public static JobDef GGFleeConfused;
    }
}