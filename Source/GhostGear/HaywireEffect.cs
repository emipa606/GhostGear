using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace GhostGear;

public class HaywireEffect
{
    public static Mote MakeHaywireOverlay(Thing HaywiredThing)
    {
        if (HaywiredThing.DestroyedOrNull() || HaywiredThing is not Pawn pawn ||
            !pawn.RaceProps.IsMechanoid || pawn.Dead ||
            pawn.Map == null || !pawn.Spawned ||
            !HaywireData.IsHaywired(pawn))
        {
            return null;
        }

        var mote = (Mote)ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("Mote_GGHaywired"));
        mote.Attach(HaywiredThing);
        GenSpawn.Spawn(mote, pawn.Position, pawn.Map);

        return mote;
    }

    public static void DoHWExplosion(Pawn pawn)
    {
        if (pawn?.Map == null)
        {
            return;
        }

        SetUpExpVars(pawn, out var radius, out var dmgdef, out var dmg, out var postChance,
            out var postNum, out var preTD,
            out var preChance, out var preNum, out var fireChance);
        GenExplosion.DoExplosion(pawn.Position, pawn.Map, radius, dmgdef, pawn, dmg, -1f, null, null, null,
            null, null, postChance, postNum, GasType.BlindSmoke, false, preTD, preChance, preNum, fireChance, true);
        DoHWMiniEffect(pawn);
    }

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

    public static void DoHWMiniEffect(Pawn pawn)
    {
        if (pawn is { Map: null, Spawned: true })
        {
            return;
        }

        for (var i = 0; i < 5; i++)
        {
            FleckMaker.ThrowSmoke(pawn.Position.ToVector3(), pawn.Map, 1f);
            FleckMaker.ThrowMicroSparks(pawn.Position.ToVector3(), pawn.Map);
        }
    }

    public static void SetUpExpVars(Pawn pawn, out float radius, out DamageDef dmgdef, out int dmg,
        out float postChance, out int postNum, out ThingDef preTD, out float preChance,
        out int preNum, out float fireChance)
    {
        radius = Mathf.Lerp(1.9f, 5.9f, Math.Min(pawn.BodySize, 5f) / 5f);
        dmgdef = DamageDefOf.EMP;
        dmg = 50;
        postChance = 0f;
        postNum = 0;
        preTD = null;
        preChance = 0f;
        preNum = 0;
        fireChance = 0f;
        var rnd = HaywireUtility.Rnd100();
        switch (rnd)
        {
            case < 10:
                dmgdef = DefDatabase<DamageDef>.GetNamed("GGHaywireEMP");
                return;
            case < 50:
                dmgdef = DamageDefOf.Smoke;
                dmg = 0;
                postChance = 1f;
                postNum = 1;
                return;
            case < 70:
                dmgdef = DamageDefOf.Flame;
                dmg = 20;
                dmg = (int)(dmg * pawn.BodySize);
                postChance = 1f;
                postNum = 1;
                preTD = ThingDefOf.Filth_Fuel;
                preChance = 1f;
                preNum = 1;
                fireChance = 0.95f;
                return;
            case >= 85:
                return;
        }

        dmgdef = DamageDefOf.Bomb;
        dmg = 25;
        dmg = (int)(dmg * pawn.BodySize);
        postChance = 1f;
        postNum = 1;
        fireChance = 0.5f;
    }

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

    public static float RndDmg(float min, float max)
    {
        return Rand.Range(min, max);
    }

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