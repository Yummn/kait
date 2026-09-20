from pathlib import Path
P=Path('Assets/Scripts')
def edit(f,a,b):
 p=P/f;s=p.read_text(encoding='utf-8-sig');assert a in s,(f,a);p.write_text(s.replace(a,b),encoding='utf-8')
active=['Thunderwave','MistyStep','Shatter','Fireball','LightningBolt','Counterspell','ChainLightning','MissileArray','Fog','Web','Foxfire','QuickenedCircle']
passive=['Trident','RingFlame','BackfirePush','Interwoven','StillBackfire','ArcaneRecall','ArcaneDevour','Reprise','Diffusion','DistantSpell','MirrorWard','Concord']
edit('KaitCore.cs','EldritchBlast, HungerOfHadar }','EldritchBlast, HungerOfHadar, '+', '.join('Reynard'+x for x in active)+' }')
edit('KaitPassives.cs','    EldritchBlast\n}','    EldritchBlast,\n    '+', '.join('Reynard'+x for x in passive)+'\n}')
edit('KaitAbilities.cs','public int kiExtraCost;','public int kiExtraCost;\n    public int spellLevel;')
edit('KaitAbilities.cs','?? YummnCatalog.Cards.Find(d=>d.kind==KaitAbilityKind.Active && d.skill==skill);','?? YummnCatalog.Cards.Find(d=>d.kind==KaitAbilityKind.Active && d.skill==skill) ?? ReynardCatalog.Get(skill);')
edit('KaitAbilities.cs','?? YummnCatalog.Cards.Find(d=>d.kind==KaitAbilityKind.Passive && d.passive==passive);','?? YummnCatalog.Cards.Find(d=>d.kind==KaitAbilityKind.Passive && d.passive==passive) ?? ReynardCatalog.Get(passive);')
edit('KaitYummnRules.cs','{ Kait, Yummn }','{ Kait, Yummn, Reynard }')
edit('KaitYummnRules.cs','public bool IsYummn =>','public readonly ReynardRun Reynard=new ReynardRun();\n    public bool IsReynard=>Character==KaitCharacter.Reynard;\n    public bool IsYummn =>')
edit('KaitYummnRules.cs','IsYummn?Yummn.rules.Version:KaitAbilityCatalog.RulesVersion','IsReynard?ReynardCatalog.RulesVersion:IsYummn?Yummn.rules.Version:KaitAbilityCatalog.RulesVersion')
edit('KaitCore.cs','        ResetYummnTurn();','        ResetYummnTurn();\n        Reynard.Reset(ThreatSize);')
edit('KaitCore.cs','        for (int i = 0; i < config.initialThreatTiles; i++) SpawnThreatTwo();','        for (int i = 0; i < config.initialThreatTiles; i++) SpawnThreatTwo();\n        if(IsReynard)InitializeMirrorFox();')
edit('KaitCore.cs','        if (IsYummn) return TryYummnDirection(direction);','        if (IsReynard) return TryReynardDirection(direction);\n        if (IsYummn) return TryYummnDirection(direction);')
edit('KaitCore.cs','        threat[p.x, p.y] = 2;','        threat[p.x, p.y] = 2;\n        if(IsReynard)Reynard.spellReady[p.x,p.y]=true;')
edit('KaitCore.cs','public bool merged; }','public bool merged, spellReady, mirrorFox; }')
edit('KaitCore.cs','        int appliedDamage = Mathf.Min(kateHp, amount);','        if(IsReynard && Reynard.mirrorWard){Reynard.mirrorWard=false;return 0;}\n        int appliedDamage = Mathf.Min(kateHp, amount);')
edit('KaitAbilityCombat.cs','public static bool NeedsCellTarget(KaitSkill s) =>','public static bool NeedsCellTarget(KaitSkill s) => ReynardCatalog.Get(s)!=null ||')
edit('KaitAbilityCombat.cs','        if(IsYummn&&YummnCatalog.IsActive(skill))','        if(IsReynard)return IsLegalReynardCell(skill,cell);\n        if(IsYummn&&YummnCatalog.IsActive(skill))')
edit('KaitAbilityCombat.cs','        if(IsYummn)return CastYummnCell(skill,cell,out message);','        if(IsReynard)return CastReynard(skill,cell,out message);\n        if(IsYummn)return CastYummnCell(skill,cell,out message);')
edit('YummnWait.cs','        var r=new KaitTurnResult();','        if(IsReynard)return TryReynardWait();\n        var r=new KaitTurnResult();')
edit('KaitBuildState.cs','IsYummn ? YummnCatalog.Pool() : KaitAbilityCatalog.DefaultPool()','IsReynard ? ReynardCatalog.Pool() : IsYummn ? YummnCatalog.Pool() : KaitAbilityCatalog.DefaultPool()')
edit('KaitBuildState.cs','YummnPrerequisite(d) &&','YummnPrerequisite(d) && (!IsReynard || ReynardPrerequisite(d)) &&')
for f in ['KaitBuildState.cs','KaitRunReplay.cs']:
 edit(f,'cardPoolVersion=IsYummn?YummnCatalog.Version:KaitAbilityCatalog.Version','cardPoolVersion=IsReynard?ReynardCatalog.Version:IsYummn?YummnCatalog.Version:KaitAbilityCatalog.Version')
edit('KaitRunReplay.cs','string expected=save.characterId==KaitCharacter.Yummn?','string expected=save.characterId==KaitCharacter.Reynard?ReynardCatalog.RulesVersion:save.characterId==KaitCharacter.Yummn?')
edit('KaitRunReplay.cs','bool compatiblePool=save.characterId==KaitCharacter.Yummn?','bool compatiblePool=save.characterId==KaitCharacter.Reynard?save.cardPoolVersion==ReynardCatalog.Version:save.characterId==KaitCharacter.Yummn?')
edit('KaitVersion.cs','App="0.9.23"','App="0.9.24"')
edit('KaitVersion.cs','AndroidCode=923;','AndroidCode=924;\n    public const string ReynardSaveKey="Kait.Run.Reynard.0.1";')
# Preserve card text verbatim from the frozen design.
text=Path('Logs/reynard-source.txt').read_text(encoding='utf-8').splitlines()
rows=[]
for i,line in enumerate(text):
 if line in ['R%02d'%n for n in range(1,25)]:
  j=int(line[1:]); rows.append((j,text[i+1],text[i+2],text[i+3],text[i+4],text[i+5]))
assert len(rows)==24,len(rows)
s='''using System;
using System.Collections.Generic;
public static class ReynardCatalog
{
 public const string Version="reynard-core24-20260920";
 public const string RulesVersion="Reynard.0.1-direction-mirror-slot";
 public static readonly List<KaitAbilityDef> Cards=new List<KaitAbilityDef> {
'''
for j,name,kind,rar,cost,desc in rows:
 tags=[]
 if j<=12:
  tags.append('ReynardInstant' if j<=7 else 'ReynardSustain')
  if j in [1,3,4,5,7]:tags.append('ReynardAttackSpell')
  if j in [8,11]:tags.append('ReynardSustainAttack')
  if j in [2,3,4,7,9,10]:tags.append('ReynardTargeted')
 rarity=['普通','罕见','稀有'].index(rar)
 s+=' new KaitAbilityDef { id="reynard.R%02d", nameZh="%s", nameEn="%s", cardText="%s", kind=KaitAbilityKind.%s, rarity=(KaitRarity)%d, '%(j,name,(active+passive)[j-1],desc,'Active' if j<=12 else 'Passive',rarity)
 s+=('skill=KaitSkill.Reynard'+active[j-1]+', spellLevel='+cost[0] if j<=12 else 'passive=KaitPassive.Reynard'+passive[j-13])
 s+=', allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{'+','.join('"'+t+'"' for t in tags)+'} },\n'
s+=''' };
 public static List<KaitAbilityDef> Pool()=>new List<KaitAbilityDef>(Cards);
 public static KaitAbilityDef Get(KaitSkill s)=>Cards.Find(d=>d.kind==KaitAbilityKind.Active&&d.skill==s);
 public static KaitAbilityDef Get(KaitPassive p)=>Cards.Find(d=>d.kind==KaitAbilityKind.Passive&&d.passive==p);
 public static KaitAbilityDef Get(string id)=>Cards.Find(d=>d.id==id);
 public static bool Has(KaitAbilityDef d,string tag)=>d!=null&&d.effectTags!=null&&Array.IndexOf(d.effectTags,tag)>=0;
 public static int Level(int value){int n=0;while(value>1){value>>=1;n++;}return n;}
 public static string Roman(int n)=>n>0&&n<11?new[]{"","I","II","III","IV","V","VI","VII","VIII","IX","X"}[n]:n.ToString();
 public static string TargetHint(KaitSkill s)=>Has(Get(s),"ReynardTargeted")?"点选目标格":s==KaitSkill.ReynardLightningBolt?"点选四邻方向":"点选自身施放";
}
'''
(P/'ReynardCatalog.cs').write_text(s,encoding='utf-8')
(P/'ReynardRun.cs').write_text('''using System;
using UnityEngine;
[Serializable] public sealed class ReynardRun
{
 public bool hasLastDirection,mirrorWard;
 public KaitDirection lastDirection;
 public Vector2Int mirrorFoxCell=new Vector2Int(-1,-1),focusedCell;
 public bool[,] spellReady;
 public KaitSkill focusedSkill=KaitSkill.None;
 public int actionId,directionSpells,throughFires,turnFires,backfires,casts,summons,enemyPhases,devours;
 public void Reset(int size){spellReady=new bool[size,size];mirrorFoxCell=new Vector2Int(-1,-1);focusedCell=Vector2Int.zero;focusedSkill=KaitSkill.None;hasLastDirection=mirrorWard=false;lastDirection=KaitDirection.Up;actionId=directionSpells=throughFires=turnFires=backfires=casts=summons=enemyPhases=devours=0;}
}
public enum ReynardDamageKind { DirectionSpell,SpellDirect,SpellSplash }
public enum ReynardDirectionSpell { Through,Turn,Backfire }
[Serializable] public sealed class ReynardVisualEvent
{ public string kind; public Vector2Int from,to; public int amount; }
''',encoding='utf-8')
