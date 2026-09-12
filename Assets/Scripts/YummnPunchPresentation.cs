// Visual-only continuity: a landed ordinary punch arms the next adjacent punch.
// No action, damage, Ki, or save rules depend on this state.
public sealed class YummnPunchPresentation
{
    public int ReadyEnemyId {get;private set;}=-1;
    private bool followUp,attackSkill,killStarted;
    public string RestAnimation=>ReadyEnemyId>=0?KaitSpineView.YummnFollowUpReady:KaitSpineView.Idle;
    public void Reset(){ReadyEnemyId=-1;followUp=attackSkill=killStarted=false;}
    public void Begin(KaitTurnResult r)
    {
        followUp=ReadyEnemyId>=0&&ReadyEnemyId==r.yummnAction.mainEnemyId&&r.yummnAction.preHitTravelCells==0&&r.yummnAction.isPunchAction;
        attackSkill=r.yummnAction.plannedSkills.Exists(id=>YummnCatalog.Get(id)!=null&&KaitSpineView.YummnSkillAnimation(YummnCatalog.Get(id).skill)==KaitSpineView.YummnAttackSkill);
        ReadyEnemyId=-1;killStarted=false;
    }
    public void ClearReady(){ReadyEnemyId=-1;}
    public string Hit(YummnCombatEvent ev,bool started)
    {
        if(ev.amount>0&&ev.hpAfter<=0)return Kill();
        if(ev.damageCause==YummnDamageCause.Punch)
        {
            ReadyEnemyId=!attackSkill&&ev.amount>0&&!ev.blocked?ev.targetId:-1;
            return started?null:attackSkill?KaitSpineView.YummnAttackSkill:KaitSpineView.YummnFollowUpAttack;
        }
        if(ev.damageCause==YummnDamageCause.Counter){ClearReady();return KaitSpineView.YummnFollowUpAttack;}
        if(ev.damageCause==YummnDamageCause.WaterWhip||ev.damageCause==YummnDamageCause.WinterBreath)
        {ClearReady();return started?null:KaitSpineView.YummnAttackSkill;}
        return null;
    }
    public string Kill(){ClearReady();if(killStarted)return null;killStarted=true;return KaitSpineView.YummnKill;}
    public void Validate(KaitRun run)
    {
        var enemy=run.enemies.Find(e=>e.id==ReadyEnemyId&&e.life!=KaitEnemyLife.Dead);
        if(enemy==null||(enemy.pos-run.katePos).sqrMagnitude!=1||run.ended)ClearReady();
    }
}
