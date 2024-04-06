using Verse;

namespace GhostGear;

public class JobDriver_HWBreakDown : JobDriver_HWExplosion
{
    public override void DoHWEffect(Pawn pawn)
    {
        if (HaywireUtility.Rnd100() < 50)
        {
            HaywireEffect.DoHWBreakDown(pawn);
        }
    }
}