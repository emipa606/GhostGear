using System.Reflection;
using HarmonyLib;
using Verse;

namespace PelShield;

[StaticConstructorOnStartup]
internal static class HarmonyPatching
{
    static HarmonyPatching()
    {
        new Harmony("com.Pelador.Rimworld.PelShield").PatchAll(Assembly.GetExecutingAssembly());
    }
}