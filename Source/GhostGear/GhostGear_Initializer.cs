using System;
using System.Collections.Generic;
using Verse;

namespace GhostGear;

[StaticConstructorOnStartup]
internal static class GhostGear_Initializer
{
    static GhostGear_Initializer()
    {
        LongEventHandler.QueueLongEvent(Setup, "LibraryStartup", false, null);
    }

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
            Resbase = checked((int)Math.Round(Resbase * (Controller.Settings.ResPct / 100f)));
            ResDef.baseCost = Resbase;
        }
    }

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