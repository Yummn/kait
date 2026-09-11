using System.Collections;
using System;
using UnityEngine;

public sealed partial class KaitGame
{
    private IEnumerator VerifyYummn081Runtime(string path)
    {
        run.SelectCharacter(KaitCharacter.Yummn,8101,new YummnRulesSnapshot());
        ConfigureCharacterVisuals();run.StateCommitted=null;EnsureKaitSpine();
        run.enemies.Clear();run.spawns.Clear();Array.Clear(run.threat,0,run.threat.Length);
        typeof(KaitRun).GetProperty("katePos").SetValue(run,new Vector2Int(1,3));
        run.skills.Add(KaitSkill.Flurry);run.passives.Add(KaitPassive.FireSnake);
        run.enemies.Add(new KaitEnemy{id=9801,type=KaitEnemyType.Grunt,pos=new Vector2Int(2,3),hp=2,maxHp=2,life=KaitEnemyLife.Active});
        run.enemies.Add(new KaitEnemy{id=9802,type=KaitEnemyType.Swordsman,pos=new Vector2Int(3,3),hp=1,maxHp=3,life=KaitEnemyLife.Active});
        run.enemies.Add(new KaitEnemy{id=9803,type=KaitEnemyType.Archer,pos=new Vector2Int(4,4),hp=2,maxHp=2,life=KaitEnemyLife.Active});
        run.spawns.Add(new KaitSpawnRequest{tier=1,targetCell=new Vector2Int(4,2),sourceThreatCell=new Vector2Int(3,1),createdTurn=0});
        RefreshAll();yield return new WaitForSecondsRealtime(.2f);CaptureCanvasToPng(path+".v081-multikill-before.png");
        run.TryUseSkill(KaitSkill.Flurry,-1,out _);HandleDirection(KaitDirection.Right);
        yield return new WaitForSecondsRealtime(.12f);CaptureCanvasToPng(path+".v081-multikill-action.png");
        while(busy)yield return null;
        yield return new WaitForSecondsRealtime(2f);CaptureCanvasToPng(path+".v081-multikill-after.png");
        if(run.kills!=2||run.Yummn.metrics.insertedTwos!=2||run.EnemyResolveCount!=1)Debug.LogError("YUMMN_QA: v081 multikill transaction failed");
        var book=tutorialOverlay.GetComponent<KaitTutorialBook>();tutorialOverlay.SetActive(true);
        for(int i=0;i<4;i++){book.ShowPage(i);yield return null;CaptureCanvasToPng(path+".v081-tutorial-"+i+".png");}
        tutorialOverlay.SetActive(false);
        settingsOverlay.SetActive(true);yield return null;CaptureCanvasToPng(path+".v082-settings-legacy-run.png");
        if(run.Yummn.profile.maxKi!=5||run.Yummn.profile.killKi!=2)Debug.LogError("YUMMN_QA: current preset mutated legacy run");
        settingsOverlay.SetActive(false);
        run.SelectCharacter(KaitCharacter.Yummn,8102,new YummnRulesSnapshot());run.StateCommitted=null;
        run.enemies.Clear();run.spawns.Clear();typeof(KaitRun).GetProperty("katePos").SetValue(run,new Vector2Int(1,3));
        for(int x=0;x<5;x++)for(int y=0;y<5;y++)if(!run.threatPillars[x,y])run.threat[x,y]=(x+y)%2==0?2:4;
        RefreshAll();yield return null;HandleDirection(KaitDirection.Right);while(busy)yield return null;
        yield return new WaitForSecondsRealtime(.2f);CaptureCanvasToPng(path+".v081-locked-defeat.png");
        if(!run.ended||run.endReason!="ThreatBoardLocked")Debug.LogError("YUMMN_QA: v081 lock defeat not displayed");
        Debug.Log("YUMMN_V081_QA_COMPLETE multikill=2 supply=2 enemyPhases=1 snapshot immutable; locked board retained with defeat");
        endOverlay.SetActive(false);
        yield return VerifyYummnPunchPoses(path);
        yield return VerifyYummnBossLine(path);
    }

    private IEnumerator VerifyYummnBossLine(string path)
    {
        run.SelectCharacter(KaitCharacter.Yummn,8130,new YummnRulesSnapshot());ConfigureCharacterVisuals();run.StateCommitted=null;EnsureKaitSpine();
        run.enemies.Clear();run.spawns.Clear();Array.Clear(run.threat,0,run.threat.Length);
        typeof(KaitRun).GetProperty("katePos").SetValue(run,new Vector2Int(4,3));run.Yummn.phase=YummnPhase.Exhausted;run.Yummn.ki=0;
        var boss=new KaitEnemy{id=9950,type=KaitEnemyType.ShieldKnight,pos=new Vector2Int(1,3),hp=8,maxHp=8,life=KaitEnemyLife.Active,facing=Vector2Int.down};run.enemies.Add(boss);
        RefreshAll();HandleDirection(KaitDirection.Up);while(busy)yield return null;
        if(boss.facing!=Vector2Int.right||boss.intent.affectedCells.Count!=4)Debug.LogError("YUMMN_QA: distant off-axis boss did not turn and aim");
        foreach(var p in boss.intent.affectedCells)if(!battleWarningLines[p.x+p.y*7].gameObject.activeSelf)Debug.LogError("YUMMN_QA: missing boss row warning");
        yield return null;CaptureCanvasToPng(path+".boss-line-aim.png");
        HandleDirection(KaitDirection.Down);while(busy)yield return null;
        if(run.kateHp!=2||boss.intent.type!=KaitIntentType.None||boss.facing!=Vector2Int.right)Debug.LogError("YUMMN_QA: boss locked line attack did not resolve");
        CaptureCanvasToPng(path+".boss-line-after.png");
        Debug.Log("YUMMN_BOSS_LINE_QA_COMPLETE off-axis aim, four warning cells, locked facing and next-phase damage");
    }

    private IEnumerator VerifyYummnPunchPoses(string path)
    {
        run.SelectCharacter(KaitCharacter.Yummn,8120,new YummnRulesSnapshot());ConfigureCharacterVisuals();run.StateCommitted=null;EnsureKaitSpine();
        run.enemies.Clear();run.spawns.Clear();Array.Clear(run.threat,0,run.threat.Length);
        typeof(KaitRun).GetProperty("katePos").SetValue(run,new Vector2Int(2,3));
        run.enemies.Add(new KaitEnemy{id=9901,type=KaitEnemyType.Guard,pos=new Vector2Int(3,3),hp=3,maxHp=3,life=KaitEnemyLife.Active});
        RefreshAll();HandleDirection(KaitDirection.Right);
        if(kaitSpine.CurrentAnimation.Animation.Name!="01_attack")Debug.LogError("YUMMN_QA: first punch clip");
        while(busy)yield return null;
        yield return new WaitForSecondsRealtime(kaitSpine.Duration(KaitSpineView.Attack)+.1f);
        if(kaitSpine.CurrentAnimation.Animation.Name!=KaitSpineView.YummnFollowUpReady)Debug.LogError("YUMMN_QA: first hit did not arm follow-up rest");
        CaptureCanvasToPng(path+".punch-ready.png");HandleDirection(KaitDirection.Right);
        if(kaitSpine.CurrentAnimation.Animation.Name!=KaitSpineView.YummnFollowUpAttack)Debug.LogError("YUMMN_QA: follow-up punch clip");
        yield return new WaitForSecondsRealtime(.06f);CaptureCanvasToPng(path+".punch-follow-up.png");while(busy)yield return null;
        HandleDirection(KaitDirection.Right);
        if(kaitSpine.CurrentAnimation.Animation.Name!=KaitSpineView.YummnKill)Debug.LogError("YUMMN_QA: killing hit did not use standBy");
        yield return new WaitForSecondsRealtime(.12f);CaptureCanvasToPng(path+".punch-kill.png");
        if(kaitSpine.CurrentAnimation.Animation.Name==KaitSpineView.YummnRun)Debug.LogError("YUMMN_QA: kill follow movement cut off standBy");
        while(busy)yield return null;
        run.Yummn.ki=5;run.skills.Add(KaitSkill.Flurry);
        run.enemies.Add(new KaitEnemy{id=9902,type=KaitEnemyType.Guard,pos=new Vector2Int(4,3),hp=5,maxHp=5,life=KaitEnemyLife.Active});
        run.TryUseSkill(KaitSkill.Flurry,-1,out _);PlayYummnSkillFeedback(KaitSkill.Flurry);RefreshAll();
        if(kaitSpine.RestAnimation!=KaitSpineView.Idle)Debug.LogError("YUMMN_QA: Flurry still arms dedicated rest");
        HandleDirection(KaitDirection.Right);
        if(kaitSpine.CurrentAnimation.Animation.Name!=KaitSpineView.YummnAttackSkill)Debug.LogError("YUMMN_QA: Flurry did not use generic attack skill");
        yield return new WaitForSecondsRealtime(.06f);CaptureCanvasToPng(path+".punch-flurry.png");while(busy)yield return null;
        HandleDirection(KaitDirection.Left);
        if(kaitSpine.CurrentAnimation.Animation.Name!=KaitSpineView.YummnRun)Debug.LogError("YUMMN_QA: movement cannot interrupt attack");
        while(busy)yield return null;
        Debug.Log("YUMMN_PUNCH_QA_COMPLETE first hit, follow-up rest/attack, standBy kill, generic Flurry, movement interruption");
    }
}
