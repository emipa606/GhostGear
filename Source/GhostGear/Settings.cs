using UnityEngine;
using Verse;

namespace GhostGear
{
    // Token: 0x02000022 RID: 34
    public class Settings : ModSettings
    {
        // Token: 0x0400002E RID: 46
        public bool AllowDanger;

        // Token: 0x0400002D RID: 45
        public bool AllowRoofPunch;

        // Token: 0x04000032 RID: 50
        public float CaltropDuration = 6f;

        // Token: 0x04000033 RID: 51
        public bool DoAutoRearm;

        // Token: 0x0400002F RID: 47
        public float GHInj = 20f;

        // Token: 0x04000031 RID: 49
        public float GHSpeed = 100f;

        // Token: 0x04000030 RID: 48
        public float HWChance = 50f;

        // Token: 0x0400002A RID: 42
        public float MaxGhostDist = 40f;

        // Token: 0x04000029 RID: 41
        public float MinGhostDist = 15f;

        // Token: 0x0400002C RID: 44
        public float ResPct = 100f;

        // Token: 0x04000034 RID: 52
        public bool ShowAutoRearmMsg = true;

        // Token: 0x0400002B RID: 43
        public bool ShowConfusion = true;

        // Token: 0x060000AC RID: 172 RVA: 0x00005FA4 File Offset: 0x000041A4
        public void DoWindowContents(Rect canvas)
        {
            var gap = 3f;
            var listing_Standard = new Listing_Standard {ColumnWidth = canvas.width};
            listing_Standard.Begin(canvas);
            listing_Standard.Gap(gap);
            checked
            {
                listing_Standard.Label("GhostGear.MinGhostDist".Translate() + "  " + (int) MinGhostDist);
                MinGhostDist = (int) listing_Standard.Slider((int) MinGhostDist, 10f, 20f);
                listing_Standard.Gap(gap);
                listing_Standard.Label("GhostGear.MaxGhostDist".Translate() + "  " + (int) MaxGhostDist);
                MaxGhostDist = (int) listing_Standard.Slider((int) MaxGhostDist, 30f, 50f);
                listing_Standard.Gap(gap);
                listing_Standard.CheckboxLabeled("GhostGear.ShowConfusion".Translate(), ref ShowConfusion);
                listing_Standard.Gap(gap * 2f);
                listing_Standard.Label("GhostGear.HWChance".Translate() + "  " + (int) HWChance);
                HWChance = (int) listing_Standard.Slider((int) HWChance, 25f, 75f);
                listing_Standard.Gap(gap * 2f);
                listing_Standard.Label("GhostGear.GHSpeed".Translate() + "  " + (int) GHSpeed);
                GHSpeed = (int) listing_Standard.Slider((int) GHSpeed, 50f, 200f);
                listing_Standard.Gap(gap);
                listing_Standard.CheckboxLabeled("GhostGear.AllowDanger".Translate(), ref AllowDanger);
                listing_Standard.Gap(gap);
                listing_Standard.CheckboxLabeled("GhostGear.AllowRoofPunch".Translate(), ref AllowRoofPunch);
                listing_Standard.Gap(gap);
                listing_Standard.Label("GhostGear.GHInj".Translate() + "  " + (int) GHInj);
                GHInj = (int) listing_Standard.Slider((int) GHInj, 0f, 50f);
                listing_Standard.Gap(gap * 2f);
                listing_Standard.Label("GhostGear.CaltropDuration".Translate() + "  " + (int) CaltropDuration);
                CaltropDuration = (int) listing_Standard.Slider((int) CaltropDuration, 1f, 12f);
                listing_Standard.Gap(gap);
                listing_Standard.CheckboxLabeled("GhostGear.DoAutoRearm".Translate(), ref DoAutoRearm);
                listing_Standard.Gap(gap);
                listing_Standard.CheckboxLabeled("GhostGear.ShowAutoRearmMsg".Translate(), ref ShowAutoRearmMsg);
                listing_Standard.Gap(gap * 2f);
                listing_Standard.Label("GhostGear.ResPct".Translate() + "  " + (int) ResPct);
                ResPct = (int) listing_Standard.Slider((int) ResPct, 10f, 200f);
                listing_Standard.Gap(gap);
                Text.Font = GameFont.Tiny;
                listing_Standard.Label("          " + "GhostGear.ResWarn".Translate());
                listing_Standard.Gap(gap);
                listing_Standard.Label("          " + "GhostGear.ResTip".Translate());
                Text.Font = GameFont.Small;
                listing_Standard.Gap(gap);
                listing_Standard.End();
            }
        }

        // Token: 0x060000AD RID: 173 RVA: 0x00006394 File Offset: 0x00004594
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref MinGhostDist, "MinGhostDist", 15f);
            Scribe_Values.Look(ref MaxGhostDist, "MaxGhostDist", 40f);
            Scribe_Values.Look(ref ShowConfusion, "ShowConfusion", true);
            Scribe_Values.Look(ref ResPct, "ResPct", 100f);
            Scribe_Values.Look(ref AllowRoofPunch, "AllowRoofPunch");
            Scribe_Values.Look(ref AllowDanger, "AllowDanger");
            Scribe_Values.Look(ref GHInj, "GHInj", 20f);
            Scribe_Values.Look(ref HWChance, "HWChance", 50f);
            Scribe_Values.Look(ref GHSpeed, "GHSpeed", 100f);
            Scribe_Values.Look(ref CaltropDuration, "CaltropDuration", 6f);
            Scribe_Values.Look(ref DoAutoRearm, "DoAutoRearm");
            Scribe_Values.Look(ref ShowAutoRearmMsg, "ShowAutoRearmMsg", true);
        }
    }
}