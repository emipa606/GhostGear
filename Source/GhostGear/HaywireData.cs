using System;
using System.Linq;
using Verse;

namespace GhostGear
{
    // Token: 0x02000015 RID: 21
    public class HaywireData : ThingComp
    {
        // Token: 0x04000022 RID: 34
        private const int CheckTicks = 120;

        // Token: 0x04000021 RID: 33
        private int HaywireTicks;

        // Token: 0x17000008 RID: 8
        // (get) Token: 0x06000063 RID: 99 RVA: 0x0000464F File Offset: 0x0000284F
        private Pawn pawn => (Pawn) parent;

        // Token: 0x06000062 RID: 98 RVA: 0x00004635 File Offset: 0x00002835
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref HaywireTicks, "HaywireTicks");
        }

        // Token: 0x06000064 RID: 100 RVA: 0x0000465C File Offset: 0x0000285C
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

        // Token: 0x06000065 RID: 101 RVA: 0x000046C8 File Offset: 0x000028C8
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
            if (raceProps?.FleshType.defName != "Mechanoid")
            {
                return false;
            }

            return true;
        }

        // Token: 0x06000066 RID: 102 RVA: 0x00004700 File Offset: 0x00002900
        public static bool IsHaywired(Pawn pawn)
        {
            if (!IsValidForHaywire(pawn))
            {
                return false;
            }

            var hwd = pawn.TryGetComp<HaywireData>();
            if (hwd is {HaywireTicks: > 0})
            {
                return true;
            }

            return false;
        }

        // Token: 0x06000067 RID: 103 RVA: 0x0000472C File Offset: 0x0000292C
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

            HWD.HaywireTicks = (int) Rand.Range(2500f * Math.Min(minHrs, 2f), 2500f * Math.Min(maxHrs, 5f));
            return true;
        }

        // Token: 0x0200002D RID: 45
        public class CompProperties_HaywireData : CompProperties
        {
            // Token: 0x060000DB RID: 219 RVA: 0x00006E54 File Offset: 0x00005054
            public CompProperties_HaywireData()
            {
                compClass = typeof(HaywireData);
            }
        }

        // Token: 0x0200002E RID: 46
        [StaticConstructorOnStartup]
        private static class HaywireData_Setup
        {
            // Token: 0x060000DC RID: 220 RVA: 0x00006E6C File Offset: 0x0000506C
            static HaywireData_Setup()
            {
                HaywireData_Setup_Pawns();
            }

            // Token: 0x060000DD RID: 221 RVA: 0x00006E73 File Offset: 0x00005073
            private static void HaywireData_Setup_Pawns()
            {
                HaywireDataSetup_Comp(typeof(CompProperties_HaywireData), delegate(ThingDef def)
                {
                    var race = def.race;
                    if (race is {IsMechanoid: true})
                    {
                        return true;
                    }

                    var race2 = def.race;
                    return race2?.FleshType.defName == "Mechanoid";
                });
            }

            // Token: 0x060000DE RID: 222 RVA: 0x00006EA4 File Offset: 0x000050A4
            private static void HaywireDataSetup_Comp(Type compType, Func<ThingDef, bool> qualifier)
            {
                var list = DefDatabase<ThingDef>.AllDefsListForReading.Where(qualifier).ToList();
                list.RemoveDuplicates();
                foreach (var def in list)
                {
                    if (def.comps != null && !def.comps.Any(c => c.GetType() == compType))
                    {
                        def.comps.Add((CompProperties) Activator.CreateInstance(compType));
                    }
                }
            }
        }
    }
}