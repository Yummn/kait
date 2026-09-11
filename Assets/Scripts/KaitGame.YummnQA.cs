using System.Collections;
using UnityEngine;

public sealed partial class KaitGame
{
    private IEnumerator VerifyYummnRuntime(string path)
    {
        yield return new WaitForSecondsRealtime(.4f);
        ShowCharacterSelection();yield return new WaitForSecondsRealtime(.2f);
        Canvas.ForceUpdateCanvases();
        foreach(var label in characterSelection.GetComponentsInChildren<UnityEngine.UI.Text>())
            if(label.text=="继续上次"&&label.cachedTextGenerator.vertexCount==0)Debug.LogError("YUMMN_QA: continuation label was clipped");
        CaptureCanvasToPng(path+".selection.png");characterSelection.SetActive(false);
        run.config.enableThreatPillars=true;run.SelectCharacter(KaitCharacter.Yummn,707,new YummnRulesSnapshot());
        ConfigureCharacterVisuals();run.StateCommitted=null;EnsureKaitSpine();
        tutorialOverlay.SetActive(true);yield return null;
        CaptureCanvasToPng(path+".first-open-tutorial.png");
        var firstBook=tutorialOverlay.GetComponent<KaitTutorialBook>();
        if(!firstBook.YummnMode||firstBook.PageIndex!=0||firstBook.PageCount!=4)Debug.LogError("YUMMN_QA: stale character tutorial");
        tutorialOverlay.SetActive(false);
        run.enemies.Clear();run.spawns.Clear();
        run.skills.AddRange(new[]{KaitSkill.Flurry,KaitSkill.Palm,KaitSkill.FrostBreath});
        run.passives.AddRange(new[]{KaitPassive.Tranquility,KaitPassive.FollowThrough,KaitPassive.DeflectMissiles});
        run.enemies.Add(new KaitEnemy{id=901,type=KaitEnemyType.Guard,pos=new Vector2Int(4,3),hp=5,maxHp=5,life=KaitEnemyLife.Active});
        run.enemies.Add(new KaitEnemy{id=902,type=KaitEnemyType.Archer,pos=new Vector2Int(2,2),hp=2,maxHp=2,life=KaitEnemyLife.Active});
        RefreshAll();yield return new WaitForSecondsRealtime(.3f);CaptureCanvasToPng(path);
        VerifySnowCourtyardPresentation(true);
        for(int i=0;i<actionPips.Length;i++)
            if(actionPips[i].GetComponent<YummnKiWisp>()==null||actionPips[i].sprite==null||actionPips[i].transform.parent!=yummnHud.transform)
                Debug.LogError("YUMMN_QA: missing five Ki pips");
        if(Mathf.Abs(yummnCourtyard.rectTransform.rect.width/yummnCourtyard.rectTransform.rect.height-yummnCourtyard.sprite.rect.width/yummnCourtyard.sprite.rect.height)>.01f)
            Debug.LogError("YUMMN_QA: stretched courtyard");
        if(kaitSpine==null)Debug.LogError("YUMMN_QA: missing skeleton");
        // Screenshot encoding is synchronous. Do not count that instrumentation
        // stall as the first movement's delta time; let normal frames resume.
        yield return null;yield return new WaitForSecondsRealtime(.3f);
        yield return VerifyYummnMovementFrames(path);
        if(!run.TryUseSkill(KaitSkill.Flurry,-1,out _))Debug.LogError("YUMMN_QA: skill rejected");
        HandleDirection(KaitDirection.Right);
        yield return new WaitForSecondsRealtime(.02f);
        HandleDirection(KaitDirection.Up); // one buffered follow-up within the same phase
        while(busy)yield return null;
        yield return new WaitForSecondsRealtime(.1f);
        if(run.turn!=2||run.KiPhase!=YummnPhase.Burst||run.Ki!=2)Debug.LogError("YUMMN_QA: buffered action/phase mismatch");
        if(kaitSpine.CurrentAnimation?.Animation.Name!=(run.ExactKi>0?KaitSpineView.YummnFollowUpReady:"01_idle"))Debug.LogError("YUMMN_QA: movement rest does not match Ki");
        CaptureCanvasToPng(path+".after-two-actions.png");
        run.Yummn.phase=YummnPhase.Exhausted;run.Yummn.ki=0;
        for(int i=0;i<5;i++)
        {
            HandleDirection(i%2==0?KaitDirection.Left:KaitDirection.Right);
            if(kaitSpine.CurrentAnimation.Animation.Name!=KaitSpineView.YummnWalk)Debug.LogError("YUMMN_QA: exhausted step did not use walk, step="+i);
            bool sawGhost=false;
            while(busy){sawGhost|=activeTrailVisuals.Count>0;if(i==0&&yummnMoveProgress>.25f&&yummnMoveProgress<.8f)CaptureCanvasToPng(path+".exhausted-walk.png");yield return null;}
            if(sawGhost)Debug.LogError("YUMMN_QA: exhausted movement still emits ghosts");
        }
        if(run.KiPhase!=YummnPhase.Burst||run.Ki!=5)Debug.LogError("YUMMN_QA: recovery did not return to full Ki");
        Debug.Log("YUMMN_WALK_QA_COMPLETE five exhausted steps use walk without ghosts, including full-Ki recovery step");
        CaptureCanvasToPng(path+".recovered.png");
        var pack=new KaitRewardPack();pack.choices.Add(YummnCatalog.Cards[0]);pack.choices.Add(YummnCatalog.Cards[6]);pack.choices.Add(YummnCatalog.Cards[5]);run.rewardQueue.Enqueue(pack);RefreshAll();
        yield return new WaitForSecondsRealtime(.3f);CaptureCanvasToPng(path+".cards.png");run.rewardQueue.Clear();RefreshAll();
        // Exercise the same drop and cancel handlers as the real card gesture.
        var originalPassives=new System.Collections.Generic.List<KaitPassive>(run.passives);
        run.passives.Clear();run.passives.Add(KaitPassive.OpenHand);
        var replacing=new KaitRewardPack{id=9001};replacing.choices.Add(YummnCatalog.Get("E01"));run.rewardQueue.Enqueue(replacing);RefreshAll();
        var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
        typeof(KaitRewardDeck).GetMethod("BeginDrag",flags).Invoke(rewardDeck,new object[]{0});
        var dropSlots=(UnityEngine.UI.Image[])typeof(KaitRewardDeck).GetField("slots",flags).GetValue(rewardDeck);
        typeof(KaitRewardDeck).GetMethod("EndDrag",flags).Invoke(rewardDeck,new object[]{0,dropSlots[0].rectTransform.anchoredPosition});
        yield return null;
        var prompt=rewardDeck.transform.parent.GetComponentsInChildren<UnityEngine.UI.Image>();
        var confirmation=System.Array.Find(prompt,i=>i.name=="Confirm Prerequisite Replacement");
        if(confirmation==null)Debug.LogError("YUMMN_QA: missing prerequisite confirmation");
        else{CaptureCanvasToPng(path+".replace-confirm.png");confirmation.transform.Find("取消").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();}
        if(!run.skills.Contains(KaitSkill.Flurry))Debug.LogError("YUMMN_QA: cancellation replaced a card");
        run.rewardQueue.Clear();run.passives.Clear();run.passives.AddRange(originalPassives);RefreshAll();
        HandleSkillCardCast(0);skillDeck.Owned[0].PreviewAt(new Vector2(-70,10));yield return null;CaptureCanvasToPng(path+".prepared.png");
        run.TryUseSkill(KaitSkill.Flurry,-1,out _);skillDeck.Owned[0].Hide();RefreshAll();
        for(int kind=0;kind<6;kind++)PlayYummnFx(new Vector2Int(2+kind%3,2+kind/3),kind);
        // Capture the creation frame: the old .06s delay hid blank first-frame Images.
        foreach(var effect in battleEnemyHitLayer.GetComponentsInChildren<YummnEffectGraphic>())
            if(effect.sprite==null)Debug.LogError("YUMMN_QA: effect has no sprite on creation frame");
        CaptureCanvasToPng(path+".effects-first-frame.png");
        yield return new WaitForSecondsRealtime(.06f);CaptureCanvasToPng(path+".effects.png");
        run.Yummn.icePillar=new Vector2Int(3,3);run.Yummn.darkness=new Vector2Int(4,4);
        run.Yummn.palmEnemyId=902;run.Yummn.palmDirection=Vector2Int.right;RefreshAll();
        int darkIndex=4+4*7;
        if(yummnDarkness.transform.parent!=battleCells[darkIndex].transform||yummnDarkness.transform.GetSiblingIndex()>=battleWarningLines[darkIndex].transform.parent.GetSiblingIndex())Debug.LogError("YUMMN_QA: darkness covers attack warnings");
        CaptureCanvasToPng(path+".terrain.png");
        for(int kind=0;kind<12;kind++)PlayV08Fx(new Vector2Int(2+kind%4,1+kind/4),kind,KaitDirection.Right,82);
        foreach(var effect in battleEnemyHitLayer.GetComponentsInChildren<YummnV08Effect>())if(effect.sprite==null)Debug.LogError("YUMMN_QA: V08 empty first frame");
        yield return new WaitForSecondsRealtime(.05f);CaptureCanvasToPng(path+".v08-effects.png");
        var book=tutorialOverlay.GetComponent<KaitTutorialBook>();tutorialOverlay.SetActive(true);
        for(int page=0;page<book.PageCount;page++){book.ShowPage(page);yield return null;CaptureCanvasToPng(path+".tutorial-"+page+".png");}
        tutorialOverlay.SetActive(false);
        yield return VerifyYummn081Runtime(path);
        yield return VerifyYummnSelectedFrames(path);
        yield return VerifyYummnElementFrames(path);
        yield return VerifyYummnIcePalmFrames(path);
        yield return VerifyYummnShadowFrames(path);
        yield return VerifyYummnKiFrames(path);
        run.SelectCharacter(KaitCharacter.Kait,707);ConfigureCharacterVisuals();run.StateCommitted=null;EnsureKaitSpine();RefreshAll();
        yield return new WaitForSecondsRealtime(.3f);CaptureCanvasToPng(path+".kait-regression.png");
        settingsOverlay.SetActive(true);yield return null;CaptureCanvasToPng(path+".kait-settings.png");settingsOverlay.SetActive(false);
        if(yummnCourtyard.gameObject.activeSelf||!gardenMapping.GetComponent<UnityEngine.UI.Image>().enabled)Debug.LogError("YUMMN_QA: Kait garden was not restored");
        VerifySnowCourtyardPresentation(false);
        Debug.Log("YUMMN_QA_COMPLETE character selection, Ki burst/recovery, buffered actions, skills, art, terrain, effects, tutorial, Kait restoration");
        Application.Quit();
    }

    private IEnumerator VerifyYummnKiFrames(string path)
    {
        foreach(int index in new[]{9,10,11})
        {
            PlayV08Fx(run.katePos,index,KaitDirection.Right,110);
            var layer=index==10?battleUnderEffectLayer:battleEnemyHitLayer;
            var fx=System.Array.Find(layer.GetComponentsInChildren<YummnV08Effect>(),f=>f.gameObject.name=="Yummn V08 "+index);
            if(fx==null||fx.sprite==null||fx.raycastTarget||fx.maskable)Debug.LogError("YUMMN_QA: Ki cue invalid at creation "+index);
            yield return new WaitForSecondsRealtime(.14f);
            if(fx==null||fx.FrameIndex<1||busy)Debug.LogError("YUMMN_QA: Ki cue did not advance or blocked input "+index);
            CaptureCanvasToPng(path+".ki-"+index+".png");
            yield return new WaitForSecondsRealtime(.6f);
            if(fx!=null)Debug.LogError("YUMMN_QA: Ki cue did not expire "+index);
        }
        Debug.Log("YUMMN_KI_FRAMES_QA_COMPLETE KiA ExhaustC RecoverB");
    }

    private IEnumerator VerifyYummnShadowFrames(string path)
    {
        run.Yummn.darkness=new Vector2Int(2,3);RefreshAll();
        yield return new WaitForSecondsRealtime(.2f);
        if(!(yummnDarkness is YummnV08Effect dark)||dark.FrameIndex<1)Debug.LogError("YUMMN_QA: darkness not animated");
        int active=0;foreach(var s in yummnShadows)if(s!=null&&s.gameObject.activeInHierarchy){active++;if(s.sprite==null||s.raycastTarget||s.maskable)Debug.LogError("YUMMN_QA: invalid shadow cell");}
        if(active==0)Debug.LogError("YUMMN_QA: shadow cells missing");
        CaptureCanvasToPng(path+".shadow-terrain.png");
        foreach(int index in new[]{6,13})
        {
            PlayV08Fx(run.katePos,index,KaitDirection.Right,104);
            var layer=index==6?battleUnderEffectLayer:battleEnemyHitLayer;
            var fx=System.Array.Find(layer.GetComponentsInChildren<YummnV08Effect>(),f=>f.gameObject.name=="Yummn V08 "+index);
            yield return new WaitForSecondsRealtime(.14f);
            if(fx==null||fx.sprite==null||fx.FrameIndex<1||busy)Debug.LogError("YUMMN_QA: shadow one-shot invalid "+index);
            CaptureCanvasToPng(path+".shadow-"+index+".png");
            yield return new WaitForSecondsRealtime(.6f);if(fx!=null)Debug.LogError("YUMMN_QA: shadow one-shot did not expire");
        }
        if(yummnDarkness==null||!yummnDarkness.gameObject.activeInHierarchy)Debug.LogError("YUMMN_QA: darkness loop expired");
        run.Yummn.darkness=YummnRun.NoCell;RefreshAll();
        if(yummnDarkness.gameObject.activeSelf)Debug.LogError("YUMMN_QA: darkness remained after removal");
        Debug.Log("YUMMN_SHADOW_QA_COMPLETE TeleportB DarknessB AimDeniedC ShadowA");
    }

    private IEnumerator VerifyYummnIcePalmFrames(string path)
    {
        run.Yummn.icePillar=new Vector2Int(2,2);run.Yummn.palmEnemyId=992;RefreshAll();
        yield return new WaitForSecondsRealtime(.9f);
        if(!(yummnIce is YummnV08Effect ice)||ice.FrameIndex!=7)Debug.LogError("YUMMN_QA: ice did not remain at complete frame");
        if(!(yummnPalm is YummnV08Effect)||!yummnPalm.gameObject.activeSelf||yummnPalm.transform.parent!=battleEnemyHitLayer)Debug.LogError("YUMMN_QA: palm marker missing or behind enemy");
        CaptureCanvasToPng(path+".ice-palm.png");
        foreach(int index in new[]{5,8})
        {
            PlayV08Fx(new Vector2Int(4,3),index,KaitDirection.Right,85);
            yield return new WaitForSecondsRealtime(.14f);CaptureCanvasToPng(path+".ice-palm-"+index+".png");
            yield return new WaitForSecondsRealtime(.5f);
        }
        run.Yummn.icePillar=YummnRun.NoCell;run.Yummn.palmEnemyId=-1;RefreshAll();
        if(yummnIce.gameObject.activeSelf||yummnPalm.gameObject.activeSelf)Debug.LogError("YUMMN_QA: persistent terrain not hidden on removal");
        Debug.Log("YUMMN_ICE_PALM_QA_COMPLETE PillarC ShatterA PalmMarkB PalmBurstA");
    }

    private IEnumerator VerifyYummnElementFrames(string path)
    {
        run.SelectCharacter(KaitCharacter.Yummn,707,new YummnRulesSnapshot());run.StateCommitted=null;run.enemies.Clear();run.spawns.Clear();
        run.enemies.Add(new KaitEnemy{id=992,type=KaitEnemyType.Guard,pos=new Vector2Int(4,3),hp=5,maxHp=5,life=KaitEnemyLife.Active});
        ConfigureCharacterVisuals();EnsureKaitSpine();RefreshAll();yield return new WaitForSecondsRealtime(.8f);
        foreach(int index in new[]{0,1,3,4})
        {
            PlayV08Fx(new Vector2Int(4,3),index,KaitDirection.Right,90);
            var fx=battleEnemyHitLayer.GetComponentInChildren<YummnV08Effect>();
            if(fx==null||fx.sprite==null){Debug.LogError("YUMMN_QA: element clip missing");continue;}
            var first=fx.sprite;
            yield return new WaitForSecondsRealtime(YummnV08Effect.ClipDuration(index)*.28f);
            if(fx==null||fx.sprite==first||fx.FrameIndex<1)Debug.LogError("YUMMN_QA: element clip did not advance");
            CaptureCanvasToPng(path+".element-"+YummnV08Effect.SelectedClip(index)+".png");
            if(busy)Debug.LogError("YUMMN_QA: element effect locked input");
            yield return new WaitForSecondsRealtime(.7f);
            if(fx!=null)Debug.LogError("YUMMN_QA: element effect did not expire");
        }
        Debug.Log("YUMMN_ELEMENT_FRAMES_QA_COMPLETE WaterC AirA WinterA FireA");
    }

    private IEnumerator VerifyYummnSelectedFrames(string path)
    {
        run.SelectCharacter(KaitCharacter.Yummn,707,new YummnRulesSnapshot());run.StateCommitted=null;
        run.enemies.Clear();run.spawns.Clear(); // SelectCharacter already resets the position to the open center.
        run.enemies.Add(new KaitEnemy{id=991,type=KaitEnemyType.Guard,pos=new Vector2Int(4,3),hp=5,maxHp=5,life=KaitEnemyLife.Active});
        ConfigureCharacterVisuals();EnsureKaitSpine();RefreshAll();
        yield return new WaitForSecondsRealtime(.8f);
        foreach(int kind in new[]{0,1,3,4})
        {
            PlayYummnFx(kind==4?run.katePos:new Vector2Int(4,3),kind,KaitDirection.Right);
            var fx=System.Array.Find(battleEnemyHitLayer.GetComponentsInChildren<YummnEffectGraphic>(),f=>f.kind==kind);
            if(fx==null||fx.sprite==null){Debug.LogError("YUMMN_QA: selected frame effect missing "+kind);continue;}
            var first=fx.sprite;
            if(fx.raycastTarget||fx.maskable||busy)Debug.LogError("YUMMN_QA: selected effect blocks input or uses mask");
            yield return new WaitForSecondsRealtime(YummnEffectGraphic.Duration(kind)*.3f);
            if(fx==null||fx.sprite==first||fx.FrameIndex<1)Debug.LogError("YUMMN_QA: selected clip is not advancing "+kind);
            CaptureCanvasToPng(path+".selected-"+YummnEffectGraphic.SelectedClip(kind)+".png");
            yield return new WaitForSecondsRealtime(.7f);
            if(fx!=null)Debug.LogError("YUMMN_QA: selected effect did not expire "+kind);
        }
        PlayYummnFx(run.katePos,4,KaitDirection.Right);
        int previousTurn=run.turn;HandleDirection(KaitDirection.Up);
        if(run.turn<=previousTurn)Debug.LogError("YUMMN_QA: selected effect blocked movement");
        while(busy)yield return null;
        Debug.Log("YUMMN_SELECTED_FRAMES_QA_COMPLETE four animated clips, placement captures, lifetime and nonblocking input");
    }

    private IEnumerator VerifyYummnMovementFrames(string path)
    {
        var origin=run.katePos;
        foreach(var direction in new[]{KaitDirection.Right,KaitDirection.Up,KaitDirection.Left,KaitDirection.Down})
        {
            var target=origin+KaitRun.Delta(direction)*2;
            displayKate=origin;RefreshBattle();
            bool done=false;int intermediates=0;float previous=-1;bool captured=false;
            StartCoroutine(RunPhase(AnimateYummnMove(origin,target,YummnMoveCause.Player,direction),()=>done=true));
            while(!done)
            {
                yield return new WaitForEndOfFrame();
                var a=YummnCellPosition(origin);var delta=YummnCellPosition(target)-a;
                float actual=Vector3.Dot(kaitSpine.Root.position-a,delta)/delta.sqrMagnitude;
                if(actual+.001f<previous)Debug.LogError("YUMMN_QA: movement snapped backward after LateUpdate");
                if(yummnMoving&&Vector3.Distance(kaitSpine.Root.position,KaitVisualPosition(origin))>.1f)Debug.LogError("YUMMN_QA: late positioning overwrote interpolation");
                if(actual>.01f&&actual<.99f)intermediates++;
                if(!captured&&actual>.25f&&actual<.9f){CaptureCanvasToPng(path+".move-"+direction+".png");captured=true;}
                previous=actual;
                // An unrelated refresh must also retain the in-flight pose.
                RefreshBattle();
            }
            if(intermediates<2||Vector3.Distance(kaitSpine.Root.position,YummnCellPosition(target))>.1f)Debug.LogError("YUMMN_QA: missing smooth intermediate positions");
            if(kaitSpine.CurrentAnimation.Animation.Name!="01_run"||kaitSpine.CurrentAnimation.TimeScale!=1)Debug.LogError("YUMMN_QA: run clip or speed changed");
            Debug.Log("YUMMN_MOVE_VERIFIED "+direction+" intermediateFrames="+intermediates);
        }
        displayKate=null;RefreshBattle();
        var interrupted=StartCoroutine(AnimateYummnMove(origin,origin+Vector2Int.left,YummnMoveCause.Player,KaitDirection.Left));
        yield return null;StopCoroutine(interrupted);ResetInterruptedAnimationState();displayKate=null;RefreshBattle();
        yield return new WaitForEndOfFrame();
        if(yummnMoving||Vector3.Distance(kaitSpine.Root.position,YummnCellPosition(origin))>.1f)Debug.LogError("YUMMN_QA: stale movement after reset");
        kaitSpine.PlayLoop(KaitSpineView.Idle);
    }

    private void VerifySnowCourtyardPresentation(bool snow)
    {
        int edges=0;
        if(snow&&yummnCourtyard.sprite!=YummnSnowCourtyard.Load("Ground"))Debug.LogError("YUMMN_QA: wrong snow ground");
        for(int y=1;y<=5;y++)for(int x=1;x<=5;x++)
        {
            int index=x+y*KaitRun.BattleSize;
            var floor=battleCellTiles[index];
            if(floor.sprite!=(snow?YummnSnowCourtyard.Load("Floor"):originalYummnFloor)||floor.color!=Color.white)
                Debug.LogError("YUMMN_QA: floor material or color regression");
            if(battleObstacles[index].sprite!=(snow?YummnSnowCourtyard.Load("Pillar"):originalYummnWall))
                Debug.LogError("YUMMN_QA: obstacle material regression");
            if(battleCells[index].rectTransform.rect.size!=new Vector2(120,120))Debug.LogError("YUMMN_QA: changed cell dimensions");
            var edge=yummnSnowEdges[index];
            if(edge!=null&&edge.activeSelf)
            {
                edges++;
                if(edge.transform.GetSiblingIndex()>=battleWarningLines[index].transform.parent.GetSiblingIndex())
                    Debug.LogError("YUMMN_QA: snow covers attack warning");
            }
        }
        if(edges!=(snow?16:0))Debug.LogError("YUMMN_QA: snow perimeter or character restoration failed");
    }
}
