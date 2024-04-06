using UnityEngine;
using Verse;

namespace GhostGear;

public class Settings : ModSettings
{
    public bool AllowDanger;

    public bool AllowRoofPunch;

    public float CaltropDuration = 6f;

    public bool DoAutoRearm;

    public float GHInj = 20f;

    public float GHSpeed = 100f;

    public float HWChance = 50f;

    public float MaxGhostDist = 40f;

    public float MinGhostDist = 15f;

    public float ResPct = 100f;

    public bool ShowAutoRearmMsg = true;

    public bool ShowConfusion = true;

    public void DoWindowContents(Rect canvas)
    {
        var gap = 3f;
        var listing_Standard = new Listing_Standard { ColumnWidth = canvas.width };
        listing_Standard.Begin(canvas);
        listing_Standard.Gap(gap);
        checked
        {
            listing_Standard.Label("GhostGear.MinGhostDist".Translate() + "  " + (int)MinGhostDist);
            MinGhostDist = (int)listing_Standard.Slider((int)MinGhostDist, 10f, 20f);
            listing_Standard.Gap(gap);
            listing_Standard.Label("GhostGear.MaxGhostDist".Translate() + "  " + (int)MaxGhostDist);
            MaxGhostDist = (int)listing_Standard.Slider((int)MaxGhostDist, 30f, 50f);
            listing_Standard.Gap(gap);
            listing_Standard.CheckboxLabeled("GhostGear.ShowConfusion".Translate(), ref ShowConfusion);
            listing_Standard.Gap(gap * 2f);
            listing_Standard.Label("GhostGear.HWChance".Translate() + "  " + (int)HWChance);
            HWChance = (int)listing_Standard.Slider((int)HWChance, 25f, 75f);
            listing_Standard.Gap(gap * 2f);
            listing_Standard.Label("GhostGear.GHSpeed".Translate() + "  " + (int)GHSpeed);
            GHSpeed = (int)listing_Standard.Slider((int)GHSpeed, 50f, 200f);
            listing_Standard.Gap(gap);
            listing_Standard.CheckboxLabeled("GhostGear.AllowDanger".Translate(), ref AllowDanger);
            listing_Standard.Gap(gap);
            listing_Standard.CheckboxLabeled("GhostGear.AllowRoofPunch".Translate(), ref AllowRoofPunch);
            listing_Standard.Gap(gap);
            listing_Standard.Label("GhostGear.GHInj".Translate() + "  " + (int)GHInj);
            GHInj = (int)listing_Standard.Slider((int)GHInj, 0f, 50f);
            listing_Standard.Gap(gap * 2f);
            listing_Standard.Label("GhostGear.CaltropDuration".Translate() + "  " + (int)CaltropDuration);
            CaltropDuration = (int)listing_Standard.Slider((int)CaltropDuration, 1f, 12f);
            listing_Standard.Gap(gap);
            listing_Standard.CheckboxLabeled("GhostGear.DoAutoRearm".Translate(), ref DoAutoRearm);
            listing_Standard.Gap(gap);
            listing_Standard.CheckboxLabeled("GhostGear.ShowAutoRearmMsg".Translate(), ref ShowAutoRearmMsg);
            listing_Standard.Gap(gap * 2f);
            listing_Standard.Label("GhostGear.ResPct".Translate() + "  " + (int)ResPct);
            ResPct = (int)listing_Standard.Slider((int)ResPct, 10f, 200f);
            listing_Standard.Gap(gap);
            Text.Font = GameFont.Tiny;
            listing_Standard.Label("          " + "GhostGear.ResWarn".Translate());
            listing_Standard.Gap(gap);
            listing_Standard.Label("          " + "GhostGear.ResTip".Translate());
            Text.Font = GameFont.Small;
            if (Controller.currentVersion != null)
            {
                listing_Standard.Gap();
                GUI.contentColor = Color.gray;
                listing_Standard.Label("GhostGear.CurrentModVersion".Translate(Controller.currentVersion));
                GUI.contentColor = Color.white;
            }

            listing_Standard.End();
        }
    }

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