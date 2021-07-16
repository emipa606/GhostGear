using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace GhostGear
{
    // Token: 0x0200001A RID: 26
    public class JobDriver_HWAttackPawn : JobDriver
    {
        // Token: 0x04000026 RID: 38
        private const TargetIndex VictimInd = TargetIndex.A;

        // Token: 0x06000091 RID: 145 RVA: 0x00005944 File Offset: 0x00003B44
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            var pawn1 = pawn;
            var target = job.GetTarget(TargetIndex.A);
            var job1 = job;
            return pawn1.CanReserve(target) && pawn1.Reserve(target, job1, 1, -1, null, errorOnFailed);
        }

        // Token: 0x06000092 RID: 146 RVA: 0x00005989 File Offset: 0x00003B89
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var count = 0;
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => ((Pawn) job.GetTarget(TargetIndex.A).Thing).Dead);
            this.FailOn(() => !HaywireData.IsHaywired(pawn));
            var count1 = count;
            this.FailOn(() => count1 > 4);
            yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);
            var gotoCastPos = Toils_Combat.GotoCastPosition(TargetIndex.A, TargetIndex.None, false, 0.95f);
            yield return gotoCastPos;
            var count2 = count;
            count = count2 + 1;
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
}