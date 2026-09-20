from pathlib import Path
P=Path('Assets/Scripts')
p=P/'GameAudio.cs';s=p.read_text(encoding='utf-8-sig');i=s.index('    public static void PlayMagicImpact()');s=s[:i]+'''    public static void PlayReynard(string cue)
    {var clip=Resources.Load<AudioClip>("Audio/Reynard/"+cue);PlayOneShot(instance?.magicSource,clip,.65f,1f,1f);}
''' +s[i:];p.write_text(s,encoding='utf-8')
p=P/'ReynardRules.cs';s=p.read_text(encoding='utf-8-sig').replace('  ResolveSpawnRequests(r);','  bool ward=Reynard.mirrorWard;Reynard.mirrorWard=false;\n  ResolveSpawnRequests(r);').replace('  if(!ended){ResolveEnemyIntents(r);','  Reynard.mirrorWard=ward;\n  if(!ended){ResolveEnemyIntents(r);').replace('}AgePreparingEnemies();','}MoveReynardEnemies(r);AgePreparingEnemies();');s=s.replace('  var r=BeginReynardAction();lastSkillResult=r;','  var r=BeginReynardAction();r.reynardCast=s;lastSkillResult=r;');p.write_text(s,encoding='utf-8')
p=P/'KaitCore.cs';s=p.read_text(encoding='utf-8-sig').replace('public readonly List<ReynardVisualEvent> reynardEvents','public KaitSkill reynardCast;\n    public readonly List<ReynardVisualEvent> reynardEvents');p.write_text(s,encoding='utf-8')
p=P/'ReynardCombat.cs';s=p.read_text(encoding='utf-8-sig');idx=s.rfind('}');s=s[:idx]+''' private void MoveReynardEnemies(KaitTurnResult r)
 {
  foreach(var e in enemies){
   if(e.life!=KaitEnemyLife.Active||e.frozenActions>0||e.type==KaitEnemyType.Archer||e.type==KaitEnemyType.Warlock||Manhattan(e.pos,katePos)<=1)continue;
   if(Reynard.focusedSkill==KaitSkill.ReynardWeb&&Manhattan(e.pos,Reynard.focusedCell)<=1)continue;
   var delta=DirectionToward(e.pos,katePos);var to=e.pos+delta;if(IsHardBlocked(to)||to==katePos||EnemyAt(to)!=null)continue;
   var from=e.pos;e.pos=to;r.enemyActions.Add(new KaitEnemyAction{enemyId=e.id,type=KaitIntentType.Move,from=from,to=to});
  }
 }
''' +s[idx:];p.write_text(s,encoding='utf-8')
