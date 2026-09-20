using System.Collections.Generic;
using UnityEngine;
public sealed partial class KaitRun
{
 public int ReynardSlotValue=>Reynard.mirrorFoxCell.x>=0?threat[Reynard.mirrorFoxCell.x,Reynard.mirrorFoxCell.y]:0;
 public bool ReynardSlotReady=>ReynardSlotValue>0&&Reynard.spellReady[Reynard.mirrorFoxCell.x,Reynard.mirrorFoxCell.y];
 private void InitializeMirrorFox(){for(int y=0;y<ThreatSize;y++)for(int x=0;x<ThreatSize;x++)if(threat[x,y]>0){Reynard.mirrorFoxCell=new Vector2Int(x,y);return;}}
 private List<KaitMergeEvent> MoveThreatReynard(KaitDirection direction,List<KaitThreatMotion> motions)
 {
  var merges=new List<KaitMergeEvent>();bool horizontal=direction==KaitDirection.Left||direction==KaitDirection.Right,reverse=direction==KaitDirection.Up||direction==KaitDirection.Right;
  for(int line=0;line<ThreatSize;line++){
   var segment=new List<Vector2Int>();
   for(int i=0;i<ThreatSize;i++){int index=reverse?ThreatSize-1-i:i;var p=horizontal?new Vector2Int(index,line):new Vector2Int(line,index);
    if(IsThreatPillar(p)){ProcessReynardLine(segment,motions,merges);segment.Clear();}else segment.Add(p);
   }ProcessReynardLine(segment,motions,merges);
  }return merges;
 }
 private void ProcessReynardLine(List<Vector2Int> cells,List<KaitThreatMotion> motions,List<KaitMergeEvent> merges)
 {
  int fox=cells.IndexOf(Reynard.mirrorFoxCell);
  if(fox<0){PackReynard(cells,false,motions,merges);return;}
  PackReynard(cells.GetRange(0,fox),false,motions,merges);
  PackReynard(cells.GetRange(fox,cells.Count-fox),true,motions,merges);
 }
 private void PackReynard(List<Vector2Int> cells,bool anchored,List<KaitThreatMotion> motions,List<KaitMergeEvent> merges)
 {
  if(cells.Count==0)return;var tokens=new List<ThreatToken>();
  foreach(var p in cells){if(threat[p.x,p.y]==0)continue;var t=new ThreatToken{value=threat[p.x,p.y],birthOrder=threatTwoBirth[p.x,p.y],spellReady=Reynard.spellReady[p.x,p.y],mirrorFox=p==Reynard.mirrorFoxCell};t.sources.Add(p);tokens.Add(t);}
  var packed=new List<ThreatToken>();
  for(int i=0;i<tokens.Count;i++){
   var a=tokens[i];if(i+1<tokens.Count&&a.value==tokens[i+1].value){var b=tokens[++i];var t=new ThreatToken{value=a.value*2,merged=true,spellReady=true,mirrorFox=a.mirrorFox||b.mirrorFox,birthOrder=++nextThreatTwoBirth};t.sources.AddRange(a.sources);t.sources.AddRange(b.sources);packed.Add(t);}else packed.Add(a);
  }
  foreach(var p in cells){threat[p.x,p.y]=threatTwoBirth[p.x,p.y]=0;Reynard.spellReady[p.x,p.y]=false;}
  for(int i=0;i<packed.Count;i++){
   var t=packed[i];var p=cells[i];threat[p.x,p.y]=t.value;threatTwoBirth[p.x,p.y]=t.birthOrder;Reynard.spellReady[p.x,p.y]=t.spellReady;
   if(t.mirrorFox)Reynard.mirrorFoxCell=p;
   foreach(var from in t.sources)motions.Add(new KaitThreatMotion{from=from,to=p,value=t.merged?t.value/2:t.value,merged=t.merged});
   if(t.merged)merges.Add(new KaitMergeEvent{sourceValue=t.value/2,resultValue=t.value,threatCell=p,sequence=merges.Count,actualThreatDirection=actualThreatDirection,mergeSource="ReynardSlide",rootActionId=Reynard.actionId});
  }
 }
 private void ProcessReynardMerges(KaitTurnResult r)
 {
  bool foxMerged=false;foreach(var m in r.merges){RegisterReynardMerge(m,r);foxMerged|=m.threatCell==Reynard.mirrorFoxCell;}
  if(!foxMerged||!HasPassive(KaitPassive.ReynardArcaneDevour))return;
  while(true){int best=int.MaxValue;var found=new Vector2Int(-1,-1);for(int y=0;y<ThreatSize;y++)for(int x=0;x<ThreatSize;x++){
   var p=new Vector2Int(x,y);if(p==Reynard.mirrorFoxCell||threat[x,y]!=ReynardSlotValue)continue;int dist=Manhattan(p,Reynard.mirrorFoxCell);if(dist<best){best=dist;found=p;}}
   if(found.x<0)break;int v=ReynardSlotValue;threat[found.x,found.y]=threatTwoBirth[found.x,found.y]=0;Reynard.spellReady[found.x,found.y]=false;
   var fox=Reynard.mirrorFoxCell;threat[fox.x,fox.y]=v*2;Reynard.spellReady[fox.x,fox.y]=true;
   r.threatMotions.Add(new KaitThreatMotion{from=found,to=fox,value=v,merged=true});
   var m=new KaitMergeEvent{sourceValue=v,resultValue=v*2,threatCell=fox,sequence=r.merges.Count,actualThreatDirection=actualThreatDirection,mergeSource="ReynardDevour",rootActionId=Reynard.actionId};r.merges.Add(m);RegisterReynardMerge(m,r);Reynard.devours++;
  }
 }
 private void RegisterReynardMerge(KaitMergeEvent m,KaitTurnResult r)
 {
  m.mergeId=m.sequence+1;highestThreat=Mathf.Max(highestThreat,m.resultValue);mergeHeatmap[m.threatCell.x,m.threatCell.y]++;
  HandleMilestoneMergeWithResult(m,r);if(m.resultValue<config.winValue)QueueSpawn(m,r);
  if(HasPassive(KaitPassive.ReynardArcaneRecall)&&Manhattan(m.threatCell,Reynard.mirrorFoxCell)==1&&ReynardSlotValue>0)Reynard.spellReady[Reynard.mirrorFoxCell.x,Reynard.mirrorFoxCell.y]=true;
 }
 private static int Manhattan(Vector2Int a,Vector2Int b)=>Mathf.Abs(a.x-b.x)+Mathf.Abs(a.y-b.y);
}
