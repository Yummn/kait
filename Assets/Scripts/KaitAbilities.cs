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
    public int kiExtraCost;
    public string traditionTag, actionOverride, sigil;
    public string[] allowedCharacters, prerequisiteIds, effectTags;
    public bool copyable = true, experimental;
    public string[] tags;
}
[Serializable] public sealed class KaitRewardPack
{
    public int id, sourceTurn, generationSeed;
    public KaitCharacter characterId;
    public string rulesProfileId, cardPoolVersion;
    public Vector2Int sourceMergeCell;
    public bool rerolled;
    public readonly List<KaitAbilityDef> choices = new List<KaitAbilityDef>();
}
public static class KaitAbilityCatalog
{
    public static readonly List<KaitAbilityDef> All = new List<KaitAbilityDef>
    {
        A(KaitSkill.SwiftBoots, "大步奔行之靴", "Boots of Striding and Springing", 0, 2, "本回合速度+1。"),
        A(KaitSkill.HexCurse, "咒剑诅咒", "Hexblade's Curse", 0, 3, "使目标下一次受到的伤害+1。"),
        A(KaitSkill.IceTomb, "怪物定身术", "Hold Monster", 0, 3, "使一个敌人跳过下一次行动。"),
        A(KaitSkill.DispelMagic, "解除魔法", "Dispel Magic", 0, 4, "移除一个裂隙。"),
        A(KaitSkill.DreadSlash, "雷鸣波", "Thunderwave", 1, 5, "下一次移动改为原地将所有敌人推至尽头，副盘照常移动。"),
        A(KaitSkill.LesserPhantom, "次级幻影", "Misty Visions", 1, 4, "下一敌方回合，所有敌人改为锁定指定敌人。"),
        A(KaitSkill.Command, "命令术", "Command", 1, 3, "将一个已锁定攻击顺时针旋转90°。"),
        A(KaitSkill.MistyStep, "迷踪步", "Misty Step", 1, 3, "移动到相邻空格，副盘不移动。"),
        A(KaitSkill.GraspHadar, "哈达之握", "Grasp of Hadar", 1, 4, "将同行或同列的敌人拉至身前。"),
        A(KaitSkill.CatAgility, "猫之迅捷", "Feline Agility", 2, 5, "本回合速度翻倍。"),
        A(KaitSkill.EldritchSmite, "魔能斩", "Eldritch Smite", 2, 4, "下一次命中后，将存活敌人推至尽头。"),
        A(KaitSkill.RelentlessHex, "不息咒缚", "Relentless Hex", 2, 4, "传送到诅咒敌人的相邻空格。"),
        A(KaitSkill.LevistusTomb, "列维斯图斯之墓", "Tomb of Levistus", 2, 5, "本回合停止移动，并免疫伤害。"),
        A(KaitSkill.DimensionDoor, "任意门", "Dimension Door", 2, 4, "使下一道裂隙生成在中心对称格。"),
        P(KaitPassive.BirdEye, "警戒武器", "Weapon of Warning", 0, "显示下一枚2的生成位置。"),
        P(KaitPassive.CheshireCat, "非致命击倒", "Knocking a Creature Out", 0, "敌人受到友方伤害后，至少保留1点生命。"),
        P(KaitPassive.StaggeringSmite, "震慑斩", "Staggering Smite", 0, "敌人撞上柱子时，跳过下一次行动。"),
        P(KaitPassive.ArcaneLock, "秘法锁", "Arcane Lock", 0, "被占据的裂隙，首次生成延迟1回合。"),
        P(KaitPassive.Enfeeblement, "衰弱射线", "Ray of Enfeeblement", 0, "敌人受到友方伤害时，跳过下一次行动。"),
        P(KaitPassive.Devil, "守契者权杖", "Rod of the Pact Keeper", 1, "每回合首次使用主动牌后，随机另一张主动牌冷却-1。"),
        P(KaitPassive.BladeCovenant, "刃之契约", "Pact of the Blade", 1, "连杀达到3次时，冷却最长的主动牌冷却-1。"),
        P(KaitPassive.HexArmor, "咒术护甲", "Armor of Hexes", 1, "抵消诅咒敌人的下一次攻击。"),
        P(KaitPassive.MasterHex, "咒术大师", "Master of Hexes", 1, "诅咒敌人死亡后，将诅咒转移给本次连杀的下一个命中目标。"),
        P(KaitPassive.MaddeningHex, "疯狂咒缚", "Maddening Hex", 1, "每回合首次伤害诅咒目标时，对其相邻敌人造成1点伤害。"),
        P(KaitPassive.LuckBlade, "幸运剑", "Luck Blade", 1, "每组选牌可重抽一次。"),
        P(KaitPassive.Lifedrinker, "饮命者", "Lifedrinker", 1, "每次连杀首次击杀诅咒敌人时，恢复1点生命。"),
        P(KaitPassive.BloodBookmark, "守卫刻文", "Glyph of Warding", 1, "连杀结束时留下刻文，使下一道被阻挡的裂隙转移至此。"),
        P(KaitPassive.MomentumResonance, "念动力", "Telekinesis", 2, "每回合首次推动敌人时，使对应副盘数字同向移动1格。"),
        P(KaitPassive.Simulacrum, "拟像术", "Simulacrum", 2, "获得时，复制一张其他被动牌。"),
        P(KaitPassive.AccursedSpecter, "诅咒幽魂", "Accursed Specter", 2, "每次连杀首次击杀时，生成抵挡一次敌方攻击的幽魂。"),
        P(KaitPassive.RepellingBlast, "斥力魔爆", "Repelling Blast", 2, "推动可以传递给后方一个敌人。"),
        P(KaitPassive.Trend, "鸦后预兆", "Raven Omen", 2, "新数字2优先在移动方向的反侧半区生成。"),
        P(KaitPassive.DisplacementCloak, "移位斗篷", "Cloak of Displacement", 2, "每个敌方回合，抵挡首次对你的远程攻击。"),
        P(KaitPassive.OldNewsArchive, "远古奥秘之书", "Book of Ancient Secrets", 2, "至少有5枚2时，回合结束后合成最旧的两枚。"),
        P(KaitPassive.Passwall, "穿墙术", "Passwall", 2, "副盘数字可以穿过柱子。"),
        P(KaitPassive.BagHolding, "异次元袋", "Bag of Holding", 2, "每次合成后，移除后方一个同原料值的数字。"),
        P(KaitPassive.ReverseGravity, "重力反转", "Reverse Gravity", 2, "副盘改为沿输入的反方向移动。"),
        P(KaitPassive.Simplify, "化零为整", "Consolidation", 2, "每回合前两道同级裂隙，合为高一级裂隙。"),
        P(KaitPassive.WildMagic, "狂野魔法涌动", "Wild Magic Surge", 2, "裂隙随机偏移1格；新生敌人跳过首次行动。"),
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
    public static KaitAbilityDef Get(KaitSkill skill) => All.Find(d=>d.kind==KaitAbilityKind.Active && d.skill==skill) ?? YummnCatalog.Cards.Find(d=>d.kind==KaitAbilityKind.Active && d.skill==skill);
    public static KaitAbilityDef Get(KaitPassive passive) => All.Find(d=>d.kind==KaitAbilityKind.Passive && d.passive==passive) ?? YummnCatalog.Cards.Find(d=>d.kind==KaitAbilityKind.Passive && d.passive==passive);
    public static string RarityName(KaitRarity rarity) => rarity==KaitRarity.Common?"普通":rarity==KaitRarity.Uncommon?"罕见":"稀有";
    public static Color RarityColor(KaitRarity rarity) => rarity==KaitRarity.Common?new Color(.82f,.87f,.92f):rarity==KaitRarity.Uncommon?new Color(.25f,.62f,1f):new Color(1f,.76f,.27f);
}
