using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace PelShield
{
	// Token: 0x02000002 RID: 2
	[StaticConstructorOnStartup]
	public class Gizmo_EnergyPelShieldStatus : Gizmo
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public Gizmo_EnergyPelShieldStatus()
		{
			this.order = -100f;
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002063 File Offset: 0x00000263
		public override float GetWidth(float maxWidth)
		{
			return 140f;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x0000206C File Offset: 0x0000026C
		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
		{
			Rect overRect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
			Find.WindowStack.ImmediateWindow(984688, overRect, WindowLayer.GameUI, delegate
			{
				Rect rect2;
				Rect rect4 = rect2 = overRect.AtZero().ContractedBy(6f);
				rect2.height = overRect.height / 2f;
				Text.Font = GameFont.Tiny;
				Widgets.Label(rect2, this.shield.LabelCap);
				Rect rect3 = rect4;
				rect3.yMin = overRect.height / 2f;
				float fillPercent = this.shield.energy / Mathf.Max(1f, this.shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true));
				Widgets.FillableBar(rect3, fillPercent, Gizmo_EnergyPelShieldStatus.FullShieldBarTex, Gizmo_EnergyPelShieldStatus.EmptyShieldBarTex, false);
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect3, (this.shield.energy * 100f).ToString("F0") + " / " + (this.shield.GetStatValue(StatDefOf.EnergyShieldEnergyMax, true) * 100f).ToString("F0"));
				Text.Anchor = TextAnchor.UpperLeft;
			}, true, false, 1f);
			return new GizmoResult(GizmoState.Clear);
		}

		// Token: 0x04000001 RID: 1
		public PelShieldApparel shield;

		// Token: 0x04000002 RID: 2
		private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

		// Token: 0x04000003 RID: 3
		private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
	}
}
