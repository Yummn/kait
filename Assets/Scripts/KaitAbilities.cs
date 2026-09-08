using System;
using System.Collections.Generic;
using UnityEngine;

public enum KaitAbilityKind { Active, Passive }
public enum KaitRarity { Common, Uncommon, Rare }
public enum KaitAbilityOrigin { Dnd5e, KaitOriginal, ProjectOriginal }

[Serializable] public sealed class KaitAbilityDef
{
    public string id, nameZh, nameEn, cardText;
    public KaitAbilityKind kind;
    public KaitRarity rarity;
    public KaitAbilityOrigin origin;
    public KaitSkill skill;
    public KaitPassive passive;
    public int cooldown;
    public bool copyable = true, experimental;
    public string[] tags;
}
[Serializable] public sealed class KaitRewardPack
{
    public int id, sourceTurn;
    public Vector2Int sourceMergeCell;
    public bool rerolled;
    public readonly List<KaitAbilityDef> choices = new List<KaitAbilityDef>();
}
public static class KaitAbilityCatalog
{
    public static readonly List<KaitAbilityDef> All = new List<KaitAbilityDef>
    {
        A(KaitSkill.SwiftBoots, "大步奔行之靴", "Boots of Striding and Springing", 0, 2, "本回合速度+1。"),
        A(KaitSkill.HexCurse, "咒剑诅咒", "Hexblade's Curse", 0, 3, "目标下一次受到的伤害+1。"),
        A(KaitSkill.IceTomb, "怪物定身术", "Hold Monster", 0, 3, "一个敌人跳过下一次行动。"),
        A(KaitSkill.DispelMagic, "解除魔法", "Dispel Magic", 0, 4, "移除一个裂隙。"),
        A(KaitSkill.DreadSlash, "雷鸣波", "Thunderwave", 1, 5, "下一次移动改为将所有敌人沿该方向推到底。Kait不移动，威胁盘正常移动。"),
        A(KaitSkill.LesserPhantom, "次级幻影", "Misty Visions", 1, 4, "选择一个敌人；下一敌人阶段所有敌人以它为目标。"),
        A(KaitSkill.Command, "命令术", "Command", 1, 3, "将一个已锁定攻击顺时针旋转90°。"),
        A(KaitSkill.MistyStep, "迷踪步", "Misty Step", 1, 3, "移动到一个相邻空格，不移动威胁盘。"),
        A(KaitSkill.GraspHadar, "哈达之握", "Grasp of Hadar", 1, 4, "将同一行或列的一个敌人拉到Kait前一格。"),
        A(KaitSkill.CatAgility, "猫之迅捷", "Feline Agility", 2, 5, "本回合速度×2；固定加成先计算。"),
        A(KaitSkill.EldritchSmite, "魔能斩", "Eldritch Smite", 2, 4, "下一次命中未击杀时，将目标沿攻击方向推到尽头。"),
        A(KaitSkill.RelentlessHex, "不息咒缚", "Relentless Hex", 2, 4, "传送到被诅咒敌人的相邻空格。"),
        A(KaitSkill.LevistusTomb, "列维斯图斯之墓", "Tomb of Levistus", 2, 5, "本回合Kait不移动且不会受伤。"),
        A(KaitSkill.DimensionDoor, "任意门", "Dimension Door", 2, 4, "下一道裂隙在中心对称格生成。"),
        P(KaitPassive.BirdEye, "警戒武器", "Weapon of Warning", 0, "显示下一枚2的出生格。"),
        P(KaitPassive.CheshireCat, "非致命击倒", "Knocking a Creature Out", 0, "敌军友伤最多将敌人降至1HP。"),
        P(KaitPassive.StaggeringSmite, "震慑斩", "Staggering Smite", 0, "敌人撞上柱子后跳过下一次行动。"),
        P(KaitPassive.ArcaneLock, "秘法锁", "Arcane Lock", 0, "被占据的裂隙第一次结算时延迟1回合。"),
        P(KaitPassive.Enfeeblement, "衰弱射线", "Ray of Enfeeblement", 0, "受到友伤的敌人跳过下一次行动。"),
        P(KaitPassive.Devil, "守契者权杖", "Rod of the Pact Keeper", 1, "每回合首次使用主动后，随机另一主动冷却-1。"),
        P(KaitPassive.BladeCovenant, "刃之契约", "Pact of the Blade", 1, "每条连杀首次达到3杀时，冷却最长的主动冷却-1。"),
        P(KaitPassive.HexArmor, "咒术护甲", "Armor of Hexes", 1, "被诅咒敌人的下一次攻击失效。"),
        P(KaitPassive.MasterHex, "咒术大师", "Master of Hexes", 1, "诅咒目标死亡后，本条连杀下一次命中的敌人成为诅咒目标。"),
        P(KaitPassive.MaddeningHex, "疯狂咒缚", "Maddening Hex", 1, "每回合首次伤害诅咒目标时，相邻敌人受到1伤。"),
        P(KaitPassive.LuckBlade, "幸运剑", "Luck Blade", 1, "每组三选一可以重抽一次。"),
        P(KaitPassive.Lifedrinker, "饮命者", "Lifedrinker", 1, "每条连杀首次击杀诅咒敌人时，恢复1HP。"),
        P(KaitPassive.BloodBookmark, "守卫刻文", "Glyph of Warding", 1, "连杀结束格留下刻文；下一次被阻挡裂隙改在此处生成。"),
        P(KaitPassive.MomentumResonance, "念动力", "Telekinesis", 2, "每回合第一次推动敌人时，同步移动对应威胁数字1格；不能合并。"),
        P(KaitPassive.Simulacrum, "拟像术", "Simulacrum", 2, "获得时选择并复制一张其他被动；占用一个被动槽。"),
        P(KaitPassive.AccursedSpecter, "诅咒幽魂", "Accursed Specter", 2, "每条连杀第一次击杀留下幽魂，抵消下一次敌方攻击。"),
        P(KaitPassive.RepellingBlast, "斥力魔爆", "Repelling Blast", 2, "推动可额外传递1个敌人，最多传递一次。"),
        P(KaitPassive.Trend, "鸦后预兆", "Raven Omen", 2, "新2只在威胁盘实际移动方向反侧半区生成；无格则回退全盘。"),
        P(KaitPassive.DisplacementCloak, "移位斗篷", "Cloak of Displacement", 2, "每个敌人阶段第一次远程攻击Kait时失效。"),
        P(KaitPassive.OldNewsArchive, "远古奥秘之书", "Book of Ancient Secrets", 2, "至少有5个2时，回合末最早的两个2合并；每回合一次。"),
        P(KaitPassive.Passwall, "穿墙术", "Passwall", 2, "威胁数字可穿过柱子，但不能停在柱子上。"),
        P(KaitPassive.BagHolding, "异次元袋", "Bag of Holding", 2, "每次合并后，收纳后方一个对应同值数字。"),
        P(KaitPassive.ReverseGravity, "重力反转", "Reverse Gravity", 2, "威胁盘始终沿主棋盘输入的相反方向移动。"),
        P(KaitPassive.Simplify, "化零为整", "Consolidation", 2, "同回合前两个同级裂隙合为高一级，保留先出现位置；每回合一次。"),
        P(KaitPassive.WildMagic, "狂野魔法涌动", "Wild Magic Surge", 2, "裂隙随机偏移1格；新生敌人跳过首次行动。（实验，未启用）"),
    };
    private static KaitAbilityDef A(KaitSkill skill,string zh,string en,int rarity,int cd,string text) =>
        new KaitAbilityDef { id="active."+skill, nameZh=zh,nameEn=en,rarity=(KaitRarity)rarity,cardText=text,
            cooldown=cd,skill=skill,kind=KaitAbilityKind.Active,
            origin=skill==KaitSkill.CatAgility||skill==KaitSkill.LesserPhantom?KaitAbilityOrigin.KaitOriginal:KaitAbilityOrigin.Dnd5e,
            tags=new[]{ "Active", rarity==2?"BuildCore":"Component" }};
    private static KaitAbilityDef P(KaitPassive passive,string zh,string en,int rarity,string text) =>
        new KaitAbilityDef { id="passive."+passive,nameZh=zh,nameEn=en,rarity=(KaitRarity)rarity,cardText=text,
            passive=passive,kind=KaitAbilityKind.Passive,copyable=passive!=KaitPassive.Simulacrum,
            experimental=passive==KaitPassive.WildMagic,
            origin=passive==KaitPassive.Simplify||passive==KaitPassive.Trend?KaitAbilityOrigin.ProjectOriginal:KaitAbilityOrigin.Dnd5e,
            tags=new[]{ "Passive",rarity==2?"BuildCore":"Component" }};
    public static KaitAbilityDef Get(KaitSkill skill) => All.Find(d=>d.kind==KaitAbilityKind.Active && d.skill==skill);
    public static KaitAbilityDef Get(KaitPassive passive) => All.Find(d=>d.kind==KaitAbilityKind.Passive && d.passive==passive);
    public static string RarityName(KaitRarity rarity) => rarity==KaitRarity.Common?"普通":rarity==KaitRarity.Uncommon?"罕见":"稀有";
    public static Color RarityColor(KaitRarity rarity) => rarity==KaitRarity.Common?new Color(.82f,.87f,.92f):rarity==KaitRarity.Uncommon?new Color(.25f,.62f,1f):new Color(1f,.76f,.27f);
}

