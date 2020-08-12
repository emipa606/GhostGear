using System;
using UnityEngine;
using Verse;

namespace GhostGear
{
	// Token: 0x02000022 RID: 34
	public class Settings : ModSettings
	{
		// Token: 0x060000AC RID: 172 RVA: 0x00005FA4 File Offset: 0x000041A4
		public void DoWindowContents(Rect canvas)
		{
			float gap = 3f;
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.ColumnWidth = canvas.width;
			listing_Standard.Begin(canvas);
			listing_Standard.Gap(gap);
			checked
			{
				listing_Standard.Label("GhostGear.MinGhostDist".Translate() + "  " + (int)this.MinGhostDist, -1f, null);
				this.MinGhostDist = (float)((int)listing_Standard.Slider((float)((int)this.MinGhostDist), 10f, 20f));
				listing_Standard.Gap(gap);
				listing_Standard.Label("GhostGear.MaxGhostDist".Translate() + "  " + (int)this.MaxGhostDist, -1f, null);
				this.MaxGhostDist = (float)((int)listing_Standard.Slider((float)((int)this.MaxGhostDist), 30f, 50f));
				listing_Standard.Gap(gap);
				listing_Standard.CheckboxLabeled("GhostGear.ShowConfusion".Translate(), ref this.ShowConfusion, null);
				listing_Standard.Gap(unchecked(gap * 2f));
				listing_Standard.Label("GhostGear.HWChance".Translate() + "  " + (int)this.HWChance, -1f, null);
				this.HWChance = (float)((int)listing_Standard.Slider((float)((int)this.HWChance), 25f, 75f));
				listing_Standard.Gap(unchecked(gap * 2f));
				listing_Standard.Label("GhostGear.GHSpeed".Translate() + "  " + (int)this.GHSpeed, -1f, null);
				this.GHSpeed = (float)((int)listing_Standard.Slider((float)((int)this.GHSpeed), 50f, 200f));
				listing_Standard.Gap(gap);
				listing_Standard.CheckboxLabeled("GhostGear.AllowDanger".Translate(), ref this.AllowDanger, null);
				listing_Standard.Gap(gap);
				listing_Standard.CheckboxLabeled("GhostGear.AllowRoofPunch".Translate(), ref this.AllowRoofPunch, null);
				listing_Standard.Gap(gap);
				listing_Standard.Label("GhostGear.GHInj".Translate() + "  " + (int)this.GHInj, -1f, null);
				this.GHInj = (float)((int)listing_Standard.Slider((float)((int)this.GHInj), 0f, 50f));
				listing_Standard.Gap(unchecked(gap * 2f));
				listing_Standard.Label("GhostGear.CaltropDuration".Translate() + "  " + (int)this.CaltropDuration, -1f, null);
				this.CaltropDuration = (float)((int)listing_Standard.Slider((float)((int)this.CaltropDuration), 1f, 12f));
				listing_Standard.Gap(gap);
				listing_Standard.CheckboxLabeled("GhostGear.DoAutoRearm".Translate(), ref this.DoAutoRearm, null);
				listing_Standard.Gap(gap);
				listing_Standard.CheckboxLabeled("GhostGear.ShowAutoRearmMsg".Translate(), ref this.ShowAutoRearmMsg, null);
				listing_Standard.Gap(unchecked(gap * 2f));
				listing_Standard.Label("GhostGear.ResPct".Translate() + "  " + (int)this.ResPct, -1f, null);
				this.ResPct = (float)((int)listing_Standard.Slider((float)((int)this.ResPct), 10f, 200f));
				listing_Standard.Gap(gap);
				Text.Font = GameFont.Tiny;
				listing_Standard.Label("          " + "GhostGear.ResWarn".Translate(), -1f, null);
				listing_Standard.Gap(gap);
				listing_Standard.Label("          " + "GhostGear.ResTip".Translate(), -1f, null);
				Text.Font = GameFont.Small;
				listing_Standard.Gap(gap);
				listing_Standard.End();
			}
		}

		// Token: 0x060000AD RID: 173 RVA: 0x00006394 File Offset: 0x00004594
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.MinGhostDist, "MinGhostDist", 15f, false);
			Scribe_Values.Look<float>(ref this.MaxGhostDist, "MaxGhostDist", 40f, false);
			Scribe_Values.Look<bool>(ref this.ShowConfusion, "ShowConfusion", true, false);
			Scribe_Values.Look<float>(ref this.ResPct, "ResPct", 100f, false);
			Scribe_Values.Look<bool>(ref this.AllowRoofPunch, "AllowRoofPunch", false, false);
			Scribe_Values.Look<bool>(ref this.AllowDanger, "AllowDanger", false, false);
			Scribe_Values.Look<float>(ref this.GHInj, "GHInj", 20f, false);
			Scribe_Values.Look<float>(ref this.HWChance, "HWChance", 50f, false);
			Scribe_Values.Look<float>(ref this.GHSpeed, "GHSpeed", 100f, false);
			Scribe_Values.Look<float>(ref this.CaltropDuration, "CaltropDuration", 6f, false);
			Scribe_Values.Look<bool>(ref this.DoAutoRearm, "DoAutoRearm", false, false);
			Scribe_Values.Look<bool>(ref this.ShowAutoRearmMsg, "ShowAutoRearmMsg", true, false);
		}

		// Token: 0x04000029 RID: 41
		public float MinGhostDist = 15f;

		// Token: 0x0400002A RID: 42
		public float MaxGhostDist = 40f;

		// Token: 0x0400002B RID: 43
		public bool ShowConfusion = true;

		// Token: 0x0400002C RID: 44
		public float ResPct = 100f;

		// Token: 0x0400002D RID: 45
		public bool AllowRoofPunch;

		// Token: 0x0400002E RID: 46
		public bool AllowDanger;

		// Token: 0x0400002F RID: 47
		public float GHInj = 20f;

		// Token: 0x04000030 RID: 48
		public float HWChance = 50f;

		// Token: 0x04000031 RID: 49
		public float GHSpeed = 100f;

		// Token: 0x04000032 RID: 50
		public float CaltropDuration = 6f;

		// Token: 0x04000033 RID: 51
		public bool DoAutoRearm;

		// Token: 0x04000034 RID: 52
		public bool ShowAutoRearmMsg = true;
	}
}
