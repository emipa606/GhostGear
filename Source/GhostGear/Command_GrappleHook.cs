using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GhostGear
{
	// Token: 0x0200000A RID: 10
	public class Command_GrappleHook : Command
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000014 RID: 20 RVA: 0x000024F2 File Offset: 0x000006F2
		public TargetingParameters targParms
		{
			get
			{
				return Command_GrappleHook.ForGrappleHookDestination(this.user);
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002500 File Offset: 0x00000700
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
				validator = delegate(TargetInfo x)
				{
					IntVec3 destcell;
					return Command_GrappleHook.IsGoodGrappleSpot(user, x.Cell, x.Map, out destcell);
				}
			};
		}

		// Token: 0x06000016 RID: 22 RVA: 0x0000255C File Offset: 0x0000075C
		public override void ProcessInput(Event ev)
		{
			base.ProcessInput(ev);
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
			Texture2D GrappleIcon = ContentFinder<Texture2D>.Get(Command_GrappleHook.GrappleIconPath, true);
			Find.Targeter.BeginTargeting(this.targParms, delegate(LocalTargetInfo target)
			{
				this.action(target.Cell);
			}, this.user, null, GrappleIcon);
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000017 RID: 23 RVA: 0x000025AB File Offset: 0x000007AB
		public override Color IconDrawColor
		{
			get
			{
				return base.IconDrawColor;
			}
		}

		// Token: 0x06000018 RID: 24 RVA: 0x000025B3 File Offset: 0x000007B3
		public override bool InheritInteractionsFrom(Gizmo other)
		{
			return false;
		}

		// Token: 0x06000019 RID: 25 RVA: 0x000025B8 File Offset: 0x000007B8
		public static bool IsGoodGrappleSpot(Pawn user, IntVec3 cell, Map map, out IntVec3 destcell)
		{
			destcell = cell;
			if (user != null && ((user != null) ? user.Map : null) != null && user.Map == map && Command_GrappleHook.AllowsRoofMove(user, user.Position, map) && Command_GrappleHook.IsHookPointValid(cell, map))
			{
				IntVec3 chkcell = Command_GrappleHook.GetChkCell(user, cell);
				if (chkcell.InBounds(map) && Command_GrappleHook.AllowsRoofMove(user, chkcell, map) && !chkcell.Filled(map) && chkcell.Standable(map))
				{
					if (Controller.Settings.AllowDanger)
					{
						destcell = chkcell;
						return true;
					}
					if (chkcell.GetDangerFor(user, map) != Danger.Deadly)
					{
						destcell = chkcell;
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002658 File Offset: 0x00000858
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

		// Token: 0x0600001B RID: 27 RVA: 0x00002694 File Offset: 0x00000894
		public static bool IsHookPointValid(IntVec3 cell, Map map)
		{
			List<Thing> thingList = cell.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] is Building && (thingList[i] as Building).def.holdsRoof)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600001C RID: 28 RVA: 0x000026E4 File Offset: 0x000008E4
		public static IntVec3 GetChkCell(Pawn user, IntVec3 cell)
		{
			IntVec3 chkcell = cell;
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
			if (user.Position.AdjacentToDiagonal(cell))
			{
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
			}
			return chkcell;
		}

		// Token: 0x04000003 RID: 3
		public Action<IntVec3> action;

		// Token: 0x04000004 RID: 4
		public Pawn user;

		// Token: 0x04000005 RID: 5
		[NoTranslate]
		public static string GrappleIconPath = "Things/Special/GGGrappleHookIcon";
	}
}
