using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class KaitGame
{
    IEnumerator VerifyHoldInputRuntime(string path)
    {
        yield return new WaitForSecondsRealtime(.3f);
        PlayerPrefs.SetInt(Yummn082Preference+"KiGuard",1);
        mainMenu.Select(KaitCharacter.Yummn);StartSelectedCharacter(KaitCharacter.Yummn);
        run.StateCommitted=null;run.enemies.Clear();run.spawns.Clear();run.config.playerInvincible=true;
        System.Array.Clear(run.threat,0,run.threat.Length);run.threat[1,1]=2;run.threat[2,2]=4;
        typeof(KaitRun).GetProperty("katePos").SetValue(run,new Vector2Int(2,3));
        run.enemies.Add(new KaitEnemy{id=91911,type=KaitEnemyType.Swordsman,pos=new Vector2Int(3,3),hp=40,maxHp=40,life=KaitEnemyLife.Active});
        RefreshAll();yield return new WaitForSecondsRealtime(.2f);
        var point=RectTransformUtility.WorldToScreenPoint(null,battleCells[2+3*7].transform.position);
        if(IsTouchOverButton(point))Debug.LogError("HOLD_QA: battle cell still blocks gesture");
        if(!IsTouchOverButton(RectTransformUtility.WorldToScreenPoint(null,waitButton.transform.position)))Debug.LogError("HOLD_QA: wait button does not block gesture");
        PlayerPrefs.SetInt(HoldInputPreference,1);
        int first=run.ActionIndex;BeginHeldDirection(KaitDirection.Right,42);
        float deadline=Time.unscaledTime+12;
        while(run.ActionIndex<first+3&&Time.unscaledTime<deadline)yield return null;
        heldInput.End(42);
        if(run.ActionIndex<first+3)Debug.LogError("HOLD_QA: held punches did not repeat");
        int stopped=run.ActionIndex;
        yield return new WaitForSecondsRealtime(1.5f);
        if(run.ActionIndex!=stopped)Debug.LogError("HOLD_QA: action continued after release");
        CaptureCanvasToPng(path+".punches.png");
        while(!AutoInputReady)yield return null;
        int phases=run.Yummn.metrics.enemyPhases;var pos=run.katePos;var board=(int[,])run.threat.Clone();
        // The same classifier used by touch input; a stationary touch need not hit the actor.
        var gesture=new KaitStationaryHold();gesture.Begin(point,Time.unscaledTime-.6f);
        if(gesture.Poll(point,Time.unscaledTime,27,AutoInputReady))HandleWait();
        while(busy)yield return null;
        if(run.Yummn.metrics.enemyPhases!=phases+1||run.katePos!=pos)Debug.LogError("HOLD_QA: long wait changed wrong state");
        for(int x=0;x<run.ThreatSize;x++)for(int y=0;y<run.ThreatSize;y++)if(board[x,y]!=run.threat[x,y])Debug.LogError("HOLD_QA: wait changed threat board");
        if(gesture.Poll(point,Time.unscaledTime+2,27,true))Debug.LogError("HOLD_QA: wait repeated without release");
        run.enemies.Clear();run.spawns.Clear();run.config.playerInvincible=false;
        run.Yummn.phase=YummnPhase.Burst;run.Yummn.ki=run.Yummn.profile.maxKi;
        var source=run.katePos+Vector2Int.up;
        var intent=new KaitIntent{type=KaitIntentType.Melee,origin=source,target=run.katePos,damage=1};intent.affectedCells.Add(run.katePos);
        run.enemies.Add(new KaitEnemy{id=92912,type=KaitEnemyType.Grunt,pos=source,hp=2,maxHp=2,life=KaitEnemyLife.Active,intent=intent});
        RefreshAll();int hp=run.kateHp;HandleWait();
        float guardDeadline=Time.unscaledTime+5;
        while(kaitSpine.CurrentAnimation.Animation.Name!=KaitSpineView.YummnKiGuard&&Time.unscaledTime<guardDeadline)yield return null;
        if(kaitSpine.CurrentAnimation.Animation.Name!=KaitSpineView.YummnKiGuard)Debug.LogError("HOLD_QA: Ki guard animation missing");
        else{yield return new WaitForSecondsRealtime(.2f);CaptureCanvasToPng(path+".ki-guard.png");}
        while(busy)yield return null;
        if(run.kateHp!=hp||run.KiPhase!=YummnPhase.Exhausted)Debug.LogError("HOLD_QA: Ki guard state incorrect");
        settingsOverlay.SetActive(true);RefreshCharacterSettings();yield return null;
        CaptureCanvasToPng(path+".settings.png");
        foreach(var text in settingsOverlay.GetComponentsInChildren<Text>())if(text.preferredHeight>text.rectTransform.rect.height+2)Debug.LogError("HOLD_QA: settings text overflow "+text.text);
        Debug.Log("HOLD_QA_COMPLETE");Application.Quit();
    }
}
