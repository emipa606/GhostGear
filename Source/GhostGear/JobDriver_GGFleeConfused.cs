using System.Collections.Generic;
using RimWorld;
using Verse.AI;

namespace GhostGear
{
    // Token: 0x02000003 RID: 3
    public class JobDriver_GGFleeConfused : JobDriver_Flee
    {
        // Token: 0x06000005 RID: 5 RVA: 0x00002327 File Offset: 0x00000527
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
}