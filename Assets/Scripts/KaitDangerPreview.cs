using System.Collections.Generic;
using UnityEngine;

public sealed partial class KaitRun
{
    // Current-position warning, not a prediction of the player's next direction.
    // Never spend defenses, advance RNG or mutate enemies while drawing UI.
    public bool IsKateInImminentDanger()
    {
        if (ended || config.playerInvincible || tombArmed) return false;
        bool specter = specterReady, cloak = turnTriggers.Contains("Cloak");
        bool yMelee=Yummn.tranquility, yDefense=Yummn.defense, yArrow=HasPassive(KaitPassive.DeflectMissiles);
        var ordered = new List<KaitEnemy>(enemies);
        ordered.Sort((a,b) => a.id.CompareTo(b.id));
        var forced = enemies.Find(e => e.id == forcedTargetEnemyId && e.life != KaitEnemyLife.Dead);
        foreach (var e in ordered)
        {
            if (e.life != KaitEnemyLife.Active || e.frozenActions > 0 || e == forced) continue;
            if ((IsTwoPhaseRanged(e)||IsYummnLineBoss(e)) && e.rangedState != KaitRangedState.Aim) continue;
            KaitIntent intent = forced != null
                ? (e.type == KaitEnemyType.Archer ? BuildLineIntent(e.pos, DirectionToward(e.pos, forced.pos), config.archerRange, true) : BuildIntentToward(e, forced.pos))
                : (e.type == KaitEnemyType.Archer ? IsYummn?BuildYummnArrow(e.pos,e.intent.direction):BuildArcherFireIntent(e) : e.intent);
            if(IsYummnLineBoss(e)&&e.intent.type!=KaitIntentType.None)intent=BuildYummnBossLine(e.pos,e.intent.direction);
            if (intent == null || intent.type == KaitIntentType.None || intent.type == KaitIntentType.Move) continue;
            if (e.cursed && !e.hexArmorSpent && HasPassive(KaitPassive.HexArmor)) continue;
            if (specter) { specter = false; continue; }
            if (!intent.affectedCells.Contains(katePos)) continue;
            if (IsYummn && intent.damage>0)
            {
                if(intent.type==KaitIntentType.LineShot && yArrow){yArrow=false;continue;}
                if(intent.type==KaitIntentType.Melee && yMelee){yMelee=false;continue;}
                if(yDefense){yDefense=false;continue;}
            }
            if (IsTwoPhaseRanged(e) && HasPassive(KaitPassive.DisplacementCloak) && !cloak) { cloak = true; continue; }
            if (intent.damage > 0) return true;
        }
        if (IsYummn || !config.enableRiftDamage || config.riftBlockDamage <= 0) return false;
        bool redirect = HasPassive(KaitPassive.BloodBookmark) && hasBookmark && !IsHardBlocked(bookmarkCell) && bookmarkCell != katePos && EnemyAt(bookmarkCell) == null;
        foreach (var s in spawns)
        {
            if (s.targetCell != katePos || s.turnsUntilSpawn > 1) continue;
            if (HasPassive(KaitPassive.ArcaneLock) && !s.lockDelayed) continue;
            if (redirect) { redirect = false; continue; }
            return true;
        }
        return false;
    }
}
