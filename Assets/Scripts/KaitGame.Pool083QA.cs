using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class KaitGame
{
    private IEnumerator VerifyPool083Runtime()
    {
        yield return new WaitForSecondsRealtime(.5f);
        logPath=System.IO.Path.Combine(Application.persistentDataPath,"pool083-qa.csv");
        run.SelectCharacter(KaitCharacter.Yummn,8313);run.config.playerInvincible=true;
        ConfigureCharacterVisuals();EnsureKaitSpine();mainMenu.gameObject.SetActive(false);gameplayRoot.SetActive(true);
        run.skills.AddRange(new[]{KaitSkill.MageHand,KaitSkill.MirrorImage,KaitSkill.ShatterWave});
        run.passives.AddRange(new[]{KaitPassive.MagicMissile,KaitPassive.WardingGlyph,KaitPassive.SpellEcho});
        run.enemies.Clear();run.spawns.Clear();
        var enemy=new KaitEnemy{id=9801,pos=new Vector2Int(2,3),type=KaitEnemyType.Grunt,hp=8,maxHp=8,life=KaitEnemyLife.Active,intent=new KaitIntent()};run.enemies.Add(enemy);
        System.Array.Clear(run.threat,0,run.threat.Length);run.threat[1,2]=4;run.threat[2,2]=4;run.threat[3,2]=16;
        RefreshAll();yield return null;
        targetingSkill=KaitSkill.MirrorImage;HandleBattleCellClick(new Vector2Int(2,2));
        while(busy)yield return null;
        yield return new WaitForSecondsRealtime(.3f);
        if(run.Yummn.afterimages.Count==0)Debug.LogError("POOL083_QA: mirror missing");
        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(Application.dataPath,"../../Logs/pool083-mirror.png"));
        yield return new WaitForSecondsRealtime(.3f);
        run.Yummn.ki=run.Yummn.profile.maxKi;
        targetingSkill=KaitSkill.MageHand;HandleBattleCellClick(new Vector2Int(3,2),true);
        while(busy)yield return null;
        if(run.threat[3,2]!=0)Debug.LogError("POOL083_QA: hand failed");
        var before=SnapshotEnemies();var pending=SnapshotSpawns();var origin=run.katePos;
        var result=run.TryGlobalInput(KaitDirection.Left);
        yield return PlayTurn(result,origin,before,pending);
        RefreshAll();yield return new WaitForSecondsRealtime(.4f);
        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(Application.dataPath,"../../Logs/pool083-final.png"));
        yield return new WaitForSecondsRealtime(.3f);
        Debug.Log("POOL083_QA_COMPLETE");Application.Quit();
    }
}
