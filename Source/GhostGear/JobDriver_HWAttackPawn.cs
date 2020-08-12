using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace GhostGear
{
	// Token: 0x0200001A RID: 26
	public class JobDriver_HWAttackPawn : JobDriver
	{
		// Token: 0x06000091 RID: 145 RVA: 0x00005944 File Offset: 0x00003B44
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = this.pawn;
			LocalTargetInfo target = this.job.GetTarget(TargetIndex.A);
			Job job = this.job;
			return pawn.CanReserve(target, 1, -1, null, false) && pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
		}

		// Token: 0x06000092 RID: 146 RVA: 0x00005989 File Offset: 0x00003B89
		protected override IEnumerable<Toil> MakeNewToils()
		{
			int count = 0;
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOn(() => (this.job.GetTarget(TargetIndex.A).Thing as Pawn).Dead);
			this.FailOn(() => !HaywireData.IsHaywired(this.pawn));
			this.FailOn(() => count > 4);
			yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);
			Toil gotoCastPos = Toils_Combat.GotoCastPosition(TargetIndex.A, false, 0.95f);
			yield return gotoCastPos;
			int count2 = count;
			count = count2 + 1;
			if (count > 8)
			{
				base.EndJobWith(JobCondition.Incompletable);
			}
			Toil jumpIfCannotHit = Toils_Jump.JumpIfTargetNotHittable(TargetIndex.A, gotoCastPos);
			yield return jumpIfCannotHit;
			yield return Toils_Combat.CastVerb(TargetIndex.A, true);
			yield return Toils_Jump.Jump(jumpIfCannotHit);
			yield break;
		}

		// Token: 0x04000026 RID: 38
		private const TargetIndex VictimInd = TargetIndex.A;
	}
}
