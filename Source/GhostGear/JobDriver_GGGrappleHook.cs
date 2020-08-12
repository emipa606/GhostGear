using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace GhostGear
{
	// Token: 0x02000018 RID: 24
	public class JobDriver_GGGrappleHook : JobDriver_Wait
	{
		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000082 RID: 130 RVA: 0x000052F4 File Offset: 0x000034F4
		private int totalWaitTicks
		{
			get
			{
				int ticks = (int)(300f * (Controller.Settings.GHSpeed / 100f / this.pawn.GetStatValue(StatDefOf.WorkSpeedGlobal, true)));
				if (ticks < 60)
				{
					ticks = 60;
				}
				if (ticks > 1500)
				{
					ticks = 1500;
				}
				return ticks;
			}
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00005342 File Offset: 0x00003542
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<int>(ref this.waitTicks, "waitTicks", 0, false);
		}

		// Token: 0x06000084 RID: 132 RVA: 0x0000535C File Offset: 0x0000355C
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			this.pawn.Map.pawnDestinationReservationManager.Reserve(this.pawn, this.job, this.job.targetA.Cell);
			return true;
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00005390 File Offset: 0x00003590
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Pawn actor = base.GetActor();
			this.FailOn(() => !this.isWearingHook(actor, this.job.targetB.Thing));
			this.FailOn(() => actor.Position == this.job.targetA.Cell);
			this.FailOn(() => actor.Dead || actor.Downed || !actor.Drafted || !actor.Spawned);
			Toil wait = new Toil
			{
				initAction = delegate()
				{
					this.Map.pawnDestinationReservationManager.Reserve(actor, this.job, actor.Position);
					actor.pather.StopDead();
					actor.Rotation = this.GetRotation(actor, this.job.targetA.Cell);
					this.CheckForAutoAttack();
				},
				tickAction = delegate()
				{
					if (this.job.expiryInterval == -1 && this.job.def == JobDefOf.Wait_Combat && !this.pawn.Drafted)
					{
						Log.Error(actor + " in eternal WaitCombat without being drafted.", false);
						this.ReadyForNextToil();
					}
					else if ((Find.TickManager.TicksGame + actor.thingIDNumber) % 4 == 0)
					{
						this.CheckForAutoAttack();
					}
					this.waitTicks++;
					if (this.waitTicks < this.totalWaitTicks)
					{
						actor.Rotation = this.GetRotation(actor, this.job.targetA.Cell);
						return;
					}
					this.Teleport(actor, this.job.targetA.Cell);
					this.EndJobWith(JobCondition.Succeeded);
				}
			};
			this.DecorateWaitToil(wait);
			wait.handlingFacing = true;
			wait.defaultCompleteMode = ToilCompleteMode.Never;
			yield return wait.WithProgressBar(TargetIndex.A, () => (float)this.waitTicks / (float)this.totalWaitTicks, false, -0.5f).PlaySustainerOrSound(SoundDefOf.Interact_Tend);
			yield break;
		}

		// Token: 0x06000086 RID: 134 RVA: 0x000053A0 File Offset: 0x000035A0
		private bool isWearingHook(Pawn pawn, Thing hook)
		{
			if (Find.TickManager.TicksGame % 5 != 0)
			{
				return true;
			}
			if (pawn == null || hook == null)
			{
				return false;
			}
			if (pawn.apparel.WornApparelCount <= 0)
			{
				return false;
			}
			List<Apparel> list = pawn.apparel.WornApparel;
			if (list.Count > 0)
			{
				using (List<Apparel>.Enumerator enumerator = list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current == hook)
						{
							return true;
						}
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06000087 RID: 135 RVA: 0x00005430 File Offset: 0x00003630
		private void Teleport(Pawn pawn, IntVec3 cell)
		{
			if (pawn != null && ((pawn != null) ? pawn.Map : null) != null && pawn.Spawned && pawn.Drafted && !pawn.Dead && !pawn.Downed)
			{
				IntVec3 potential = Command_GrappleHook.GetChkCell(pawn, cell);
				if (potential != cell)
				{
					Rot4 facing = this.GetRotation(pawn, potential);
					pawn.Rotation = facing;
					if (pawn.Position.Roofed(pawn.Map))
					{
						this.GHHitRoof(pawn.Position, pawn);
						GHInjury.DoGHRelatedInjury(pawn, true);
					}
					pawn.SetPositionDirect(potential);
					if (potential.Roofed(pawn.Map))
					{
						this.GHHitRoof(potential, pawn);
						GHInjury.DoGHRelatedInjury(pawn, false);
					}
				}
			}
		}

		// Token: 0x06000088 RID: 136 RVA: 0x000054E4 File Offset: 0x000036E4
		internal Rot4 GetRotation(Pawn user, IntVec3 destcell)
		{
			Rot4 facing = Rot4.North;
			if (destcell.x > user.Position.x)
			{
				facing = Rot4.East;
			}
			else if (destcell.x < user.Position.x)
			{
				facing = Rot4.West;
			}
			else if (destcell.z < user.Position.z)
			{
				facing = Rot4.South;
			}
			return facing;
		}

		// Token: 0x06000089 RID: 137 RVA: 0x00005548 File Offset: 0x00003748
		internal void GHHitRoof(IntVec3 hrpcell, Pawn user)
		{
			IntVec2 punchsize = new IntVec2(2, 2);
			CellRect cr = GenAdj.OccupiedRect(hrpcell, user.Rotation, punchsize);
			if (cr.Cells.Any((IntVec3 x) => x.Roofed(this.Map)))
			{
				RoofDef roof = cr.Cells.First((IntVec3 x) => x.Roofed(this.Map)).GetRoof(base.Map);
				if (!roof.soundPunchThrough.NullOrUndefined())
				{
					roof.soundPunchThrough.PlayOneShot(new TargetInfo(user.Position, base.Map, false));
				}
				RoofCollapserImmediate.DropRoofInCells(cr.ExpandedBy(1).ClipInsideMap(base.Map).Cells.Where(delegate(IntVec3 c)
				{
					if (!c.InBounds(this.Map))
					{
						return false;
					}
					if (cr.Contains(c))
					{
						return true;
					}
					if (c.GetFirstPawn(this.Map) != null)
					{
						return false;
					}
					Building edifice = c.GetEdifice(this.Map);
					return edifice == null || !edifice.def.holdsRoof;
				}), base.Map, null);
			}
		}

		// Token: 0x0600008A RID: 138 RVA: 0x00005635 File Offset: 0x00003835
		public override void DecorateWaitToil(Toil wait)
		{
			wait.AddFailCondition(() => base.GetActor().Position == this.job.targetA.Cell);
		}

		// Token: 0x0600008B RID: 139 RVA: 0x0000564C File Offset: 0x0000384C
		private void CheckForAutoAttack()
		{
			if (this.pawn.Downed || this.pawn.stances.FullBodyBusy)
			{
				return;
			}
			this.collideWithPawns = false;
			bool flag = this.pawn.story == null || !this.pawn.WorkTagIsDisabled(WorkTags.Violent);
			bool flag2 = this.pawn.RaceProps.ToolUser && this.pawn.Faction == Faction.OfPlayer && !this.pawn.WorkTagIsDisabled(WorkTags.Firefighting);
			if (!flag && !flag2)
			{
				return;
			}
			Fire fire = null;
			for (int i = 0; i < 9; i++)
			{
				IntVec3 c = this.pawn.Position + GenAdj.AdjacentCellsAndInside[i];
				if (c.InBounds(this.pawn.Map))
				{
					List<Thing> thingList = c.GetThingList(base.Map);
					for (int j = 0; j < thingList.Count; j++)
					{
						if (flag)
						{
							Pawn pawn = thingList[j] as Pawn;
							if (pawn != null && !pawn.Downed && this.pawn.HostileTo(pawn))
							{
								this.pawn.meleeVerbs.TryMeleeAttack(pawn, null, false);
								this.collideWithPawns = true;
								return;
							}
						}
						if (flag2)
						{
							Fire fire2 = thingList[j] as Fire;
							if (fire2 != null && (fire == null || fire2.fireSize < fire.fireSize || i == 8) && (fire2.parent == null || fire2.parent != this.pawn))
							{
								fire = fire2;
							}
						}
					}
				}
			}
			if (fire != null && (!this.pawn.InMentalState || this.pawn.MentalState.def.allowBeatfire))
			{
				this.pawn.natives.TryBeatFire(fire);
				return;
			}
			if (!flag || this.pawn.Faction == null || this.job.def != JobDefOf.Wait_Combat || (this.pawn.drafter != null && !this.pawn.drafter.FireAtWill))
			{
				return;
			}
			Verb currentEffectiveVerb = this.pawn.CurrentEffectiveVerb;
			if (currentEffectiveVerb != null && !currentEffectiveVerb.verbProps.IsMeleeAttack)
			{
				TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedThreat;
				if (currentEffectiveVerb.IsIncendiary())
				{
					targetScanFlags |= TargetScanFlags.NeedNonBurning;
				}
				Thing thing = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(this.pawn, targetScanFlags, null, 0f, 9999f);
				if (thing != null)
				{
					this.pawn.TryStartAttack(thing);
					this.collideWithPawns = true;
				}
			}
		}

		// Token: 0x04000024 RID: 36
		private const int TargetSearchInterval = 4;

		// Token: 0x04000025 RID: 37
		private int waitTicks = 1;
	}
}
