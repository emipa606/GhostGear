using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GhostGear;

public class JobDriver_GGWaitConfused : JobDriver_Wait
{
    protected override IEnumerable<Toil> MakeNewToils()
    {
        var actor = GetActor();
        this.FailOn(() => actor.Dead || actor.Downed || !actor.Spawned);
        var wait = new Toil
        {
            initAction = delegate
            {
                Map.pawnDestinationReservationManager.Reserve(pawn, job, pawn.Position);
                pawn.pather.StopDead();
            },
            tickAction = delegate
            {
                if (job.expiryInterval != -1 || job.def != JobDefOf.Wait_Combat || pawn.Drafted)
                {
                    return;
                }

                Log.Error($"{pawn} in eternal WaitCombat without being drafted.");
                ReadyForNextToil();
            }
        };
        DecorateWaitToil(wait);
        wait.defaultCompleteMode = ToilCompleteMode.Never;
        yield return wait;
    }

    public override void DecorateWaitToil(Toil wait)
    {
        wait.AddFailCondition(() => pawn.Dead || pawn.Downed || !pawn.Spawned);
    }
}