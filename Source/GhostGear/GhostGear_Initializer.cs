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
			LongEventHandler.QueueLongEvent(new Action(GhostGear_Initializer.Setup), "LibraryStartup", false, null, true);
		}

		// Token: 0x0600005B RID: 91 RVA: 0x0000449C File Offset: 0x0000269C
		public static void Setup()
		{
			List<ResearchProjectDef> allDefs = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
			if (allDefs.Count > 0)
			{
				List<string> GGList = GhostGear_Initializer.GGResearchList();
				foreach (ResearchProjectDef ResDef in allDefs)
				{
					if (GGList.Contains(ResDef.defName))
					{
						float Resbase = ResDef.baseCost;
						Resbase = (float)(checked((int)Math.Round((double)(unchecked(Resbase * (Controller.Settings.ResPct / 100f))))));
						ResDef.baseCost = Resbase;
					}
				}
			}
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00004534 File Offset: 0x00002734
		public static List<string> GGResearchList()
		{
			List<string> list = new List<string>();
			list.AddDistinct("RimPlas_GhostGear");
			list.AddDistinct("RimPlas_NerveToxin");
			list.AddDistinct("RimPlas_Haywire");
			list.AddDistinct("RimPlas_GGCaltrops");
			return list;
		}
	}
}
