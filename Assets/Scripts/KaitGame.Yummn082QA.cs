using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class KaitGame
{
    private IEnumerator VerifyYummn082Runtime(string path)
    {
        yield return new WaitForSecondsRealtime(.5f);
        run.SelectCharacter(KaitCharacter.Yummn,8201);ConfigureCharacterVisuals();run.StateCommitted=null;EnsureKaitSpine();
        run.config.playerInvincible=true;run.enemies.Clear();run.spawns.Clear();Array.Clear(run.threat,0,run.threat.Length);
        typeof(KaitRun).GetProperty("katePos").SetValue(run,new Vector2Int(1,3));
        var archer=new KaitEnemy{id=9821,type=KaitEnemyType.Archer,pos=new Vector2Int(1,2),hp=2,maxHp=2,life=KaitEnemyLife.Active,rangedState=KaitRangedState.Aim};
        archer.intent=new KaitIntent{type=KaitIntentType.LineShot,origin=archer.pos,target=new Vector2Int(1,5),direction=Vector2Int.up,damage=1};
        archer.intent.affectedCells.AddRange(new[]{new Vector2Int(1,3),new Vector2Int(1,4),new Vector2Int(1,5)});run.enemies.Add(archer);
        run.threat[1,1]=2;run.threat[2,2]=8;run.threat[3,3]=16;
        RefreshAll();yield return new WaitForSecondsRealtime(.25f);CaptureCanvasToPng(path+".six-ki.png");
        if(actionPips.Length!=6)Debug.LogError("YUMMN082_QA: default HUD not six pips");
        HandleDirection(KaitDirection.Right);while(busy)yield return null;
        yield return new WaitForSecondsRealtime(.35f);CaptureCanvasToPng(path+".afterimage.png");
        if(run.Ki!=2||run.Yummn.afterimages.Count!=1||yummnLogicalGhosts.Count!=1)Debug.LogError("YUMMN082_QA: movement cost or marker missing");
        PlayV08Fx(new Vector2Int(1,3),15,KaitDirection.Right,90);
        yield return new WaitForSecondsRealtime(.12f);CaptureCanvasToPng(path+".ghost-hit-c.png");
        yield return new WaitForSecondsRealtime(.35f);
        foreach(var view in yummnLogicalGhosts.Values)
        {
            if(Vector3.Distance(view.Root.position,YummnCellPosition(new Vector2Int(1,3)))>.1f)Debug.LogError("YUMMN082_QA: marker is off-cell");
            if(view.Root.GetComponentsInChildren<Mask>(true).Length>0||view.Root.GetComponentsInChildren<RectMask2D>(true).Length>0)Debug.LogError("YUMMN082_QA: marker unexpectedly masked");
        }
        HandleDirection(KaitDirection.Down);while(busy)yield return null; // (5,2), then the lower-right pillar at (5,1)
        HandleDirection(KaitDirection.Left);while(busy)yield return null; // spend the final point, latch exhaustion
        if(run.KiPhase!=YummnPhase.Exhausted)Debug.LogError("YUMMN082_QA: zero Ki did not exhaust");
        HandleDirection(KaitDirection.Up);
        yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(path+".enemy-phase.png");
        while(busy)yield return null;
        yield return new WaitForSecondsRealtime(.4f);CaptureCanvasToPng(path+".partial-recovery.png");
        if(run.Yummn.metrics.afterimageKi!=1||run.Yummn.afterimages.Count!=0||yummnLogicalGhosts.Count!=0||run.Ki!=2||run.KiPhase!=YummnPhase.Exhausted)Debug.LogError($"YUMMN082_QA: hit/expire/recovery mismatch: ghostKi={run.Yummn.metrics.afterimageKi}, markers={run.Yummn.afterimages.Count}, visuals={yummnLogicalGhosts.Count}, ki={run.Ki}, phase={run.KiPhase}");
        var activeRules=run.SaveReplay();settingsOverlay.SetActive(true);RefreshCharacterSettings();yield return null;
        CaptureCanvasToPng(path+".settings.png");
        if(yummnSettingsControls.Count!=9)Debug.LogError("YUMMN082_QA: settings count");
        bool wasAttack=yummnAttackPhaseToggle.isOn;yummnAttackPhaseToggle.isOn=!wasAttack;
        if(run.SaveReplay()!=activeRules)Debug.LogError("YUMMN082_QA: preset mutated run");yummnAttackPhaseToggle.isOn=wasAttack;
        CheckYummn082Text(settingsOverlay);
        settingsOverlay.SetActive(false);
        var book=tutorialOverlay.GetComponent<KaitTutorialBook>();tutorialOverlay.SetActive(true);
        for(int i=0;i<4;i++){book.ShowPage(i);yield return null;CaptureCanvasToPng(path+".tutorial-"+i+".png");CheckYummn082Text(tutorialOverlay);}
        tutorialOverlay.SetActive(false);
        foreach(int max in new[]{5,4,3,6})
        {
            run.SelectCharacter(KaitCharacter.Yummn,8202,YummnRulesSnapshot.Current(max));run.StateCommitted=null;ConfigureCharacterVisuals();EnsureKaitSpine();RefreshAll();yield return null;
            if(actionPips.Length!=max)Debug.LogError("YUMMN082_QA: dynamic pips "+max);
        }
        run.SelectCharacter(KaitCharacter.Kait,707);ConfigureCharacterVisuals();run.StateCommitted=null;EnsureKaitSpine();RefreshAll();yield return null;
        CaptureCanvasToPng(path+".kait.png");settingsOverlay.SetActive(true);yield return null;CaptureCanvasToPng(path+".kait-settings.png");
        foreach(var control in yummnSettingsControls)if(control.activeSelf)Debug.LogError("YUMMN082_QA: Yummn setting leaked to Kait");
        Debug.Log("YUMMN082_QA_COMPLETE six-Ki, exact ghost cell, attack consumes marker, partial exhaustion, seven frozen settings, tutorials, dynamic HUD, Kait restoration");
        Application.Quit();
    }
    private void CheckYummn082Text(GameObject parent)
    {
        Canvas.ForceUpdateCanvases();
        foreach(var text in parent.GetComponentsInChildren<Text>())
            if(text.preferredHeight>text.rectTransform.rect.height+2)Debug.LogError("YUMMN082_QA: overflowing text "+text.text);
    }
}
