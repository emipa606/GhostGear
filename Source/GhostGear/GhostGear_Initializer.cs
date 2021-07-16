using System;
using System.Collections.Generic;
using Verse;

namespace GhostGear
{
    // Token: 0x02000012 RID: 18
    [StaticConstructorOnStartup]
    internal static class GhostGear_Initializer
    {
        // Token: 0x0600005A RID: 90 RVA: 0x00004481 File Offset: 0x00002681
        static GhostGear_Initializer()
        {
            LongEventHandler.QueueLongEvent(Setup, "LibraryStartup", false, null);
        }

        // Token: 0x0600005B RID: 91 RVA: 0x0000449C File Offset: 0x0000269C
        public static void Setup()
        {
            var allDefs = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
            if (allDefs.Count <= 0)
            {
                return;
            }

            var GGList = GGResearchList();
            foreach (var ResDef in allDefs)
            {
                if (!GGList.Contains(ResDef.defName))
                {
                    continue;
                }

                var Resbase = ResDef.baseCost;
                Resbase = checked((int) Math.Round(Resbase * (Controller.Settings.ResPct / 100f)));
                ResDef.baseCost = Resbase;
            }
        }

        // Token: 0x0600005C RID: 92 RVA: 0x00004534 File Offset: 0x00002734
        public static List<string> GGResearchList()
        {
            var list = new List<string>();
            list.AddDistinct("RimPlas_GhostGear");
            list.AddDistinct("RimPlas_NerveToxin");
            list.AddDistinct("RimPlas_Haywire");
            list.AddDistinct("RimPlas_GGCaltrops");
            return list;
        }
    }
}