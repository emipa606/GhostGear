using System.Reflection;
using HarmonyLib;
using Verse;

namespace GhostGear;

[StaticConstructorOnStartup]
internal static class HarmonyPatching
{
    static HarmonyPatching()
    {
        new Harmony("com.Pelador.Rimworld.GhostGear").PatchAll(Assembly.GetExecutingAssembly());
    }
}