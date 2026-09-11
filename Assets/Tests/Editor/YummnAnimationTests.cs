using NUnit.Framework;
using Spine.Unity;
using UnityEngine;

public sealed class YummnAnimationTests
{
    private GameObject canvas;
    private KaitSpineView view;
    [Test] public void KiSelectsAllRestAndDoesNotInterruptAttack()
    {
        Create();view.SetYummnKiState(true);view.PlayLoop(KaitSpineView.Idle);
        Assert.AreEqual(KaitSpineView.YummnFollowUpReady,view.CurrentAnimation.Animation.Name);
        view.PlayOnce(KaitSpineView.Attack);var attack=view.CurrentAnimation;
        view.SetYummnKiState(false);Assert.AreSame(attack,view.CurrentAnimation);
        view.Root.GetComponentInChildren<SkeletonGraphic>().Update(10);
        view.Root.GetComponentInChildren<SkeletonGraphic>().Update(.1f);
        Assert.AreEqual("01_idle",view.CurrentAnimation.Animation.Name);
        view.SetYummnKiState(true);Assert.AreEqual(KaitSpineView.YummnFollowUpReady,view.CurrentAnimation.Animation.Name);
        view.SetYummnKiState(false);view.RestAnimation=KaitSpineView.YummnFollowUpReady;view.PlayLoop(KaitSpineView.Idle);
        Assert.AreEqual("01_idle",view.CurrentAnimation.Animation.Name);
    }

    [TearDown] public void Cleanup()
    {
        view?.Destroy();
        if(canvas!=null)Object.DestroyImmediate(canvas);
    }

    private void Create(string resource="Characters/Yummn/108231_SkeletonData")
    {
        canvas=new GameObject("Animation test",typeof(Canvas));
        var data=Resources.Load<SkeletonDataAsset>(resource);
        Assert.NotNull(data);
        view=KaitSpineView.Create(data,canvas.transform,new Vector2(115,115));
        Assert.IsTrue(view.IsReady);
    }

    [Test] public void RequestedClipsExistInYummnAsset()
    {
        var data=Resources.Load<SkeletonDataAsset>("Characters/Yummn/108231_SkeletonData").GetSkeletonData(false);
        foreach(var clip in new[]{KaitSpineView.YummnFollowUpReady,KaitSpineView.YummnFollowUpAttack,KaitSpineView.YummnKill,
            KaitSpineView.YummnRun,KaitSpineView.YummnWalk,KaitSpineView.YummnBuff,KaitSpineView.YummnHeal,
            KaitSpineView.YummnAttackSkill,KaitSpineView.YummnVictory})
            Assert.NotNull(data.FindAnimation(clip),clip);
    }

    [Test] public void FollowUpRestCanBeInterruptedWithoutWaitingForAttackEnd()
    {
        Create();view.RestAnimation=KaitSpineView.YummnFollowUpReady;
        view.PlayOnce(KaitSpineView.Attack);
        Assert.AreEqual("01_attack",view.CurrentAnimation.Animation.Name);
        Assert.AreEqual("01_multi_idle_standBy",view.CurrentAnimation.Next.Animation.Name);
        var first=view.CurrentAnimation;view.RefreshRestPose();Assert.AreSame(first,view.CurrentAnimation);
        view.PlayLoop(KaitSpineView.Run);
        Assert.AreEqual("01_run",view.CurrentAnimation.Animation.Name);
        Assert.IsTrue(view.CurrentAnimation.Loop);
        Assert.AreEqual(1f,view.CurrentAnimation.TimeScale);
        view.PlayLoop(KaitSpineView.Idle);
        Assert.AreEqual("01_multi_idle_standBy",view.CurrentAnimation.Animation.Name);
        view.RestAnimation=KaitSpineView.Idle;
        view.PlayOnce(KaitSpineView.YummnFollowUpAttack);
        Assert.AreEqual("01_attack_skipQuest",view.CurrentAnimation.Animation.Name);
        Assert.AreEqual("01_idle",view.CurrentAnimation.Next.Animation.Name);
    }

    [Test] public void FlurryUsesGenericSkillAndNeverArmsItsOwnWaitingPose()
    {
        Create();view.PlayOnce(KaitSpineView.YummnSkillAnimation(KaitSkill.Flurry));
        Assert.AreEqual("01_multi_standBy",view.CurrentAnimation.Animation.Name);
        Assert.AreEqual("01_idle",view.CurrentAnimation.Next.Animation.Name);
    }

    [TestCase(YummnPhase.Exhausted,"01_walk",0)]
    [TestCase(YummnPhase.Burst,"01_run",1)]
    public void MovementUsesPhaseClipAtNormalSpeedAndMatchingTrail(YummnPhase phase,string clip,int ghosts)
    {
        Create();view.PlayLoop(YummnMovementStyle.Animation(phase));
        Assert.AreEqual(clip,view.CurrentAnimation.Animation.Name);Assert.IsTrue(view.CurrentAnimation.Loop);
        Assert.AreEqual(1f,view.CurrentAnimation.TimeScale);Assert.AreEqual(.02f,view.CurrentAnimation.MixDuration);
        Assert.AreEqual(ghosts,YummnMovementStyle.GhostCount(phase,YummnMoveCause.Player,1));
    }
    [TestCase(YummnPhase.Exhausted)] [TestCase(YummnPhase.Burst)]
    public void TeleportDoesNotAddMovementGhosts(YummnPhase phase)
    {Assert.AreEqual(0,YummnMovementStyle.GhostCount(phase,YummnMoveCause.Teleport,4));}
    [Test] public void ExhaustedFollowAndLongMovementHaveNoGhosts()
    {foreach(float distance in new[]{1f,2f,4f})Assert.AreEqual(0,YummnMovementStyle.GhostCount(YummnPhase.Exhausted,YummnMoveCause.Player,distance));Assert.AreEqual(7,YummnMovementStyle.GhostCount(YummnPhase.Burst,YummnMoveCause.Player,4));}

    static KaitTurnResult Punch(bool flurry=false,int travel=0,int target=7)
    {
        var r=new KaitTurnResult{yummnFlurry=flurry,yummnAction=new YummnActionContext{isPunchAction=true,mainEnemyId=target,preHitTravelCells=travel}};
        if(flurry)r.yummnAction.plannedSkills.Add("yummn.M01");return r;
    }
    static YummnCombatEvent Hit(int hp=2,int damage=1,int target=7)=>new YummnCombatEvent{kind=YummnEventKind.Hit,damageCause=YummnDamageCause.Punch,targetId=target,hpAfter=hp,amount=damage,blocked=damage==0};
    [Test] public void LandedFirstPunchArmsWaitingAndNextSameTargetPunchUsesFollowUp()
    {
        var p=new YummnPunchPresentation();p.Begin(Punch());Assert.AreEqual(KaitSpineView.Attack,p.Hit(Hit(),false));
        Assert.AreEqual(KaitSpineView.YummnFollowUpReady,p.RestAnimation);
        p.Begin(Punch());Assert.AreEqual(KaitSpineView.YummnFollowUpAttack,p.Hit(Hit(),false));
    }
    [TestCase(true)] [TestCase(false)] public void KillingHitUsesStandByEvenForFlurry(bool flurry)
    {
        var p=new YummnPunchPresentation();p.Begin(Punch(flurry));Assert.AreEqual(KaitSpineView.YummnKill,p.Hit(Hit(0),false));
        Assert.AreEqual(KaitSpineView.Idle,p.RestAnimation);Assert.IsNull(p.Kill(),"death event must not restart the same kill clip");
        Create();view.PlayOnce(KaitSpineView.YummnKill);Assert.AreEqual("01_standBy",view.CurrentAnimation.Animation.Name);
        var entry=view.CurrentAnimation;view.RefreshRestPose();Assert.AreSame(entry,view.CurrentAnimation);
    }
    [Test] public void NonlethalFlurryUsesGenericAndHasNoFollowUpPose()
    {
        var p=new YummnPunchPresentation();p.Begin(Punch(true));Assert.AreEqual(KaitSpineView.YummnAttackSkill,p.Hit(Hit(),false));
        Assert.AreEqual(KaitSpineView.Idle,p.RestAnimation);Assert.IsNull(p.Hit(Hit(1),true));Assert.AreEqual(KaitSpineView.YummnKill,p.Hit(Hit(0),true));
    }
    [TestCase(1,7)] [TestCase(0,8)] public void TravelOrDifferentTargetStartsFirstAttack(int travel,int target)
    {var p=new YummnPunchPresentation();p.Begin(Punch());p.Hit(Hit(),false);p.Begin(Punch(travel:travel,target:target));Assert.AreEqual(KaitSpineView.Attack,p.Hit(Hit(target:target),false));}
    [Test] public void BlockResetAndSecondaryKillClearWaiting()
    {var p=new YummnPunchPresentation();p.Begin(Punch());p.Hit(Hit(),false);p.Begin(Punch());p.Hit(Hit(damage:0),false);Assert.AreEqual(KaitSpineView.Idle,p.RestAnimation);p.Hit(Hit(),false);p.Kill();Assert.AreEqual(KaitSpineView.Idle,p.RestAnimation);p.Reset();Assert.AreEqual(-1,p.ReadyEnemyId);}

    [TestCase(KaitSkill.WindStep,"108201_skill1")]
    [TestCase(KaitSkill.PatientDefense,"108201_skill1")]
    [TestCase(KaitSkill.Flurry,"01_multi_standBy")]
    [TestCase(KaitSkill.Palm,"01_multi_standBy")]
    [TestCase(KaitSkill.StunningFist,"01_multi_standBy")]
    [TestCase(KaitSkill.FrostBreath,"01_multi_standBy")]
    public void SkillCategories(KaitSkill skill,string expected)
    {
        Create();view.PlayOnce(KaitSpineView.YummnSkillAnimation(skill));
        Assert.AreEqual(expected,view.CurrentAnimation.Animation.Name);
    }

    [Test] public void YummnVictoryUsesSmileNotExistingManaJump()
    {
        Create();view.PlayLoop(KaitSpineView.Victory);
        Assert.AreEqual("000000_smile",view.CurrentAnimation.Animation.Name);
    }

    [Test] public void KaitAnimationMappingIsUnchanged()
    {
        Create("Characters/Makoto/Makoto_SkeletonData");
        view.PlayLoop(KaitSpineView.Run);
        Assert.AreEqual(KaitSpineView.Run,view.CurrentAnimation.Animation.Name);
        view.PlayLoop(KaitSpineView.Victory);
        Assert.AreEqual(KaitSpineView.Victory,view.CurrentAnimation.Animation.Name);
    }

    [Test] public void FlurryStillUsesComboWhenFirstPunchKills()
    {
        var run=new KaitRun(new KaitBalanceConfig{newThreatTilesPerTurn=0});
        run.SelectCharacter(KaitCharacter.Yummn,42,new YummnRulesSnapshot());run.enemies.Clear();run.spawns.Clear();
        typeof(KaitRun).GetProperty("katePos").SetValue(run,new Vector2Int(1,3));
        run.enemies.Add(new KaitEnemy{id=501,type=KaitEnemyType.Grunt,pos=new Vector2Int(2,3),hp=1,maxHp=1,life=KaitEnemyLife.Active});
        run.skills.Add(KaitSkill.Flurry);Assert.IsTrue(run.TryUseSkill(KaitSkill.Flurry,-1,out _));
        var result=run.TryGlobalInput(KaitDirection.Right);
        Assert.AreEqual(1,result.yummnPunches);
        Assert.IsTrue(result.yummnFlurry);
    }
}
