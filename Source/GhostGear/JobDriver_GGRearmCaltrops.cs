using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GhostGear
{
	// Token: 0x02000019 RID: 25
	public class JobDriver_GGRearmCaltrops : JobDriver
	{
		// Token: 0x0600008E RID: 142 RVA: 0x00005907 File Offset: 0x00003B07
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null, true);
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00005929 File Offset: 0x00003B29
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Pawn actor = base.GetActor();
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			Toil refuel = Toils_General.Wait(180, TargetIndex.None);
			refuel.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			refuel.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			refuel.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return refuel;
			yield return new Toil
			{
				initAction = delegate()
				{
					int CaltropsHave = 0;
					int CaltropsMax = 0;
					if (actor == null || actor.apparel.WornApparelCount <= 0)
					{
						this.EndJobWith(JobCondition.Incompletable);
						return;
					}
					Apparel GhostGear = null;
					List<Apparel> list = actor.apparel.WornApparel;
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i] is GhostGearApparel)
						{
							GhostGear = list[i];
							break;
						}
					}
					if (GhostGear == null)
					{
						this.EndJobWith(JobCondition.Incompletable);
						return;
					}
					if (GhostGear is GhostGearApparel)
					{
						CaltropsHave = (GhostGear as GhostGearApparel).CaltropsUses;
						CaltropsMax = (GhostGear as GhostGearApparel).CaltropsMax;
					}
					if (CaltropsMax - CaltropsHave <= 0)
					{
						this.EndJobWith(JobCondition.Incompletable);
						return;
					}
					Pawn actor2 = actor;
					string actorlabel = (((actor2 != null) ? actor2.LabelShort.CapitalizeFirst() : null) != null) ? actor.LabelShort.CapitalizeFirst() : "Someone";
					string gglabel = (GhostGear != null && GhostGear.Label.CapitalizeFirst() != null) ? GhostGear.Label.CapitalizeFirst() : "Ghost Gear";
					ThingDef def = this.TargetThingA.def;
					string thinglabel = (((def != null) ? def.label.CapitalizeFirst() : null) != null) ? this.TargetThingA.def.label.CapitalizeFirst() : "Caltrops Pod";
					if (this.TargetThingA.stackCount > CaltropsMax - CaltropsHave)
					{
						(GhostGear as GhostGearApparel).CaltropsUses = CaltropsMax;
						this.TargetThingA.stackCount -= CaltropsMax - CaltropsHave;
						if (Controller.Settings.ShowAutoRearmMsg)
						{
							Messages.Message("GhostGear.FullyRearmed".Translate(actorlabel, gglabel, thinglabel), actor, MessageTypeDefOf.NeutralEvent, false);
						}
						this.EndJobWith(JobCondition.Succeeded);
						return;
					}
					(GhostGear as GhostGearApparel).CaltropsUses = CaltropsHave + this.TargetThingA.stackCount;
					if (Controller.Settings.ShowAutoRearmMsg)
					{
						Messages.Message("GhostGear.CaltropsRearmed".Translate(actorlabel, gglabel, this.TargetThingA.stackCount.ToString(), (this.TargetThingA.stackCount > 1) ? "s" : "", thinglabel), actor, MessageTypeDefOf.NeutralEvent, false);
					}
					this.TargetThingA.Destroy(DestroyMode.Vanish);
					this.EndJobWith(JobCondition.Succeeded);
				}
			};
			yield break;
		}
	}
}
