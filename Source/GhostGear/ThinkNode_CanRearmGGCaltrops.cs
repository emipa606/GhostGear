using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace GhostGear
{
	// Token: 0x02000023 RID: 35
	public class ThinkNode_CanRearmGGCaltrops : ThinkNode_Conditional
	{
		// Token: 0x060000AF RID: 175 RVA: 0x0000650C File Offset: 0x0000470C
		protected override bool Satisfied(Pawn pawn)
		{
			return Controller.Settings.DoAutoRearm && (pawn.IsColonistPlayerControlled && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving)) && (!pawn.Downed && !pawn.IsBurning() && !pawn.InMentalState && !pawn.Drafted && pawn.Awake()) && !HealthAIUtility.ShouldSeekMedicalRest(pawn);
		}
	}
}
