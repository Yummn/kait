using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

public sealed partial class KaitGame
{
    private GameObject characterSelection;
    private readonly YummnPunchPresentation yummnPunchPose=new YummnPunchPresentation();
    private KaitDirection? yummnBufferedDirection;
    private int yummnBufferedTurn;
    private int yummnBufferedAction;
    private bool yummnAcceptBuffer;
    private Sprite originalYummnFloor;
    private Sprite originalYummnWall;
    private readonly GameObject[] yummnSnowEdges = new GameObject[KaitRun.BattleSize * KaitRun.BattleSize];
    private HybridStyleGraphic yummnHud;
    private Image[] actionPips=new Image[0];
    private YummnKiWisp[] kiWisps=new YummnKiWisp[0];
    private readonly Image[] buffPips=new Image[6];
    private readonly Dictionary<string,Sprite> yummnSprites=new Dictionary<string,Sprite>();
    private Image bossPendingMarker;
    private Image yummnCourtyard;
    private void ShowCharacterSelection()
    {
        yummnBufferedDirection=null;
        if(characterSelection!=null){characterSelection.SetActive(false);Destroy(characterSelection);}
        var selector=KaitCharacterSelection.Create(canvas.transform,threatBoardFont,PlayerPrefs.HasKey);
        characterSelection=selector.gameObject;
        selector.StartCharacter=StartSelectedCharacter;
        selector.ContinueCharacter=ResumeCharacter;
        selector.Back=()=>characterSelection.SetActive(false);
    }
    private void StartSelectedCharacter(KaitCharacter c)
    {
        run.config.playerInvincible=PlayerPrefs.GetInt(PlayerInvinciblePreference,0)!=0;
        run.config.enableRiftDamage=PlayerPrefs.GetInt(DisableRiftDamagePreference,0)==0;
        run.config.enableFriendlyFire=PlayerPrefs.GetInt(DisableFriendlyFirePreference,0)==0;
        run.config.enableCollisionDamage=PlayerPrefs.GetInt(DisableCollisionDamagePreference,0)==0;
        run.config.enableThreatPillars=c==KaitCharacter.Yummn||!PlayerPrefs.GetInt(DisableThreatPillarsPreference,0).Equals(1);
        run.config.winValue=128;
        run.config.kaitEffectiveMoveSupply=PlayerPrefs.GetInt(KaitEffectiveMoveSupplyPreference,0)==1;
        run.SelectCharacter(c,System.Environment.TickCount,YummnPreset());
        PlayerPrefs.SetInt("Kait.Character",(int)c);PlayerPrefs.Save();
        if(characterSelection!=null)characterSelection.SetActive(false);StartFromMainMenu();
    }
    private void ConfigureCharacterVisuals()
    {
        ClearYummnLogicalGhosts();
        yummnPunchPose.Reset();
        yummnMoving=false;StopYummnThreatPulses();
        yummnBufferedDirection=null;GameAudio.YummnMode=run.IsYummn;
        run.StateCommitted=SaveCharacterRun;
        disableThreatPillarsToggle?.SetIsOnWithoutNotify(!run.config.enableThreatPillars);
        if(disableThreatPillarsToggle!=null)disableThreatPillarsToggle.interactable=!run.IsYummn;
        foreach(var toggle in new[]{disableRiftDamageToggle,disableFriendlyFireToggle,disableCollisionDamageToggle})if(toggle!=null)toggle.interactable=!run.IsYummn;
        disableRiftDamageToggle?.SetIsOnWithoutNotify(run.IsYummn||!run.config.enableRiftDamage);
        disableFriendlyFireToggle?.SetIsOnWithoutNotify(run.IsYummn||!run.config.enableFriendlyFire);
        disableCollisionDamageToggle?.SetIsOnWithoutNotify(run.IsYummn||!run.config.enableCollisionDamage);
        if(rewardDeck!=null)rewardDeck.SelectionStarted=()=>yummnBufferedDirection=null;
        if(tutorialOverlay!=null){var book=tutorialOverlay.GetComponent<KaitTutorialBook>();book.YummnRules=run.Yummn.rules;book.YummnMode=run.IsYummn;book.ShowPage(book.PageIndex);}
        RefreshCharacterSettings();
        makotoSkeletonData=Resources.Load<SkeletonDataAsset>(run.IsYummn?"Characters/Yummn/108231_SkeletonData":"Characters/Makoto/Makoto_SkeletonData");
        kaitSpine?.Destroy();kaitSpine=null;
        if(originalYummnFloor==null)originalYummnFloor=dungeonFloorSprite;
        if(originalYummnWall==null)originalYummnWall=dungeonWallSprite;
        dungeonFloorSprite=run.IsYummn ? YummnSnowCourtyard.Load("Floor") ?? originalYummnFloor : originalYummnFloor;
        dungeonWallSprite=run.IsYummn ? YummnSnowCourtyard.Load("Pillar") ?? originalYummnWall : originalYummnWall;
        if(gardenMapping!=null)
        {
            var ground=gardenMapping.GetComponent<Image>();ground.enabled=!run.IsYummn;
            if(run.IsYummn&&yummnCourtyard==null)
            {
                yummnCourtyard=Rect("Yummn Courtyard",gardenMapping,Vector2.zero,Vector2.zero,Color.white);
                yummnCourtyard.sprite=YummnSnowCourtyard.Load("Ground") ?? YummnSprite("Courtyard");yummnCourtyard.raycastTarget=false;
                // Cover the left surface at the source aspect ratio, never squash its circles.
                var fit=yummnCourtyard.gameObject.AddComponent<AspectRatioFitter>();
                fit.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio=yummnCourtyard.sprite.rect.width/yummnCourtyard.sprite.rect.height;
            }
            if(yummnCourtyard!=null)yummnCourtyard.gameObject.SetActive(run.IsYummn);
            foreach(var decal in gardenMapping.GetComponentsInChildren<KaitGroundDecal>(true))decal.gameObject.SetActive(!run.IsYummn);
        }
        if(layeredGarden!=null){layeredGarden.enabled=!run.IsYummn;var decor=layeredGarden.transform.Find("Garden Decorations");if(decor!=null)decor.gameObject.SetActive(!run.IsYummn);}
        // Decorations live in a separate actor-level root in the layered garden layout.
        foreach(var rt in canvas.GetComponentsInChildren<RectTransform>(true))if(rt.name=="Garden Decorations")rt.gameObject.SetActive(!run.IsYummn);
        for(int y=1;y<=5;y++)for(int x=1;x<=5;x++)
        {
            int index=x+y*KaitRun.BattleSize;
            if(run.IsYummn&&yummnSnowEdges[index]==null)
                yummnSnowEdges[index]=YummnSnowCourtyard.CreateSnowEdges(battleCells[index].transform,x,y);
            if(yummnSnowEdges[index]!=null)yummnSnowEdges[index].SetActive(run.IsYummn);
            battleObstacles[index].sprite=dungeonWallSprite;
            YummnSnowCourtyard.SetWallGrounding(battleObstacles[index],battleObstacleShadows[index].GetComponent<KaitSoftShadow>(),run.IsYummn&&dungeonWallSprite!=originalYummnWall);
        }
    }
    private void SaveCharacterRun()
    {
        string key=run.IsYummn?(run.Yummn.rules.Is082?"Kait.Run.Yummn.0.8.2":run.Yummn.rules.Legacy?"Kait.Run.Yummn.0.8":"Kait.Run.Yummn.0.8.1"):"Kait.Run.Kait";
        if(run.ended){PlayerPrefs.DeleteKey(key);RecordCharacterScore();}
        else PlayerPrefs.SetString(key,run.SaveReplay());
        PlayerPrefs.Save();
    }
    private void ResumeCharacter(string key)
    {
        string saved=PlayerPrefs.GetString(key,"");
        NewRun();
        if(!run.RestoreReplay(saved)){NewRun();statusText.text="存档与当前规则不匹配";ShowMainMenu();return;}
        if(characterSelection!=null)characterSelection.SetActive(false);mainMenu.gameObject.SetActive(false);gameplayRoot.SetActive(true);
        mainMenu.Select(run.Character);
        ConfigureCharacterVisuals();EnsureKaitSpine();RefreshAll();menuClosedFrame=Time.frameCount;
    }
    private Sprite YummnSprite(string name)
    {
        if(yummnSprites.TryGetValue(name,out var sprite))return sprite;
        var t=Resources.Load<Texture2D>("KaitVisuals/Yummn/"+name);
        if(t==null)return null;
        return yummnSprites[name]=Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f,100);
    }
    private void RefreshYummnKiDisplay()
    {
        if(run.IsYummn)kaitSpine?.SetYummnKiState(run.ExactKi>0);
        if(yummnHud==null)
        {
            yummnHud=MakeHybridSurface("Yummn Ki",turnText.transform.parent,new Vector2(0,-80),new Vector2(250,90),dungeonPanelSprite,Panel,5f,10f);
            yummnHud.raycastTarget=false;
        }
        if(actionPips.Length!=run.Yummn.profile.maxKi)
        {
            foreach(var pip in actionPips)if(pip!=null){pip.gameObject.SetActive(false);if(Application.isPlaying)Destroy(pip.gameObject);else DestroyImmediate(pip.gameObject);}
            actionPips=new Image[run.Yummn.profile.maxKi];
            kiWisps=new YummnKiWisp[actionPips.Length];
            for(int i=0;i<actionPips.Length;i++)
            {
                actionPips[i]=Rect("Ki "+(i+1),yummnHud.transform,Vector2.zero,new Vector2(22,36),Color.white);
                kiWisps[i]=actionPips[i].gameObject.AddComponent<YummnKiWisp>();
                kiWisps[i].Configure(styleSplit);
            }
        }
        yummnHud.gameObject.SetActive(run.IsYummn);
        bool exhausted=run.KiPhase==YummnPhase.Exhausted;
        for(int i=0;i<actionPips.Length;i++)
        {
            actionPips[i].gameObject.SetActive(i<run.Yummn.profile.maxKi);
            float spacing=Mathf.Min(24,120f/Mathf.Max(1,actionPips.Length-1));
            actionPips[i].rectTransform.localScale=Vector3.one*Mathf.Min(1,(spacing-2)/22f);
            actionPips[i].rectTransform.anchoredPosition=new Vector2((i-(actionPips.Length-1)*.5f)*spacing,0);
            kiWisps[i].SetState(run.ExactKi-i,exhausted);
        }
    }
    private void RefreshYummnHud()
    {
        if(waitButton!=null){waitButton.gameObject.SetActive(run.IsYummn);waitButton.interactable=!busy&&!run.ended;}
        RefreshYummnKiDisplay();
        RefreshYummnTerrain();
        if(!busy)SyncYummnLogicalGhosts();
        if(!run.IsYummn){if(bossPendingMarker!=null)bossPendingMarker.gameObject.SetActive(false);return;}
        if(kaitSpine!=null&&!busy)
        {
            yummnPunchPose.Validate(run);
            kaitSpine.RestAnimation=yummnPunchPose.RestAnimation;
            kaitSpine.RefreshRestPose();
        }
        if(bossPendingMarker==null)
        {
            bossPendingMarker=Rect("Boss Reserved Cell",battleUnderEffectLayer,Vector2.zero,new Vector2(108,108),new Color(1,.73f,.22f,.8f));
            bossPendingMarker.sprite=YummnSprite("Effects/Ward");bossPendingMarker.raycastTarget=false;bossPendingMarker.maskable=false;
        }
        bossPendingMarker.gameObject.SetActive(run.PendingBossCell.x>=0);
        if(run.PendingBossCell.x>=0)bossPendingMarker.rectTransform.position=YummnCellPosition(run.PendingBossCell);
    }
    private IEnumerator PlayYummnTurn(KaitTurnResult r,Vector2Int start,List<KaitEnemy> before,List<KaitSpawnRequest> spawnBefore)
    {
        busy=true;yummnAcceptBuffer=!r.yummnAction.isWait&&r.yummnAction.phaseAtStart==r.yummnAction.phaseAtEnd&&!run.ended;
        if(YummnAudio.HasKiExpenditure(r.yummnAction))YummnAudio.Play("KiSpendB");
        yummnPunchPose.Begin(r);
        if(kaitSpine!=null)kaitSpine.RestAnimation=KaitSpineView.Idle;
        if(yummnVisualIce==YummnRun.NoCell)yummnVisualIce=run.Yummn.icePillar;
        if(!yummnAcceptBuffer)yummnBufferedDirection=null;
        animatedEnemies=before;animatedSpawns=new List<KaitSpawnRequest>(spawnBefore);displayKate=start;displayedThreat=r.threatBefore;
        foreach(var merge in r.merges)
        {
            if(!run.Yummn.rules.Legacy&&merge.systemMerge)continue;
            var p=run.MapThreatToBattle(merge.threatCell);
            int offset=!run.Yummn.rules.Legacy&&run.Yummn.rules.SpawnFromEight?2:1;
            if(merge.resultValue>=run.config.winValue||merge.resultValue<(1<<(offset+1))||merge.spawnSuppressed||animatedSpawns.Exists(s=>s.targetCell==p))continue;
            animatedSpawns.Add(new KaitSpawnRequest{targetCell=p,sourceThreatCell=merge.threatCell,tier=Mathf.Clamp((int)Mathf.Log(merge.resultValue,2)-offset,1,5),createdTurn=run.turn-1,state=KaitSpawnState.Preview});
        }
        yummnVisualIce=r.yummnAction.iceAtStart;yummnVisualDarkness=r.yummnAction.darknessAtStart;yummnVisualDecoy=r.yummnAction.decoyAtStart;
        var opening=r;
        if(r.yummnThreatAfterMerge!=null)
        {
            opening=new KaitTurnResult{threatBefore=r.threatBefore,threatAfter=r.yummnThreatAfterMerge};
            opening.threatMotions.AddRange(r.threatMotions.GetRange(0,r.yummnInitialMotionCount));
            opening.merges.AddRange(r.merges.FindAll(m=>!m.systemMerge));
        }
        RefreshAll();bool threatDone=false;StartCoroutine(RunPhase(AnimateThreat(opening),()=>threatDone=true));
        kaitSpine?.Face(r.kaitDirection);
        if(r.yummnAction.plannedSkills.Contains("yummn.M02")){kaitSpine?.PlayOnce(KaitSpineView.YummnBuff);PlayYummnFx(start,4,r.kaitDirection);}
        bool attackStarted=false,flurryFxPlayed=false,attackVoicePlayed=false;
        int voicedKills=0;
        if(r.yummnAction.plannedSkills.Count>0)GameAudio.PlayKaitSmallAttackSkillVoice();
        for(int i=0;i<r.yummnEvents.Count;i++)
        {
            var ev=r.yummnEvents[i];
            if(ev.kind==YummnEventKind.Move&&ev.targetId<0)
            {
                var from=ev.from;var to=ev.to;
                while(i+1<r.yummnEvents.Count&&r.yummnEvents[i+1].kind==YummnEventKind.Move&&r.yummnEvents[i+1].targetId<0&&r.yummnEvents[i+1].moveCause==ev.moveCause){i++;to=r.yummnEvents[i].to;}
                bool killFollow=ev.moveCause==YummnMoveCause.KillFollow||r.yummnEvents.GetRange(0,i).Exists(e=>e.kind==YummnEventKind.Kill&&e.to==to)&&ev.moveCause==YummnMoveCause.Player;
                yummnPunchPose.ClearReady();
                if(kaitSpine!=null)kaitSpine.RestAnimation=KaitSpineView.Idle;
                yield return AnimateYummnMove(from,to,ev.moveCause,r.kaitDirection,killFollow,r.yummnAction.phaseAtStart);
                displayKate=to;RefreshBattle();
            }
            else if(ev.kind==YummnEventKind.Move)
            {
                var actor=before.Find(e=>e.id==ev.targetId);
                if(actor!=null)
                {
                    yield return AnimateYummnEnemyMove(actor,ev.from,ev.to);
                    actor.pos=ev.to;RefreshBattle();
                }
                if(ev.moveCause==YummnMoveCause.Forced){PlayV08Fx(ev.from,1,r.kaitDirection,88);YummnAudio.Play("Air");}
            }
            else if(ev.kind==YummnEventKind.EnemyAttack)
            {
                StrikeSelectedTelegraphs(ev.affectedCells);
                bool ghostHit=false;
                foreach(var hit in r.yummnEvents)if(hit.kind==YummnEventKind.AfterimageHit&&hit.attackEventId==ev.attackEventId)
                {PlayV08Fx(hit.to,15,r.kaitDirection,90);ghostHit=true;}
                if(ghostHit)YummnAudio.Play("GhostHit");
                yield return AnimateAllEnemyActions(r.enemyActions.FindAll(a=>a.enemyId==ev.sourceId&&a.type!=KaitIntentType.Move));
            }
            else if(ev.kind==YummnEventKind.AfterimageCreated)CreateYummnLogicalGhost(ev.markerId,ev.to,ev.direction.x>0?KaitDirection.Right:ev.direction.x<0?KaitDirection.Left:ev.direction.y>0?KaitDirection.Up:KaitDirection.Down);
            else if(ev.kind==YummnEventKind.AfterimageCleared)RemoveYummnLogicalGhost(ev.markerId,.3f);
            else if(ev.kind==YummnEventKind.Spawn&&run.Yummn.rules.Is082)
            {
                var source=run.enemies.Find(e=>e.id==ev.targetId);
                if(source!=null&&!before.Exists(e=>e.id==ev.targetId))
                {
                    var actor=new KaitEnemy{id=source.id,type=source.type,pos=ev.to,hp=source.maxHp,maxHp=source.maxHp,life=KaitEnemyLife.Active};
                    before.Add(actor);animatedSpawns.RemoveAll(s=>s.targetCell==ev.to);RefreshBattle();
                    PlayEnemyLanding(EnemySpine(actor));GameAudio.PlayEnemySpawnVoice(actor.type,actor.id);
                }
            }
            else if(ev.kind==YummnEventKind.Hit&&ev.targetId>=0)
            {
                if(ev.direction!=Vector2Int.zero&&(ev.damageCause==YummnDamageCause.Punch||ev.damageCause==YummnDamageCause.Counter))
                    kaitSpine?.Face(ev.direction.x>0?KaitDirection.Right:ev.direction.x<0?KaitDirection.Left:ev.direction.y>0?KaitDirection.Up:KaitDirection.Down);
                string attackClip=yummnPunchPose.Hit(ev,attackStarted);
                if(attackClip!=null&&!attackVoicePlayed&&ev.hpAfter>0)
                {GameAudio.PlayKaitNormalAttackVoice();attackVoicePlayed=true;}
                if(kaitSpine!=null)kaitSpine.RestAnimation=yummnPunchPose.RestAnimation;
                if(attackClip!=null){kaitSpine?.PlayOnce(attackClip);attackStarted=true;}
                var actor=before.Find(e=>e.id==ev.targetId);
                if(actor!=null){actor.hp=ev.hpAfter;EnemySpine(actor)?.PlayDamage();if(ev.amount>0)StartCoroutine(FlashEnemyWhite(actor.id));}
                if(actor!=null&&ev.amount>0&&ev.hpAfter>0)GameAudio.PlayEnemyHurt(actor.type,actor.id,ev.hpAfter,false);
                int effect=ev.damageCause==YummnDamageCause.WaterWhip?0:ev.damageCause==YummnDamageCause.WinterBreath?3:ev.damageCause==YummnDamageCause.FireSnake?4:ev.damageCause==YummnDamageCause.Shatter?5:ev.damageCause==YummnDamageCause.Quivering?8:-1;
                if(effect>=0){PlayV08Fx(ev.to,effect,r.kaitDirection,90);YummnAudio.Play(effect==0?"Water":effect==3?"Frost":effect==4?"Fire":effect==5?"Shatter":"PalmSeal");}
                else
                {
                    if(ev.blocked)PlayYummnFx(ev.to,4,KaitRun.Opposite(r.kaitDirection));
                    else if(!r.yummnFlurry||!flurryFxPlayed)
                    {PlayYummnFx(ev.to,r.yummnFlurry?1:0,r.kaitDirection);flurryFxPlayed|=r.yummnFlurry;}
                    YummnAudio.Play(ev.blocked?"Block":"Punch");
                }
                RefreshBattle();yield return new WaitForSecondsRealtime(.075f);
            }
            else if(ev.kind==YummnEventKind.Hit&&ev.targetId<0)
            {
                if(ev.amount>0)GameAudio.PlayKaitDamageVoice(ev.hpAfter,run.config.kateMaxHp);
                if(ev.amount>0){yummnPunchPose.ClearReady();if(kaitSpine!=null)kaitSpine.RestAnimation=KaitSpineView.Idle;StartCoroutine(FlashKaitWhite());kaitSpine?.PlayOnce(ev.hpAfter<=0?KaitSpineView.Die:KaitSpineView.Damage);}
                else if(ev.blocked)
                {
                    var source=before.Find(e=>e.id==ev.sourceId);
                    var incoming=source!=null?source.pos-ev.to:-KaitRun.Delta(r.kaitDirection);
                    var facing=Mathf.Abs(incoming.x)>=Mathf.Abs(incoming.y)?(incoming.x>=0?KaitDirection.Right:KaitDirection.Left):(incoming.y>=0?KaitDirection.Up:KaitDirection.Down);
                    PlayYummnFx(ev.to,4,facing);YummnAudio.Play("Block");
                }
            }
            else if(ev.kind==YummnEventKind.Kill)
            {
                string killClip=yummnPunchPose.Kill();
                if(kaitSpine!=null)kaitSpine.RestAnimation=KaitSpineView.Idle;
                if(killClip!=null)kaitSpine?.PlayOnce(killClip);
                var actor=before.Find(e=>e.id==ev.targetId);
                if(actor!=null){BeginDetachedEnemyDeaths(new List<KaitEnemy>{actor});before.Remove(actor);}
                YummnAudio.Play("Kill");
                if(r.playerKilledEnemyIds.Contains(ev.targetId))GameAudio.PlayYummnKillVoice(++voicedKills);
            }
            else if(ev.kind==YummnEventKind.Resource){PlayV08Fx(displayKate??run.katePos,9,r.kaitDirection,65);YummnAudio.Play("Ki");if(ev.status=="Afterimage")ShowYummnAfterimageKi(ev.amount);}
            else if(ev.kind==YummnEventKind.Terrain)
            {
                if(ev.status=="Ice")
                {
                    kaitSpine?.PlayOnce(KaitSpineView.YummnAttackSkill);yummnVisualIce=ev.to;
                    if(yummnIce is YummnV08Effect pillar)pillar.InitializePersistent(2,false);
                    YummnAudio.Play("Ice");
                }
                if(ev.status=="Darkness"){kaitSpine?.PlayOnce(KaitSpineView.YummnAttackSkill);yummnVisualDarkness=ev.to;YummnAudio.Play("Shadow");}
                if(ev.status=="Decoy"){yummnVisualDecoy=ev.to;kaitSpine?.PlayOnce(KaitSpineView.YummnBuff);}
                if(ev.status=="DecoyExpired")yummnVisualDecoy=YummnRun.NoCell;
                if(ev.status=="DarknessExpired")yummnVisualDarkness=YummnRun.NoCell;
                RefreshYummnTerrain();
            }
            else if(ev.kind==YummnEventKind.Status)
            {
                if(ev.status=="KiGuard"){yummnPunchPose.ClearReady();if(kaitSpine!=null){kaitSpine.RestAnimation=KaitSpineView.Idle;kaitSpine.PlayOnce(KaitSpineView.YummnKiGuard);}}
                if(ev.status=="SupplyReady"||ev.status=="CounterSupplyReady")
                {while(!threatDone)yield return null;yield return AnimateYummn082Supply(r,ev);}
                if(ev.status=="Exhausted"||ev.status=="Burst"){PlayV08Fx(displayKate??run.katePos,ev.status=="Burst"?11:10,r.kaitDirection,110);YummnAudio.Play(ev.status=="Burst"?"Recover":"Exhaust");if(ev.status=="Burst"&&run.HasPassive(KaitPassive.Wholeness))kaitSpine?.PlayOnce(KaitSpineView.YummnHeal);}
                if(ev.status=="Stunned")PlayV08Fx(ev.to,16,r.kaitDirection,90);
                if(ev.status=="IceGuard")PlayV08Fx(ev.to,17,r.kaitDirection,104);
                if(ev.status=="Heal"){PlayV08Fx(ev.to,18,r.kaitDirection,104);kaitSpine?.PlayOnce(KaitSpineView.YummnHeal);}
                if(ev.status=="Reflect")PlayV08Fx(ev.to,20,r.kaitDirection,90);
                if(ev.status=="Deflect")PlayV08Fx(ev.to,21,r.kaitDirection,94);
                if(ev.status=="Frozen")PlayV08Fx(ev.to,5,r.kaitDirection,76);
                if(ev.status=="PalmDetonate")PlayV08Fx(ev.to,8,r.kaitDirection,85);
                if(ev.status=="AimDenied")PlayV08Fx(displayKate??run.katePos,13,r.kaitDirection,46);
                var actor=before.Find(e=>e.id==ev.targetId);
                if(actor!=null)
                {
                    actor.yummnFrozen=ev.status=="Frozen"||actor.yummnFrozen&&ev.status!="IceGuard"&&ev.status!="Shatter";
                    actor.yummnStunned=ev.status=="Stunned"||actor.yummnStunned&&ev.status!="ControlConsumed";
                    actor.frozenActions=actor.yummnStunned||actor.yummnFrozen?1:0;
                }
            }
        }
        while(!threatDone)yield return null;
        if(r.yummnThreatAfterSupply!=null&&!run.Yummn.rules.Is082)
        {
            StopYummnThreatPulses();displayedThreat=r.yummnThreatAfterSupply;RefreshThreat();
            if(r.newThreatCells.Count>0)
            {
                var cells=new List<RectTransform>();foreach(var p in r.newThreatCells)cells.Add(threatCells[p.x+p.y*run.ThreatSize].rectTransform);
                yield return ScalePulseMany(cells,.1f,1.1f,.1f);
            }
            if(r.merges.Exists(m=>m.systemMerge))
            {
                var archive=new KaitTurnResult{threatBefore=r.yummnThreatAfterSupply,threatAfter=r.threatAfter};
                archive.threatMotions.AddRange(r.threatMotions.GetRange(r.yummnInitialMotionCount,r.threatMotions.Count-r.yummnInitialMotionCount));
                archive.merges.AddRange(r.merges.FindAll(m=>m.systemMerge));
                yield return AnimateThreat(archive);
            }
        }
        animatedEnemies=null;animatedSpawns=null;displayKate=null;displayedThreat=null;busy=false;
        bool continueBuffered=!run.ended&&yummnAcceptBuffer&&yummnBufferedDirection.HasValue&&yummnBufferedTurn==run.turn&&yummnBufferedAction==run.ActionIndex;
        if(!run.ended&&!continueBuffered&&kaitSpine?.CurrentAnimation?.Loop==true)kaitSpine.PlayLoop(KaitSpineView.Idle);RefreshAll();PlayCrossBoardResonance(r);
        if(run.spawns.Exists(s=>!spawnBefore.Exists(old=>old.targetCell==s.targetCell)))GameAudio.PlayRiftWarning();
        if(!run.Yummn.rules.Is082)foreach(var p in r.spawnedEnemyCells){var e=run.EnemyAt(p);if(e==null)continue;PlayEnemyLanding(EnemySpine(e));GameAudio.PlayEnemySpawnVoice(e.type,e.id);}
        foreach(var ev in r.yummnEvents)if(ev.kind==YummnEventKind.Aim){var e=run.enemies.Find(x=>x.id==ev.sourceId&&x.life!=KaitEnemyLife.Dead);if(e!=null){EnemySpine(e)?.PlayPrepareAttack();GameAudio.PlayEnemyPrepareVoice(e.type,e.id);}}
        if(run.ended){yummnBufferedDirection=null;RecordCharacterScore();ShowEnd();}
        else if(continueBuffered)
        {var d=yummnBufferedDirection.Value;yummnBufferedDirection=null;HandleDirection(d);}
    }
    private void PlayYummnSkillFeedback(KaitSkill skill)
    {
        yummnBufferedDirection=null;yummnAcceptBuffer=false;
        YummnAudio.Play("Ready");
        statusText.text=run.IsYummnPrepared(skill)?(run.Yummn.rules.Is082?"已准备 · 技能气 "+run.PreparedKiCost+"，移动气按方向预览":"已准备 · 额外气 "+KaitAbilityCatalog.Get(skill).kiExtraCost+" / 本次总气 "+run.PreparedKiCost):"已取消准备";
        // Preparation is not execution: do not play attack/terrain effects early.
        if(!busy&&kaitSpine!=null){yummnPunchPose.Validate(run);kaitSpine.RestAnimation=yummnPunchPose.RestAnimation;kaitSpine.RefreshRestPose();}
    }
    private void PlayYummnFx(Vector2Int cell,int kind,KaitDirection direction=KaitDirection.Right)
    {
        if(cell.x<0||cell.y<0||cell.x>=KaitRun.BattleSize||cell.y>=KaitRun.BattleSize)return;
        var go=new GameObject("Yummn "+kind,typeof(RectTransform),typeof(YummnEffectGraphic));go.transform.SetParent(battleEnemyHitLayer,false);
        var fx=go.GetComponent<YummnEffectGraphic>();
        float size=kind==3?70:kind==0||kind==1||kind==4?116:88;
        fx.rectTransform.sizeDelta=kind==5?new Vector2(120,110):Vector2.one*size;
        fx.rectTransform.position=battleCells[cell.x+cell.y*KaitRun.BattleSize].rectTransform.position;
        // Canvas-local offsets, independent of resolution and visual split.
        var delta=KaitRun.Delta(direction);
        fx.rectTransform.anchoredPosition+=kind==3?new Vector2(0,65):kind==4?(Vector2)delta*22:new Vector2(0,8);
        fx.Initialize(kind);
        if(kind!=3)fx.rectTransform.localEulerAngles=new Vector3(0,0,HalfArrowAngle(delta));
    }
    private void RecordCharacterScore()
    {
        string key="Kait.Best."+run.ScoreRulesKey;int score=run.kills*100+(run.won?1000:0);
        if(score>PlayerPrefs.GetInt(key,0))PlayerPrefs.SetInt(key,score);PlayerPrefs.Save();
    }
}
