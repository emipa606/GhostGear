using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GhostGear
{
	// Token: 0x02000004 RID: 4
	public class JobDriver_GGWaitConfused : JobDriver_Wait
	{
		// Token: 0x06000007 RID: 7 RVA: 0x0000233F File Offset: 0x0000053F
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Pawn actor = base.GetActor();
			this.FailOn(() => actor.Dead || actor.Downed || !actor.Spawned);
			Toil wait = new Toil
			{
				initAction = delegate()
				{
					this.Map.pawnDestinationReservationManager.Reserve(this.pawn, this.job, this.pawn.Position);
					this.pawn.pather.StopDead();
				},
				tickAction = delegate()
				{
					if (this.job.expiryInterval == -1 && this.job.def == JobDefOf.Wait_Combat && !this.pawn.Drafted)
					{
						Log.Error(this.pawn + " in eternal WaitCombat without being drafted.", false);
						this.ReadyForNextToil();
					}
				}
			};
			this.DecorateWaitToil(wait);
			wait.defaultCompleteMode = ToilCompleteMode.Never;
			yield return wait;
			yield break;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x0000234F File Offset: 0x0000054F
		public override void DecorateWaitToil(Toil wait)
		{
			wait.AddFailCondition(() => this.pawn.Dead || this.pawn.Downed || !this.pawn.Spawned);
		}
	}
}
