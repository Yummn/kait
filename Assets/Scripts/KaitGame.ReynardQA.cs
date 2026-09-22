using System.Collections;
using System.IO;
using UnityEngine;
public sealed partial class KaitGame
{
 private IEnumerator VerifyReynardRuntime()
 {
  yield return new WaitForSecondsRealtime(1);
  string root=Path.Combine(Application.dataPath,"../Logs");
  mainMenu.Select(KaitCharacter.Reynard);mainMenu.gameObject.SetActive(true);gameplayRoot.SetActive(false);yield return new WaitForSecondsRealtime(.4f);CaptureCanvasToPng(Path.Combine(root,"reynard-menu.png"));
  for (int home=0;home<3;home++)
  {
   mainMenu.CharacterButtons[home].onClick.Invoke();
   if(mainMenu.Selected!=(KaitCharacter)home) throw new System.Exception("Home selection mismatch");
   yield return new WaitForSecondsRealtime(.7f);
   CaptureCanvasToPng(Path.Combine(root,"home0926-"+home+".png"));
  }
  Debug.Log("HOME0926_SELECTION_QA_COMPLETE");
  var library=KaitCardLibrary.Open(canvas.transform,threatBoardFont,KaitCharacter.Reynard);
  for(int i=0;i<7;i++){yield return new WaitForSecondsRealtime(.25f);CaptureCanvasToPng(Path.Combine(root,"reynard-cards-"+i+".png"));library.ChangePage(1);}Destroy(library.gameObject);
  mainMenu.gameObject.SetActive(false);gameplayRoot.SetActive(true);run.SelectCharacter(KaitCharacter.Reynard,924);ConfigureCharacterVisuals();run.StateCommitted=null;EnsureKaitSpine();run.enemies.Clear();run.spawns.Clear();
  var positions=new[]{new Vector2Int(1,1),new Vector2Int(2,4),new Vector2Int(4,4),new Vector2Int(5,2),new Vector2Int(3,5),new Vector2Int(4,1)};
  for(int i=0;i<6;i++){var e=new KaitEnemy{id=100+i,type=(KaitEnemyType)(i+1),pos=positions[i],hp=3,maxHp=3,life=KaitEnemyLife.Active};run.enemies.Add(e);}
  run.skills.Add(KaitSkill.ReynardThunderwave);run.skills.Add(KaitSkill.ReynardFireball);run.skills.Add(KaitSkill.ReynardFog);run.passives.Add(KaitPassive.ReynardRingFlame);
  run.Reynard.mirrorFoxCell=new Vector2Int(2,2);run.threat[2,2]=8;run.Reynard.sigils[2,2]=true;run.Reynard.sigilCreated[2,2]=1;run.Reynard.tails=2;RefreshAll();yield return new WaitForSecondsRealtime(.8f);
  if(runHealthBar!=null&&runHealthBar.root.gameObject.activeSelf)throw new System.Exception("REYNARD_LEGACY_PLAYER_HEALTH_BAR_VISIBLE");
  if(reynardMirrorGhost==null||!reynardMirrorGhost.Root.gameObject.activeInHierarchy||reynardMirrorGhost.Root.parent!=threatCells[2+2*run.ThreatSize].transform)throw new System.Exception("REYNARD_MIRROR_GHOST_MISSING");
  CaptureCanvasToPng(Path.Combine(root,"reynard-mirror-ghost.png"));CaptureCanvasToPng(Path.Combine(root,"reynard-battle.png"));
  var foxBefore=(int[,])run.threat.Clone();run.threat[2,2]=0;run.threat[4,2]=8;run.Reynard.mirrorFoxCell=new Vector2Int(4,2);var foxSlide=new KaitTurnResult{threatBefore=foxBefore,threatAfter=(int[,])run.threat.Clone()};foxSlide.threatMotions.Add(new KaitThreatMotion{from=new Vector2Int(2,2),to=new Vector2Int(4,2),value=8,mirrorFox=true});RefreshAll();bool foxSlideDone=false;reynardMirrorSlideObserved=false;reynardCaptureMirrorSlide=true;StartCoroutine(RunPhase(AnimateThreat(foxSlide),()=>foxSlideDone=true));while(!foxSlideDone)yield return null;if(!reynardMirrorSlideObserved||reynardCaptureMirrorSlide)throw new System.Exception("REYNARD_MIRROR_GHOST_DID_NOT_SLIDE_WITH_NUMBER");
  var mergePulse=new KaitTurnResult();mergePulse.merges.Add(new KaitMergeEvent{sourceValue=4,resultValue=8,threatCell=new Vector2Int(2,2)});StartCoroutine(AnimateThreatPulses(mergePulse));yield return new WaitForSecondsRealtime(.06f);
  if((threatCells[2+2*run.ThreatSize].rectTransform.localScale-Vector3.one).sqrMagnitude>.0001f)throw new System.Exception("REYNARD_MERGE_SCALED_BOARD_BACKGROUND");
  CaptureCanvasToPng(Path.Combine(root,"reynard-merge-stable.png"));yield return new WaitForSecondsRealtime(.15f);
  var riftCell=new Vector2Int(1,3);run.spawns.Add(new KaitSpawnRequest{targetCell=riftCell,sourceThreatCell=Vector2Int.zero,tier=4,state=KaitSpawnState.Preview,turnsUntilSpawn=1});RefreshAll();yield return new WaitForSecondsRealtime(.2f);
  var rift=battleRifts[riftCell.x+riftCell.y*KaitRun.BattleSize];var core=rift.transform.Find("Fracture Core")?.GetComponent<UnityEngine.UI.Image>();
  if(!rift.gameObject.activeSelf||rift.sprite==null||transparentSpawnRiftSprite==null||rift.sprite.texture!=transparentSpawnRiftSprite.texture||rift.material==KaitGroundDecal.MaskMaterial||core==null||!core.gameObject.activeSelf||core.sprite==null||core.sprite.texture!=transparentSpawnRiftSprite.texture||core.material==KaitGroundDecal.MaskMaterial)throw new System.Exception("REYNARD_RIFT_WHITE_RECT: shared Yummn alpha rift route not active");
  foreach(var cell in battleCells)if(cell!=null)foreach(var image in cell.GetComponentsInChildren<UnityEngine.UI.Image>(true))if(image.gameObject.activeInHierarchy&&image.sprite==null&&image.color.a>.95f&&image.color.r>.95f&&image.color.g>.95f&&image.color.b>.95f)throw new System.Exception("REYNARD_WHITE_RECT: "+image.name+" in "+cell.name);
  CaptureCanvasToPng(Path.Combine(root,"reynard-rift.png"));run.spawns.Clear();RefreshAll();
  targetingSkill=KaitSkill.ReynardFireball;RefreshAll();yield return new WaitForSecondsRealtime(.3f);CaptureCanvasToPng(Path.Combine(root,"reynard-targets.png"));targetingSkill=KaitSkill.None;
  foreach(var e in run.enemies)EnemySpine(e)?.PlayPrepareAttack();kaitSpine.PlayLoop("prepare");yield return new WaitForSecondsRealtime(.35f);CaptureCanvasToPng(Path.Combine(root,"reynard-preparing.png"));
  foreach(var e in run.enemies)EnemySpine(e)?.PlayAttack();kaitSpine.PlayOnce("skill","idle");yield return new WaitForSecondsRealtime(.25f);CaptureCanvasToPng(Path.Combine(root,"reynard-attacking.png"));
  foreach(var e in run.enemies)EnemySpine(e)?.PlayDamage();yield return new WaitForSecondsRealtime(.25f);CaptureCanvasToPng(Path.Combine(root,"reynard-hit.png"));
  run.Reynard.fogCell=new Vector2Int(2,3);run.Reynard.webCell=new Vector2Int(4,3);RefreshAll();
  reynardCaptureFx=true;StartCoroutine(ReynardBurst(new Vector2Int(4,3),"R04",85));yield return new WaitForSecondsRealtime(.08f);CaptureCanvasToPng(Path.Combine(root,"reynard-fx.png"));
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R30",68,"SigilCreated"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-sigil-created.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(run.katePos,"R34",68,"TailGained"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-tail-gained.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R30",68,"SigilSpent"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-sigil-spent.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(run.katePos,"R34",68,"TailSpent"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-tail-spent.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(run.katePos,"R23",78,"MirrorWardBlock"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-mirror-ward-block.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R26",68,"SplitOrb"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-split-orb.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R27",68,"StarChaser"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-star-chaser.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(run.katePos,"R37",68,"TailOverflow"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-tail-overflow.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R28",68,"ChargedOrb"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-charged-orb.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R30",68,"RuneDetonation"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-rune-detonation.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R31",68,"CircleResonance"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-circle-resonance.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R33",68,"ArcaneManifest"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-arcane-manifest.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R35",68,"FoxfireGuard"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-foxfire-guard.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R36",68,"FoxflameRite"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-foxflame-rite.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R37",68,"TailToSigil"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-tail-to-sigil.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R38",72,"SpiritFoxHunt"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-spirit-fox-hunt.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R39",68,"TailwindChant"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-tailwind-chant.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R40",68,"ScorchingRay"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-scorching-ray.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R41",68,"RuneStep"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-rune-step.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R42",72,"StoneWall"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-stone-wall.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R43",92,"TailNova"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-tail-nova.png"));yield return new WaitForSecondsRealtime(.22f);
  StartCoroutine(ReynardBurst(new Vector2Int(2,2),"R44",86,"MassHold"));yield return new WaitForSecondsRealtime(.13f);CaptureCanvasToPng(Path.Combine(root,"reynard-mass-hold.png"));yield return new WaitForSecondsRealtime(.22f);
  run.Reynard.fogCell=run.Reynard.webCell=new Vector2Int(-1,-1);
  var blockedTarget=new KaitEnemy{id=999,type=KaitEnemyType.Grunt,pos=run.katePos+Vector2Int.right,hp=3,maxHp=3,life=KaitEnemyLife.Preparing};run.enemies.Add(blockedTarget);RefreshAll();
  var origin=run.katePos;var snapshot=SnapshotEnemies();var pending=SnapshotSpawns();reynardCaptureBullet=true;var result=run.TryTurn(KaitDirection.Right);
  if(run.katePos!=origin||blockedTarget.hp!=2||!result.reynardEvents.Exists(e=>e.kind=="Bullet"&&e.from==origin&&e.to==origin+Vector2Int.right))throw new System.Exception("REYNARD_BLOCKED_SHOT_NOT_RESOLVED");
  yield return PlayReynardTurn(result,origin,snapshot,pending);yield return null;run.enemies.RemoveAll(e=>e.id==999);RefreshAll();CaptureCanvasToPng(Path.Combine(root,"reynard-blocked-shot.png"));
  if(reynardCaptureBullet)throw new System.Exception("REYNARD_FORWARD_BULLET_NOT_PRESENTED");
  foreach(var item in canvas.GetComponentsInChildren<Transform>(true))if(item!=null&&item.name=="Reynard Forward Bullet")throw new System.Exception("REYNARD_FORWARD_BULLET_STUCK_AT_CENTER");
  origin=run.katePos;snapshot=SnapshotEnemies();pending=SnapshotSpawns();result=run.TryTurn(KaitDirection.Up);yield return PlayReynardTurn(result,origin,snapshot,pending);CaptureCanvasToPng(Path.Combine(root,"reynard-after-turns.png"));
  Debug.Log("REYNARD_QA_COMPLETE");yield return new WaitForSecondsRealtime(.2f);Application.Quit();
 }
}
