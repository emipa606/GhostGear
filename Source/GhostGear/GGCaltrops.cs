using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace GhostGear
{
	// Token: 0x02000002 RID: 2
	public class GGCaltrops : Filth
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.spawnTick, "spawnTick", 0, false);
		}

		// Token: 0x06000002 RID: 2 RVA: 0x0000206C File Offset: 0x0000026C
		public override void Tick()
		{
			this.spawnTick++;
			if ((float)this.spawnTick >= 2500f * Controller.Settings.CaltropDuration + (float)HaywireUtility.RndTicks(-25, 25))
			{
				this.DoCaltropBurst(null, "TM", this);
				return;
			}
			if (this.spawnTick >= 300 && Find.TickManager.TicksGame % 60 == 0)
			{
				Map TargetMap = base.Map;
				IntVec3 TargetCell = base.Position;
				List<Thing> Pawnlist = TargetCell.GetThingList(TargetMap);
				if (Pawnlist.Count > 0)
				{
					for (int i = 0; i < Pawnlist.Count; i++)
					{
						Pawn pawn;
						Apparel GGItem;
						if ((pawn = (Pawnlist[i] as Pawn)) != null && !GhostGearUtility.IsWearingGhostGear(pawn, out GGItem) && Pawnlist[i].Position == TargetCell)
						{
							if ((Pawnlist[i] as Pawn).RaceProps.IsMechanoid)
							{
								this.DoCaltropBurst(Pawnlist[i] as Pawn, "ME", this);
							}
							else if ((Pawnlist[i] as Pawn).RaceProps.FleshType.defName == "Insectoid")
							{
								this.DoCaltropBurst(Pawnlist[i] as Pawn, "IF", this);
							}
							else
							{
								this.DoCaltropBurst(Pawnlist[i] as Pawn, "OS", this);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000003 RID: 3 RVA: 0x000021D8 File Offset: 0x000003D8
		private void DoCaltropBurst(Pawn p, string mode, Filth caltrop)
		{
			if (caltrop != null && ((caltrop != null) ? caltrop.Map : null) != null)
			{
				IntVec3 pos = caltrop.Position;
				Map map = caltrop.Map;
				float radius = 1.9f;
				DamageDef dmgdef = DamageDefOf.Smoke;
				int dmg = 0;
				ThingDef postTD = ThingDefOf.Gas_Smoke;
				float postChance = 1f;
				int postNum = 1;
				ThingDef preTD = null;
				float preChance = 0f;
				int preNum = 0;
				float fireChance = 0f;
				if (mode != "TM" && p != null && p.Map != null)
				{
					if (!(mode == "ME"))
					{
						if (!(mode == "IF"))
						{
							if (mode == "OS")
							{
								dmgdef = DamageDefOf.Stun;
								dmg = 30;
							}
						}
						else
						{
							dmgdef = DamageDefOf.Flame;
							dmg = 20;
							preTD = ThingDefOf.Filth_Fuel;
							preChance = 1f;
							preNum = 1;
							fireChance = 0.75f;
						}
					}
					else
					{
						dmgdef = DamageDefOf.EMP;
						dmg = 50;
					}
				}
				for (int i = 0; i < 3; i++)
				{
					MoteMaker.ThrowSmoke(pos.ToVector3(), map, 1f);
					MoteMaker.ThrowMicroSparks(pos.ToVector3(), map);
				}
				GenExplosion.DoExplosion(pos, map, radius, dmgdef, p, dmg, -1f, null, null, null, null, postTD, postChance, postNum, false, preTD, preChance, preNum, fireChance, true, null, null);
				this.Destroy(DestroyMode.Vanish);
			}
		}

		// Token: 0x04000001 RID: 1
		private int spawnTick;
	}
}
