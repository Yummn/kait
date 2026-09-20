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
  for(int i=0;i<4;i++){yield return new WaitForSecondsRealtime(.25f);CaptureCanvasToPng(Path.Combine(root,"reynard-cards-"+i+".png"));library.ChangePage(1);}Destroy(library.gameObject);
  mainMenu.gameObject.SetActive(false);gameplayRoot.SetActive(true);run.SelectCharacter(KaitCharacter.Reynard,924);ConfigureCharacterVisuals();run.StateCommitted=null;EnsureKaitSpine();run.enemies.Clear();run.spawns.Clear();
  var positions=new[]{new Vector2Int(1,1),new Vector2Int(2,4),new Vector2Int(4,4),new Vector2Int(5,2),new Vector2Int(3,5),new Vector2Int(4,1)};
  for(int i=0;i<6;i++){var e=new KaitEnemy{id=100+i,type=(KaitEnemyType)(i+1),pos=positions[i],hp=3,maxHp=3,life=KaitEnemyLife.Active};run.enemies.Add(e);}
  run.skills.Add(KaitSkill.ReynardThunderwave);run.skills.Add(KaitSkill.ReynardFireball);run.skills.Add(KaitSkill.ReynardFog);run.passives.Add(KaitPassive.ReynardRingFlame);
  run.Reynard.mirrorFoxCell=new Vector2Int(2,2);run.threat[2,2]=8;run.Reynard.spellReady[2,2]=true;RefreshAll();yield return new WaitForSecondsRealtime(.8f);
  if(reynardMirrorGhost==null||!reynardMirrorGhost.Root.gameObject.activeInHierarchy||reynardMirrorGhost.Root.parent!=threatCells[2+2*run.ThreatSize].transform)throw new System.Exception("REYNARD_MIRROR_GHOST_MISSING");
  CaptureCanvasToPng(Path.Combine(root,"reynard-mirror-ghost.png"));CaptureCanvasToPng(Path.Combine(root,"reynard-battle.png"));
  var riftCell=new Vector2Int(1,3);run.spawns.Add(new KaitSpawnRequest{targetCell=riftCell,sourceThreatCell=Vector2Int.zero,tier=4,state=KaitSpawnState.Preview,turnsUntilSpawn=1});RefreshAll();yield return new WaitForSecondsRealtime(.2f);
  var rift=battleRifts[riftCell.x+riftCell.y*KaitRun.BattleSize];var core=rift.transform.Find("Fracture Core")?.GetComponent<UnityEngine.UI.Image>();
  if(!rift.gameObject.activeSelf||rift.sprite==null||transparentSpawnRiftSprite==null||rift.sprite.texture!=transparentSpawnRiftSprite.texture||rift.material==KaitGroundDecal.MaskMaterial||core==null||!core.gameObject.activeSelf||core.sprite==null||core.sprite.texture!=transparentSpawnRiftSprite.texture||core.material==KaitGroundDecal.MaskMaterial)throw new System.Exception("REYNARD_RIFT_WHITE_RECT: shared Yummn alpha rift route not active");
  foreach(var cell in battleCells)if(cell!=null)foreach(var image in cell.GetComponentsInChildren<UnityEngine.UI.Image>(true))if(image.gameObject.activeInHierarchy&&image.sprite==null&&image.color.a>.95f&&image.color.r>.95f&&image.color.g>.95f&&image.color.b>.95f)throw new System.Exception("REYNARD_WHITE_RECT: "+image.name+" in "+cell.name);
  CaptureCanvasToPng(Path.Combine(root,"reynard-rift.png"));run.spawns.Clear();RefreshAll();
  targetingSkill=KaitSkill.ReynardFireball;RefreshAll();yield return new WaitForSecondsRealtime(.3f);CaptureCanvasToPng(Path.Combine(root,"reynard-targets.png"));targetingSkill=KaitSkill.None;
  foreach(var e in run.enemies)EnemySpine(e)?.PlayPrepareAttack();kaitSpine.PlayLoop("prepare");yield return new WaitForSecondsRealtime(.35f);CaptureCanvasToPng(Path.Combine(root,"reynard-preparing.png"));
  foreach(var e in run.enemies)EnemySpine(e)?.PlayAttack();kaitSpine.PlayOnce("skill","idle");yield return new WaitForSecondsRealtime(.25f);CaptureCanvasToPng(Path.Combine(root,"reynard-attacking.png"));
  foreach(var e in run.enemies)EnemySpine(e)?.PlayDamage();yield return new WaitForSecondsRealtime(.25f);CaptureCanvasToPng(Path.Combine(root,"reynard-hit.png"));
  run.Reynard.focusedSkill=KaitSkill.ReynardFog;run.Reynard.focusedCell=new Vector2Int(2,3);RefreshAll();
  reynardCaptureFx=true;StartCoroutine(ReynardBurst(new Vector2Int(4,3),"R04",85));yield return new WaitForSecondsRealtime(.08f);CaptureCanvasToPng(Path.Combine(root,"reynard-fx.png"));
  run.Reynard.focusedSkill=KaitSkill.None;
  var blockedTarget=new KaitEnemy{id=999,type=KaitEnemyType.Grunt,pos=run.katePos+Vector2Int.right,hp=3,maxHp=3,life=KaitEnemyLife.Preparing};run.enemies.Add(blockedTarget);RefreshAll();
  var origin=run.katePos;var snapshot=SnapshotEnemies();var pending=SnapshotSpawns();reynardCaptureBullet=true;var result=run.TryTurn(KaitDirection.Right);
  if(run.katePos!=origin||blockedTarget.hp!=2||!result.reynardEvents.Exists(e=>e.kind=="Bullet"&&e.from==origin&&e.to==origin+Vector2Int.right))throw new System.Exception("REYNARD_BLOCKED_SHOT_NOT_RESOLVED");
  yield return PlayReynardTurn(result,origin,snapshot,pending);run.enemies.RemoveAll(e=>e.id==999);RefreshAll();CaptureCanvasToPng(Path.Combine(root,"reynard-blocked-shot.png"));
  if(reynardCaptureBullet)throw new System.Exception("REYNARD_FORWARD_BULLET_NOT_PRESENTED");
  origin=run.katePos;snapshot=SnapshotEnemies();pending=SnapshotSpawns();result=run.TryTurn(KaitDirection.Up);yield return PlayReynardTurn(result,origin,snapshot,pending);CaptureCanvasToPng(Path.Combine(root,"reynard-after-turns.png"));
  Debug.Log("REYNARD_QA_COMPLETE");yield return new WaitForSecondsRealtime(.2f);Application.Quit();
 }
}
