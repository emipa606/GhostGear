using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace GhostGear;

public class JobDriver_GGFleeConfused : JobDriver_Flee
{
    protected override IEnumerable<Toil> MakeNewToils()
    {
        var actor = GetActor();
        this.FailOn(() => actor.Dead || actor.Downed || !actor.Spawned);
        yield return new Toil
        {
            atomicWithPrevious = true,
            defaultCompleteMode = ToilCompleteMode.Instant,
            initAction = delegate
            {
                if (pawn.IsColonist)
                {
                    MoteMaker.MakeColonistActionOverlay(pawn, ThingDefOf.Mote_ColonistFleeing);
                }
            }
        };
        yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
    }
}