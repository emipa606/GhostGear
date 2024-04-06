using RimWorld;
using Verse;

namespace GhostGear;

public class GGCaltrops : Filth
{
    private int spawnTick;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref spawnTick, "spawnTick");
    }

    public override void Tick()
    {
        spawnTick++;
        if (spawnTick >= (2500f * Controller.Settings.CaltropDuration) + HaywireUtility.RndTicks(-25, 25))
        {
            DoCaltropBurst(null, "TM", this);
            return;
        }

        if (spawnTick < 300 || Find.TickManager.TicksGame % 60 != 0)
        {
            return;
        }

        var TargetMap = Map;
        var TargetCell = Position;
        var Pawnlist = TargetCell.GetThingList(TargetMap);
        if (Pawnlist.Count <= 0)
        {
            return;
        }

        foreach (var thing in Pawnlist)
        {
            Pawn pawn;
            if ((pawn = thing as Pawn) == null || GhostGearUtility.IsWearingGhostGear(pawn, out _) ||
                thing.Position != TargetCell)
            {
                continue;
            }

            if ((thing as Pawn).RaceProps.IsMechanoid)
            {
                DoCaltropBurst(thing as Pawn, "ME", this);
            }
            else if ((thing as Pawn).RaceProps.FleshType.defName == "Insectoid")
            {
                DoCaltropBurst(thing as Pawn, "IF", this);
            }
            else
            {
                DoCaltropBurst(thing as Pawn, "OS", this);
            }
        }
    }

    private void DoCaltropBurst(Pawn p, string mode, Filth caltrop)
    {
        if (caltrop?.Map == null)
        {
            return;
        }

        var pos = caltrop.Position;
        var map = caltrop.Map;
        var radius = 1.9f;
        var dmgdef = DamageDefOf.Smoke;
        var dmg = 0;
        var postChance = 1f;
        var postNum = 1;
        ThingDef preTD = null;
        var preChance = 0f;
        var preNum = 0;
        var fireChance = 0f;
        if (mode != "TM" && p is { Map: not null })
        {
            switch (mode)
            {
                case "ME":
                    dmgdef = DamageDefOf.EMP;
                    dmg = 50;
                    break;
                case "IF":
                    dmgdef = DamageDefOf.Flame;
                    dmg = 20;
                    preTD = ThingDefOf.Filth_Fuel;
                    preChance = 1f;
                    preNum = 1;
                    fireChance = 0.75f;
                    break;
                case "OS":
                    dmgdef = DamageDefOf.Stun;
                    dmg = 30;
                    break;
            }
        }

        for (var i = 0; i < 3; i++)
        {
            FleckMaker.ThrowSmoke(pos.ToVector3(), map, 1f);
            FleckMaker.ThrowMicroSparks(pos.ToVector3(), map);
        }

        GenExplosion.DoExplosion(pos, map, radius, dmgdef, p, dmg, -1f, null, null, null, null, null,
            postChance, postNum, GasType.BlindSmoke, false, preTD, preChance, preNum, fireChance, true);
        Destroy();
    }
}