using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GhostGear
{
	// Token: 0x0200001F RID: 31
	public class JobGiver_GGAutoRearm : ThinkNode_JobGiver
	{
		// Token: 0x060000A2 RID: 162 RVA: 0x00005AD8 File Offset: 0x00003CD8
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!Controller.Settings.DoAutoRearm || !pawn.IsColonistPlayerControlled)
			{
				return null;
			}
			if (pawn.InMentalState)
			{
				return null;
			}
			if (((pawn != null) ? pawn.Map : null) == null)
			{
				return null;
			}
			JobDef jobdef = DefDatabase<JobDef>.GetNamed("GGRearmCaltrops", true);
			if (((pawn != null) ? pawn.CurJobDef : null) == jobdef)
			{
				return null;
			}
			Apparel GG = GhostGearUtility.GetWornGG(pawn);
			if (GG != null)
			{
				int podsMax = (GG as GhostGearApparel).CaltropsMax;
				int podsFuel = (GG as GhostGearApparel).CaltropsUses;
				ThingDef podsItem = DefDatabase<ThingDef>.GetNamed("GGCaltropsPod", true);
				if (podsMax > 0 && podsFuel < podsMax && podsFuel * 100 / podsMax < 100)
				{
					Thing targ;
					this.FindBestRearm(pawn, podsItem, podsMax, podsFuel, out targ);
					if (targ != null)
					{
						return new Job(jobdef, targ);
					}
				}
			}
			return null;
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x00005B94 File Offset: 0x00003D94
		internal void FindBestRearm(Pawn p, ThingDef podsItem, int podsMax, int podsFuel, out Thing targ)
		{
			targ = null;
			if (((p != null) ? p.Map : null) != null)
			{
				List<Thing> listpods = (p != null) ? p.Map.listerThings.ThingsOfDef(podsItem) : null;
				int fuelneeded = podsMax - podsFuel;
				if (fuelneeded > podsItem.stackLimit)
				{
					fuelneeded = podsItem.stackLimit;
				}
				if (listpods.Count > 0)
				{
					Thing besttarg = null;
					float bestpoints = 0f;
					for (int i = 0; i < listpods.Count; i++)
					{
						Thing targchk = listpods[i];
						if (!targchk.IsForbidden(p) && (((targchk != null) ? targchk.Faction : null) == null || targchk.Faction.IsPlayer) && p.CanReserveAndReach(targchk, PathEndMode.ClosestTouch, Danger.None, 1, -1, null, false))
						{
							float targpoints;
							if (targchk.stackCount >= fuelneeded)
							{
								targpoints = (float)targchk.stackCount / p.Position.DistanceTo(targchk.Position);
							}
							else
							{
								targpoints = (float)targchk.stackCount / (p.Position.DistanceTo(targchk.Position) * 2f);
							}
							if (targpoints > bestpoints)
							{
								besttarg = targchk;
								bestpoints = targpoints;
							}
						}
					}
					if (besttarg != null)
					{
						targ = besttarg;
					}
				}
			}
		}
	}
}
