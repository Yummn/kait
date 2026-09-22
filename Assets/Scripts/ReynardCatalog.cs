using System;
using System.Collections.Generic;

public static class ReynardCatalog
{
 public const string Version="reynard-dual42-animation-20260921";
 public const string RulesVersion="Reynard.0.6-targeted-shot-animation";
 private static readonly string[] Allowed={"Reynard"};
 private static KaitAbilityDef A(string id,string zh,string en,KaitRarity rarity,int cost,string text,params string[] tags)=>new KaitAbilityDef{id="reynard."+id,nameZh=zh,nameEn=en,cardText=text,kind=KaitAbilityKind.Active,rarity=rarity,skill=Skill(id),spellLevel=cost,allowedCharacters=Allowed,effectTags=tags};
 private static KaitAbilityDef P(string id,string zh,string en,KaitRarity rarity,KaitPassive passive,string text,params string[] tags)=>new KaitAbilityDef{id="reynard."+id,nameZh=zh,nameEn=en,cardText=text,kind=KaitAbilityKind.Passive,rarity=rarity,passive=passive,allowedCharacters=Allowed,effectTags=tags};
 private static KaitSkill Skill(string id)
 {
  switch(id){
   case "R01":return KaitSkill.ReynardThunderwave;case "R02":return KaitSkill.ReynardMistyStep;case "R03":return KaitSkill.ReynardShatter;
   case "R04":return KaitSkill.ReynardFireball;case "R05":return KaitSkill.ReynardLightningBolt;case "R06":return KaitSkill.ReynardCounterspell;
   case "R07":return KaitSkill.ReynardChainLightning;case "R09":return KaitSkill.ReynardFog;case "R10":return KaitSkill.ReynardWeb;
   case "R40":return KaitSkill.ReynardScorchingRay;case "R41":return KaitSkill.ReynardRuneStep;case "R42":return KaitSkill.ReynardStoneWall;
   case "R43":return KaitSkill.ReynardTailNova;case "R44":return KaitSkill.ReynardMassHold;default:return KaitSkill.None;
  }
 }
 public static readonly List<KaitAbilityDef> Cards=new List<KaitAbilityDef>{
  A("R01","雷鸣波","Thunderwave",KaitRarity.Common,1,"四邻敌人各受2伤，存活者向外推1格","ReynardInstant","ReynardAttackSpell"),
  A("R02","迷踪步","Misty Step",KaitRarity.Uncommon,2,"传送至三格内的一处合法空格","ReynardInstant","ReynardTargeted"),
  A("R03","粉碎音波","Shatter",KaitRarity.Uncommon,2,"三格内选一格，对该格及四邻敌人各造成2伤","ReynardInstant","ReynardAttackSpell","ReynardTargeted"),
  A("R04","火球术","Fireball",KaitRarity.Uncommon,3,"三格内选一格，对该格及八邻敌人各造成2伤","ReynardInstant","ReynardAttackSpell","ReynardTargeted"),
  A("R05","闪电束","Lightning Bolt",KaitRarity.Uncommon,3,"向指定方向贯穿射击，直线上每名敌人各受3伤","ReynardInstant","ReynardAttackSpell"),
  A("R06","法术反制","Counterspell",KaitRarity.Uncommon,3,"清除全场敌人当前已有的攻击预告","ReynardInstant"),
  A("R07","连锁闪电","Chain Lightning",KaitRarity.Rare,5,"选择三格内敌人，至多跳向五名近敌，各造成3伤","ReynardInstant","ReynardAttackSpell","ReynardTargeted"),
  P("R08","魔法飞弹阵","Missile Array",KaitRarity.Uncommon,KaitPassive.ReynardMissileArrayPassive,"每次完整射击结束后，向最近敌人追加一枚1伤飞弹","ReynardShotAfter"),
  A("R09","云雾术","Fog Cloud",KaitRarity.Common,1,"三格内一格及四邻留下阻挡敌我射线的云雾","ReynardField","ReynardTargeted"),
  A("R10","蛛网结界","Web",KaitRarity.Uncommon,2,"三格内一格及四邻留下蛛网，禁止敌人自主移动","ReynardField","ReynardTargeted"),
  P("R11","环身狐火","Foxfire Ring",KaitRarity.Rare,KaitPassive.ReynardFoxfirePassive,"每次完整射击结束后，对本体四邻敌人各造成1伤","ReynardShotAfter"),
  P("R12","瞬发秘典","Quickened Tome",KaitRarity.Uncommon,KaitPassive.ReynardQuickenedTome,"即时法术结束后，沿当前朝向追加一次完整射击","ReynardCastAfter"),
  P("R13","三岔弹幕","Trident Barrage",KaitRarity.Common,KaitPassive.ReynardTrident,"每次完整射击，额外向左右各发射一枚法球"),
  P("R14","爆裂弹芯","Burst Core",KaitRarity.Uncommon,KaitPassive.ReynardRingFlame,"法球命中时，对命中格四邻其他敌人各造成1伤"),
  P("R15","冲击弹","Impact Orb",KaitRarity.Uncommon,KaitPassive.ReynardBackfirePush,"法球命中后，将存活目标沿弹道推开1格"),
  P("R16","穿透弹","Piercing Orb",KaitRarity.Rare,KaitPassive.ReynardInterwoven,"普通法球贯穿直线上的敌人，实体地形仍阻挡"),
  P("R17","静心射击","Still Shot",KaitRarity.Common,KaitPassive.ReynardStillBackfire,"等待时，沿最近移动方向发射一次完整射击"),
  P("R19","奥术吞并","Arcane Devour",KaitRarity.Rare,KaitPassive.ReynardArcaneDevour,"镜狐数字合并后，连续吞并全场最近的同值数字"),
  P("R20","超魔·重奏","Metamagic Reprise",KaitRarity.Rare,KaitPassive.ReynardReprise,"即时攻击法术造成生命伤害后，对存活目标追加等量伤害"),
  P("R21","超魔·扩散","Metamagic Diffusion",KaitRarity.Rare,KaitPassive.ReynardDiffusion,"秘术直接伤害造成生命损失后，四邻其他敌人各受1伤"),
  P("R22","远距法术","Distant Spell",KaitRarity.Uncommon,KaitPassive.ReynardDistantSpell,"主动法术的目标选取距离增加2格"),
  P("R23","镜衣结界","Mirror Ward",KaitRarity.Uncommon,KaitPassive.ReynardMirrorWard,"支付材料施法后，抵挡随后敌方阶段的第一次伤害"),
  P("R25","双矢秘法","Twin Volley",KaitRarity.Rare,KaitPassive.ReynardDoubleVolley,"每次完整射击连续发射两轮法球"),
  P("R26","分裂弹","Split Orb",KaitRarity.Uncommon,KaitPassive.ReynardSplitOrb,"法球造成生命伤害后，向弹道两侧各发射1伤碎片"),
  P("R27","逐星弹","Star Chaser",KaitRarity.Uncommon,KaitPassive.ReynardStarOrb,"法球击杀后，向两格内最近敌人发射1伤追星弹"),
  P("R28","聚能弹","Charged Orb",KaitRarity.Common,KaitPassive.ReynardChargedOrb,"法球命中本体四邻敌人时，额外造成1伤"),
  P("R29","倒卷法球","Reverse Orb",KaitRarity.Common,KaitPassive.ReynardReverseOrb,"每次完整射击，额外向反方向发射一枚法球"),
  P("R30","符文引爆","Rune Detonation",KaitRarity.Uncommon,KaitPassive.ReynardRuneDetonation,"法印作为施法材料被消耗时，对映射格敌人造成1伤"),
  P("R31","法阵共振","Circle Resonance",KaitRarity.Rare,KaitPassive.ReynardCircleResonance,"消耗法印时，对其原先连通法印位置各造成1伤"),
  P("R32","溢墨之瓶","Overflow Ink",KaitRarity.Uncommon,KaitPassive.ReynardOverflowInk,"真实合并格已有法印时，尝试在四邻补一枚法印"),
  P("R33","秘法显形","Arcane Manifest",KaitRarity.Common,KaitPassive.ReynardArcaneManifest,"新法印真正生成时，对映射格敌人造成1伤"),
  P("R34","九尾血脉","Nine-Tailed Blood",KaitRarity.Rare,KaitPassive.ReynardNineTails,"狐尾储存上限提高到9"),
  P("R35","狐火护身","Foxfire Guard",KaitRarity.Uncommon,KaitPassive.ReynardTailGuard,"每有1条狐尾用于抵伤，对攻击者造成1伤"),
  P("R36","狐焰祭仪","Foxflame Rite",KaitRarity.Uncommon,KaitPassive.ReynardFoxfireRite,"每使用1条狐尾支付法术，对本体四邻敌人各造成1伤"),
  P("R37","聚尾成印","Tail to Sigil",KaitRarity.Uncommon,KaitPassive.ReynardTailToSigil,"每条将要溢出的狐尾，尝试在镜狐四邻补一枚法印"),
  P("R38","灵狐猎场","Spirit Fox Hunt",KaitRarity.Uncommon,KaitPassive.ReynardFoxHunt,"镜狐参与真实合并时，对映射格及四邻敌人各造成1伤"),
  P("R39","尾流咏唱","Tailwind Chant",KaitRarity.Rare,KaitPassive.ReynardTailChant,"每消耗1条狐尾，沿当前朝向追加一次完整射击"),
  A("R40","灼热射线","Scorching Ray",KaitRarity.Uncommon,2,"向指定方向连射三道火线，每道对当前首敌造成1伤","ReynardInstant","ReynardAttackSpell"),
  A("R41","符文跃迁","Rune Step",KaitRarity.Uncommon,2,"选择法印映射的空格传送；该法印计入费用","ReynardInstant","ReynardTargeted","ReynardSigilTarget"),
  A("R42","石墙术","Stone Wall",KaitRarity.Uncommon,2,"三格内空格升起永久石柱，仅保留最新一根","ReynardField","ReynardTargeted"),
  A("R43","燃尾新星","Tail Nova",KaitRarity.Rare,2,"消耗2条狐尾，对本体八邻敌人各造成3伤","ReynardInstant","ReynardAttackSpell","ReynardTailOnly"),
  A("R44","群体定身术","Mass Hold",KaitRarity.Rare,4,"三格内一格及四邻敌人跳过下一次行动","ReynardInstant","ReynardTargeted")
 };
 public static readonly List<KaitAbilityDef> Legacy=new List<KaitAbilityDef>{
  P("R18","奥术回想","Arcane Recall",KaitRarity.Uncommon,KaitPassive.ReynardArcaneRecall,"旧规则：镜狐四邻合并时恢复法术位"),
  P("R24","法术协鸣","Concord",KaitRarity.Uncommon,KaitPassive.ReynardConcord,"旧规则：即时法术后追加持续法术攻击")
 };
 public static List<KaitAbilityDef> Pool()=>new List<KaitAbilityDef>(Cards);
 public static KaitAbilityDef Get(KaitSkill s)=>Cards.Find(d=>d.kind==KaitAbilityKind.Active&&d.skill==s)??Legacy.Find(d=>d.kind==KaitAbilityKind.Active&&d.skill==s);
 public static KaitAbilityDef Get(KaitPassive p)=>Cards.Find(d=>d.kind==KaitAbilityKind.Passive&&d.passive==p)??Legacy.Find(d=>d.kind==KaitAbilityKind.Passive&&d.passive==p);
 public static KaitAbilityDef Get(string id)=>Cards.Find(d=>d.id==id)??Legacy.Find(d=>d.id==id);
 public static bool Has(KaitAbilityDef d,string tag)=>d!=null&&d.effectTags!=null&&Array.IndexOf(d.effectTags,tag)>=0;
 public static bool IsField(KaitSkill s)=>Has(Get(s),"ReynardField");
 public static bool IsInstant(KaitSkill s)=>Has(Get(s),"ReynardInstant");
 public static bool IsAttackSpell(KaitSkill s)=>Has(Get(s),"ReynardAttackSpell");
 public static bool IsDefenseSpell(KaitSkill s)=>s==KaitSkill.ReynardCounterspell||s==KaitSkill.ReynardFog||s==KaitSkill.ReynardStoneWall;
 public static string CastAnimation(KaitSkill s)=>IsAttackSpell(s)?"skill":IsDefenseSpell(s)?"uniqueskill":"prepare";
 public static int Cost(KaitSkill s)=>Get(s)?.spellLevel??0;
 public static int Level(int value){int n=0;while(value>1){value>>=1;n++;}return n;}
 public static string Roman(int n)=>n>0&&n<11?new[]{"","I","II","III","IV","V","VI","VII","VIII","IX","X"}[n]:n.ToString();
 public static string TargetHint(KaitSkill s)=>s==KaitSkill.ReynardLightningBolt||s==KaitSkill.ReynardScorchingRay?"点选四邻方向":s==KaitSkill.ReynardThunderwave||s==KaitSkill.ReynardCounterspell||s==KaitSkill.ReynardTailNova?"点选自身施放":"点选目标格";
}
