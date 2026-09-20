using System;
using System.Collections.Generic;
public static class ReynardCatalog
{
 public const string Version="reynard-core24-direction-shot-20260920";
 public const string RulesVersion="Reynard.0.4-direction-shot";
 public static readonly List<KaitAbilityDef> Cards=new List<KaitAbilityDef> {
 new KaitAbilityDef { id="reynard.R01", nameZh="雷鸣波", nameEn="Thunderwave", cardText="四邻敌人受2伤，存活者向外推1格", kind=KaitAbilityKind.Active, rarity=(KaitRarity)0, skill=KaitSkill.ReynardThunderwave, spellLevel=1, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{"ReynardInstant","ReynardAttackSpell"} },
 new KaitAbilityDef { id="reynard.R02", nameZh="迷踪步", nameEn="MistyStep", cardText="传送至三格内的一个空格", kind=KaitAbilityKind.Active, rarity=(KaitRarity)1, skill=KaitSkill.ReynardMistyStep, spellLevel=2, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{"ReynardInstant","ReynardTargeted"} },
 new KaitAbilityDef { id="reynard.R03", nameZh="粉碎音波", nameEn="Shatter", cardText="三格内选一格，对该格及四邻造成2伤", kind=KaitAbilityKind.Active, rarity=(KaitRarity)1, skill=KaitSkill.ReynardShatter, spellLevel=2, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{"ReynardInstant","ReynardAttackSpell","ReynardTargeted"} },
 new KaitAbilityDef { id="reynard.R04", nameZh="火球术", nameEn="Fireball", cardText="三格内选一格，对该格及八邻造成2伤", kind=KaitAbilityKind.Active, rarity=(KaitRarity)1, skill=KaitSkill.ReynardFireball, spellLevel=3, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{"ReynardInstant","ReynardAttackSpell","ReynardTargeted"} },
 new KaitAbilityDef { id="reynard.R05", nameZh="闪电束", nameEn="LightningBolt", cardText="朝指定方向贯穿射击，每个敌人受3伤", kind=KaitAbilityKind.Active, rarity=(KaitRarity)1, skill=KaitSkill.ReynardLightningBolt, spellLevel=3, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{"ReynardInstant","ReynardAttackSpell"} },
 new KaitAbilityDef { id="reynard.R06", nameZh="法术反制", nameEn="Counterspell", cardText="清除全场敌人的当前攻击预告", kind=KaitAbilityKind.Active, rarity=(KaitRarity)1, skill=KaitSkill.ReynardCounterspell, spellLevel=3, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{"ReynardInstant"} },
 new KaitAbilityDef { id="reynard.R07", nameZh="连锁闪电", nameEn="ChainLightning", cardText="选三格内敌人，依次跳向两格内近敌；至多5名，各3伤", kind=KaitAbilityKind.Active, rarity=(KaitRarity)2, skill=KaitSkill.ReynardChainLightning, spellLevel=5, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{"ReynardInstant","ReynardAttackSpell","ReynardTargeted"} },
 new KaitAbilityDef { id="reynard.R08", nameZh="魔法飞弹阵", nameEn="MissileArray", cardText="每次方向射击后，向最近敌人追加1伤飞弹", kind=KaitAbilityKind.Active, rarity=(KaitRarity)0, skill=KaitSkill.ReynardMissileArray, spellLevel=1, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{"ReynardSustain","ReynardSustainAttack"} },
 new KaitAbilityDef { id="reynard.R09", nameZh="云雾术", nameEn="Fog", cardText="在三格内一格及四邻维持阻挡敌我射线的雾", kind=KaitAbilityKind.Active, rarity=(KaitRarity)0, skill=KaitSkill.ReynardFog, spellLevel=1, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{"ReynardSustain","ReynardTargeted"} },
 new KaitAbilityDef { id="reynard.R10", nameZh="蛛网结界", nameEn="Web", cardText="在三格内一格及四邻维持蛛网，禁止敌人自主移动", kind=KaitAbilityKind.Active, rarity=(KaitRarity)1, skill=KaitSkill.ReynardWeb, spellLevel=2, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{"ReynardSustain","ReynardTargeted"} },
 new KaitAbilityDef { id="reynard.R11", nameZh="环身狐火", nameEn="Foxfire", cardText="每次方向射击后，对四邻敌人造成1伤", kind=KaitAbilityKind.Active, rarity=(KaitRarity)1, skill=KaitSkill.ReynardFoxfire, spellLevel=3, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{"ReynardSustain","ReynardSustainAttack"} },
 new KaitAbilityDef { id="reynard.R12", nameZh="瞬发法阵", nameEn="QuickenedCircle", cardText="即时法术后，沿上次方向追加一发子弹", kind=KaitAbilityKind.Active, rarity=(KaitRarity)1, skill=KaitSkill.ReynardQuickenedCircle, spellLevel=2, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{"ReynardSustain"} },
 new KaitAbilityDef { id="reynard.R13", nameZh="三岔弹幕", nameEn="Trident", cardText="方向射击时，额外向左右各发射一发子弹", kind=KaitAbilityKind.Passive, rarity=(KaitRarity)0, passive=KaitPassive.ReynardTrident, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{} },
 new KaitAbilityDef { id="reynard.R14", nameZh="爆裂弹芯", nameEn="RingFlame", cardText="子弹命中时，对目标四邻敌人各造成1伤", kind=KaitAbilityKind.Passive, rarity=(KaitRarity)0, passive=KaitPassive.ReynardRingFlame, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{} },
 new KaitAbilityDef { id="reynard.R15", nameZh="冲击弹", nameEn="BackfirePush", cardText="子弹命中后，将存活目标向前推1格", kind=KaitAbilityKind.Passive, rarity=(KaitRarity)1, passive=KaitPassive.ReynardBackfirePush, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{} },
 new KaitAbilityDef { id="reynard.R16", nameZh="穿透弹", nameEn="Interwoven", cardText="方向射击的子弹贯穿直线上的敌人", kind=KaitAbilityKind.Passive, rarity=(KaitRarity)2, passive=KaitPassive.ReynardInterwoven, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{} },
 new KaitAbilityDef { id="reynard.R17", nameZh="静心射击", nameEn="StillBackfire", cardText="等待时，沿上次移动方向发射一发子弹", kind=KaitAbilityKind.Passive, rarity=(KaitRarity)0, passive=KaitPassive.ReynardStillBackfire, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{} },
 new KaitAbilityDef { id="reynard.R18", nameZh="奥术回想", nameEn="ArcaneRecall", cardText="镜狐四邻真实合并时，恢复镜狐法术位", kind=KaitAbilityKind.Passive, rarity=(KaitRarity)1, passive=KaitPassive.ReynardArcaneRecall, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{} },
 new KaitAbilityDef { id="reynard.R19", nameZh="奥术吞并", nameEn="ArcaneDevour", cardText="镜狐合并后，连续吞并最近的同值数字", kind=KaitAbilityKind.Passive, rarity=(KaitRarity)2, passive=KaitPassive.ReynardArcaneDevour, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{} },
 new KaitAbilityDef { id="reynard.R20", nameZh="超魔·重奏", nameEn="Reprise", cardText="即时攻击法术命中后，追加一次等量伤害", kind=KaitAbilityKind.Passive, rarity=(KaitRarity)2, passive=KaitPassive.ReynardReprise, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{} },
 new KaitAbilityDef { id="reynard.R21", nameZh="超魔·扩散", nameEn="Diffusion", cardText="法术直接伤害扩散至四邻其他敌人，各1伤", kind=KaitAbilityKind.Passive, rarity=(KaitRarity)2, passive=KaitPassive.ReynardDiffusion, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{} },
 new KaitAbilityDef { id="reynard.R22", nameZh="远距法术", nameEn="DistantSpell", cardText="主动法术的目标选取距离增加2格", kind=KaitAbilityKind.Passive, rarity=(KaitRarity)1, passive=KaitPassive.ReynardDistantSpell, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{} },
 new KaitAbilityDef { id="reynard.R23", nameZh="镜衣结界", nameEn="MirrorWard", cardText="施法后，抵挡随后敌方阶段的首次伤害", kind=KaitAbilityKind.Passive, rarity=(KaitRarity)1, passive=KaitPassive.ReynardMirrorWard, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{} },
 new KaitAbilityDef { id="reynard.R24", nameZh="法术协鸣", nameEn="Concord", cardText="即时法术后，追加一次当前持续法术的自动攻击", kind=KaitAbilityKind.Passive, rarity=(KaitRarity)1, passive=KaitPassive.ReynardConcord, allowedCharacters=new[]{"Reynard"}, effectTags=new string[]{} },
 };
 public static List<KaitAbilityDef> Pool()=>new List<KaitAbilityDef>(Cards);
 public static KaitAbilityDef Get(KaitSkill s)=>Cards.Find(d=>d.kind==KaitAbilityKind.Active&&d.skill==s);
 public static KaitAbilityDef Get(KaitPassive p)=>Cards.Find(d=>d.kind==KaitAbilityKind.Passive&&d.passive==p);
 public static KaitAbilityDef Get(string id)=>Cards.Find(d=>d.id==id);
 public static bool Has(KaitAbilityDef d,string tag)=>d!=null&&d.effectTags!=null&&Array.IndexOf(d.effectTags,tag)>=0;
 public static int Level(int value){int n=0;while(value>1){value>>=1;n++;}return n;}
 public static string Roman(int n)=>n>0&&n<11?new[]{"","I","II","III","IV","V","VI","VII","VIII","IX","X"}[n]:n.ToString();
 public static string TargetHint(KaitSkill s)=>Has(Get(s),"ReynardTargeted")?"点选目标格":s==KaitSkill.ReynardLightningBolt?"点选四邻方向":"点选自身施放";
}
