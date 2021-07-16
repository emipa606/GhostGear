using System.Reflection;
using HarmonyLib;
using Verse;

namespace GhostGear
{
    // Token: 0x02000014 RID: 20
    [StaticConstructorOnStartup]
    internal static class HarmonyPatching
    {
        // Token: 0x06000061 RID: 97 RVA: 0x0000461F File Offset: 0x0000281F
        static HarmonyPatching()
        {
            new Harmony("com.Pelador.Rimworld.GhostGear").PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}