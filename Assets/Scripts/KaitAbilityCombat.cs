using System.Collections.Generic;
using UnityEngine;

public sealed partial class KaitRun
{
    public static bool NeedsEnemyTarget(KaitSkill s) => s==KaitSkill.HexCurse || s==KaitSkill.IceTomb || s==KaitSkill.LesserPhantom || s==KaitSkill.Command || s==KaitSkill.GraspHadar || s==KaitSkill.EldritchBlast;
    public static bool NeedsCellTarget(KaitSkill s) => YummnCatalog.IsActive(s) || s==KaitSkill.DispelMagic || s==KaitSkill.MistyStep || s==KaitSkill.RelentlessHex || s==KaitSkill.HungerOfHadar;
    public KaitTurnResult lastSkillResult { get; private set; }
    public bool IsLegalSkillCell(KaitSkill skill,Vector2Int cell)
    {
        if(IsYummn&&YummnCatalog.IsActive(skill))
        {
            if(skill==KaitSkill.MageHand)return cell.x>=0&&cell.y>=0&&cell.x<ThreatSize&&cell.y<ThreatSize&&!IsThreatPillar(cell)&&threat[cell.x,cell.y]>0;
            if(skill==KaitSkill.CommandAct)return EnemyAt(cell)!=null;
            if(skill==KaitSkill.Darkness)return cell.x>=1&&cell.y>=1&&cell.x<BattleSize-1&&cell.y<BattleSize-1;
            if(skill==KaitSkill.ShatterWave)return Inside(cell)&&cell.x>0&&cell.y>0&&cell.x<6&&cell.y<6;
            if(skill==KaitSkill.YummnShadowStep)return Inside(cell)&&YummnEmpty(cell)&&IsYummnShadow(cell);
            if(YummnCatalog.TargetsSelf(skill))return cell==katePos;
            if(YummnCatalog.TargetsGround(skill))return YummnEmpty(cell)&&cell!=PendingBossCell;
            if((cell-katePos).sqrMagnitude!=1)return false;
            if(skill==KaitSkill.ShapeIce||skill==KaitSkill.Darkness||skill==KaitSkill.UniqueDecoy||skill==KaitSkill.PreciseStep)return YummnEmpty(cell)&&cell!=PendingBossCell;
            return true; // Direction selectors remain clickable even at the map edge.
        }
        if(skill==KaitSkill.DispelMagic) return SpawnAt(cell)!=null;
        if(skill==KaitSkill.HungerOfHadar)return Inside(cell)&&!IsHardBlocked(cell);
        if(IsHardBlocked(cell) || cell==katePos || EnemyAt(cell)!=null) return false;
        if(skill==KaitSkill.MistyStep) return Mathf.Abs(cell.x-katePos.x)+Mathf.Abs(cell.y-katePos.y)==1;
        if(skill==KaitSkill.RelentlessHex) return enemies.Exists(e=>e.life!=KaitEnemyLife.Dead && e.cursed && Mathf.Abs(cell.x-e.pos.x)+Mathf.Abs(cell.y-e.pos.y)==1);
        return false;
    }
    public bool TryUseSkillAt(KaitSkill skill,Vector2Int cell,out string message)
    {
        message="目标格不合法";
        if(IsYummn)return CastYummnCell(skill,cell,out message);
        if(ended || !IsSkillActive(skill) || SkillCooldown(skill)>0 || !IsLegalSkillCell(skill,cell)) return false;
        lastSkillResult=new KaitTurnResult { valid=true };
        if(skill==KaitSkill.DispelMagic) spawns.Remove(SpawnAt(cell));
        else if(skill==KaitSkill.HungerOfHadar)ApplyKaitSpellEffect(skill,null,cell,DirectionToward(katePos,cell),lastSkillResult,false);
        else { katePos=cell;lastSkillResult.katePath.Add(cell);lastSkillResult.pathMomentum.Add(momentum); }
        skillCooldowns[skill]=BaseCooldown(skill);skillsUsedBeforeInput.Add(skill);ResolveDevil(skill,lastSkillResult);
        message="已使用："+SkillName(skill);RecordReplay("cellskill",(int)skill,cell.x,cell.y);return true;
    }
    private bool CanPull(KaitEnemy target)
    {
        if(target==null || (target.pos.x!=katePos.x && target.pos.y!=katePos.y)) return false;
        Vector2Int delta=DirectionToward(katePos,target.pos);
        if(delta==Vector2Int.zero || katePos+delta==target.pos) return false;
        for(Vector2Int p=katePos+delta;p!=target.pos;p+=delta) if(IsHardBlocked(p)||EnemyAt(p)!=null) return false;
        return true;
    }
    private void PullEnemy(KaitEnemy target)
    {
        Vector2Int from=target.pos;target.pos=katePos+DirectionToward(katePos,target.pos);
        lastSkillResult=new KaitTurnResult { valid=true };
        lastSkillResult.enemyActions.Add(new KaitEnemyAction {enemyId=target.id,type=KaitIntentType.Move,from=from,to=target.pos});
        Vector2Int d=target.pos-from;d=new Vector2Int(d.x==0?0:d.x>0?1:-1,d.y==0?0:d.y>0?1:-1);
        ResolveMomentumResonance(from,d,lastSkillResult);
    }
    private void RotateIntent(KaitEnemy target)
    {
        Vector2Int Rotate(Vector2Int p)=>new Vector2Int(p.y,-p.x);
        if(target.intent.type==KaitIntentType.LineShot)
            target.intent=BuildLineIntent(target.pos,Rotate(target.intent.direction),config.archerRange,false);
        else if(target.intent.type==KaitIntentType.CrossBlast)
            target.intent=BuildCrossIntent(target.pos,target.pos+Rotate(target.intent.target-target.pos));
        else
        {
            Vector2Int cell=target.pos+Rotate(target.intent.target-target.pos);
            target.intent=new KaitIntent { type=KaitIntentType.Melee,origin=target.pos,target=cell,damage=1 };
            if(!IsHardBlocked(cell)) target.intent.affectedCells.Add(cell);
        }
    }
    private void ApplyPillarStagger(KaitEnemy enemy,Vector2Int wall,KaitTurnResult result)
    {
        // Compatibility entry point for old callers; the rule now keys off collision damage.
        ApplyCollisionStagger(enemy,wall,result);
    }
    private void PushToEnd(KaitEnemy enemy,Vector2Int delta,KaitTurnResult result)
    {
        Vector2Int from=enemy.pos;
        for(int guard=0;guard<BattleSize*BattleSize;guard++)
        {
            Vector2Int next=enemy.pos+delta;
            if(IsHardBlocked(next)||next==katePos)break;
            var blocker=EnemyAt(next);
            if(blocker==null){enemy.pos=next;continue;}
            if(!HasPassive(KaitPassive.RepellingBlast)||!TryShiftEnemyLine(next,delta,result))break;
            enemy.pos=next;
        }
        result.pushFrom=from;result.pushTo=enemy.pos;result.pushed|=enemy.pos!=from;
        if(enemy.pos!=from)
        {
            result.enemyActions.Add(new KaitEnemyAction {enemyId=enemy.id,type=KaitIntentType.Move,from=from,to=enemy.pos});
            ResolveMomentumResonance(from,delta,result);katePos=from;
        }
        if(IsHardBlocked(enemy.pos+delta))
        {
            int damage=config.enableCollisionDamage?config.wallCollisionDamage:0;
            DamageEnemy(enemy,damage,true,result);result.collisionDamage+=damage;if(damage>0)ApplyCollisionStagger(enemy,enemy.pos+delta,result);
        }
        else if(EnemyAt(enemy.pos+delta) is KaitEnemy blocker)
        {
            int damage=config.enableCollisionDamage?config.unitCollisionDamage:0;
            DamageEnemy(enemy,damage,true,result);DamageEnemy(blocker,damage,false,result);result.collisionDamage+=damage*2;if(damage>0)ApplyCollisionStagger(enemy,blocker.pos,result);
        }
    }
    private bool CancelEnemyAttack(KaitEnemy attacker,KaitIntent intent,KaitTurnResult result)
    {
        if(intent.type==KaitIntentType.None || intent.type==KaitIntentType.Move)return false;
        if(attacker.cursed && !attacker.hexArmorSpent && HasPassive(KaitPassive.HexArmor))
        {
            attacker.cursed=false;attacker.hexArmorSpent=true;TriggerPassive(KaitPassive.HexArmor,result,attacker.pos-Vector2Int.one,attacker.pos,"咒术护甲消耗诅咒并使攻击失效");return true;
        }
        if(specterReady)
        {
            specterReady=false;TriggerPassive(KaitPassive.AccursedSpecter,result,katePos-Vector2Int.one,katePos,"幽魂抵消攻击");return true;
        }
        if(IsTwoPhaseRanged(attacker) && intent.affectedCells.Contains(katePos) && HasPassive(KaitPassive.DisplacementCloak) && turnTriggers.Add("Cloak"))
        {
            TriggerPassive(KaitPassive.DisplacementCloak,result,katePos-Vector2Int.one,katePos,"移位斗篷闪避远程攻击");return true;
        }
        return false;
    }
    private void OnCursedDamage(KaitEnemy enemy,bool cursed,KaitTurnResult result)
    {
        if(!cursed || !HasPassive(KaitPassive.MaddeningHex)) return;
        foreach(var neighbour in new List<KaitEnemy>(enemies))
            if(neighbour.id!=enemy.id && neighbour.life!=KaitEnemyLife.Dead &&
                Mathf.Abs(neighbour.pos.x-enemy.pos.x)+Mathf.Abs(neighbour.pos.y-enemy.pos.y)==1)
                DamageEnemyWithContext(neighbour,new KaitDamageContext{kind=KaitDamageKind.CurseBurst,baseAmount=PassiveCopies(KaitPassive.MaddeningHex),
                    creditKate=true,primaryEnemyId=primaryContact?.enemyId??-1,parentEventId=++nextDamageEventId},result);
        TriggerPassive(KaitPassive.MaddeningHex,result,enemy.pos-Vector2Int.one,enemy.pos,"疯狂咒缚波及相邻敌人");
    }
    private void OnAbilityKill(KaitEnemy enemy,bool cursed,bool creditKate,KaitTurnResult result)
    {
        if(cursed && creditKate && HasPassive(KaitPassive.Lifedrinker) && chainTriggers.Add("Lifedrinker"))
        { kateHp=Mathf.Min(KateMaxHp,kateHp+PassiveCopies(KaitPassive.Lifedrinker));TriggerPassive(KaitPassive.Lifedrinker,result,enemy.pos-Vector2Int.one,katePos,"饮命者恢复生命"); }
        if(creditKate && HasPassive(KaitPassive.AccursedSpecter) && chainTriggers.Add("Specter"))
        { specterReady=true;TriggerPassive(KaitPassive.AccursedSpecter,result,enemy.pos-Vector2Int.one,katePos,"幽魂已准备抵消攻击"); }
    }
    private void GenerateTwoPriority()
    {
        nextTwoPriority.Clear();
        for(int y=0;y<ThreatSize;y++) for(int x=0;x<ThreatSize;x++) if(!threatPillars[x,y]) nextTwoPriority.Add(new Vector2Int(x,y));
        for(int i=nextTwoPriority.Count-1;i>0;i--) { int j=random.Next(i+1);var p=nextTwoPriority[i];nextTwoPriority[i]=nextTwoPriority[j];nextTwoPriority[j]=p; }
    }
    public void RefreshThreatPreview()
    {
        if(nextTwoPriority.Count==0) GenerateTwoPriority();
        var legal=nextTwoPriority.FindAll(p=>threat[p.x,p.y]==0 && !threatPillars[p.x,p.y]);
        if(HasPassive(KaitPassive.Trend) && legal.Exists(IsOppositeThreatSide)) legal=legal.FindAll(IsOppositeThreatSide);
        nextThreatTwoPreview=HasPassive(KaitPassive.BirdEye) && legal.Count>0?legal[0]:new Vector2Int(-1,-1);
    }
}
