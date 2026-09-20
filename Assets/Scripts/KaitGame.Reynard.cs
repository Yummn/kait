using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public sealed partial class KaitGame
{
 private bool reynardReadyWas,reynardCaptureFx,reynardCaptureBullet;
 private Sprite reynardPortrait;
 private readonly List<GameObject> reynardPersistent=new List<GameObject>();
 private ReynardSceneFrame reynardScene;
 private DiagonalCutGraphic reynardFlatField;
 private KaitSpineView reynardMirrorGhost;
 private void ConfigureReynardScene(){
  if(reynardScene==null&&run.IsReynard){
   var go=new GameObject("Reynard Complete Cutout Scene",typeof(RectTransform),typeof(ReynardSceneFrame));go.transform.SetParent(gameContent,false);go.transform.SetSiblingIndex(2);
   reynardScene=go.GetComponent<ReynardSceneFrame>();reynardScene.raycastTarget=false;reynardScene.maskable=false;reynardScene.Texture=Resources.Load<Texture2D>("Reynard/MoonShrineOverlay");reynardScene.Background=storybookBackdrop.rectTransform;reynardScene.ConfigureSplit(WorldStyleTopSplit,WorldStyleBottomSplit);
   var cells=new List<RectTransform>();for(int y=1;y<6;y++)for(int x=1;x<6;x++)cells.Add(battleCells[x+y*KaitRun.BattleSize].rectTransform);reynardScene.Battle=cells.ToArray();cells.Clear();foreach(var cell in threatCells)cells.Add(cell.rectTransform);reynardScene.Threat=cells.ToArray();
  }
  if(reynardFlatField==null&&storybookBackdrop!=null){
   var field=new GameObject("Reynard Flat Right Field",typeof(RectTransform),typeof(CanvasRenderer),typeof(DiagonalCutGraphic));field.transform.SetParent(storybookBackdrop.transform,false);Stretch(field.GetComponent<RectTransform>(),0);
   reynardFlatField=field.GetComponent<DiagonalCutGraphic>();reynardFlatField.raycastTarget=false;reynardFlatField.SetStyle(WorldStyleTopSplit,WorldStyleBottomSplit,Background,new Color(.98f,.80f,.70f,.92f),5f);
  }
  if(reynardScene!=null)reynardScene.gameObject.SetActive(run.IsReynard);
  if(reynardFlatField!=null)reynardFlatField.gameObject.SetActive(run.IsReynard);
  foreach(string name in new[]{"Tutorial Button","Settings Gear"})gameplayRoot.transform.Find(name)?.SetAsLastSibling();
  if(storybookForestDetail!=null)storybookForestDetail.gameObject.SetActive(!run.IsReynard);
  if(storybookGroundEdge!=null)storybookGroundEdge.gameObject.SetActive(!run.IsReynard);
  if(storybookForegroundBough!=null)storybookForegroundBough.gameObject.SetActive(!run.IsReynard);
  foreach(var g in gameContent.GetComponentsInChildren<KaitBoardGrounding>(true))g.enabled=!run.IsReynard;
 }
 private IEnumerator PlayReynardTurn(KaitTurnResult r,Vector2Int start,List<KaitEnemy> before,List<KaitSpawnRequest> rifts)
 {
  busy=true;yummnAcceptBuffer=r.katePath.Count>0&&!run.ended;
  if(!yummnAcceptBuffer)yummnBufferedDirection=null;
  animatedEnemies=before;animatedSpawns=rifts;displayKate=start;displayedThreat=r.threatBefore;RefreshAll();EnsureKaitSpine();
  bool threatDone=r.threatAfter==null;if(!threatDone)StartCoroutine(RunPhase(AnimateThreat(r),()=>threatDone=true));
  if(r.katePath.Count>0)
  {
   // Use exactly the same motion path as Yummn's exhausted single-step:
   // cubic ease-out, distance-based duration, walk loop and no afterimages.
   yield return AnimateYummnMove(start,run.katePos,YummnMoveCause.Player,r.kaitDirection,false,YummnPhase.Exhausted,"walk",false);
   displayKate=run.katePos;RefreshBattle();
  }
  kaitSpine?.PlayOnce(r.reynardCast!=KaitSkill.None?"skill":r.reynardEvents.Count>0?"attack":"idle","idle");
  bool hasBullet=false;
  foreach(var e in r.reynardEvents)if(e.kind=="Bullet"&&InsideBattle(e.from)){hasBullet=true;StartCoroutine(ReynardBulletVisual(e.from,e.to));}
  if(hasBullet){yield return new WaitForSecondsRealtime(.14f);if(reynardCaptureBullet){reynardCaptureBullet=false;Canvas.ForceUpdateCanvases();CaptureCanvasToPng(System.IO.Path.Combine(Application.dataPath,"../Logs/reynard-forward-bullet.png"));}}
  foreach(var e in r.reynardEvents){if(e.kind=="Bullet"||!InsideBattle(e.to))continue;string icon=e.kind=="Ray"?"R05":e.kind=="SpellDirect"?"R04":"R01";StartCoroutine(ReynardBurst(e.to,icon,e.kind=="Ray"?40:68));}
  if(r.reynardCast!=KaitSkill.None){GameAudio.PlayReynard("SpellCast");var spell=ReynardCatalog.Get(r.reynardCast);GameAudio.PlayReynardSpellVoice(spell!=null&&spell.spellLevel>=3);}
  else if(r.reynardEvents.Count>0){GameAudio.PlayReynard("Foxfire");GameAudio.PlayKaitNormalAttackVoice();}
  foreach(var e in before){var now=run.enemies.Find(x=>x.id==e.id);if(now!=null&&now.hp<e.hp){var view=EnemySpine(e);view?.SetHitFlash(1);}}
  yield return new WaitForSecondsRealtime(.12f);
  foreach(var e in before)EnemySpine(e)?.SetHitFlash(0);
  BeginDetachedEnemyDeaths(before);
  while(!threatDone)yield return null;
  yield return AnimateAllEnemyActions(r.enemyActions);
  animatedEnemies=null;animatedSpawns=null;displayKate=null;displayedThreat=null;hideKate=false;
  busy=false;
  if(run.ended||run.CurrentReward!=null){ClearHeldInput();yummnBufferedDirection=null;yummnAcceptBuffer=false;}
  bool continueBuffered=!run.ended&&yummnAcceptBuffer&&yummnBufferedDirection.HasValue&&yummnBufferedTurn==run.turn&&yummnBufferedAction==run.ActionIndex;
  RefreshAll();
  foreach(var e in run.enemies)if(e.life!=KaitEnemyLife.Dead&&!before.Exists(x=>x.id==e.id))EnemySpine(e)?.PlayLanding();
  if(run.ended){kaitSpine?.PlayOnce(run.won?KaitSpineView.Victory:KaitSpineView.Die,null);RecordCharacterScore();ShowEnd();}
  else if(continueBuffered){var d=yummnBufferedDirection.Value;yummnBufferedDirection=null;HandleDirection(d);}
  else kaitSpine?.PlayLoop("idle");
 }
 private IEnumerator ReynardBulletVisual(Vector2Int from,Vector2Int to)
 {
  var sprite=ReynardArt.ImpactFrame(0);if(sprite==null)yield break;
  var go=new GameObject("Reynard Forward Bullet",typeof(RectTransform),typeof(Image));go.transform.SetParent(battleEnemyHitLayer,false);
  var image=go.GetComponent<Image>();image.sprite=sprite;image.raycastTarget=false;image.preserveAspect=true;image.rectTransform.sizeDelta=Vector2.one*46;
  Vector3 a=battleCells[from.x+from.y*KaitRun.BattleSize].transform.position,b;
  if(InsideBattle(to))b=battleCells[to.x+to.y*KaitRun.BattleSize].transform.position;
  else
  {
   Vector2Int d=to-from,inside=from-d;
   b=InsideBattle(inside)?a+(a-battleCells[inside.x+inside.y*KaitRun.BattleSize].transform.position):a+new Vector3(d.x*100,d.y*100,0);
  }
  for(float t=0;t<.14f;t+=Time.unscaledDeltaTime){if(go==null)yield break;float u=1f-Mathf.Pow(1f-Mathf.Clamp01(t/.14f),3f);go.transform.position=Vector3.LerpUnclamped(a,b,u);image.color=new Color(.86f,.96f,1f,Mathf.Lerp(.96f,.72f,u));image.rectTransform.localScale=Vector3.one*(1f+.12f*Mathf.Sin(u*Mathf.PI));if(reynardCaptureBullet&&u>.45f){reynardCaptureBullet=false;Canvas.ForceUpdateCanvases();CaptureCanvasToPng(System.IO.Path.Combine(Application.dataPath,"../Logs/reynard-forward-bullet.png"));}yield return null;}
  if(go!=null)Destroy(go);
 }
 private IEnumerator ReynardBurst(Vector2Int p,string icon,float size)
 {
  var first=ReynardArt.ImpactFrame(0);if(first==null)yield break;
  var go=new GameObject("Reynard Spell FX",typeof(RectTransform),typeof(Image));go.transform.SetParent(battleEnemyHitLayer,false);go.transform.position=battleCells[p.x+p.y*KaitRun.BattleSize].transform.position;
  var image=go.GetComponent<Image>();image.sprite=first;image.raycastTarget=false;image.preserveAspect=true;
  for(float t=0;t<.3f;t+=Time.unscaledDeltaTime){if(go==null)yield break;var frame=ReynardArt.ImpactFrame(Mathf.Clamp((int)(t/.3f*8),0,7));if(frame!=null)image.sprite=frame;image.rectTransform.sizeDelta=Vector2.one*size;image.color=new Color(1,1,1,1-t/.3f);if(reynardCaptureFx){reynardCaptureFx=false;var capture=ReynardArt.ImpactFrame(3);if(capture!=null)image.sprite=capture;Canvas.ForceUpdateCanvases();CaptureCanvasToPng(System.IO.Path.Combine(Application.dataPath,"../Logs/reynard-live-impact.png"));}yield return null;}Destroy(go);
 }
 private void RefreshReynardHud()
 {
  foreach(var go in reynardPersistent)if(go!=null)Destroy(go);reynardPersistent.Clear();if(!run.IsReynard){if(reynardMirrorGhost!=null)reynardMirrorGhost.Root.gameObject.SetActive(false);return;}
  if(!reynardReadyWas&&run.ReynardSlotReady&&run.turn>0)GameAudio.PlayReynard("SlotReady");reynardReadyWas=run.ReynardSlotReady;
  if(storybookPower!=null)storybookPower.text=ReynardCatalog.Roman(ReynardCatalog.Level(run.ReynardSlotValue));
  if(buildDirectionText!=null)buildDirectionText.text=run.Reynard.hasLastDirection?"方向射击：向 "+DirectionGlyph(run.Reynard.lastDirection)+" 前方发射 1 伤子弹":"输入方向，即使受阻也发射 1 伤子弹";
  var fist=storybookPower!=null?storybookPower.transform.parent.Find("Fist")?.GetComponent<Image>():null;
  if(fist!=null)fist.sprite=ReynardArt.Load("Cards/R08");
  RefreshReynardSustain();
  var p=run.Reynard.mirrorFoxCell;if(p.x<0){if(reynardMirrorGhost!=null)reynardMirrorGhost.Root.gameObject.SetActive(false);return;}
  var parent=threatCells[p.x+p.y*run.ThreatSize].transform;
  if(reynardMirrorGhost==null)reynardMirrorGhost=KaitSpineView.Create(makotoSkeletonData,parent,new Vector2(82,82),"Reynard Mirror Trail");
  if(reynardMirrorGhost==null)return;
  reynardMirrorGhost.Root.SetParent(parent,false);reynardMirrorGhost.Root.anchoredPosition=Vector2.zero;reynardMirrorGhost.Root.gameObject.SetActive(true);reynardMirrorGhost.Root.SetAsFirstSibling();
  reynardMirrorGhost.Face(run.Reynard.hasLastDirection?run.Reynard.lastDirection:KaitDirection.Right);reynardMirrorGhost.SetTint(run.ReynardSlotReady?new Color(.68f,.90f,1f,.48f):new Color(.58f,.55f,.82f,.30f));reynardMirrorGhost.PlayLoop("idle");

 }
 private void RefreshReynardSustain()
 {
  var skill=run.Reynard.focusedSkill;if(skill==KaitSkill.None)return;
  var def=ReynardCatalog.Get(skill);if(def==null||string.IsNullOrEmpty(def.id)||def.id.Length<=8)return;
  string id=def.id.Substring(8);var sustainSprite=ReynardArt.Load("Cards/"+id);if(sustainSprite==null)return;
  for(int y=1;y<6;y++)for(int x=1;x<6;x++){
   var p=new Vector2Int(x,y);bool area=skill==KaitSkill.ReynardFog||skill==KaitSkill.ReynardWeb;
   if(area?Mathf.Abs(x-run.Reynard.focusedCell.x)+Mathf.Abs(y-run.Reynard.focusedCell.y)>1:p!=run.katePos)continue;
   var go=new GameObject("Reynard Sustain",typeof(RectTransform),typeof(Image));go.transform.SetParent(battleUnderEffectLayer,false);go.transform.position=battleCells[x+y*KaitRun.BattleSize].transform.position;
   var im=go.GetComponent<Image>();im.sprite=sustainSprite;im.preserveAspect=true;im.raycastTarget=false;im.color=new Color(1,1,1,area?.44f:.23f);im.rectTransform.sizeDelta=Vector2.one*(area?100:118);reynardPersistent.Add(go);
  }
 }
}

