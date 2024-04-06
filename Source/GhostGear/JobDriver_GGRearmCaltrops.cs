using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GhostGear;

public class JobDriver_GGRearmCaltrops : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.targetA, job);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var actor = GetActor();
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        yield return Toils_Reserve.Reserve(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch)
            .FailOnDespawnedNullOrForbidden(TargetIndex.A);
        var refuel = Toils_General.Wait(180);
        refuel.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        refuel.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
        refuel.WithProgressBarToilDelay(TargetIndex.A);
        yield return refuel;
        yield return new Toil
        {
            initAction = delegate
            {
                if (actor == null || actor.apparel.WornApparelCount <= 0)
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }

                Apparel GhostGear = null;
                var list = actor.apparel.WornApparel;
                foreach (var ghostGear in list)
                {
                    if (ghostGear is not GhostGearApparel)
                    {
                        continue;
                    }

                    GhostGear = ghostGear;
                    break;
                }

                if (GhostGear == null)
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }

                var CaltropsHave = (GhostGear as GhostGearApparel).CaltropsUses;
                var CaltropsMax = (GhostGear as GhostGearApparel).CaltropsMax;

                if (CaltropsMax - CaltropsHave <= 0)
                {
                    EndJobWith(JobCondition.Incompletable);
                    return;
                }

                var actorlabel = actor.LabelShort.CapitalizeFirst() != null
                    ? actor.LabelShort.CapitalizeFirst()
                    : "Someone";
                var gglabel = GhostGear.Label.CapitalizeFirst() != null
                    ? GhostGear.Label.CapitalizeFirst()
                    : "Ghost Gear";
                var def = TargetThingA.def;
                var thinglabel = def?.label.CapitalizeFirst() != null
                    ? TargetThingA.def.label.CapitalizeFirst()
                    : "Caltrops Pod";
                if (TargetThingA.stackCount > CaltropsMax - CaltropsHave)
                {
                    (GhostGear as GhostGearApparel).CaltropsUses = CaltropsMax;
                    TargetThingA.stackCount -= CaltropsMax - CaltropsHave;
                    if (Controller.Settings.ShowAutoRearmMsg)
                    {
                        Messages.Message("GhostGear.FullyRearmed".Translate(actorlabel, gglabel, thinglabel), actor,
                            MessageTypeDefOf.NeutralEvent, false);
                    }

                    EndJobWith(JobCondition.Succeeded);
                    return;
                }

                (GhostGear as GhostGearApparel).CaltropsUses = CaltropsHave + TargetThingA.stackCount;
                if (Controller.Settings.ShowAutoRearmMsg)
                {
                    Messages.Message(
                        "GhostGear.CaltropsRearmed".Translate(actorlabel, gglabel,
                            TargetThingA.stackCount.ToString(), TargetThingA.stackCount > 1 ? "s" : "", thinglabel),
                        actor, MessageTypeDefOf.NeutralEvent, false);
                }

                TargetThingA.Destroy();
                EndJobWith(JobCondition.Succeeded);
            }
        };
    }
}