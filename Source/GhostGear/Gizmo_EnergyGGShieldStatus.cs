using RimWorld;
using UnityEngine;
using Verse;

namespace GhostGear
{
    // Token: 0x02000013 RID: 19
    [StaticConstructorOnStartup]
    public class Gizmo_EnergyGGShieldStatus : Gizmo
    {
        // Token: 0x0400001F RID: 31
        private static readonly Texture2D FullShieldBarTex =
            SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

        // Token: 0x04000020 RID: 32
        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        // Token: 0x0400001E RID: 30
        public GhostGearApparel shield;

        // Token: 0x0600005D RID: 93 RVA: 0x00004567 File Offset: 0x00002767
        public Gizmo_EnergyGGShieldStatus()
        {
            order = -100f;
        }

        // Token: 0x0600005E RID: 94 RVA: 0x0000457A File Offset: 0x0000277A
        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        // Token: 0x0600005F RID: 95 RVA: 0x00004584 File Offset: 0x00002784
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            var overRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Find.WindowStack.ImmediateWindow(984688, overRect, WindowLayer.GameUI, delegate
            {
                Rect rect2;
                var rect4 = rect2 = overRect.AtZero().ContractedBy(6f);
                rect2.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect2, shield.LabelCap);
                var rect3 = rect4;
                rect3.yMin = overRect.height / 2f;
                var fillPercent = shield.energy / Mathf.Max(1f, shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax));
                Widgets.FillableBar(rect3, fillPercent, FullShieldBarTex, EmptyShieldBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect3,
                    (shield.energy * 100f).ToString("F0") + " / " +
                    (shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax) * 100f).ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
            });
            return new GizmoResult(GizmoState.Clear);
        }
    }
}