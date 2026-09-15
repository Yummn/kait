using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class KaitGame
{
    private void DragRepoolCard(KaitSkillCard card)
    {
        var e=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){pointerId=-1,button=UnityEngine.EventSystems.PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,card.Rect.position)};
        card.OnPointerDown(e);card.OnBeginDrag(e);
        e.position=RectTransformUtility.WorldToScreenPoint(null,card.Rect.parent.TransformPoint(KaitSkillDeck.CastZone.center));
        card.OnDrag(e);card.OnPointerUp(e);card.OnEndDrag(e);card.OnPointerClick(e);
    }
    // Enter after ordinary UI setup, which cancels startup coroutines.
    private IEnumerator VerifyRepool()
    {
        while(gameContent==null)yield return null;
        yield return new WaitForSecondsRealtime(.6f);
        if(mainMenu!=null)mainMenu.gameObject.SetActive(false);
        gameplayRoot.SetActive(true);
        run.SelectCharacter(KaitCharacter.Yummn,9102);ConfigureCharacterVisuals();run.StateCommitted=null;EnsureKaitSpine();
        run.enemies.Clear();run.spawns.Clear();Array.Clear(run.threat,0,run.threat.Length);
        run.skills.AddRange(new[]{KaitSkill.ShapeIce,KaitSkill.MendWait,KaitSkill.UniqueDecoy,KaitSkill.EchoStep});
        run.passives.AddRange(new[]{KaitPassive.StunStrike,KaitPassive.EchoReprisal});
        run.config.playerInvincible=true;
        var enemy=new KaitEnemy{id=9876,type=KaitEnemyType.Grunt,pos=new Vector2Int(4,3),hp=4,maxHp=4,life=KaitEnemyLife.Active};
        run.enemies.Add(enemy);RefreshAll();
        if(CommandLineValue("-storybook0915QA")=="1")
        {yield return VerifyStorybook0915();Application.Quit();yield break;}
        if(CommandLineValue("-storybook0914QA")=="1")
        {yield return VerifyStorybook0914();Application.Quit();yield break;}
        if(CommandLineValue("-storybook0913QA")=="1")
        {yield return VerifyStorybook0913();Application.Quit();yield break;}
        if(CommandLineValue("-mobileLayoutQA")=="1")
        {yield return VerifyMobileLayout0912();Application.Quit();yield break;}
        if(CommandLineValue("-storybook098QA")=="1")
        {yield return VerifyStorybook098();Application.Quit();yield break;}
        if(CommandLineValue("-cards096QA")=="1")
        {
            mainMenu.gameObject.SetActive(true);gameplayRoot.SetActive(false);
            yield return null;CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/cards096-home.png");
            mainMenu.LibraryButton.onClick.Invoke();yield return null;
            var library=canvas.GetComponentInChildren<KaitCardLibrary>();
            foreach(var character in new[]{KaitCharacter.Kait,KaitCharacter.Yummn})
            {
                library.Select(character,-1);yield return null;
                CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/cards096-"+character+".png");
                for(int rarity=0;rarity<3;rarity++)
                {
                    library.Select(character,rarity);int pages=(library.Total+5)/6;
                    for(int page=0;page<pages;page++)
                    {
                        yield return null;Canvas.ForceUpdateCanvases();
                        foreach(var label in library.GetComponentsInChildren<Text>())
                            if(!string.IsNullOrWhiteSpace(label.text)&&label.cachedTextGenerator.vertexCount==0)Debug.LogError("CARDS096_QA: missing text "+label.text);
                        library.ChangePage(1);
                    }
                }
            }
            library.Select(KaitCharacter.Yummn,2);yield return null;CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/cards096-rare.png");
            Destroy(library.gameObject);mainMenu.gameObject.SetActive(false);gameplayRoot.SetActive(true);
            yield return null;
            var active=KaitSkillCard.Create(gameContent as RectTransform,styleSplit,threatBoardFont,null,null,null,null,null);
            active.Show(KaitSkill.ShapeIce,true,new Vector2(-330,0),0);active.PreviewAt(new Vector2(-330,0));
            var passive=KaitPassiveCard.Create(gameContent as RectTransform,styleSplit,threatBoardFont,null,null,null,null);
            passive.Show(KaitPassive.Opportunist,true,new Vector2(-100,0),0);passive.ApplyDefinition(YummnCatalog.Cards.Find(d=>d.passive==KaitPassive.Opportunist));
            yield return new WaitForSecondsRealtime(.5f);CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/cards096-in-game.png");
            Debug.Log("CARDS096_QA_COMPLETE");Application.Quit();yield break;
        }
        if(CommandLineValue("-merge091QA")=="1")
        {
            run.enemies.Clear();run.skills.Clear();run.passives.Clear();
            run.passives.AddRange(new[]{KaitPassive.SpellEcho,KaitPassive.ResonanceCrystal});
            run.threat[0,2]=run.threat[1,2]=2;run.threat[0,1]=run.threat[0,3]=4;
            RefreshAll();HandleDirection(KaitDirection.Right);
            while(busy)yield return null;
            yield return new WaitForSecondsRealtime(.5f);
            // Normal movement supply may put a new 2 into the vacated source.
            if(run.threat[4,2]!=4||run.threat[4,1]!=8||(run.threat[4,3]!=0&&run.threat[4,3]!=2))
                Debug.LogError("MERGE091_QA: wrong result or new result consumed");
            CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/merge091-result.png");
            Debug.Log("MERGE091_QA_COMPLETE");Application.Quit();yield break;
        }
        if(CommandLineValue("-victorySmileQA")=="1")
        {
            typeof(KaitRun).GetProperty("ended").SetValue(run,true);
            typeof(KaitRun).GetProperty("won").SetValue(run,true);
            ShowEnd();var victory=kaitSpine.CurrentAnimation;
            if(victory.Animation.Name!=KaitSpineView.YummnVictory||endOverlay.activeSelf)
                Debug.LogError("VICTORY_QA: smile missing or covered at start");
            ShowEnd();
            if(kaitSpine.CurrentAnimation!=victory)Debug.LogError("VICTORY_QA: duplicate result restarted smile");
            yield return new WaitForSecondsRealtime(victory.Animation.Duration*.45f);
            if(endOverlay.activeSelf||victory.TrackTime<=0)Debug.LogError("VICTORY_QA: animation hidden or not advancing");
            CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/victory-smile-playing.png");
            while(!victoryPresentationComplete)yield return null;
            if(!endOverlay.activeSelf||!victory.IsComplete||victory.Next!=null||!victory.Loop)
                Debug.LogError("VICTORY_QA: results appeared before completed terminal pose");
            CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/victory-smile-results.png");
            kaitSpine.RefreshRestPose();
            yield return new WaitForSecondsRealtime(victory.Animation.Duration*1.1f);
            ShowEnd();
            if(kaitSpine.CurrentAnimation!=victory||victory.TrackTime<victory.Animation.Duration*2f||!victory.Loop)
                Debug.LogError("VICTORY_QA: smile did not repeat or was replaced after results");
            Debug.Log("VICTORY_LOOP: cycles="+(victory.TrackTime/victory.Animation.Duration));
            ResetRunPresentation();
            if(victoryPresentationStarted||victoryPresentationComplete)Debug.LogError("VICTORY_QA: ending state survives reset");
            Debug.Log("VICTORY_SMILE_QA_COMPLETE");Application.Quit();yield break;
        }
        if(CommandLineValue("-darknessQA")=="1")
        {
            run.skills.Clear();run.skills.Add(KaitSkill.Darkness);RefreshAll();
            HandleSkillCardCast(0);yield return null;
            CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/darkness-targets.png");
            HandleBattleCellClick(enemy.pos);
            while(busy)yield return null;
            yield return new WaitForSecondsRealtime(.25f);RefreshAll();
            if(run.Yummn.darkness!=enemy.pos||yummnDarkness==null||yummnDarkness.transform.parent!=battleEffectLayer||battleEffectLayer.GetSiblingIndex()<=battleKaitLayer.GetSiblingIndex())
                Debug.LogError("DARKNESS_QA: occupied target or upper layer failed");
            CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/darkness-enemy.png");
            HandleSkillCardCast(0);HandleBattleCellClick(run.katePos);
            while(busy)yield return null;
            yield return new WaitForSecondsRealtime(.25f);RefreshAll();
            if(run.Yummn.darkness!=run.katePos)Debug.LogError("DARKNESS_QA: self target failed");
            CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/darkness-self.png");
            Debug.Log("DARKNESS_QA_COMPLETE");Application.Quit();yield break;
        }
        if(CommandLineValue("-dragTargetQA")=="1")
        {
            run.skills.Clear();run.skills.AddRange(new[]{KaitSkill.MendWait,KaitSkill.ShapeIce,KaitSkill.FrostBreath});RefreshAll();
            var origin=run.katePos;int before=run.turn;
            var card=skillDeck.Owned[0];
            card.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){pointerId=-1});
            if(targetingSkill!=KaitSkill.None||run.turn!=before)Debug.LogError("REPOOL_QA: click armed or cast");
            DragRepoolCard(card);
            if(targetingSkill!=KaitSkill.MendWait)Debug.LogError("REPOOL_QA: drag did not arm heal");
            foreach(var b in yummnCastDirections)if(b!=null&&b.gameObject.activeSelf)Debug.LogError("REPOOL_QA: heal showed direction selectors");
            yield return new WaitForSecondsRealtime(.3f);CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/drag-target-self.png");
            battleCells[origin.x+origin.y*KaitRun.BattleSize].GetComponent<Button>().onClick.Invoke();while(busy)yield return null;
            if(run.turn!=before+1||run.katePos!=origin||targetingSkill!=KaitSkill.None)Debug.LogError("REPOOL_QA: self confirmation failed");
            run.Yummn.ki=6;RefreshAll();DragRepoolCard(skillDeck.Owned[1]);
            var cell=new Vector2Int(2,5);battleCells[cell.x+cell.y*KaitRun.BattleSize].GetComponent<Button>().onClick.Invoke();while(busy)yield return null;
            if(targetingSkill!=KaitSkill.None||run.katePos!=origin)Debug.LogError("REPOOL_QA: ground confirmation failed");
            yield return new WaitForSecondsRealtime(.3f);CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/drag-target-ground.png");
            run.Yummn.ki=6;RefreshAll();DragRepoolCard(skillDeck.Owned[2]);
            if(targetingSkill!=KaitSkill.FrostBreath||!yummnCastDirections[0].gameObject.activeSelf)Debug.LogError("REPOOL_QA: directional drag failed");
            yield return new WaitForSecondsRealtime(.3f);CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/drag-target-direction.png");
            Debug.Log("REPOOL_QA_COMPLETE drag only, self, ground and direction targets");Application.Quit();yield break;
        }
        if(CommandLineValue("-punchUnifiedQA")=="1")
        {
            run.skills.Clear();run.skills.Add(KaitSkill.FrostBreath);run.Yummn.ki=6;RefreshAll();
            var origin=run.katePos;int turnBefore=run.turn;
            HandleSkillCardCast(0);HandleDirection(KaitDirection.Left);
            if(run.katePos!=origin||run.turn!=turnBefore||targetingSkill!=KaitSkill.FrostBreath)Debug.LogError("REPOOL_QA: selection allowed movement");
            yield return new WaitForSecondsRealtime(.4f);
            CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/punch-targets.png");
            yummnCastDirections[0].onClick.Invoke();
            if(targetingSkill!=KaitSkill.None||run.Yummn.prepared.Count>0||run.turn!=turnBefore)Debug.LogError("REPOOL_QA: failed cast remained prepared");
            yield return new WaitForSecondsRealtime(.15f);
            CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/punch-cast-failure.png");
            HandleSkillCardCast(0);yummnCastDirections[1].onClick.Invoke();while(busy)yield return null;
            if(!enemy.yummnFrozen||run.katePos!=origin)Debug.LogError("REPOOL_QA: frost target failed or moved player");
            var cards=new KaitRewardPack();cards.choices.Add(YummnCatalog.Get("R01"));cards.choices.Add(YummnCatalog.Get("S06"));cards.choices.Add(YummnCatalog.Get("R06"));run.rewardQueue.Enqueue(cards);RefreshAll();
            yield return new WaitForSecondsRealtime(.5f);CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/punch-card-fonts.png");
            Debug.Log("REPOOL_QA_COMPLETE unified casting, input isolation, failure and card fonts");Application.Quit();yield break;
        }
        if(CommandLineValue("-cardNamesQA")=="1")
        {
            var gallery=Rect("Card Icon Audit",canvas.transform,Vector2.zero,new Vector2(1920,1080),new Color(.88f,.86f,.82f,1));
            for(int i=0;i<YummnCatalog.Cards.Count;i++)
            {
                var d=YummnCatalog.Cards[i];var p=new Vector2((i%7-3)*260,440-i/7*174);
                var icon=Rect(d.id,gallery.transform,p,new Vector2(110,110),Color.white);
                icon.sprite=KaitCardSkin.Icon(d);icon.preserveAspect=true;
                if(YummnRepoolArt.NewIcon(d)==null)icon.material=YummnV08Art.Material;
                MakeText(d.nameZh,gallery.transform,p+new Vector2(0,-72),new Vector2(245,34),22,Color.black,TextAnchor.MiddleCenter,FontStyle.Bold,false);
            }
            yield return new WaitForSecondsRealtime(.5f);
            CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/formal-icon-gallery.png");
            Destroy(gallery.gameObject);yield return null;
            var names=new KaitRewardPack();names.choices.Add(YummnCatalog.Get("R02"));names.choices.Add(YummnCatalog.Get("O02"));names.choices.Add(YummnCatalog.Get("R34"));run.rewardQueue.Enqueue(names);RefreshAll();
            yield return new WaitForSecondsRealtime(.5f);
            CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/formal-long-names.png");
            Debug.Log("REPOOL_QA_COMPLETE formal names and 38 icon audit");Application.Quit();yield break;
        }
        yield return new WaitForSecondsRealtime(.8f);
        for(int i=0;i<4;i++)skillDeck.Owned[i].PreviewAt(new Vector2((i-1.5f)*215,-210));
        var pack=new KaitRewardPack();pack.choices.Add(YummnCatalog.Get("R26"));pack.choices.Add(YummnCatalog.Get("R39"));pack.choices.Add(YummnCatalog.Get("R22"));
        run.rewardQueue.Enqueue(pack);RefreshAll();
        yield return new WaitForSecondsRealtime(.6f);
        CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/repool-cards.png");
        typeof(KaitRewardDeck).GetMethod("Select",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(rewardDeck,new object[]{0});
        yield return new WaitForSecondsRealtime(.3f);
        CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/repool-six-slots.png");
        run.rewardQueue.Clear();RefreshAll();
        HandleDirection(KaitDirection.Right);while(busy)yield return null;
        yield return new WaitForSecondsRealtime(.15f);
        if(!enemy.yummnStunned)Debug.LogError("REPOOL_QA: Stun did not reach actor");
        if(EnemyStillFrozen(enemy.id))Debug.LogError("REPOOL_QA: Stun incorrectly uses ice visuals");
        for(int i=16;i<=21;i++)
        {
            var p=i==16||i==17||i==20?enemy.pos:run.katePos;
            PlayV08Fx(p,i,KaitDirection.Right,100);
            yield return new WaitForSecondsRealtime(.16f);
            CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/repool-vfx-"+i+".png");
            yield return new WaitForSecondsRealtime(.35f);
        }
        var previous=SnapshotEnemies();var pending=SnapshotSpawns();var start=run.katePos;
        if(run.TryUseSkillAt(KaitSkill.ShapeIce,new Vector2Int(2,2),out _))
        {yield return PlayTurn(run.lastSkillResult,start,previous,pending);}
        previous=SnapshotEnemies();pending=SnapshotSpawns();
        if(run.TryUseSkillAt(KaitSkill.UniqueDecoy,new Vector2Int(2,4),out _))
        {yield return PlayTurn(run.lastSkillResult,run.katePos,previous,pending);}
        yield return new WaitForSecondsRealtime(.5f);CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/repool-terrain.png");
        Debug.Log("REPOOL_QA_COMPLETE cards, six mixed targets, real stun, six selected clips, ice and decoy");
        Application.Quit();
    }
}
