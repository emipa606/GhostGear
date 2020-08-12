using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GhostGear
{
	// Token: 0x02000008 RID: 8
	[StaticConstructorOnStartup]
	public static class NonPublicMethods
	{
		// Token: 0x04000002 RID: 2
		public static Action<ShieldBelt> ShieldBelt_Break = (Action<ShieldBelt>)Delegate.CreateDelegate(typeof(Action<ShieldBelt>), null, AccessTools.Method(typeof(ShieldBelt), "Break", null, null));
	}
}
