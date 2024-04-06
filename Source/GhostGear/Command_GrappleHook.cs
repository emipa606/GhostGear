using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GhostGear;

public class Command_GrappleHook : Command
{
    [NoTranslate] public static readonly string GrappleIconPath = "Things/Special/GGGrappleHookIcon";

    public Action<IntVec3> action;

    public Pawn user;

    public TargetingParameters targParms => ForGrappleHookDestination(user);


    public static TargetingParameters ForGrappleHookDestination(Pawn user)
    {
        return new TargetingParameters
        {
            canTargetLocations = false,
            canTargetSelf = false,
            canTargetPawns = false,
            canTargetFires = false,
            canTargetBuildings = true,
            canTargetItems = false,
            validator = x => IsGoodGrappleSpot(user, x.Cell, x.Map)
        };
    }

    public override void ProcessInput(Event ev)
    {
        base.ProcessInput(ev);
        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        var GrappleIcon = ContentFinder<Texture2D>.Get(GrappleIconPath);
        Find.Targeter.BeginTargeting(targParms, delegate(LocalTargetInfo target) { action(target.Cell); }, user,
            null, GrappleIcon);
    }

    public override bool InheritInteractionsFrom(Gizmo other)
    {
        return false;
    }

    public static bool IsGoodGrappleSpot(Pawn user, IntVec3 cell, Map map)
    {
        if (user?.Map == null || user.Map != map ||
            !AllowsRoofMove(user, user.Position, map) || !IsHookPointValid(cell, map))
        {
            return false;
        }

        var chkcell = GetChkCell(user, cell);
        if (!chkcell.InBounds(map) || !AllowsRoofMove(user, chkcell, map) || chkcell.Filled(map) ||
            !chkcell.Standable(map))
        {
            return false;
        }

        if (Controller.Settings.AllowDanger)
        {
            return true;
        }

        return chkcell.GetDangerFor(user, map) != Danger.Deadly;
    }

    public static bool AllowsRoofMove(Pawn user, IntVec3 cell, Map map)
    {
        if (!Controller.Settings.AllowRoofPunch)
        {
            if (!cell.Roofed(map))
            {
                return true;
            }
        }
        else
        {
            if (!cell.Roofed(map))
            {
                return true;
            }

            if (!map.roofGrid.RoofAt(cell).isThickRoof)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsHookPointValid(IntVec3 cell, Map map)
    {
        var thingList = cell.GetThingList(map);
        foreach (var thing in thingList)
        {
            if (thing is Building building && building.def.holdsRoof)
            {
                return true;
            }
        }

        return false;
    }

    public static IntVec3 GetChkCell(Pawn user, IntVec3 cell)
    {
        var chkcell = cell;
        if (user.Position.AdjacentToCardinal(cell))
        {
            if (user.Position.x == cell.x)
            {
                if (user.Position.z > cell.z)
                {
                    chkcell.z--;
                }
                else
                {
                    chkcell.z++;
                }
            }
            else if (user.Position.z == cell.z)
            {
                if (user.Position.x > cell.x)
                {
                    chkcell.x--;
                }
                else
                {
                    chkcell.x++;
                }
            }
        }

        if (!user.Position.AdjacentToDiagonal(cell))
        {
            return chkcell;
        }

        if (user.Position.x > cell.x)
        {
            chkcell.x--;
        }
        else
        {
            chkcell.x++;
        }

        if (user.Position.z > cell.z)
        {
            chkcell.z--;
        }
        else
        {
            chkcell.z++;
        }

        return chkcell;
    }
}