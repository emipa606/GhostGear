using RimWorld;
using Verse;
using Verse.AI;

namespace GhostGear;

public class JobGiver_GGAutoRearm : ThinkNode_JobGiver
{
    protected override Job TryGiveJob(Pawn pawn)
    {
        if (!Controller.Settings.DoAutoRearm || !pawn.IsColonistPlayerControlled)
        {
            return null;
        }

        if (pawn.InMentalState)
        {
            return null;
        }

        if (pawn.Map == null)
        {
            return null;
        }

        var jobdef = DefDatabase<JobDef>.GetNamed("GGRearmCaltrops");
        if (pawn.CurJobDef == jobdef)
        {
            return null;
        }

        var GG = GhostGearUtility.GetWornGG(pawn);
        if (GG == null)
        {
            return null;
        }

        var podsMax = ((GhostGearApparel)GG).CaltropsMax;
        var podsFuel = (GG as GhostGearApparel).CaltropsUses;
        var podsItem = DefDatabase<ThingDef>.GetNamed("GGCaltropsPod");
        if (podsMax <= 0 || podsFuel >= podsMax || podsFuel * 100 / podsMax >= 100)
        {
            return null;
        }

        FindBestRearm(pawn, podsItem, podsMax, podsFuel, out var targ);
        return targ != null ? new Job(jobdef, targ) : null;
    }

    internal void FindBestRearm(Pawn p, ThingDef podsItem, int podsMax, int podsFuel, out Thing targ)
    {
        targ = null;
        if (p?.Map == null)
        {
            return;
        }

        var listpods = p.Map.listerThings.ThingsOfDef(podsItem);
        var fuelneeded = podsMax - podsFuel;
        if (fuelneeded > podsItem.stackLimit)
        {
            fuelneeded = podsItem.stackLimit;
        }

        if (listpods.Count <= 0)
        {
            return;
        }

        Thing besttarg = null;
        var bestpoints = 0f;
        foreach (var targchk in listpods)
        {
            if (targchk.IsForbidden(p) ||
                targchk?.Faction is { IsPlayer: false } ||
                !p.CanReserveAndReach(targchk, PathEndMode.ClosestTouch, Danger.None))
            {
                continue;
            }

            float targpoints = 0;
            if (targchk != null)
            {
                if (targchk.stackCount >= fuelneeded)
                {
                    targpoints = targchk.stackCount / p.Position.DistanceTo(targchk.Position);
                }
                else
                {
                    targpoints = targchk.stackCount / (p.Position.DistanceTo(targchk.Position) * 2f);
                }
            }

            if (!(targpoints > bestpoints))
            {
                continue;
            }

            besttarg = targchk;
            bestpoints = targpoints;
        }

        if (besttarg != null)
        {
            targ = besttarg;
        }
    }
}