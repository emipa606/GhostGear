using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace GhostGear;

public class JobDriver_HWExplosion : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var wait = new Toil
        {
            initAction = delegate
            {
                Map.pawnDestinationReservationManager.Reserve(pawn, job, pawn.Position);
                pawn.pather.StopDead();
            },
            tickAction = delegate
            {
                if ((Find.TickManager.TicksGame + pawn.thingIDNumber) % 200 == 0)
                {
                    DoHWEffect(pawn);
                }
            }
        };
        DecorateWaitToil(wait);
        wait.defaultCompleteMode = ToilCompleteMode.Never;
        yield return wait;
    }

    public virtual void DecorateWaitToil(Toil wait)
    {
        wait.AddFailCondition(() => !HaywireData.IsHaywired(pawn));
    }

    public virtual void DoHWEffect(Pawn pawn)
    {
        if (HaywireUtility.Rnd100() < 40)
        {
            HaywireEffect.DoHWExplosion(pawn);
        }
    }
}