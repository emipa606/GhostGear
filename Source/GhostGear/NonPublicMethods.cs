using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GhostGear;

[StaticConstructorOnStartup]
public static class NonPublicMethods
{
    public static Action<CompShield> ShieldBelt_Break =
        (Action<CompShield>)Delegate.CreateDelegate(typeof(Action<CompShield>), null,
            AccessTools.Method(typeof(CompShield), "Break"));
}