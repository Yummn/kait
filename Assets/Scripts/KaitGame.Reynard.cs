using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public sealed partial class KaitGame
{
 private bool reynardReadyWas,reynardCaptureFx,reynardCaptureBullet,reynardCaptureMirrorSlide,reynardMirrorSlideObserved;
 private Sprite reynardPortrait;
 private readonly List<GameObject> reynardPersistent=new List<GameObject>();
 private int reynardVisualHash=int.MinValue;
 private ReynardSceneFrame reynardScene;
 private DiagonalCutGraphic reynardFlatField;
 private KaitSpineView reynardMirrorGhost;
 private void ConfigureReynardScene(){
  if(reynardFlatField==null&&storybookBackdrop!=null){
   var field=new GameObject("Reynard Flat Right Field",typeof(RectTransform),typeof(CanvasRenderer),typeof(DiagonalCutGraphic));field.transform.SetParent(storybookBackdrop.transform,false);Stretch(field.GetComponent<RectTransform>(),0);
   reynardFlatField=field.GetComponent<DiagonalCutGraphic>();reynardFlatField.raycastTarget=false;reynardFlatField.SetStyle(WorldStyleTopSplit,WorldStyleBottomSplit,new Color(.18f,.19f,.31f),new Color(.98f,.80f,.70f,.92f),5f);
  }
  if(reynardScene!=null)reynardScene.gameObject.SetActive(false);
  if(reynardFlatField!=null)reynardFlatField.gameObject.SetActive(run.IsReynard);
  foreach(string name in new[]{"Tutorial Button","Settings Gear"})gameplayRoot.transform.Find(name)?.SetAsLastSibling();
  if(storybookForestDetail!=null)storybookForestDetail.gameObject.SetActive(!run.IsReynard);
  if(storybookGroundEdge!=null){
   storybookGroundEdge.gameObject.SetActive(true);
   if(run.IsReynard){storybookGroundEdge.sprite=ReynardArt.Load("GardenFoliageApron");storybookGroundEdge.color=Color.white;}
   else storybookGroundEdge.color=Color.white;
  }
  if(storybookForegroundBough!=null)storybookForegroundBough.gameObject.SetActive(!run.IsReynard);
  foreach(var g in gameContent.GetComponentsInChildren<KaitBoardGrounding>(true))g.enabled=!run.IsReynard;
 }
 private IEnumerator PlayReynardTurn(KaitTurnResult r,Vector2Int start,List<KaitEnemy> before,List<KaitSpawnRequest> rifts)
 {
  busy=true;yummnAcceptBuffer=r.katePath.Count>0&&!run.ended;
  if(!yummnAcceptBuffer)yummnBufferedDirection=null;
  animatedEnemies=before;animatedSpawns=rifts;displayKate=start;displayedThreat=r.threatBefore;RefreshAll();EnsureKaitSpine();
  bool threatDone=r.threatAfter==null;if(!threatDone)StartCoroutine(RunPhase(AnimateThreat(r),()=>threatDone=true));
  bool hasBullet=r.reynardEvents.Exists(e=>e.kind=="Bullet"&&InsideBattle(e.from));
  if(r.katePath.Count>0)
  {
   // Use exactly the same motion path as Yummn's exhausted single-step:
   // cubic ease-out, distance-based duration, walk loop and no afterimages.
   yield return AnimateYummnMove(start,run.katePos,YummnMoveCause.Player,r.kaitDirection,false,YummnPhase.Exhausted,hasBullet?"jump_f":"walk",false);
   displayKate=run.katePos;RefreshBattle();
  }
  if(r.reynardCast!=KaitSkill.None)kaitSpine?.PlayOnce(ReynardCatalog.CastAnimation(r.reynardCast),"idle");
  else if(hasBullet)kaitSpine?.PlayOnce("attack","idle");
  else kaitSpine?.PlayLoop("idle");
  foreach(var e in r.reynardEvents)if(e.kind=="Bullet"&&InsideBattle(e.from))StartCoroutine(ReynardBulletVisual(e.from,e.to));
  if(hasBullet){yield return new WaitForSecondsRealtime(.14f);if(reynardCaptureBullet){reynardCaptureBullet=false;Canvas.ForceUpdateCanvases();CaptureCanvasToPng(System.IO.Path.Combine(Application.dataPath,"../Logs/reynard-forward-bullet.png"));}}
  foreach(var e in r.reynardEvents){if(e.kind=="Bullet"||!InsideBattle(e.to))continue;string icon=e.kind.Contains("Sigil")?"R30":e.kind.Contains("Tail")?"R34":e.kind=="Ray"?"R05":e.kind=="SpellDirect"?"R04":"R01";string sequence=e.kind=="SigilCreated"?"SigilCreated":e.kind=="SigilSpent"?"SigilSpent":e.kind=="TailGained"?"TailGained":e.kind.StartsWith("TailSpent")?"TailSpent":e.kind=="TailOverflow"?"TailOverflow":e.kind=="Fragment"?"SplitOrb":e.kind=="Star"?"StarChaser":e.kind=="ChargedOrb"?"ChargedOrb":e.kind=="RuneDetonation"?"RuneDetonation":e.kind=="CircleResonance"?"CircleResonance":e.kind=="ArcaneManifest"?"ArcaneManifest":e.kind=="FoxfireGuard"?"FoxfireGuard":e.kind=="FoxflameRite"?"FoxflameRite":e.kind=="TailToSigil"?"TailToSigil":e.kind=="SpiritFoxHunt"?"SpiritFoxHunt":e.kind=="TailwindChant"?"TailwindChant":e.kind=="ScorchingRay"?"ScorchingRay":e.kind=="RuneStep"?"RuneStep":e.kind=="StoneWall"?"StoneWall":e.kind=="TailNova"?"TailNova":e.kind=="MassHold"?"MassHold":e.kind=="MirrorWardBlock"?"MirrorWardBlock":null;StartCoroutine(ReynardBurst(e.to,icon,e.kind=="Ray"?40:e.kind=="TailNova"?92:e.kind=="MassHold"?86:68,sequence));}
  if(r.reynardCast!=KaitSkill.None){GameAudio.PlayReynard("SpellCast");var spell=ReynardCatalog.Get(r.reynardCast);GameAudio.PlayReynardSpellVoice(spell!=null&&spell.spellLevel>=3);}
  else if(hasBullet){GameAudio.PlayReynard("Foxfire");GameAudio.PlayKaitNormalAttackVoice();}
  foreach(var e in before){var now=run.enemies.Find(x=>x.id==e.id);if(now!=null&&now.hp<e.hp){var view=EnemySpine(e);view?.PlayDamage();view?.SetHitFlash(1);}}
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
  if(run.ended){
   if(endOverlay!=null)endOverlay.SetActive(false);
   kaitSpine?.PlayOnce(run.won?KaitSpineView.Victory:KaitSpineView.Die,null);
   if(!run.won&&kaitDefeatingEnemyId>=0){EnemySpine(kaitDefeatingEnemyId)?.PlayVictoryLoop();yield return new WaitForSecondsRealtime(.72f);}
   RecordCharacterScore();ShowEnd();
  }
  else if(continueBuffered){var d=yummnBufferedDirection.Value;yummnBufferedDirection=null;HandleDirection(d);}
  else kaitSpine?.PlayLoop("idle");
 }
 private IEnumerator ReynardBulletVisual(Vector2Int from,Vector2Int to)
 {
  var sprite=ReynardArt.ImpactFrame(0);if(sprite==null)yield break;
  var go=new GameObject("Reynard Forward Bullet",typeof(RectTransform),typeof(Image));go.transform.SetParent(battleEnemyHitLayer,false);
  var image=go.GetComponent<Image>();image.enabled=false;image.sprite=sprite;image.raycastTarget=false;image.preserveAspect=true;image.rectTransform.sizeDelta=Vector2.one*46;
  Vector3 a=battleCells[from.x+from.y*KaitRun.BattleSize].transform.position,b;
  if(InsideBattle(to))b=battleCells[to.x+to.y*KaitRun.BattleSize].transform.position;
  else
  {
   Vector2Int d=to-from,inside=from-d;
   b=InsideBattle(inside)?a+(a-battleCells[inside.x+inside.y*KaitRun.BattleSize].transform.position):a+new Vector3(d.x*100,d.y*100,0);
  }
  // Place the projectile before enabling its renderer.  Otherwise its default
  // localPosition (the screen centre) can be presented for one frame.
  go.transform.position=a;go.transform.SetAsLastSibling();image.enabled=true;
  for(float t=0;t<.14f;t+=Time.unscaledDeltaTime){if(go==null)yield break;float u=1f-Mathf.Pow(1f-Mathf.Clamp01(t/.14f),3f);go.transform.position=Vector3.LerpUnclamped(a,b,u);image.color=new Color(.86f,.96f,1f,Mathf.Lerp(.96f,.72f,u));image.rectTransform.localScale=Vector3.one*(1f+.12f*Mathf.Sin(u*Mathf.PI));if(reynardCaptureBullet&&u>.45f){reynardCaptureBullet=false;Canvas.ForceUpdateCanvases();CaptureCanvasToPng(System.IO.Path.Combine(Application.dataPath,"../Logs/reynard-forward-bullet.png"));}yield return null;}
  if(go!=null)Destroy(go);
 }
 private IEnumerator ReynardBurst(Vector2Int p,string icon,float size,string sequence=null)
 {
  var first=sequence==null?ReynardArt.ImpactFrame(0):ReynardArt.SequenceFrame(sequence,0);if(first==null)yield break;
  var go=new GameObject("Reynard Spell FX",typeof(RectTransform),typeof(Image));go.transform.SetParent(battleEnemyHitLayer,false);go.transform.position=battleCells[p.x+p.y*KaitRun.BattleSize].transform.position;
  var image=go.GetComponent<Image>();image.sprite=first;image.raycastTarget=false;image.preserveAspect=true;
  for(float t=0;t<.3f;t+=Time.unscaledDeltaTime){if(go==null)yield break;int i=Mathf.Clamp((int)(t/.3f*8),0,7);var frame=sequence==null?ReynardArt.ImpactFrame(i):ReynardArt.SequenceFrame(sequence,i);if(frame!=null)image.sprite=frame;image.rectTransform.sizeDelta=Vector2.one*size;image.color=new Color(1,1,1,1-t/.3f);if(reynardCaptureFx){reynardCaptureFx=false;var capture=sequence==null?ReynardArt.ImpactFrame(3):ReynardArt.SequenceFrame(sequence,3);if(capture!=null)image.sprite=capture;Canvas.ForceUpdateCanvases();CaptureCanvasToPng(System.IO.Path.Combine(Application.dataPath,"../Logs/reynard-live-impact.png"));}yield return null;}Destroy(go);
 }
 private void RefreshReynardHud()
 {
  if(!run.IsReynard){foreach(var go in reynardPersistent)if(go!=null)Destroy(go);reynardPersistent.Clear();reynardVisualHash=int.MinValue;if(storybookPower!=null){storybookPower.rectTransform.anchoredPosition=new Vector2(-32,-19);storybookPower.rectTransform.sizeDelta=new Vector2(24,28);storybookPower.fontSize=21;storybookPower.alignment=TextAnchor.MiddleCenter;}if(reynardMirrorGhost!=null)reynardMirrorGhost.Root.gameObject.SetActive(false);return;}
  int visualHash=17;for(int y=0;y<run.ThreatSize;y++)for(int x=0;x<run.ThreatSize;x++)visualHash=visualHash*31+(run.Reynard.sigils[x,y]?1:0);visualHash=visualHash*31+run.Reynard.fogCell.GetHashCode();visualHash=visualHash*31+run.Reynard.webCell.GetHashCode();visualHash=visualHash*31+run.Reynard.stoneWallCell.GetHashCode();
  bool visualsChanged=visualHash!=reynardVisualHash;if(visualsChanged){reynardVisualHash=visualHash;foreach(var go in reynardPersistent)if(go!=null)Destroy(go);reynardPersistent.Clear();}
  if(storybookPower!=null){storybookPower.rectTransform.anchoredPosition=new Vector2(28,-19);storybookPower.rectTransform.sizeDelta=new Vector2(140,28);storybookPower.fontSize=17;storybookPower.alignment=TextAnchor.MiddleLeft;storybookPower.text=$"印 {run.ReynardSigilCount}  尾 {run.Reynard.tails}/{run.ReynardTailCap}";}
  if(buildDirectionText!=null)buildDirectionText.text=run.Reynard.hasLastDirection?"方向射击：向 "+DirectionGlyph(run.Reynard.lastDirection)+" 前方发射 1 伤子弹":"输入方向，即使受阻也发射 1 伤子弹";
  var fist=storybookPower!=null?storybookPower.transform.parent.Find("Fist")?.GetComponent<Image>():null;
  if(fist!=null)fist.sprite=ReynardArt.Load("SigilTailIcon")??ReynardArt.Load("Cards/R30");
  if(visualsChanged)RefreshReynardSustain();
  var p=run.Reynard.mirrorFoxCell;if(p.x<0){if(reynardMirrorGhost!=null)reynardMirrorGhost.Root.gameObject.SetActive(false);return;}
  var parent=threatCells[p.x+p.y*run.ThreatSize].transform;
  if(reynardMirrorGhost==null)reynardMirrorGhost=KaitSpineView.Create(makotoSkeletonData,parent,new Vector2(82,82),"Reynard Mirror Trail");
  if(reynardMirrorGhost==null)return;
  reynardMirrorGhost.Root.SetParent(parent,false);reynardMirrorGhost.Root.anchoredPosition=Vector2.zero;reynardMirrorGhost.Root.gameObject.SetActive(true);reynardMirrorGhost.Root.SetAsFirstSibling();
 reynardMirrorGhost.Face(run.Reynard.hasLastDirection?run.Reynard.lastDirection:KaitDirection.Right);reynardMirrorGhost.SetTint(new Color(.70f,.88f,1f,.42f));reynardMirrorGhost.PlayLoop("idle_2");

 }
 private void RefreshReynardSustain()
 {
  for(int y=0;y<run.ThreatSize;y++)for(int x=0;x<run.ThreatSize;x++)if(run.Reynard.sigils[x,y]){
   var go=new GameObject("Reynard Sigil",typeof(RectTransform),typeof(Image));go.transform.SetParent(threatCells[x+y*run.ThreatSize].transform,false);go.transform.SetAsFirstSibling();
   var im=go.GetComponent<Image>();im.sprite=ReynardArt.SequenceFrame("SigilIdle",0)??ReynardArt.Load("SigilMarker")??ReynardArt.Load("Cards/R30");im.preserveAspect=true;im.raycastTarget=false;im.color=new Color(1f,1f,1f,.86f);im.rectTransform.sizeDelta=Vector2.one*58;reynardPersistent.Add(go);StartCoroutine(AnimateReynardSigil(im,x+y*run.ThreatSize));
  }
  DrawReynardField(run.Reynard.fogCell,"FogField","Cards/R09",true,.42f);
  DrawReynardField(run.Reynard.webCell,"WebField","Cards/R10",true,.48f);
  DrawReynardField(run.Reynard.stoneWallCell,"StoneWallField","Cards/R42",false,.94f);
 }
 private void DrawReynardField(Vector2Int center,string art,string fallback,bool cross,float alpha)
 {
  if(!InsideBattle(center))return;var sprite=ReynardArt.Load(art)??ReynardArt.Load(fallback);if(sprite==null)return;
  for(int y=1;y<6;y++)for(int x=1;x<6;x++){
   var p=new Vector2Int(x,y);if(cross&&Mathf.Abs(x-center.x)+Mathf.Abs(y-center.y)>1||!cross&&p!=center)continue;
   var go=new GameObject("Reynard Field",typeof(RectTransform),typeof(Image));go.transform.SetParent(battleUnderEffectLayer,false);go.transform.position=battleCells[x+y*KaitRun.BattleSize].transform.position;
   var im=go.GetComponent<Image>();im.sprite=sprite;im.preserveAspect=true;im.raycastTarget=false;im.color=new Color(1,1,1,alpha);im.rectTransform.sizeDelta=Vector2.one*(cross?102:96);reynardPersistent.Add(go);
  }
 }
 private IEnumerator AnimateReynardSigil(Image image,int phase)
 {
  while(image!=null){int frame=((int)(Time.unscaledTime/.11f)+phase)%8;var sprite=ReynardArt.SequenceFrame("SigilIdle",frame);if(sprite!=null)image.sprite=sprite;float pulse=.82f+.12f*Mathf.Sin((Time.unscaledTime+phase*.07f)*Mathf.PI*2f);image.color=new Color(1f,1f,1f,pulse);yield return null;}
 }
}

