using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace GhostGear;

public static class GhostGearUtility
{
    public static float minGGDist = Controller.Settings.MinGhostDist;

    public static float maxGGDist = Controller.Settings.MaxGhostDist;

    public static bool IsTargetGhosted(Pawn target, Thing Caster)
    {
        return !isFFGhost(target, Caster) && target is { Spawned: true } &&
               IsWearingGhostGear(target, out var GGItem) && GhostGearIsActive(GGItem) && Caster is
                   { Spawned: true } && target.Map != null && Caster.Map != null && target.Map == Caster.Map &&
               GenSight.LineOfSight(Caster.Position, target.Position, Caster.Map) &&
               GhostEffectWorked(target, Caster, GGItem);
    }

    public static bool isFFGhost(Pawn target, Thing caster)
    {
        return target?.Faction != null && target.Faction ==
            caster?.Faction;
    }

    public static bool GhostEffectWorked(Pawn target, Thing caster, Apparel GGItem)
    {
        var distance = target.Position.DistanceTo(caster.Position);
        if (!(distance >= minGGDist))
        {
            return false;
        }

        float scaler;
        if (distance > maxGGDist)
        {
            scaler = 1f;
        }
        else
        {
            scaler = (distance - minGGDist) / (maxGGDist - minGGDist);
        }

        if (scaler < 0f)
        {
            scaler = 0f;
        }

        if (scaler > 1f)
        {
            scaler = 1f;
        }

        var qualOffset = 0;
        if (GGItem.TryGetQuality(out var qc))
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

        var chance = (int)Mathf.Lerp(95f, 5f, scaler);
        if (caster is not Pawn pawn)
        {
            return false;
        }

        if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
        {
            return true;
        }

        var sight = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight);
        chance = (int)(chance * sight);
        return chance < 100 && (chance < 5 || HaywireUtility.Rnd100() + qualOffset > chance);
    }

    internal static bool GhostGearIsActive(Apparel GGItem)
    {
        return GGItem is GhostGearApparel { ShieldState: ShieldState.Active };
    }

    internal static bool IsWearingGhostGear(Pawn pawn, out Apparel GGItem)
    {
        GGItem = null;
        if (pawn?.apparel == null)
        {
            return false;
        }

        var apparel = pawn.apparel;
        if (apparel is not { WornApparelCount: > 0 })
        {
            return false;
        }

        foreach (var item in pawn.apparel.WornApparel)
        {
            if (item is not GhostGearApparel || !GhostGearList().Contains(item.def.defName))
            {
                continue;
            }

            GGItem = item;
            return true;
        }

        return false;
    }

    internal static List<string> GhostGearList()
    {
        var list = new List<string>();
        list.AddDistinct("Apparel_PowerArmorGhostGear");
        return list;
    }

    internal static bool GetIsGGApparel(ThingDef def)
    {
        return def?.thingClass.FullName == "GhostGear.GhostGearApparel";
    }

    internal static Apparel GetWornGG(Thing pawn)
    {
        Apparel GG = null;
        bool hasApparel;
        if (pawn is not Pawn pawn2)
        {
            hasApparel = false;
        }
        else
        {
            var apparel = pawn2.apparel;
            var num = apparel != null ? new int?(apparel.WornApparelCount) : null;
            var num2 = 0;
            hasApparel = (num.GetValueOrDefault() > num2) & (num != null);
        }

        if (!hasApparel)
        {
            return null;
        }

        var pawn3 = (Pawn)pawn;

        var apparel2 = pawn3.apparel;
        var list = apparel2?.WornApparel;

        if (list is not { Count: > 0 })
        {
            return null;
        }

        foreach (var apparel in list)
        {
            if (apparel is not GhostGearApparel)
            {
                continue;
            }

            GG = apparel;
            break;
        }

        return GG;
    }

    internal static void DoConfusedMote(Thing thing, Thing ghost)
    {
        var doMote = false;
        if (thing is { Map: null, Spawned: true })
        {
            return;
        }

        if (thing is Pawn)
        {
            doMote = true;
        }

        if (!doMote)
        {
            return;
        }

        if (isValidThingForConfusion(thing))
        {
            var confusedDef = DefDatabase<ThoughtDef>.GetNamed("GGConfused");
            if (thing is Pawn pawn)
            {
                var needs = pawn.needs;
                var mood = needs?.mood;
                var thoughts = mood?.thoughts;
                var memories = thoughts?.memories;
                memories?.TryGainMemory(confusedDef);
            }

            DoConfusionJob(thing, false, ghost);
            return;
        }

        MoteMaker.ThrowText(thing.Position.ToVector3(), thing.Map, "GhostGear.ConfusedMsg".Translate(),
            Color.green);
        DoConfusionJob(thing, true, ghost);
    }

    internal static bool IsConfusionJob(JobDef jobdef, JobDef flee, JobDef wait)
    {
        return jobdef == flee || jobdef == wait;
    }

    internal static void DoConfusionJob(Thing thing, bool isHiveMind, Thing ghost)
    {
        if (thing is not Pawn pawn)
        {
            return;
        }

        var confusedJob = GetConfusedJob(thing, isHiveMind, ghost, out _, out _);
        if (confusedJob == null)
        {
            return;
        }

        var jobs = pawn.jobs;

        if (jobs?.jobQueue != null && pawn.jobs.jobQueue.Count > 0)
        {
            pawn.jobs.ClearQueuedJobs();
        }

        if (pawn.jobs?.curJob != null)
        {
            pawn.jobs.EndCurrentJob(JobCondition.Incompletable, false);
        }

        pawn.jobs?.jobQueue.EnqueueFirst(confusedJob);
    }

    internal static Job GetConfusedJob(Thing thing, bool isHiveMind, Thing ghost, out JobDef ConfusionFleeJobDef,
        out JobDef ConfusionWaitJobDef)
    {
        var minTicks = 120;
        var maxTicks = 240;
        var flee = false;
        var confusionFleeCell = thing.Position;
        ConfusionFleeJobDef = DefDatabase<JobDef>.GetNamed("GGFleeConfused");
        ConfusionWaitJobDef = DefDatabase<JobDef>.GetNamed("GGWaitConfused");
        bool hasJob;
        if (thing is not Pawn pawn)
        {
            hasJob = false;
        }
        else
        {
            var jobs = pawn.jobs;
            hasJob = jobs?.curJob != null;
        }

        if (hasJob && IsConfusionJob(((Pawn)thing).CurJob.def, ConfusionFleeJobDef, ConfusionWaitJobDef))
        {
            return null;
        }

        bool hasJobQueue;
        if (thing is not Pawn pawn2)
        {
            hasJobQueue = false;
        }
        else
        {
            var jobs2 = pawn2.jobs;
            hasJobQueue = jobs2?.jobQueue != null;
        }

        if (hasJobQueue && ((Pawn)thing).jobs.jobQueue.Count > 0 &&
            IsConfusionJob(((Pawn)thing).jobs.jobQueue.Peek().job.def, ConfusionFleeJobDef, ConfusionWaitJobDef))
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
            if (!RCellFinder.TryFindDirectFleeDestination(ghost.Position, 10f, thing as Pawn, out var result))
            {
                flee = false;
                ConfusionJobDef = ConfusionWaitJobDef;
            }
            else
            {
                confusionFleeCell = result;
            }
        }

        var confusedJob = flee ? new Job(ConfusionJobDef, confusionFleeCell, ghost) : new Job(ConfusionJobDef);

        confusedJob.expiryInterval = HaywireUtility.RndTicks(minTicks, maxTicks);
        confusedJob.locomotionUrgency = flee ? LocomotionUrgency.Sprint : LocomotionUrgency.None;

        return confusedJob;
    }

    internal static bool isValidThingForConfusion(Thing thing)
    {
        return thing is { Map: { } } and Pawn && ((Pawn)thing).Spawned && !((Pawn)thing).RaceProps.IsMechanoid &&
               (!((Pawn)thing).RaceProps.Animal ||
                ((Pawn)thing).RaceProps.FleshType != FleshTypeDefOf.Insectoid);
    }

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

        if (p.Awake() && p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
        {
            return true;
        }

        Reason = "GhostGear.ReasonOperate".Translate() + GGArmour.Label.CapitalizeFirst();
        return false;
    }
}