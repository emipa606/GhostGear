using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace GhostGear
{
    // Token: 0x02000016 RID: 22
    public class HaywireEffect
    {
        // Token: 0x06000069 RID: 105 RVA: 0x0000479C File Offset: 0x0000299C
        public static Mote MakeHaywireOverlay(Thing HaywiredThing)
        {
            if (HaywiredThing.DestroyedOrNull() || HaywiredThing is not Pawn pawn ||
                !pawn.RaceProps.IsMechanoid || pawn.Dead ||
                pawn.Map == null || !pawn.Spawned ||
                !HaywireData.IsHaywired(pawn))
            {
                return null;
            }

            var mote = (Mote) ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("Mote_GGHaywired"));
            mote.Attach(HaywiredThing);
            GenSpawn.Spawn(mote, pawn.Position, pawn.Map);

            return mote;
        }

        // Token: 0x0600006A RID: 106 RVA: 0x00004838 File Offset: 0x00002A38
        public static void DoHWExplosion(Pawn pawn)
        {
            if (pawn == null || pawn.Map == null)
            {
                return;
            }

            SetUpExpVars(pawn, out var radius, out var dmgdef, out var dmg, out var postTD, out var postChance,
                out var postNum, out var preTD,
                out var preChance, out var preNum, out var fireChance);
            GenExplosion.DoExplosion(pawn.Position, pawn.Map, radius, dmgdef, pawn, dmg, -1f, null, null, null,
                null, postTD, postChance, postNum, false, preTD, preChance, preNum, fireChance, true);
            DoHWMiniEffect(pawn);
        }

        // Token: 0x0600006B RID: 107 RVA: 0x000048B0 File Offset: 0x00002AB0
        public static void DoHWBreakDown(Pawn pawn)
        {
            if (pawn == null || !pawn.RaceProps.IsMechanoid || pawn.Map == null)
            {
                return;
            }

            SetUpBDVars(pawn, pawn, out var candidate, out var DamDef, out var dmg);
            if (candidate == null)
            {
                return;
            }

            var dinfo = new DamageInfo(DamDef, dmg, 0f, -1f, pawn, candidate);
            pawn.TakeDamage(dinfo);
            DoHWMiniEffect(pawn);
        }

        // Token: 0x0600006C RID: 108 RVA: 0x00004918 File Offset: 0x00002B18
        public static void DoHWMiniEffect(Pawn pawn)
        {
            if (pawn == null || pawn.Map == null || !pawn.Spawned)
            {
                return;
            }

            for (var i = 0; i < 5; i++)
            {
                FleckMaker.ThrowSmoke(pawn.Position.ToVector3(), pawn.Map, 1f);
                FleckMaker.ThrowMicroSparks(pawn.Position.ToVector3(), pawn.Map);
            }
        }

        // Token: 0x0600006D RID: 109 RVA: 0x00004984 File Offset: 0x00002B84
        public static void SetUpExpVars(Pawn pawn, out float radius, out DamageDef dmgdef, out int dmg,
            out ThingDef postTD, out float postChance, out int postNum, out ThingDef preTD, out float preChance,
            out int preNum, out float fireChance)
        {
            radius = Mathf.Lerp(1.9f, 5.9f, Math.Min(pawn.BodySize, 5f) / 5f);
            dmgdef = DamageDefOf.EMP;
            dmg = 50;
            postTD = null;
            postChance = 0f;
            postNum = 0;
            preTD = null;
            preChance = 0f;
            preNum = 0;
            fireChance = 0f;
            var rnd = HaywireUtility.Rnd100();
            if (rnd < 10)
            {
                dmgdef = DefDatabase<DamageDef>.GetNamed("GGHaywireEMP");
                return;
            }

            if (rnd < 50)
            {
                dmgdef = DamageDefOf.Smoke;
                dmg = 0;
                postTD = ThingDefOf.Gas_Smoke;
                postChance = 1f;
                postNum = 1;
                return;
            }

            if (rnd < 70)
            {
                dmgdef = DamageDefOf.Flame;
                dmg = 20;
                dmg = (int) (dmg * pawn.BodySize);
                postTD = ThingDefOf.Gas_Smoke;
                postChance = 1f;
                postNum = 1;
                preTD = ThingDefOf.Filth_Fuel;
                preChance = 1f;
                preNum = 1;
                fireChance = 0.95f;
                return;
            }

            if (rnd >= 85)
            {
                return;
            }

            dmgdef = DamageDefOf.Bomb;
            dmg = 25;
            dmg = (int) (dmg * pawn.BodySize);
            postTD = ThingDefOf.Gas_Smoke;
            postChance = 1f;
            postNum = 1;
            fireChance = 0.5f;
        }

        // Token: 0x0600006E RID: 110 RVA: 0x00004AB0 File Offset: 0x00002CB0
        public static void SetUpBDVars(Pawn Victim, Thing instigator, out BodyPartRecord candidate,
            out DamageDef DamDef, out float dmg)
        {
            DamDef = null;
            var Rnd = HaywireUtility.Rnd100();
            if (Rnd < 50)
            {
                DamDef = DamageDefOf.Cut;
            }
            else if (Rnd < 87)
            {
                DamDef = DamageDefOf.Blunt;
            }
            else
            {
                DamDef = DamageDefOf.EMP;
            }

            dmg = RndDmg(7f, 15f);
            if (Victim != null)
            {
                var bodyV = Victim.BodySize;
                if (bodyV > 0f)
                {
                    dmg *= bodyV;
                }
            }

            if (DamDef == DamageDefOf.EMP)
            {
                dmg *= 3f;
            }

            candidate = null;
            var potentials = new List<BodyPartRecord>();
            var raceProps = Victim?.RaceProps;
            bool hasParts;
            if (raceProps == null)
            {
                hasParts = false;
            }
            else
            {
                var body = raceProps.body;
                hasParts = body?.AllParts != null;
            }

            if (hasParts)
            {
                potentials.AddRange(Victim.RaceProps.body.AllParts);
            }

            if (potentials.Count > 0)
            {
                candidate = GetCandidate(potentials);
            }
        }

        // Token: 0x0600006F RID: 111 RVA: 0x00004B7F File Offset: 0x00002D7F
        public static float RndDmg(float min, float max)
        {
            return Rand.Range(min, max);
        }

        // Token: 0x06000070 RID: 112 RVA: 0x00004B88 File Offset: 0x00002D88
        public static BodyPartRecord GetCandidate(List<BodyPartRecord> potentials)
        {
            var candidates = new List<BodyPartRecord>();
            foreach (var BPR in potentials)
            {
                if (!BPR.IsCorePart && !BPR.def.defName.EndsWith("CentipedeBodyFirstRing"))
                {
                    candidates.AddDistinct(BPR);
                }
            }

            if (candidates.Count <= 0)
            {
                return null;
            }

            var candidate = candidates.RandomElement();
            if (candidate.IsCorePart)
            {
                candidate = null;
            }

            return candidate;
        }
    }
}