using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace GhostGear;

[StaticConstructorOnStartup]
public class GhostGearApparel : Apparel
{
    public const float MinDrawSize = 1.2f;

    public const float MaxDrawSize = 1.55f;

    public const float MaxDamagedJitterDist = 0.05f;

    public const int JitterDurationTicks = 8;

    public static readonly Material BubbleMat =
        MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent, Color.green);

    [NoTranslate] private readonly string CaltropsIconPath = "Things/Special/GGCaltropsIcon";

    [NoTranslate] private readonly string GrappleHookIconPath = "Things/Special/GGGrappleHookIcon";

    [NoTranslate] private readonly string RepulseIconPath = "Things/Special/GGRepulseIcon";

    public bool ActiveCamo;

    public float ApparelScorePerEnergyMax = 0.25f;

    public int CaltropsMax = 1;

    public int CaltropsUses;

    public float energy;

    public float EnergyLossPerDamage = 0.03f;

    public float EnergyOnReset = 0.2f;

    public Vector3 impactAngleVect;

    public int KeepDisplayingTicks = 1000;

    public int lastAbsorbDamageTick = -9999;

    public int lastKeepDisplayTick = -9999;

    public int StartingTicksToReset = 2500;

    public int ticksToReset = -1;

    public float EnergyMax => this.GetStatValue(StatDefOf.EnergyShieldEnergyMax);

    public float EnergyGainPerTick => this.GetStatValue(StatDefOf.EnergyShieldRechargeRate) / 60f;

    public float Energy => energy;

    public ShieldState ShieldState => ticksToReset > 0 ? ShieldState.Resetting : ShieldState.Active;

    private bool ShouldDisplay =>
        Wearer.Spawned && !Wearer.Dead && !Wearer.Downed && (Wearer.InAggroMentalState ||
                                                             Wearer.Drafted ||
                                                             Wearer.Faction
                                                                 .HostileTo(Faction.OfPlayer) &&
                                                             !Wearer.IsPrisoner ||
                                                             Find.TickManager.TicksGame <
                                                             lastKeepDisplayTick + KeepDisplayingTicks);

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref energy, "energy");
        Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
        Scribe_Values.Look(ref lastKeepDisplayTick, "lastKeepDisplayTick");
        Scribe_Values.Look(ref CaltropsUses, "CaltropsUses");
        Scribe_Values.Look(ref CaltropsMax, "CaltropsMax", 1);
    }

    public override IEnumerable<Gizmo> GetWornGizmos()
    {
        if (!ModLister.HasActiveModWithName("RimPlas") || Find.Selector.SingleSelectedThing != Wearer &&
            Find.Selector.SingleSelectedThing != this)
        {
            yield break;
        }

        if (Find.Selector.SingleSelectedThing == Wearer)
        {
            var wearer = Wearer;
            if (wearer?.Map != null)
            {
                if (Wearer.Drafted)
                {
                    yield return new Command_GrappleHook
                    {
                        defaultLabel = "GhostGear.GrappleHook".Translate(),
                        defaultDesc = "GhostGear.GrappleHookDesc".Translate(def.label.CapitalizeFirst()),
                        icon = ContentFinder<Texture2D>.Get(GrappleHookIconPath),
                        user = Wearer,
                        action = delegate(IntVec3 cell)
                        {
                            SoundDefOf.Click.PlayOneShotOnCamera();
                            UseGrappleHook(Wearer, this, cell);
                        }
                    };
                }

                yield return new Command_Action
                {
                    defaultLabel = "GhostGear.RepulseLabel".Translate(def.label.CapitalizeFirst()),
                    defaultDesc = "GhostGear.RepulseDesc".Translate(def.label.CapitalizeFirst()),
                    icon = ContentFinder<Texture2D>.Get(RepulseIconPath),
                    action = delegate
                    {
                        SoundDefOf.Click.PlayOneShotOnCamera();
                        DoRepulse(Wearer, this);
                    }
                };
                if (ResearchProjectDef.Named("RimPlas_GGCaltrops").IsFinished)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "GhostGear.CaltropsLabel".Translate(def.label.CapitalizeFirst(),
                            CaltropsUses.ToString()),
                        defaultDesc = "GhostGear.CaltropsDesc".Translate(),
                        icon = ContentFinder<Texture2D>.Get(CaltropsIconPath),
                        action = delegate
                        {
                            SoundDefOf.Click.PlayOneShotOnCamera();
                            DoCaltrops(Wearer, this);
                        }
                    };
                }
            }
        }

        yield return new Gizmo_EnergyGGShieldStatus
        {
            shield = this
        };
    }

    public override void Tick()
    {
        base.Tick();
        if (Wearer == null)
        {
            energy = 0f;
            return;
        }

        switch (ShieldState)
        {
            case ShieldState.Resetting:
            {
                ticksToReset--;
                if (ticksToReset <= 0)
                {
                    Reset();
                }

                break;
            }
            case ShieldState.Active:
            {
                if (!ActiveCamo)
                {
                    energy += EnergyGainPerTick;
                }
                else
                {
                    energy -= EnergyGainPerTick / 15f;
                }

                if (energy > EnergyMax)
                {
                    energy = EnergyMax;
                    return;
                }

                if (energy <= 0f)
                {
                    Break();
                }

                break;
            }
        }
    }

    public static void DoCaltrops(Pawn pawn, ThingWithComps GGArmour)
    {
        var list = new List<FloatMenuOption>();
        string text = "GhostGear.DoNothing".Translate();
        list.Add(new FloatMenuOption(text, delegate { GGCaltropsUse(pawn, GGArmour, false, false); },
            MenuOptionPriority.Default, null, null, 29f));
        if (pawn?.Map != null && pawn.Spawned && !pawn.Dead &&
            pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) &&
            ((GhostGearApparel)GGArmour).CaltropsUses > 0)
        {
            text = "GhostGear.DoCaltrops".Translate();
            list.Add(new FloatMenuOption(text, delegate { GGCaltropsUse(pawn, GGArmour, true, false); },
                MenuOptionPriority.Default, null, null, 29f));
        }

        if (pawn?.Map != null && pawn.Spawned && !pawn.Dead &&
            pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) &&
            pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving) &&
            ((GhostGearApparel)GGArmour).CaltropsUses < ((GhostGearApparel)GGArmour).CaltropsMax)
        {
            text = "GhostGear.CaltropsRearm".Translate();
            list.Add(new FloatMenuOption(text, delegate { GGCaltropsUse(pawn, GGArmour, false, true); },
                MenuOptionPriority.Default, null, null, 29f));
        }

        Find.WindowStack.Add(new FloatMenu(list));
    }

    public static void GGCaltropsUse(Pawn pawn, ThingWithComps GGArmour, bool usingCaltrops, bool rearming)
    {
        if (!rearming)
        {
            if (!usingCaltrops)
            {
                return;
            }

            var dmdef = DefDatabase<DamageDef>.GetNamed("Damage_GGCaltrops");
            var postTD = DefDatabase<ThingDef>.GetNamed("Filth_GGCaltrops");
            GenExplosion.DoExplosion(pawn.Position, pawn.Map, 4.9f, dmdef, pawn, 0, -1f, null, null, null, null,
                postTD, 1f, 1, null, false, null, 0f, 0, 0f, true);
            ((GhostGearApparel)GGArmour).CaltropsUses--;
            if (((GhostGearApparel)GGArmour).CaltropsUses < 0)
            {
                ((GhostGearApparel)GGArmour).CaltropsUses = 0;
            }
        }
        else if (GhostGearUtility.GGComposMentis(pawn, GGArmour, out var Reason))
        {
            if (((GhostGearApparel)GGArmour).CaltropsUses >= ((GhostGearApparel)GGArmour).CaltropsMax)
            {
                Messages.Message("GhostGear.IsFullyRearmed".Translate(GGArmour.Label.CapitalizeFirst()), pawn,
                    MessageTypeDefOf.NeutralEvent, false);
                SoundDefOf.ClickReject.PlayOneShotOnCamera();
                return;
            }

            var GGRearm = DefDatabase<JobDef>.GetNamed("GGRearmCaltrops");
            FindBestGGRearm(pawn, GGArmour, out var targ);
            if (targ != null)
            {
                var job = new Job(GGRearm, targ);
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                return;
            }

            Messages.Message("GhostGear.NoCaltropsFound".Translate(GGArmour.Label.CapitalizeFirst()), pawn,
                MessageTypeDefOf.NeutralEvent, false);
            SoundDefOf.ClickReject.PlayOneShotOnCamera();
        }
        else
        {
            Messages.Message("GhostGear.CantDo".Translate(pawn, Reason, GGArmour.Label.CapitalizeFirst()), pawn,
                MessageTypeDefOf.NeutralEvent, false);
            SoundDefOf.ClickReject.PlayOneShotOnCamera();
        }
    }

    internal static void FindBestGGRearm(Pawn pilot, ThingWithComps GGArmour, out Thing targ)
    {
        targ = null;
        if (pilot?.Map == null)
        {
            return;
        }

        var CaltropsPod = DefDatabase<ThingDef>.GetNamed("GGCaltropsPod");
        var listpods = pilot.Map.listerThings.ThingsOfDef(CaltropsPod);
        var fuelneeded = ((GhostGearApparel)GGArmour).CaltropsMax -
                         ((GhostGearApparel)GGArmour).CaltropsUses;
        if (fuelneeded > CaltropsPod.stackLimit)
        {
            fuelneeded = CaltropsPod.stackLimit;
        }

        if (listpods.Count <= 0)
        {
            return;
        }

        Thing besttarg = null;
        var bestpoints = 0f;
        foreach (var targchk in listpods)
        {
            if (targchk.IsForbidden(pilot) ||
                targchk?.Faction != null && !targchk.Faction.IsPlayer ||
                !pilot.CanReserveAndReach(targchk, PathEndMode.ClosestTouch, Danger.None))
            {
                continue;
            }

            float targpoints = 0;
            if (targchk != null)
            {
                if (targchk.stackCount >= fuelneeded)
                {
                    targpoints = targchk.stackCount / pilot.Position.DistanceTo(targchk.Position);
                }
                else
                {
                    targpoints = targchk.stackCount / (pilot.Position.DistanceTo(targchk.Position) * 2f);
                }
            }

            if (!(targpoints > bestpoints))
            {
                continue;
            }

            besttarg = targchk;
            bestpoints = targpoints;
        }

        if (besttarg != null)
        {
            targ = besttarg;
        }
    }

    public static void DoRepulse(Pawn pawn, ThingWithComps GGArmour)
    {
        var list = new List<FloatMenuOption>();
        string text = "GhostGear.DoNothing".Translate();
        list.Add(new FloatMenuOption(text, delegate { GGRepulse(pawn, GGArmour, 0f); }, MenuOptionPriority.Default,
            null, null, 29f));
        if (pawn?.Map != null && pawn.Spawned && !pawn.Dead &&
            pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) &&
            ((GhostGearApparel)GGArmour).energy > 0f)
        {
            text = "GhostGear.DoRepulse".Translate();
            list.Add(new FloatMenuOption(text,
                delegate { GGRepulse(pawn, GGArmour, ((GhostGearApparel)GGArmour).energy); },
                MenuOptionPriority.Default, null, null, 29f));
        }

        Find.WindowStack.Add(new FloatMenu(list));
    }

    public static void GGRepulse(Pawn p, ThingWithComps a, float e)
    {
        if (!(e > 0f))
        {
            return;
        }

        (a as GhostGearApparel)?.Break();
        for (var i = 0; i < 3; i++)
        {
            var radius = 1.9f;
            DamageDef dmdef = null;
            var dmg = RepulseDmg(50, a, e);
            var repulsing = false;
            switch (i)
            {
                case 0:
                    dmdef = DamageDefOf.EMP;
                    radius = 2.9f;
                    break;
                case 1:
                    dmdef = DamageDefOf.Stun;
                    repulsing = true;
                    break;
                case 2:
                    dmdef = DamageDefOf.Smoke;
                    radius = 2.9f;
                    dmg = -1;
                    break;
            }

            if (!repulsing)
            {
                GenExplosion.DoExplosion(p.Position, p.Map, radius, dmdef, p, dmg, -1f, null, null, null, null,
                    null, 1f, 1, GasType.BlindSmoke, false, null, 0f, 0, 0f, true);
            }
            else
            {
                RepulseEffect(p, dmdef, dmg);
            }
        }

        HaywireEffect.DoHWMiniEffect(p);
    }

    public static void RepulseEffect(Pawn p, DamageDef def, int dmg)
    {
        if (p == null || def == null || p.Map == null)
        {
            return;
        }

        var cellList = GenAdj.CellsAdjacent8Way(p).ToList();
        if (cellList.Count <= 0)
        {
            return;
        }

        foreach (var c in cellList)
        {
            var cellThings = c.GetThingList(p.Map);
            if (cellThings.Count <= 0)
            {
                continue;
            }

            foreach (var thing in cellThings)
            {
                if (thing is not Pawn pawn)
                {
                    continue;
                }

                var dinfo = default(DamageInfo);
                dinfo.Def = def;
                dinfo.SetAmount(dmg);
                pawn.TakeDamage(dinfo);
            }
        }
    }

    public static int RepulseDmg(int baseDmg, ThingWithComps a, float e)
    {
        var dmg = 0;
        if (baseDmg <= 0)
        {
            return dmg;
        }

        var eFactor = 0f;
        if (((GhostGearApparel)a).EnergyMax > 0f)
        {
            eFactor = e / ((GhostGearApparel)a).EnergyMax;
        }

        dmg = (int)Mathf.Lerp(0f, baseDmg, eFactor);

        return dmg;
    }

    public static void UseGrappleHook(Pawn Wearer, ThingWithComps thing, IntVec3 cell)
    {
        var GrappleJob = new Job(DefDatabase<JobDef>.GetNamed("GGGrappleHook"), cell, thing);

        if (Wearer.CurJob != null)
        {
            Wearer.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
        }

        GrappleJob.expiryInterval = (int)(300f * (Controller.Settings.GHSpeed / 100f));
        Wearer.jobs.TryTakeOrderedJob(GrappleJob, JobTag.DraftedOrder);
    }

    public override float GetSpecialApparelScoreOffset()
    {
        return EnergyMax * ApparelScorePerEnergyMax;
    }

    public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
    {
        if (ShieldState != ShieldState.Active)
        {
            return false;
        }

        if (dinfo.Def == DamageDefOf.EMP)
        {
            energy = 0f;
            Break();
            return false;
        }

        var haywire = DefDatabase<DamageDef>.GetNamed("GGHaywireEMP", false);
        if (haywire != null && dinfo.Def == haywire)
        {
            energy = 0f;
            Break();
            return false;
        }

        if (!dinfo.Def.isRanged && !dinfo.Def.isExplosive)
        {
            return false;
        }

        energy -= dinfo.Amount * EnergyLossPerDamage;
        if (energy < 0f)
        {
            Break();
        }
        else
        {
            AbsorbedDamage(dinfo);
        }

        return true;
    }

    public void KeepDisplaying()
    {
        lastKeepDisplayTick = Find.TickManager.TicksGame;
    }

    public void AbsorbedDamage(DamageInfo dinfo)
    {
        var wearer = Wearer;
        SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(wearer.Position, wearer.Map));
        impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
        var loc = wearer.TrueCenter() + (impactAngleVect.RotatedBy(180f) * 0.5f);
        var num = Mathf.Min(10f, 2f + (dinfo.Amount / 10f));
        FleckMaker.Static(loc, wearer.Map, FleckDefOf.ExplosionFlash, num);
        var num2 = (int)num;
        for (var i = 0; i < num2; i++)
        {
            FleckMaker.ThrowDustPuff(loc, wearer.Map, Rand.Range(0.8f, 1.2f));
        }

        lastAbsorbDamageTick = Find.TickManager.TicksGame;
        KeepDisplaying();
    }

    public void Break()
    {
        var wearer = Wearer;
        SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(wearer.Position, wearer.Map));
        FleckMaker.Static(wearer.TrueCenter(), wearer.Map, FleckDefOf.ExplosionFlash, 12f);
        for (var i = 0; i < 6; i++)
        {
            FleckMaker.ThrowDustPuff(
                wearer.TrueCenter() + (Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) *
                                       Rand.Range(0.3f, 0.6f)), wearer.Map, Rand.Range(0.8f, 1.2f));
        }

        energy = 0f;
        ticksToReset = StartingTicksToReset;
        ActiveCamo = false;
    }

    public void Reset()
    {
        var wearer = Wearer;
        if (wearer.Spawned)
        {
            SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(wearer.Position, wearer.Map));
            FleckMaker.ThrowLightningGlow(wearer.TrueCenter(), wearer.Map, 3f);
        }

        ticksToReset = -1;
        energy = EnergyOnReset;
    }

    public override void DrawWornExtras()
    {
        if (ShieldState != ShieldState.Active || !ShouldDisplay)
        {
            return;
        }

        var wearer = Wearer;
        var num = Mathf.Lerp(1.2f, 1.55f, energy);
        var vector = wearer.Drawer.DrawPos;
        vector.y = AltitudeLayer.Blueprint.AltitudeFor();
        var num2 = Find.TickManager.TicksGame - lastAbsorbDamageTick;
        if (num2 < 8)
        {
            var num3 = (8 - num2) / 8f * 0.05f;
            vector += impactAngleVect * num3;
            num -= num3;
        }

        float angle = Rand.Range(0, 360);
        var s = new Vector3(num, 1f, num);
        var matrix = default(Matrix4x4);
        matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
        Graphics.DrawMesh(MeshPool.plane10, matrix, BubbleMat, 0);
    }

    public override bool AllowVerbCast(Verb v)
    {
        return true;
    }
}