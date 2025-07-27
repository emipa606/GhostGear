using System.Reflection;
using HarmonyLib;
using Multiplayer.API;
using Verse;

namespace GhostGear;

[StaticConstructorOnStartup]
internal static class MultiplayerSupport
{
    private static readonly Harmony harmony = new("rimworld.pelador.ghostgear.multiplayersupport");

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
        [
            AccessTools.Method(typeof(HaywireData), "TrySetHaywireTicks"),
            AccessTools.Method(typeof(HaywireUtility), "Rnd100"),
            AccessTools.Method(typeof(HaywireUtility), "RndTicks"),
            AccessTools.Method(typeof(HaywireUtility), "RndHWWanderRadius"),
            AccessTools.Method(typeof(HaywireUtility), "GetRandomThing"),
            AccessTools.Method(typeof(HaywireEffect), "RndDmg"),
            AccessTools.Method(typeof(HaywireEffect), "GetCandidate"),
            AccessTools.Method(typeof(GHInjury), "DoGHRelatedInjury"),
            AccessTools.Method(typeof(GHInjury), "SetUpInjVars")
        ];
        foreach (var methodInfo in array)
        {
            FixRNG(methodInfo);
        }
    }

    private static void FixRNG(MethodInfo method)
    {
        harmony.Patch(method, new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPre"),
            new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPos"));
    }

    private static void FixRNGPre()
    {
        Rand.PushState(Find.TickManager.TicksAbs);
    }

    private static void FixRNGPos()
    {
        Rand.PopState();
    }
}