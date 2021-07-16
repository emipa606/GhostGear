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
        // Token: 0x04000024 RID: 36
        private const int TargetSearchInterval = 4;

        // Token: 0x04000025 RID: 37
        private int waitTicks = 1;

        // Token: 0x17000009 RID: 9
        // (get) Token: 0x06000082 RID: 130 RVA: 0x000052F4 File Offset: 0x000034F4
        private int totalWaitTicks
        {
            get
            {
                var ticks = (int) (300f *
                                   (Controller.Settings.GHSpeed / 100f / pawn.GetStatValue(StatDefOf.WorkSpeedGlobal)));
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
            Scribe_Values.Look(ref waitTicks, "waitTicks");
        }

        // Token: 0x06000084 RID: 132 RVA: 0x0000535C File Offset: 0x0000355C
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
            return true;
        }

        // Token: 0x06000085 RID: 133 RVA: 0x00005390 File Offset: 0x00003590
        protected override IEnumerable<Toil> MakeNewToils()
        {
            var actor = GetActor();
            this.FailOn(() => !isWearingHook(actor, job.targetB.Thing));
            this.FailOn(() => actor.Position == job.targetA.Cell);
            this.FailOn(() => actor.Dead || actor.Downed || !actor.Drafted || !actor.Spawned);
            var wait = new Toil
            {
                initAction = delegate
                {
                    Map.pawnDestinationReservationManager.Reserve(actor, job, actor.Position);
                    actor.pather.StopDead();
                    actor.Rotation = GetRotation(actor, job.targetA.Cell);
                    CheckForAutoAttack();
                },
                tickAction = delegate
                {
                    if (job.expiryInterval == -1 && job.def == JobDefOf.Wait_Combat && !pawn.Drafted)
                    {
                        Log.Error(actor + " in eternal WaitCombat without being drafted.");
                        ReadyForNextToil();
                    }
                    else if ((Find.TickManager.TicksGame + actor.thingIDNumber) % 4 == 0)
                    {
                        CheckForAutoAttack();
                    }

                    waitTicks++;
                    if (waitTicks < totalWaitTicks)
                    {
                        actor.Rotation = GetRotation(actor, job.targetA.Cell);
                        return;
                    }

                    Teleport(actor, job.targetA.Cell);
                    EndJobWith(JobCondition.Succeeded);
                }
            };
            DecorateWaitToil(wait);
            wait.handlingFacing = true;
            wait.defaultCompleteMode = ToilCompleteMode.Never;
            yield return wait.WithProgressBar(TargetIndex.A, () => waitTicks / (float) totalWaitTicks)
                .PlaySustainerOrSound(SoundDefOf.Interact_Tend);
        }

        // Token: 0x06000086 RID: 134 RVA: 0x000053A0 File Offset: 0x000035A0
        private bool isWearingHook(Pawn localPawn, Thing hook)
        {
            if (Find.TickManager.TicksGame % 5 != 0)
            {
                return true;
            }

            if (localPawn == null || hook == null)
            {
                return false;
            }

            if (localPawn.apparel.WornApparelCount <= 0)
            {
                return false;
            }

            var list = localPawn.apparel.WornApparel;
            if (list.Count <= 0)
            {
                return false;
            }

            using var enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current == hook)
                {
                    return true;
                }
            }

            return false;
        }

        // Token: 0x06000087 RID: 135 RVA: 0x00005430 File Offset: 0x00003630
        private void Teleport(Pawn localPawn, IntVec3 cell)
        {
            if (localPawn == null || localPawn.Map == null || !localPawn.Spawned ||
                !localPawn.Drafted || localPawn.Dead || localPawn.Downed)
            {
                return;
            }

            var potential = Command_GrappleHook.GetChkCell(localPawn, cell);
            if (potential == cell)
            {
                return;
            }

            var facing = GetRotation(localPawn, potential);
            localPawn.Rotation = facing;
            if (localPawn.Position.Roofed(localPawn.Map))
            {
                GHHitRoof(localPawn.Position, localPawn);
                GHInjury.DoGHRelatedInjury(localPawn, true);
            }

            localPawn.SetPositionDirect(potential);
            if (!potential.Roofed(localPawn.Map))
            {
                return;
            }

            GHHitRoof(potential, localPawn);
            GHInjury.DoGHRelatedInjury(localPawn, false);
        }

        // Token: 0x06000088 RID: 136 RVA: 0x000054E4 File Offset: 0x000036E4
        internal Rot4 GetRotation(Pawn user, IntVec3 destcell)
        {
            var facing = Rot4.North;
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
            var punchsize = new IntVec2(2, 2);
            var cr = GenAdj.OccupiedRect(hrpcell, user.Rotation, punchsize);
            if (!cr.Cells.Any(x => x.Roofed(Map)))
            {
                return;
            }

            var roof = cr.Cells.First(x => x.Roofed(Map)).GetRoof(Map);
            if (!roof.soundPunchThrough.NullOrUndefined())
            {
                roof.soundPunchThrough.PlayOneShot(new TargetInfo(user.Position, Map));
            }

            RoofCollapserImmediate.DropRoofInCells(cr.ExpandedBy(1).ClipInsideMap(Map).Cells.Where(
                delegate(IntVec3 c)
                {
                    if (!c.InBounds(Map))
                    {
                        return false;
                    }

                    if (cr.Contains(c))
                    {
                        return true;
                    }

                    if (c.GetFirstPawn(Map) != null)
                    {
                        return false;
                    }

                    var edifice = c.GetEdifice(Map);
                    return edifice == null || !edifice.def.holdsRoof;
                }), Map);
        }

        // Token: 0x0600008A RID: 138 RVA: 0x00005635 File Offset: 0x00003835
        public override void DecorateWaitToil(Toil wait)
        {
            wait.AddFailCondition(() => GetActor().Position == job.targetA.Cell);
        }

        // Token: 0x0600008B RID: 139 RVA: 0x0000564C File Offset: 0x0000384C
        private void CheckForAutoAttack()
        {
            if (pawn.Downed || pawn.stances.FullBodyBusy)
            {
                return;
            }

            collideWithPawns = false;
            if (!(pawn.story == null || !pawn.WorkTagIsDisabled(WorkTags.Violent)) &&
                !(pawn.RaceProps.ToolUser && pawn.Faction == Faction.OfPlayer &&
                  !pawn.WorkTagIsDisabled(WorkTags.Firefighting)))
            {
                return;
            }

            Fire fire = null;
            for (var i = 0; i < 9; i++)
            {
                var c = pawn.Position + GenAdj.AdjacentCellsAndInside[i];
                if (!c.InBounds(pawn.Map))
                {
                    continue;
                }

                var thingList = c.GetThingList(Map);
                foreach (var thing in thingList)
                {
                    if (pawn.story == null || !pawn.WorkTagIsDisabled(WorkTags.Violent))
                    {
                        if (thing is Pawn {Downed: false} target && pawn.HostileTo(target))
                        {
                            pawn.meleeVerbs.TryMeleeAttack(target);
                            collideWithPawns = true;
                            return;
                        }
                    }

                    if (!pawn.RaceProps.ToolUser || pawn.Faction != Faction.OfPlayer ||
                        pawn.WorkTagIsDisabled(WorkTags.Firefighting))
                    {
                        continue;
                    }

                    if (thing is Fire fire2 && (fire == null || fire2.fireSize < fire.fireSize || i == 8) &&
                        (fire2.parent == null || fire2.parent != pawn))
                    {
                        fire = fire2;
                    }
                }
            }

            if (fire != null && (!pawn.InMentalState || pawn.MentalState.def.allowBeatfire))
            {
                pawn.natives.TryBeatFire(fire);
                return;
            }

            if (!(pawn.story == null || !pawn.WorkTagIsDisabled(WorkTags.Violent)) ||
                pawn.Faction == null || job.def != JobDefOf.Wait_Combat ||
                pawn.drafter is {FireAtWill: false})
            {
                return;
            }

            var currentEffectiveVerb = pawn.CurrentEffectiveVerb;
            if (currentEffectiveVerb == null || currentEffectiveVerb.verbProps.IsMeleeAttack)
            {
                return;
            }

            {
                var threat = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns |
                             TargetScanFlags.NeedThreat;
                if (currentEffectiveVerb.IsIncendiary())
                {
                    threat |= TargetScanFlags.NeedNonBurning;
                }

                var thing = (Thing) AttackTargetFinder.BestShootTargetFromCurrentPosition(pawn, threat);
                if (thing == null)
                {
                    return;
                }

                pawn.TryStartAttack(thing);
                collideWithPawns = true;
            }
        }
    }
}