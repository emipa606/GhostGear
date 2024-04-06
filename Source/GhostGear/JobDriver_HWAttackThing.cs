using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace GhostGear;

public class JobDriver_HWAttackThing : JobDriver
{
    private const TargetIndex VictimInd = TargetIndex.A;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        var pawn1 = pawn;
        var target = job.GetTarget(TargetIndex.A);
        var job1 = job;
        return pawn1.CanReserve(target) && pawn1.Reserve(target, job1, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var count = 0;
        this.FailOn(() => job.GetTarget(TargetIndex.A).Thing.DestroyedOrNull());
        this.FailOn(() => !HaywireData.IsHaywired(pawn));
        var count1 = count;
        this.FailOn(() => count1 > 4);
        yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);
        var gotoCastPos = Toils_Combat.GotoCastPosition(TargetIndex.A, TargetIndex.None, false, 0.95f);
        yield return gotoCastPos;
        count += 1;
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (count > 8)
        {
            EndJobWith(JobCondition.Incompletable);
        }

        var jumpIfCannotHit = Toils_Jump.JumpIfTargetNotHittable(TargetIndex.A, gotoCastPos);
        yield return jumpIfCannotHit;
        yield return Toils_Combat.CastVerb(TargetIndex.A);
        yield return Toils_Jump.Jump(jumpIfCannotHit);
    }
}