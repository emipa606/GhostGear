using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GhostGear;

public class GHInjury
{
    private static readonly BodyPartDef insectHead = DefDatabase<BodyPartDef>.GetNamedSilentFail("InsectHead");
    private static readonly BodyPartDef bodyDef = DefDatabase<BodyPartDef>.GetNamedSilentFail("Body");

    public static void DoGHRelatedInjury(Thing victim, bool headSpace)
    {
        if (victim is not Pawn pawn)
        {
            return;
        }

        SetUpInjVars(pawn, headSpace, out var candidate, out var DamDef, out var dmg);
        if (candidate == null || !(Rand.Range(1f, 100f) <= Controller.Settings.GHInj))
        {
            return;
        }

        var armPen = 0f;
        var dinfo = new DamageInfo(DamDef, dmg, armPen, -1f, null, candidate);
        pawn.TakeDamage(dinfo);
    }

    public static void SetUpInjVars(Pawn Victim, bool headspace, out BodyPartRecord candidate, out DamageDef DamDef,
        out float dmg)
    {
        DamDef = null;
        var Rnd = Rand.Range(1, 100);
        switch (Rnd)
        {
            case < 33:
                DamDef = DamageDefOf.Cut;
                break;
            case >= 33 and < 60:
                DamDef = DamageDefOf.Blunt;
                break;
            case >= 60 and < 80:
                DamDef = DamageDefOf.Stab;
                break;
            default:
                DamDef = DamageDefOf.Stun;
                break;
        }

        dmg = Rand.Range(2f, 7f);
        if (DamDef == DamageDefOf.Stun)
        {
            dmg *= 2f;
        }

        candidate = null;
        var potentials = new List<BodyPartRecord>();
        if (headspace)
        {
            var body = Victim.RaceProps.body;
            if (body?.GetPartsWithDef(BodyPartDefOf.Head) != null)
            {
                potentials.AddRange(Victim.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Head));
            }

            if (body?.GetPartsWithDef(BodyPartDefOf.Arm) != null)
            {
                potentials.AddRange(Victim.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Arm));
            }

            if (body?.GetPartsWithDef(BodyPartDefOf.Torso) != null)
            {
                potentials.AddRange(Victim.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Torso));
            }

            if (body?.GetPartsWithDef(insectHead) != null)
            {
                potentials.AddRange(Victim.RaceProps.body.GetPartsWithDef(insectHead));
            }
        }
        else
        {
            var body = Victim.RaceProps.body;
            if (body?.GetPartsWithDef(BodyPartDefOf.Leg) != null)
            {
                potentials.AddRange(Victim.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Leg));
            }
        }

        if (potentials.Count <= 0)
        {
            var body = Victim.RaceProps.body;
            if (body?.GetPartsWithDef(bodyDef) != null)
            {
                potentials.AddRange(Victim.RaceProps.body.GetPartsWithDef(bodyDef));
            }
        }

        if (potentials.Count > 0)
        {
            candidate = potentials.RandomElement();
        }
    }
}