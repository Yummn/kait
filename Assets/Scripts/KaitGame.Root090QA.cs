using System.Collections;
using UnityEngine;

public sealed partial class KaitGame
{
    private IEnumerator VerifyRoot090Runtime()
    {
        yield return new WaitForSecondsRealtime(.5f);
        logPath=System.IO.Path.Combine(Application.persistentDataPath,"root090-qa.csv");
        run.SelectCharacter(KaitCharacter.Yummn,9013);run.config.playerInvincible=true;
        ConfigureCharacterVisuals();EnsureKaitSpine();mainMenu.gameObject.SetActive(false);gameplayRoot.SetActive(true);
        run.passives.AddRange(new[]{KaitPassive.TwinPunch,KaitPassive.OpenHand,KaitPassive.FollowThrough,KaitPassive.OpportunityAttack,KaitPassive.Opportunist,KaitPassive.MagicMissile});
        run.enemies.Clear();run.spawns.Clear();
        var origin=run.katePos;
        run.enemies.Add(new KaitEnemy{id=9801,pos=origin+Vector2Int.right,type=KaitEnemyType.Grunt,hp=8,maxHp=8,life=KaitEnemyLife.Active,intent=new KaitIntent()});
        System.Array.Clear(run.threat,0,run.threat.Length);run.threat[0,2]=8;run.threat[1,2]=8;
        RefreshAll();yield return null;
        var before=SnapshotEnemies();var spawns=SnapshotSpawns();var result=run.TryGlobalInput(KaitDirection.Right);
        int mainHit=result.yummnEvents.FindIndex(e=>e.kind==YummnEventKind.Hit&&e.damageCause==YummnDamageCause.Punch);
        int threat=result.yummnEvents.FindIndex(e=>e.status=="ThreatBegin");
        if(mainHit<0||mainHit>=threat)Debug.LogError("ROOT090_QA: incorrect main/threat order");
        Debug.Log("ROOT090_ACTION "+JsonUtility.ToJson(result.yummnAction));
        yield return PlayTurn(result,origin,before,spawns);
        RefreshAll();yield return new WaitForSecondsRealtime(.4f);
        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(Application.dataPath,"../../Logs/root090-cards.png"));
        yield return new WaitForSecondsRealtime(.4f);
        foreach(var def in YummnCatalog.Cards)if(YummnRepoolArt.Icon(def)==null)Debug.LogError("ROOT090_QA: missing card "+def.id);
        Debug.Log("ROOT090_QA_COMPLETE");Application.Quit();
    }
}
