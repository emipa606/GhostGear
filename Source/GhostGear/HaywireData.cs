using System;
using System.Linq;
using Verse;

namespace GhostGear;

public class HaywireData : ThingComp
{
    private const int CheckTicks = 120;

    private int HaywireTicks;

    private Pawn pawn => (Pawn)parent;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref HaywireTicks, "HaywireTicks");
    }

    public override void CompTick()
    {
        if (!IsHaywired(pawn))
        {
            return;
        }

        HaywireTicks--;
        if ((HaywireTicks + pawn.thingIDNumber) % 120 != 0)
        {
            return;
        }

        var pawn1 = pawn;
        if (pawn1?.Map == null)
        {
            return;
        }

        HaywireEffect.MakeHaywireOverlay(pawn);
        HaywireUtility.TryStartHaywireJob(pawn, 120);
    }

    public static bool IsValidForHaywire(Pawn pawn)
    {
        if (pawn == null)
        {
            return false;
        }

        if (pawn.RaceProps.IsMechanoid)
        {
            return true;
        }

        var raceProps = pawn.RaceProps;
        return raceProps?.FleshType.defName == "Mechanoid";
    }

    public static bool IsHaywired(Pawn pawn)
    {
        if (!IsValidForHaywire(pawn))
        {
            return false;
        }

        var hwd = pawn.TryGetComp<HaywireData>();
        return hwd is { HaywireTicks: > 0 };
    }

    public static bool TrySetHaywireTicks(Pawn pawn, float minHrs, float maxHrs)
    {
        if (!IsValidForHaywire(pawn))
        {
            return false;
        }

        var HWD = pawn.TryGetComp<HaywireData>();
        if (HWD == null)
        {
            return false;
        }

        if (minHrs < 1f)
        {
            minHrs = 1f;
        }

        if (maxHrs < minHrs)
        {
            maxHrs = minHrs;
        }

        HWD.HaywireTicks = (int)Rand.Range(2500f * Math.Min(minHrs, 2f), 2500f * Math.Min(maxHrs, 5f));
        return true;
    }

    public class CompProperties_HaywireData : CompProperties
    {
        public CompProperties_HaywireData()
        {
            compClass = typeof(HaywireData);
        }
    }

    [StaticConstructorOnStartup]
    private static class HaywireData_Setup
    {
        static HaywireData_Setup()
        {
            HaywireData_Setup_Pawns();
        }

        private static void HaywireData_Setup_Pawns()
        {
            HaywireDataSetup_Comp(typeof(CompProperties_HaywireData), delegate(ThingDef def)
            {
                var race = def.race;
                if (race is { IsMechanoid: true })
                {
                    return true;
                }

                var race2 = def.race;
                return race2?.FleshType.defName == "Mechanoid";
            });
        }

        private static void HaywireDataSetup_Comp(Type compType, Func<ThingDef, bool> qualifier)
        {
            var list = DefDatabase<ThingDef>.AllDefsListForReading.Where(qualifier).ToList();
            list.RemoveDuplicates();
            foreach (var def in list)
            {
                if (def.comps != null && !def.comps.Any(c => c.GetType() == compType))
                {
                    def.comps.Add((CompProperties)Activator.CreateInstance(compType));
                }
            }
        }
    }
}