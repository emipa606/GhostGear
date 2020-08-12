using System;
using UnityEngine;
using Verse;

namespace GhostGear
{
	// Token: 0x0200000B RID: 11
	public class Controller : Mod
	{
		// Token: 0x06000020 RID: 32 RVA: 0x0000281D File Offset: 0x00000A1D
		public override string SettingsCategory()
		{
			return "GhostGear.Name".Translate();
		}

		// Token: 0x06000021 RID: 33 RVA: 0x0000282E File Offset: 0x00000A2E
		public override void DoSettingsWindowContents(Rect canvas)
		{
			Controller.Settings.DoWindowContents(canvas);
		}

		// Token: 0x06000022 RID: 34 RVA: 0x0000283B File Offset: 0x00000A3B
		public Controller(ModContentPack content) : base(content)
		{
			Controller.Settings = base.GetSettings<Settings>();
		}

		// Token: 0x04000006 RID: 6
		public static Settings Settings;
	}
}
