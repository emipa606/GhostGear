﻿using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace GhostGear
{
	// Token: 0x0200001D RID: 29
	public class JobDriver_HWExplosion : JobDriver
	{
		// Token: 0x06000099 RID: 153 RVA: 0x00005A1A File Offset: 0x00003C1A
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00005A1D File Offset: 0x00003C1D
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil wait = new Toil
			{
				initAction = delegate()
				{
					base.Map.pawnDestinationReservationManager.Reserve(this.pawn, this.job, this.pawn.Position);
					this.pawn.pather.StopDead();
				},
				tickAction = delegate()
				{
					if ((Find.TickManager.TicksGame + this.pawn.thingIDNumber) % 200 == 0)
					{
						this.DoHWEffect(this.pawn);
					}
				}
			};
			this.DecorateWaitToil(wait);
			wait.defaultCompleteMode = ToilCompleteMode.Never;
			yield return wait;
			yield break;
		}

		// Token: 0x0600009B RID: 155 RVA: 0x00005A2D File Offset: 0x00003C2D
		public virtual void DecorateWaitToil(Toil wait)
		{
			wait.AddFailCondition(() => !HaywireData.IsHaywired(this.pawn));
		}

		// Token: 0x0600009C RID: 156 RVA: 0x00005A41 File Offset: 0x00003C41
		public virtual void DoHWEffect(Pawn pawn)
		{
			if (HaywireUtility.Rnd100() < 40)
			{
				HaywireEffect.DoHWExplosion(pawn);
			}
		}
	}
}
