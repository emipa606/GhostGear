using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PelShield
{
    // Token: 0x02000005 RID: 5
    [StaticConstructorOnStartup]
    public class PelShieldApparel : Apparel
    {
        // Token: 0x04000009 RID: 9
        public const float MinDrawSize = 1.2f;

        // Token: 0x0400000A RID: 10
        public const float MaxDrawSize = 1.55f;

        // Token: 0x0400000B RID: 11
        public const float MaxDamagedJitterDist = 0.05f;

        // Token: 0x0400000C RID: 12
        public const int JitterDurationTicks = 8;

        // Token: 0x04000012 RID: 18
        public static readonly Material BubbleMat =
            MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);

        // Token: 0x04000011 RID: 17
        public float ApparelScorePerEnergyMax = 0.25f;

        // Token: 0x04000004 RID: 4
        public float energy;

        // Token: 0x0400000F RID: 15
        public float EnergyLossPerDamage = 0.03f;

        // Token: 0x0400000E RID: 14
        public float EnergyOnReset = 0.2f;

        // Token: 0x04000007 RID: 7
        public Vector3 impactAngleVect;

        // Token: 0x04000010 RID: 16
        public int KeepDisplayingTicks = 1000;

        // Token: 0x04000008 RID: 8
        public int lastAbsorbDamageTick = -9999;

        // Token: 0x04000006 RID: 6
        public int lastKeepDisplayTick = -9999;

        // Token: 0x0400000D RID: 13
        public int StartingTicksToReset = 2500;

        // Token: 0x04000005 RID: 5
        public int ticksToReset = -1;

        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000008 RID: 8 RVA: 0x00002152 File Offset: 0x00000352
        public float EnergyMax => this.GetStatValue(StatDefOf.EnergyShieldEnergyMax);

        // Token: 0x17000002 RID: 2
        // (get) Token: 0x06000009 RID: 9 RVA: 0x00002160 File Offset: 0x00000360
        public float EnergyGainPerTick => this.GetStatValue(StatDefOf.EnergyShieldRechargeRate) / 60f;

        // Token: 0x17000003 RID: 3
        // (get) Token: 0x0600000A RID: 10 RVA: 0x00002174 File Offset: 0x00000374
        public float Energy => energy;

        // Token: 0x17000004 RID: 4
        // (get) Token: 0x0600000B RID: 11 RVA: 0x0000217C File Offset: 0x0000037C
        public ShieldState ShieldState
        {
            get
            {
                if (ticksToReset > 0)
                {
                    return ShieldState.Resetting;
                }

                return ShieldState.Active;
            }
        }

        // Token: 0x17000005 RID: 5
        // (get) Token: 0x0600000C RID: 12 RVA: 0x0000218C File Offset: 0x0000038C
        private bool ShouldDisplay
        {
            get
            {
                var wearer = Wearer;
                return wearer.Spawned && !wearer.Dead && !wearer.Downed && (wearer.InAggroMentalState ||
                                                                            wearer.Drafted ||
                                                                            wearer.Faction
                                                                                .HostileTo(Faction.OfPlayer) &&
                                                                            !wearer.IsPrisoner ||
                                                                            Find.TickManager.TicksGame <
                                                                            lastKeepDisplayTick + KeepDisplayingTicks);
            }
        }

        // Token: 0x0600000D RID: 13 RVA: 0x00002208 File Offset: 0x00000408
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref energy, "energy");
            Scribe_Values.Look(ref ticksToReset, "ticksToReset", -1);
            Scribe_Values.Look(ref lastKeepDisplayTick, "lastKeepDisplayTick");
        }

        // Token: 0x0600000E RID: 14 RVA: 0x00002255 File Offset: 0x00000455
        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            if (Find.Selector.SingleSelectedThing == Wearer)
            {
                yield return new Gizmo_EnergyPelShieldStatus
                {
                    shield = this
                };
            }
        }

        // Token: 0x0600000F RID: 15 RVA: 0x00002265 File Offset: 0x00000465
        public override float GetSpecialApparelScoreOffset()
        {
            return EnergyMax * ApparelScorePerEnergyMax;
        }

        // Token: 0x06000010 RID: 16 RVA: 0x00002274 File Offset: 0x00000474
        public override void Tick()
        {
            base.Tick();
            var wearer = Wearer;
            if (wearer == null)
            {
                energy = 0f;
            }
            else if (ShieldState == ShieldState.Resetting)
            {
                ticksToReset--;
                if (ticksToReset <= 0)
                {
                    Reset();
                }
            }
            else if (ShieldState == ShieldState.Active)
            {
                energy += EnergyGainPerTick;
                if (energy > EnergyMax)
                {
                    energy = EnergyMax;
                }
            }

            if (wearer == null || !wearer.IsHashIntervalTick(2500))
            {
                return;
            }

            var regenEnergy = 0.1f;
            if (energy > regenEnergy && !wearer.Drafted && isAutoRepair(this))
            {
                DoAutoRepair(this, regenEnergy);
            }
        }

        // Token: 0x06000011 RID: 17 RVA: 0x00002334 File Offset: 0x00000534
        public void DoAutoRepair(Apparel apparel, float regenEnergy)
        {
            if (!(((PelShieldApparel) apparel).energy > regenEnergy))
            {
                return;
            }

            var regenAmount = 5;
            if (!apparel.def.useHitPoints)
            {
                return;
            }

            var maxhp = apparel.MaxHitPoints;
            var hp = apparel.HitPoints;
            if (hp >= maxhp)
            {
                return;
            }

            if (hp + regenAmount <= maxhp)
            {
                hp += regenAmount;
                apparel.HitPoints = hp;
            }

            ((PelShieldApparel) apparel).energy -= regenEnergy;
        }

        // Token: 0x06000012 RID: 18 RVA: 0x00002394 File Offset: 0x00000594
        public bool isAutoRepair(Apparel apparel)
        {
            if (apparel == null || !apparel.def.useHitPoints)
            {
                return false;
            }

            var apparel2 = apparel.def.apparel;
            if (apparel2 == null || apparel2.tags.Count <= 0)
            {
                return false;
            }

            var apparel3 = apparel.def.apparel;
            var tags = apparel3?.tags;
            if (tags is {Count: <= 0})
            {
                return false;
            }

            if (tags == null)
            {
                return false;
            }

            using var enumerator = tags.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current == "PelRegenFromShield")
                {
                    return true;
                }
            }

            return false;
        }

        // Token: 0x06000013 RID: 19 RVA: 0x00002444 File Offset: 0x00000644
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

        // Token: 0x06000014 RID: 20 RVA: 0x000024FE File Offset: 0x000006FE
        public void KeepDisplaying()
        {
            lastKeepDisplayTick = Find.TickManager.TicksGame;
        }

        // Token: 0x06000015 RID: 21 RVA: 0x00002510 File Offset: 0x00000710
        public void AbsorbedDamage(DamageInfo dinfo)
        {
            var wearer = Wearer;
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(wearer.Position, wearer.Map));
            impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            var loc = wearer.TrueCenter() + (impactAngleVect.RotatedBy(180f) * 0.5f);
            var num = Mathf.Min(10f, 2f + (dinfo.Amount / 10f));
            FleckMaker.Static(loc, wearer.Map, FleckDefOf.ExplosionFlash, num);
            var num2 = (int) num;
            for (var i = 0; i < num2; i++)
            {
                FleckMaker.ThrowDustPuff(loc, wearer.Map, Rand.Range(0.8f, 1.2f));
            }

            lastAbsorbDamageTick = Find.TickManager.TicksGame;
            KeepDisplaying();
        }

        // Token: 0x06000016 RID: 22 RVA: 0x000025F4 File Offset: 0x000007F4
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
        }

        // Token: 0x06000017 RID: 23 RVA: 0x000026B4 File Offset: 0x000008B4
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

        // Token: 0x06000018 RID: 24 RVA: 0x0000271C File Offset: 0x0000091C
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

        // Token: 0x06000019 RID: 25 RVA: 0x000027FF File Offset: 0x000009FF
        public override bool AllowVerbCast(Verb v)
        {
            return true;
        }
    }
}