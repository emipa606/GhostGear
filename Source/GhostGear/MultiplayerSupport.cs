using System;
using System.Reflection;
using HarmonyLib;
using Multiplayer.API;
using Verse;

namespace GhostGear
{
	// Token: 0x02000020 RID: 32
	[StaticConstructorOnStartup]
	internal static class MultiplayerSupport
	{
		// Token: 0x060000A5 RID: 165 RVA: 0x00005CCC File Offset: 0x00003ECC
		static MultiplayerSupport()
		{
			if (!MP.enabled)
			{
				return;
			}
			MP.RegisterSyncMethod(typeof(GhostGearApparel), "GGCaltropsUse", null);
			MP.RegisterSyncMethod(typeof(GhostGearApparel), "GGRepulse", null);
			MP.RegisterSyncMethod(typeof(GhostGearApparel), "UseGrappleHook", null);
			MethodInfo[] array = new MethodInfo[]
			{
				AccessTools.Method(typeof(HaywireData), "TrySetHaywireTicks", null, null),
				AccessTools.Method(typeof(HaywireUtility), "Rnd100", null, null),
				AccessTools.Method(typeof(HaywireUtility), "RndTicks", null, null),
				AccessTools.Method(typeof(HaywireUtility), "RndHWWanderRadius", null, null),
				AccessTools.Method(typeof(HaywireUtility), "GetRandomThing", null, null),
				AccessTools.Method(typeof(HaywireEffect), "RndDmg", null, null),
				AccessTools.Method(typeof(HaywireEffect), "GetCandidate", null, null),
				AccessTools.Method(typeof(GHInjury), "DoGHRelatedInjury", null, null),
				AccessTools.Method(typeof(GHInjury), "SetUpInjVars", null, null)
			};
			for (int i = 0; i < array.Length; i++)
			{
				MultiplayerSupport.FixRNG(array[i]);
			}
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x00005E31 File Offset: 0x00004031
		private static void FixRNG(MethodInfo method)
		{
			MultiplayerSupport.harmony.Patch(method, new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPre", null), new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPos", null), null, null);
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x00005E6B File Offset: 0x0000406B
		private static void FixRNGPre()
		{
			Rand.PushState(Find.TickManager.TicksAbs);
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x00005E7C File Offset: 0x0000407C
		private static void FixRNGPos()
		{
			Rand.PopState();
		}

		// Token: 0x04000028 RID: 40
		private static Harmony harmony = new Harmony("rimworld.pelador.ghostgear.multiplayersupport");
	}
}
