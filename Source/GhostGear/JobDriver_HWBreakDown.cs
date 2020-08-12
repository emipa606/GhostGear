using System;
using Verse;

namespace GhostGear
{
	// Token: 0x0200001C RID: 28
	public class JobDriver_HWBreakDown : JobDriver_HWExplosion
	{
		// Token: 0x06000097 RID: 151 RVA: 0x00005A01 File Offset: 0x00003C01
		public override void DoHWEffect(Pawn pawn)
		{
			if (HaywireUtility.Rnd100() < 50)
			{
				HaywireEffect.DoHWBreakDown(pawn);
			}
		}
	}
}
