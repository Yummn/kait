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
        dungeonFloorSprite=KaitStorybookArt.Floor(run.IsYummn,1,1) ?? originalYummnFloor;
        dungeonWallSprite=KaitStorybookArt.Wall(run.IsYummn) ?? originalYummnWall;
        if(storybookBackdrop!=null)storybookBackdrop.sprite=KaitStorybookArt.Load(run.IsYummn?"SnowBackdrop":"GrassBackdrop");
        if(storybookForestDetail!=null)storybookForestDetail.SetSeason(run.IsYummn);
        if(storybookForegroundBough!=null)
        {
            storybookForegroundBough.sprite=KaitStorybookArt.Detail(run.IsYummn?"SnowBough":"GrassBough");
            var bough=storybookForegroundBough.GetComponent<KaitCornerBough>();bough.Snow=run.IsYummn;bough.Fit();
        }
        if(storybookGroundEdge!=null){storybookGroundEdge.sprite=KaitStorybookArt.GroundApron(run.IsYummn);storybookGroundEdge.gameObject.SetActive(true);}
        foreach(var ground in gameContent.GetComponentsInChildren<KaitBoardGrounding>())
        {ground.Snow=run.IsYummn;ground.SetVerticesDirty();}
        for(int y=1;y<=5;y++)for(int x=1;x<=5;x++)
        {
            int index=x+y*KaitRun.BattleSize;
            battleObstacles[index].sprite=dungeonWallSprite;
            battleObstacles[index].rectTransform.sizeDelta=Vector2.one*118;
            battleObstacles[index].rectTransform.anchoredPosition=Vector2.zero;
        }
    }
    private void SaveCharacterRun()
    {
        string key=run.IsYummn?(run.Yummn.rules.Is082?KaitVersion.YummnSaveKey:run.Yummn.rules.Legacy?"Kait.Run.Yummn.0.8":"Kait.Run.Yummn.0.8.1"):"Kait.Run.Kait";
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
            yummnHud=MakeHybridSurface("Yummn Ki",turnText.transform.parent,new Vector2(124.5f,-19),new Vector2(218,34),null,Color.clear,0f,10f);
            yummnHud.Configure(styleSplit,null,Color.clear,Color.clear,Color.clear,0,0);
            yummnHud.raycastTarget=false;
        }
        if(actionPips.Length!=run.Yummn.profile.maxKi)
        {
            foreach(var pip in actionPips)if(pip!=null){pip.gameObject.SetActive(false);if(Application.isPlaying)Destroy(pip.gameObject);else DestroyImmediate(pip.gameObject);}
            actionPips=new Image[run.Yummn.profile.maxKi];
            kiWisps=new YummnKiWisp[actionPips.Length];
            for(int i=0;i<actionPips.Length;i++)
            {
                actionPips[i]=Rect("Ki "+(i+1),yummnHud.transform,Vector2.zero,new Vector2(34f*328/536,34),Color.white);
                kiWisps[i]=actionPips[i].gameObject.AddComponent<YummnKiWisp>();
                kiWisps[i].Configure(styleSplit);
            }
        }
        yummnHud.gameObject.SetActive(run.IsYummn);
        bool exhausted=run.KiPhase==YummnPhase.Exhausted;
        for(int i=0;i<actionPips.Length;i++)
        {
            actionPips[i].gameObject.SetActive(i<run.Yummn.profile.maxKi);
            // Default seven-qi layout; optional 8/9 extend without squeezing.
            const float spacing=32.5f;
            actionPips[i].rectTransform.localScale=Vector3.one;
            actionPips[i].rectTransform.anchoredPosition=new Vector2((i-3)*spacing,0);
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
        foreach(var merge in r.yummnEvents.Exists(e=>e.status=="ThreatBegin")?new List<KaitMergeEvent>():r.merges)
        {
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
        RefreshAll();bool threatDone=false;
        if(!r.yummnEvents.Exists(e=>e.status=="ThreatBegin"))StartCoroutine(RunPhase(AnimateThreat(opening),()=>threatDone=true));
        kaitSpine?.Face(r.kaitDirection);
        if(r.yummnAction.plannedSkills.Contains("yummn.M02")){kaitSpine?.PlayOnce(KaitSpineView.YummnBuff);PlayYummnFx(start,4,r.kaitDirection);}
        bool attackStarted=false,flurryFxPlayed=false,attackVoicePlayed=false;
        int voicedKills=0;
        if(r.yummnAction.plannedSkills.Count>0)GameAudio.PlayKaitSmallAttackSkillVoice();
        if(r.yummnAction.plannedSkills.Exists(id=>id=="yummn.N02"||id=="yummn.N04"||id=="yummn.N05"||id=="yummn.N06"||id=="yummn.N19"))
            kaitSpine?.PlayOnce(KaitSpineView.YummnAttackSkill);
        for(int i=0;i<r.yummnEvents.Count;i++)
        {
            var ev=r.yummnEvents[i];
            if(ev.status=="ThreatBegin")
            {yield return AnimateThreat(opening);threatDone=true;continue;}
            if(ev.status=="RiftQueued")
            {
                if(!animatedSpawns.Exists(s=>s.targetCell==ev.to))animatedSpawns.Add(new KaitSpawnRequest{targetCell=ev.to,sourceThreatCell=ev.from,tier=ev.amount,createdTurn=run.turn-1,state=KaitSpawnState.Preview});
                RefreshBattle();continue;
            }
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
                bool ghostHit=false;var resonantHits=new List<YummnCombatEvent>();
                foreach(var hit in r.yummnEvents)if(hit.kind==YummnEventKind.AfterimageHit&&hit.attackEventId==ev.attackEventId)
                {PlayV08Fx(hit.to,15,r.kaitDirection,90);ghostHit=true;resonantHits.Add(hit);}
                if(resonantHits.Count>1&&run.HasPassive(KaitPassive.MirrorResonance))
                    foreach(var hit in resonantHits)PlayPriorityBattleFx(hit.to,"MirrorResonance",KaitDirection.Right,104,.62f);
                if(ghostHit)YummnAudio.Play("GhostHit");
                yield return AnimateAllEnemyActions(r.enemyActions.FindAll(a=>a.enemyId==ev.sourceId&&a.type!=KaitIntentType.Move));
            }
            else if(ev.kind==YummnEventKind.AfterimageCreated)
            {
                CreateYummnLogicalGhost(ev.markerId,ev.to,ev.direction.x>0?KaitDirection.Right:ev.direction.x<0?KaitDirection.Left:ev.direction.y>0?KaitDirection.Up:KaitDirection.Down);
                if(run.HasPassive(KaitPassive.LastingImage))PlayPriorityBattleFx(ev.to,"LastingIllusion",KaitDirection.Right,104,.64f);
            }
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
                if(ev.direction!=Vector2Int.zero&&(ev.damageCause==YummnDamageCause.Punch||ev.damageCause==YummnDamageCause.Counter||ev.damageCause==YummnDamageCause.Kick))
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
                if(ev.damageCause==YummnDamageCause.Collision)
                {PlayPriorityBattleFx(ev.to,"StoneCollision",DirectionFromVector(ev.direction),122,.68f);YummnAudio.Play("StoneCollision");}
                else if(ev.damageCause==YummnDamageCause.ShadowBlade)
                    PlayPriorityBattleFx(ev.to,"ShadowBladeEcho",r.kaitDirection,108,.58f);
                else if(ev.damageCause==YummnDamageCause.MergeMissile)
                    PlayPriorityBattleFx(ev.to,"MagicMissile",r.kaitDirection,100,.52f);
                else if(effect>=0){PlayV08Fx(ev.to,effect,r.kaitDirection,90);YummnAudio.Play(effect==0?"Water":effect==3?"Frost":effect==4?"Fire":effect==5?"Shatter":"PalmSeal");}
                else if(ev.damageCause!=YummnDamageCause.Sonic&&ev.damageCause!=YummnDamageCause.MergeGlyph)
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
                if(ev.amount>0)GameAudio.PlayKaitDamageVoice(ev.hpAfter,run.KateMaxHp);
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
            else if(ev.kind==YummnEventKind.Resource){PlayV08Fx(displayKate??run.katePos,9,r.kaitDirection,65);if(ev.status=="MergePearl")PlayPriorityBattleFx(displayKate??run.katePos,"ManaPearl",KaitDirection.Right,94,.56f);YummnAudio.Play("Ki");if(ev.status=="Afterimage")ShowYummnAfterimageKi(ev.amount);}
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
                if(ev.status=="IceExpired")yummnVisualIce=YummnRun.NoCell;
                if(ev.status=="DarknessExpired")yummnVisualDarkness=YummnRun.NoCell;
                if(ev.status=="ThunderWave"){PlayPriorityBattleFx(ev.to,"ThunderWave",KaitDirection.Right,150,.64f);YummnAudio.Play("Air");}
                if(ev.status=="SonicBurst"){PlayPriorityBattleFx(ev.to,"SonicBurst",KaitDirection.Right,154,.72f);YummnAudio.Play("SonicBurst");}
                if(ev.status=="MirrorCreate"){PlayPriorityBattleFx(ev.to,"MirrorCreate",KaitDirection.Right,110,.62f);YummnAudio.Play("MirrorCreate");}
                if(ev.status=="CommandAct"){PlayPriorityBattleFx(ev.to,"CommandAct",KaitDirection.Right,116,.56f);YummnAudio.Play("CommandAct");}
                RefreshYummnTerrain();
            }
            else if(ev.kind==YummnEventKind.Status)
            {
                if(ev.status=="MergeMissileLaunch"){YummnAudio.Play("MagicMissile");yield return PlayPool083Missile(ev.from,ev.to);}
                if(ev.status=="MergeGlyphCast"){PlayPriorityBattleFx(ev.to,"WardingGlyph",KaitDirection.Right,148,.66f);YummnAudio.Play("WardingGlyph");}
                if(ev.status=="SweepPursuit")PlayPriorityBattleFx(ev.to,"SweepPursuit",r.kaitDirection,120,.54f);
                if(ev.status=="Misdirection")PlayPriorityBattleFx(ev.to,"Misdirection",KaitDirection.Right,108,.58f);
                if(ev.status=="SpellEchoCast")PlayPriorityBattleFx(ev.to,"SpellEcho",r.kaitDirection,116,.64f);
                if(ev.status=="MageHandCast"){PlayPriorityThreatFx(ev.to,"MageHand",110,.62f);YummnAudio.Play("MageHand");}
                if(ev.status=="GravityPendulum")PlayPriorityThreatCenter("GravityPendulum",180,.68f);
                if(ev.status=="ResonanceCrystal"){PlayPriorityThreatFx(ev.from,"ResonanceCrystal",108,.62f);PlayPriorityThreatFx(ev.to,"ResonanceCrystal",108,.62f);}
                if(ev.status=="BountyJarDeposit")PlayPriorityThreatFx(ev.to,"BountyJar",108,.64f);
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
        if(run.ended||r.yummnAction.phaseAtStart!=r.yummnAction.phaseAtEnd||run.CurrentReward!=null||targetingSkill!=KaitSkill.None)
        {ClearHeldInput();yummnBufferedDirection=null;yummnAcceptBuffer=false;}
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
    private static KaitDirection DirectionFromVector(Vector2Int delta)=>Mathf.Abs(delta.x)>=Mathf.Abs(delta.y)?(delta.x>=0?KaitDirection.Right:KaitDirection.Left):(delta.y>=0?KaitDirection.Up:KaitDirection.Down);
    private void PlayPriorityBattleFx(Vector2Int cell,string clip,KaitDirection direction,float size,float duration)
    {
        if(cell.x<0||cell.y<0||cell.x>=KaitRun.BattleSize||cell.y>=KaitRun.BattleSize)return;
        var go=new GameObject("Priority "+clip,typeof(RectTransform),typeof(YummnPriorityEffect));go.transform.SetParent(battleEnemyHitLayer,false);
        var fx=go.GetComponent<YummnPriorityEffect>();fx.rectTransform.sizeDelta=Vector2.one*size;
        fx.rectTransform.position=battleCells[cell.x+cell.y*KaitRun.BattleSize].rectTransform.position;
        fx.rectTransform.localEulerAngles=new Vector3(0,0,HalfArrowAngle(KaitRun.Delta(direction)));fx.Initialize(clip,duration);
    }
    private void PlayPriorityThreatFx(Vector2Int cell,string clip,float size,float duration)
    {
        if(cell.x<0||cell.y<0||cell.x>=run.ThreatSize||cell.y>=run.ThreatSize)return;
        var parent=threatCells[cell.x+cell.y*run.ThreatSize].rectTransform;
        var go=new GameObject("Priority "+clip,typeof(RectTransform),typeof(YummnPriorityEffect));go.transform.SetParent(parent,false);
        var fx=go.GetComponent<YummnPriorityEffect>();fx.rectTransform.anchoredPosition=Vector2.zero;fx.rectTransform.sizeDelta=Vector2.one*size;fx.Initialize(clip,duration);go.transform.SetAsLastSibling();
    }
    private void PlayPriorityThreatCenter(string clip,float size,float duration)=>PlayPriorityThreatFx(new Vector2Int(run.ThreatSize/2,run.ThreatSize/2),clip,size,duration);
    private void RecordCharacterScore()
    {
        string key="Kait.Best."+run.ScoreRulesKey;int score=run.kills*100+(run.won?1000:0);
        if(score>PlayerPrefs.GetInt(key,0))PlayerPrefs.SetInt(key,score);PlayerPrefs.Save();
    }
}
