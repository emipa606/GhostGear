using System.Reflection;
using HarmonyLib;
using Verse;

namespace PelShield
{
    // Token: 0x02000003 RID: 3
    [StaticConstructorOnStartup]
    internal static class HarmonyPatching
    {
        // Token: 0x06000005 RID: 5 RVA: 0x00002107 File Offset: 0x00000307
        static HarmonyPatching()
        {
            new Harmony("com.Pelador.Rimworld.PelShield").PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}