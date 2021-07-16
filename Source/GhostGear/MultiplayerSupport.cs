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
        // Token: 0x04000028 RID: 40
        private static readonly Harmony harmony = new Harmony("rimworld.pelador.ghostgear.multiplayersupport");

        // Token: 0x060000A5 RID: 165 RVA: 0x00005CCC File Offset: 0x00003ECC
        static MultiplayerSupport()
        {
            if (!MP.enabled)
            {
                return;
            }

            MP.RegisterSyncMethod(typeof(GhostGearApparel), "GGCaltropsUse");
            MP.RegisterSyncMethod(typeof(GhostGearApparel), "GGRepulse");
            MP.RegisterSyncMethod(typeof(GhostGearApparel), "UseGrappleHook");
            MethodInfo[] array =
            {
                AccessTools.Method(typeof(HaywireData), "TrySetHaywireTicks"),
                AccessTools.Method(typeof(HaywireUtility), "Rnd100"),
                AccessTools.Method(typeof(HaywireUtility), "RndTicks"),
                AccessTools.Method(typeof(HaywireUtility), "RndHWWanderRadius"),
                AccessTools.Method(typeof(HaywireUtility), "GetRandomThing"),
                AccessTools.Method(typeof(HaywireEffect), "RndDmg"),
                AccessTools.Method(typeof(HaywireEffect), "GetCandidate"),
                AccessTools.Method(typeof(GHInjury), "DoGHRelatedInjury"),
                AccessTools.Method(typeof(GHInjury), "SetUpInjVars")
            };
            foreach (var methodInfo in array)
            {
                FixRNG(methodInfo);
            }
        }

        // Token: 0x060000A6 RID: 166 RVA: 0x00005E31 File Offset: 0x00004031
        private static void FixRNG(MethodInfo method)
        {
            harmony.Patch(method, new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPre"),
                new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPos"));
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
    }
}