using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace GhostGear
{
	// Token: 0x0200001B RID: 27
	public class JobDriver_HWAttackThing : JobDriver
	{
		// Token: 0x06000094 RID: 148 RVA: 0x000059A4 File Offset: 0x00003BA4
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = this.pawn;
			LocalTargetInfo target = this.job.GetTarget(TargetIndex.A);
			Job job = this.job;
			return pawn.CanReserve(target, 1, -1, null, false) && pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
		}

		// Token: 0x06000095 RID: 149 RVA: 0x000059E9 File Offset: 0x00003BE9
		protected override IEnumerable<Toil> MakeNewToils()
		{
			int count = 0;
			this.FailOn(() => this.job.GetTarget(TargetIndex.A).Thing.DestroyedOrNull());
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

		// Token: 0x04000027 RID: 39
		private const TargetIndex VictimInd = TargetIndex.A;
	}
}
