using System;
using System.Collections.Generic;
using UnityEngine;

public enum KaitDamageKind { PrimarySlash, ResidualSlash, Spell, Collision, FriendlyFire, CurseBurst }

[Serializable]
public sealed class KaitDamageContext
{
    public KaitDamageKind kind;
    public int sourceEnemyId=-1, primaryEnemyId=-1, baseAmount, parentEventId;
    public Vector2Int direction;
    public bool creditKate, canConsumeCurse=true;
}

internal struct KaitDamageOutcome
{
    public int actualDamage, baseApplied;
    public bool wasCursed, killed;
}

internal sealed class KaitPrimaryContactContext
{
    public int enemyId, baseDamage;
    public Vector2Int direction;
    public bool automaticMagicUsed, killRewarded;
}

internal struct KaitRetaliation
{
    public int reactorId, attackerId;
}

public sealed partial class KaitRun
{
    private int nextDamageEventId;
    private KaitPrimaryContactContext primaryContact;
    private readonly HashSet<int> provokedThisEnemyPhase=new HashSet<int>();
    private readonly Queue<KaitRetaliation> retaliationQueue=new Queue<KaitRetaliation>();
    private bool resolvingRetaliations;

    private KaitDamageOutcome DamageEnemyWithContext(KaitEnemy enemy,KaitDamageContext context,KaitTurnResult result)
    {
        var outcome=new KaitDamageOutcome();
        if(enemy==null||enemy.life==KaitEnemyLife.Dead||context==null||context.baseAmount<=0)return outcome;
        bool shielded=(context.kind==KaitDamageKind.PrimarySlash||context.kind==KaitDamageKind.ResidualSlash||context.kind==KaitDamageKind.Spell)&&
            enemy.type==KaitEnemyType.ShieldKnight&&enemy.facing!=Vector2Int.zero&&context.direction!=Vector2Int.zero&&-context.direction==enemy.facing;
        if(shielded)return outcome;
        int before=enemy.hp;
        outcome.wasCursed=context.canConsumeCurse&&enemy.cursed;
        int curseBonus=outcome.wasCursed?1:0;
        int requested=context.baseAmount+curseBonus;
        bool friendly=context.kind==KaitDamageKind.FriendlyFire;
        bool preventedFriendlyLethal=friendly&&HasPassive(KaitPassive.CheshireCat)&&requested>=enemy.hp;
        if(preventedFriendlyLethal)
            requested=Mathf.Max(0,enemy.hp-1);
        if(requested<=0)return outcome;

        enemy.hp=Mathf.Max(0,enemy.hp-requested);
        outcome.actualDamage=before-enemy.hp;
        if(outcome.actualDamage<=0)return outcome;
        outcome.baseApplied=Mathf.Min(context.baseAmount,Mathf.Max(0,outcome.actualDamage-curseBonus));
        if(outcome.wasCursed)enemy.cursed=false;

        if(friendly&&enemy.hp>0&&HasPassive(KaitPassive.Enfeeblement))
        {
            enemy.frozenActions=1;
            TriggerPassive(KaitPassive.Enfeeblement,result,enemy.pos-Vector2Int.one,enemy.pos,"友伤：跳过下一次行动");
        }
        if(preventedFriendlyLethal&&enemy.hp==1)
            TriggerPassive(KaitPassive.CheshireCat,result,enemy.pos-Vector2Int.one,enemy.pos,"敌军友伤被限制为最低 1 点生命");

        if(enemy.hp<=0)
        {
            outcome.killed=true;enemy.life=KaitEnemyLife.Dead;enemy.intent=new KaitIntent{origin=enemy.pos};
            if(context.creditKate){kills++;if(!result.playerKilledEnemyIds.Contains(enemy.id))result.playerKilledEnemyIds.Add(enemy.id);}
            if(!result.killedEnemyIds.Contains(enemy.id)){result.killedEnemyIds.Add(enemy.id);result.killedEnemyCells.Add(enemy.pos);}
            if(outcome.wasCursed)PropagateMasterHex(enemy,result);
            OnAbilityKill(enemy,outcome.wasCursed,context.creditKate,result);
            if(context.creditKate)ReduceAllEquippedCooldowns(result);
            if(enemy.id==bossEnemyId)End("Victory: Shield Knight",true);
        }
        if(outcome.wasCursed)OnCursedDamage(enemy,true,result);
        return outcome;
    }

    private void PropagateMasterHex(KaitEnemy dead,KaitTurnResult result)
    {
        if(!HasPassive(KaitPassive.MasterHex))return;
        bool applied=false;
        foreach(var e in enemies)
            if(e.life!=KaitEnemyLife.Dead&&Mathf.Abs(e.pos.x-dead.pos.x)+Mathf.Abs(e.pos.y-dead.pos.y)==1)
            {e.cursed=true;e.hexArmorSpent=false;applied=true;}
        if(applied)TriggerPassive(KaitPassive.MasterHex,result,dead.pos-Vector2Int.one,dead.pos,"诅咒传播至四邻敌人");
    }

    private void BeginPrimaryContact(KaitEnemy enemy,Vector2Int direction,int baseDamage)
    {
        primaryContact=new KaitPrimaryContactContext{enemyId=enemy.id,direction=direction,baseDamage=baseDamage};
    }

    private void EndPrimaryContact(){primaryContact=null;}

    private void ResolveResidualSlash(Vector2Int origin,Vector2Int direction,int remaining,KaitTurnResult result)
    {
        if(remaining<=0||!HasPassive(KaitPassive.ResidualSlash))return;
        bool triggered=false;
        for(Vector2Int p=origin+direction;Inside(p)&&!IsHardBlocked(p)&&remaining>0;p+=direction)
        {
            var target=EnemyAt(p);if(target==null)continue;
            if(target.type==KaitEnemyType.ShieldKnight&&target.facing!=Vector2Int.zero&&-direction==target.facing)break;
            var outcome=DamageEnemyWithContext(target,new KaitDamageContext{kind=KaitDamageKind.ResidualSlash,baseAmount=remaining,
                creditKate=true,primaryEnemyId=primaryContact?.enemyId??-1,direction=direction,parentEventId=++nextDamageEventId},result);
            remaining=Mathf.Max(0,remaining-outcome.baseApplied);triggered|=outcome.actualDamage>0;
            if(!outcome.killed)break;
        }
        if(triggered)TriggerPassive(KaitPassive.ResidualSlash,result,origin-Vector2Int.one,origin,"余势沿斩击方向继续传递");
    }

    private void ResolvePrimaryAttachedEffects(KaitEnemy enemy,Vector2Int direction,KaitTurnResult result)
    {
        if(enemy.life==KaitEnemyLife.Dead)return;
        bool passiveSmite=HasPassive(KaitPassive.EldritchSmite);
        if(enemy.life!=KaitEnemyLife.Dead&&(passiveSmite||smiteArmed))
        {
            smiteArmed=false;PushToEnd(enemy,direction,result);
            if(passiveSmite)TriggerPassive(KaitPassive.EldritchSmite,result,enemy.pos-Vector2Int.one,enemy.pos,"魔能斩将目标推至尽头");
        }
    }

    private void ApplyHexBladeAfterEffects(Vector2Int hitCell,int primaryEnemyId,KaitTurnResult result)
    {
        if(!HasPassive(KaitPassive.HexBlade))return;
        bool applied=false;
        foreach(var enemy in enemies)
            if(enemy.id!=primaryEnemyId&&enemy.life!=KaitEnemyLife.Dead&&Mathf.Abs(enemy.pos.x-hitCell.x)+Mathf.Abs(enemy.pos.y-hitCell.y)==1)
            {enemy.cursed=true;enemy.hexArmorSpent=false;applied=true;}
        if(applied)TriggerPassive(KaitPassive.HexBlade,result,hitCell-Vector2Int.one,hitCell,"咒刃诅咒目标四邻敌人");
    }

    private static bool HasEffectTag(KaitAbilityDef def,string tag)=>def?.effectTags!=null&&Array.IndexOf(def.effectTags,tag)>=0;

    private void TryAutoEldritchBlast(Vector2Int hitCell,Vector2Int direction,KaitTurnResult result)
    {
        if(primaryContact==null||primaryContact.automaticMagicUsed||!HasPassive(KaitPassive.EldritchBlast)||PassiveCooldown(KaitPassive.EldritchBlast)>0)return;
        KaitEnemy target=null;
        for(Vector2Int p=hitCell;Inside(p)&&!IsHardBlocked(p);p+=direction)
        {
            target=EnemyAt(p);if(target!=null)break;
        }
        if(target==null)return;
        primaryContact.automaticMagicUsed=true;
        ApplyKaitSpellEffect(KaitSkill.EldritchBlast,target,target.pos,direction,result,false);
        passiveCooldowns[KaitPassive.EldritchBlast]=KaitAbilityCatalog.Get(KaitPassive.EldritchBlast).cooldown;
        passivesUsedBeforeInput.Add(KaitPassive.EldritchBlast);
        TriggerPassive(KaitPassive.EldritchBlast,result,target.pos-Vector2Int.one,target.pos,"魔能爆自动发射");
    }

    private bool CanSpellTarget(KaitSkill skill,KaitEnemy target,Vector2Int cell)
    {
        if(skill==KaitSkill.HungerOfHadar)return Inside(cell)&&!IsHardBlocked(cell);
        if(target==null||target.life==KaitEnemyLife.Dead)return false;
        if(skill==KaitSkill.EldritchBlast)
        {
            Vector2Int direction=DirectionToward(katePos,target.pos);
            if(direction==Vector2Int.zero||(target.pos.x!=katePos.x&&target.pos.y!=katePos.y))return false;
            for(Vector2Int p=katePos+direction;Inside(p);p+=direction)
            {
                var first=EnemyAt(p);
                if(first!=null)return first.id==target.id;
                if(IsHardBlocked(p))return false;
            }
            return false;
        }
        return skill==KaitSkill.HexCurse||skill==KaitSkill.IceTomb;
    }

    private void ApplyKaitSpellEffect(KaitSkill skill,KaitEnemy target,Vector2Int cell,Vector2Int direction,KaitTurnResult result,bool copied)
    {
        if(skill==KaitSkill.HexCurse&&target!=null&&target.life!=KaitEnemyLife.Dead){target.cursed=true;target.hexArmorSpent=false;}
        else if(skill==KaitSkill.IceTomb&&target!=null&&target.life!=KaitEnemyLife.Dead)target.frozenActions=1;
        else if(skill==KaitSkill.EldritchBlast&&target!=null&&target.life!=KaitEnemyLife.Dead)
        {
            var d=DirectionToward(katePos,target.pos);if(d==Vector2Int.zero)d=direction;
            var blast=DamageEnemyWithContext(target,new KaitDamageContext{kind=KaitDamageKind.Spell,baseAmount=1,creditKate=true,direction=d,
                primaryEnemyId=primaryContact?.enemyId??-1,parentEventId=++nextDamageEventId},result);
            if(blast.actualDamage>0&&target.life!=KaitEnemyLife.Dead)PushEnemyOneBySpell(target,d,result);
        }
        else if(skill==KaitSkill.HungerOfHadar)
        {
            var victims=new List<KaitEnemy>();
            Vector2Int[] offsets={Vector2Int.zero,Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};
            foreach(var o in offsets){var e=EnemyAt(cell+o);if(e!=null&&!victims.Contains(e))victims.Add(e);}
            foreach(var e in victims)
            {
                var incoming=DirectionToward(cell,e.pos);if(incoming==Vector2Int.zero)incoming=DirectionToward(katePos,e.pos);
                DamageEnemyWithContext(e,new KaitDamageContext{kind=KaitDamageKind.Spell,baseAmount=1,creditKate=true,
                    direction=incoming,primaryEnemyId=primaryContact?.enemyId??-1,parentEventId=++nextDamageEventId},result);
            }
            foreach(var e in victims)if(e.life!=KaitEnemyLife.Dead){e.cursed=true;e.hexArmorSpent=false;}
        }
        if(!copied&&HasPassive(KaitPassive.TwinSigil)&&(HasEffectTag(KaitAbilityCatalog.Get(skill),"RepeatableMagic")||HasEffectTag(KaitAbilityCatalog.Get(skill),"RepeatableSpell")))
        {
            TriggerPassive(KaitPassive.TwinSigil,result,cell-Vector2Int.one,cell,"双生秘印复制法术效果");
            KaitEnemy repeatedTarget=target;
            if(skill==KaitSkill.EldritchBlast)
            {
                repeatedTarget=null;
                for(Vector2Int p=cell;Inside(p)&&!IsHardBlocked(p);p+=direction){repeatedTarget=EnemyAt(p);if(repeatedTarget!=null)break;}
            }
            ApplyKaitSpellEffect(skill,repeatedTarget,cell,direction,result,true);
        }
    }

    private void PushEnemyOneBySpell(KaitEnemy enemy,Vector2Int delta,KaitTurnResult result)
    {
        Vector2Int from=enemy.pos,to=from+delta;
        if(!IsHardBlocked(to)&&to!=katePos&&EnemyAt(to)==null)
        {
            enemy.pos=to;result.pushed=true;result.pushFrom=from;result.pushTo=to;
            result.enemyActions.Add(new KaitEnemyAction{enemyId=enemy.id,type=KaitIntentType.Move,from=from,to=to});
            ResolveMomentumResonance(from,delta,result);return;
        }
        int damage=config.enableCollisionDamage?(IsHardBlocked(to)?config.wallCollisionDamage:config.unitCollisionDamage):0;
        if(damage>0)
        {
            DamageEnemyWithContext(enemy,new KaitDamageContext{kind=KaitDamageKind.Collision,baseAmount=damage,creditKate=true,direction=delta,
                primaryEnemyId=primaryContact?.enemyId??-1,parentEventId=++nextDamageEventId},result);
            ApplyCollisionStagger(enemy,to,result);
        }
    }

    private bool TryShiftEnemyLine(Vector2Int first,Vector2Int delta,KaitTurnResult result)
    {
        var line=new List<KaitEnemy>();Vector2Int p=first;
        while(EnemyAt(p) is KaitEnemy e){line.Add(e);p+=delta;}
        if(line.Count==0||IsHardBlocked(p)||p==katePos)return false;
        for(int i=line.Count-1;i>=0;i--)
        {
            Vector2Int from=line[i].pos;line[i].pos+=delta;
            result.enemyActions.Add(new KaitEnemyAction{enemyId=line[i].id,type=KaitIntentType.Move,from=from,to=line[i].pos});
            ResolveMomentumResonance(from,delta,result);
        }
        result.pushed=true;
        if(line.Count>1)TriggerPassive(KaitPassive.RepellingBlast,result,first-Vector2Int.one,first,"推动传递至整列敌人");
        return true;
    }

    private void ApplyPrimaryStagger(KaitEnemy enemy,Vector2Int impactCell,KaitTurnResult result)
    {
        if(enemy==null||enemy.life==KaitEnemyLife.Dead||!HasPassive(KaitPassive.StaggeringSmite))return;
        enemy.frozenActions=Mathf.Max(enemy.frozenActions,1);
        TriggerPassive(KaitPassive.StaggeringSmite,result,impactCell-Vector2Int.one,enemy.pos,"斩击未击杀：跳过下一次行动");
    }

    private void ApplyCollisionStagger(KaitEnemy enemy,Vector2Int impactCell,KaitTurnResult result){}

    private void ReduceAllEquippedCooldowns(KaitTurnResult result)
    {
        if(!HasPassive(KaitPassive.BladeCovenant))return;
        bool changed=false;
        foreach(var skill in skills)if(SkillCooldown(skill)>0){skillCooldowns[skill]=Mathf.Max(0,SkillCooldown(skill)-1);changed=true;}
        foreach(var passive in passives)if(PassiveCooldown(passive)>0){passiveCooldowns[passive]=Mathf.Max(0,PassiveCooldown(passive)-1);changed=true;}
        if(changed)TriggerPassive(KaitPassive.BladeCovenant,result,katePos-Vector2Int.one,katePos,"击杀：所有技能冷却-1");
    }

    private void ResolvePactKeeperMerge(KaitMergeEvent merge,KaitTurnResult result)
    {
        if(merge==null||!HasPassive(KaitPassive.Devil))return;
        KaitAbilityDef longest=null;int remaining=0;
        for(int i=0;i<EquippedCardCount;i++){var d=EquippedCard(i);int cd=AbilityCooldown(d);if(cd>remaining){longest=d;remaining=cd;}}
        if(longest==null)return;
        if(longest.kind==KaitAbilityKind.Active)skillCooldowns[longest.skill]=remaining-1;else passiveCooldowns[longest.passive]=remaining-1;
        TriggerPassive(KaitPassive.Devil,result,merge.threatCell,MapThreatToBattle(merge.threatCell),longest.nameZh+" 冷却-1");
    }

    private void BeginEnemyReactionPhase()
    {
        provokedThisEnemyPhase.Clear();retaliationQueue.Clear();resolvingRetaliations=false;
    }

    private void QueueProvokedRetaliation(KaitEnemy victim,KaitEnemy attacker,int actualDamage)
    {
        if(actualDamage<=0||!HasPassive(KaitPassive.ProvokingWhispers)||victim==null||attacker==null||
            victim.life==KaitEnemyLife.Dead||victim.frozenActions>0||!provokedThisEnemyPhase.Add(victim.id))return;
        retaliationQueue.Enqueue(new KaitRetaliation{reactorId=victim.id,attackerId=attacker.id});
    }

    private void DrainRetaliationQueue(KaitTurnResult result)
    {
        if(resolvingRetaliations)return;resolvingRetaliations=true;
        try
        {
            while(retaliationQueue.Count>0)
            {
                var reaction=retaliationQueue.Dequeue();
                var reactor=enemies.Find(e=>e.id==reaction.reactorId&&e.life==KaitEnemyLife.Active);
                var target=enemies.Find(e=>e.id==reaction.attackerId&&e.life!=KaitEnemyLife.Dead);
                if(reactor==null||target==null||reactor.frozenActions>0)continue;
                KaitIntent intent;
                if(reactor.type==KaitEnemyType.Archer)intent=BuildLineIntent(reactor.pos,DirectionToward(reactor.pos,target.pos),config.archerRange,true);
                else intent=BuildIntentToward(reactor,target.pos);
                if(intent.type==KaitIntentType.None||!intent.affectedCells.Contains(target.pos))continue;
                var action=new KaitEnemyAction{enemyId=reactor.id,type=intent.type,from=reactor.pos,to=intent.target,damage=intent.damage};
                action.affectedCells.AddRange(intent.affectedCells);
                foreach(var cell in intent.affectedCells)
                {
                    var hit=enemies.Find(e=>e.id!=reactor.id&&e.life!=KaitEnemyLife.Dead&&e.pos==cell);
                    if(hit!=null)
                    {
                        int before=hit.hp;DamageEnemy(hit,intent.damage,false,result,true);int dealt=before-hit.hp;
                        if(dealt>0){action.friendlyHitIds.Add(hit.id);QueueProvokedRetaliation(hit,reactor,dealt);}
                    }
                    if(katePos==cell){DamageKate(intent.damage,result);action.hitKate=true;break;}
                    if(reactor.type==KaitEnemyType.Archer&&hit!=null&&!HasPassive(KaitPassive.PiercingArrow))break;
                }
                result.enemyActions.Add(action);
                TriggerPassive(KaitPassive.ProvokingWhispers,result,reactor.pos-Vector2Int.one,reactor.pos,"友伤目标立即反击");
            }
        }
        finally{resolvingRetaliations=false;}
    }
}
