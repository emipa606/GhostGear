using RimWorld;
using Verse;
using Verse.AI;

namespace GhostGear;

public class ThinkNode_CanRearmGGCaltrops : ThinkNode_Conditional
{
    protected override bool Satisfied(Pawn pawn)
    {
        return Controller.Settings.DoAutoRearm && pawn.IsColonistPlayerControlled &&
               pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving) && !pawn.Downed && !pawn.IsBurning() &&
               !pawn.InMentalState && !pawn.Drafted && pawn.Awake() && !HealthAIUtility.ShouldSeekMedicalRest(pawn);
    }
}