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
    public string artSourceId;
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
    public const string Version="kait-round2-default24-20260919";
    public const string RulesVersion="Kait.0.9.21-round2-default24";
    public static readonly List<KaitAbilityDef> All = new List<KaitAbilityDef>
    {
        P(KaitPassive.SwiftBoots, "大步奔行之靴", "Boots of Striding and Springing", 0, "每条连斩的第一刀额外造成1点斩击伤害。"),
        A(KaitSkill.HexCurse, "咒剑诅咒", "Hexblade's Curse", 0, 2, "诅咒一个敌人。", "WarMageSpell"),
        A(KaitSkill.IceTomb, "怪物定身术", "Hold Monster", 0, 3, "使一个敌人跳过下一次行动。", "WarMageSpell"),
        A(KaitSkill.DispelMagic, "解除魔法", "Dispel Magic", 0, 3, "移除一个指定裂隙。"),
        A(KaitSkill.DreadSlash, "雷鸣波", "Thunderwave", 1, 5, "下一次移动改为原地将所有敌人推至尽头，副盘照常移动。"),
        A(KaitSkill.LesserPhantom, "次级幻影", "Misty Visions", 1, 4, "下一敌方回合，所有敌人改为锁定指定敌人。"),
        A(KaitSkill.Command, "命令术", "Command", 1, 3, "将一个已锁定攻击顺时针旋转90°。"),
        A(KaitSkill.MistyStep, "迷踪步", "Misty Step", 0, 2, "传送至相邻空格，副盘不移动。"),
        A(KaitSkill.GraspHadar, "哈达之握", "Grasp of Hadar", 1, 4, "将同行或同列的敌人拉至身前。"),
        P(KaitPassive.CatAgility, "猫之迅捷", "Feline Agility", 2, "锁定伤害翻倍；连斩中不能反向掉头。"),
        P(KaitPassive.EldritchSmite, "魔能斩", "Eldritch Smite", 1, "斩击命中后，将存活目标推至尽头。"),
        A(KaitSkill.RelentlessHex, "不息咒缚", "Relentless Hex", 1, 3, "传送至一个诅咒敌人的四邻空格。"),
        A(KaitSkill.LevistusTomb, "列维斯图斯之墓", "Tomb of Levistus", 2, 5, "本回合停止移动，并免疫伤害。"),
        A(KaitSkill.DimensionDoor, "任意门", "Dimension Door", 2, 4, "使下一道裂隙生成在中心对称格。"),
        P(KaitPassive.BirdEye, "警戒武器", "Weapon of Warning", 0, "显示下一枚2的生成位置。"),
        P(KaitPassive.CheshireCat, "非致命击倒", "Knocking a Creature Out", 0, "敌人受到友方伤害后，至少保留1点生命。"),
        P(KaitPassive.StaggeringSmite, "震慑斩", "Staggering Smite", 0, "斩击命中存活敌人后，使其跳过下一次行动。"),
        P(KaitPassive.ArcaneLock, "秘法锁", "Arcane Lock", 0, "被占据的裂隙，首次生成延迟1回合。"),
        P(KaitPassive.Enfeeblement, "衰弱射线", "Ray of Enfeeblement", 0, "敌人受到友方伤害时，跳过下一次行动。"),
        P(KaitPassive.Devil, "守契者权杖", "Rod of the Pact Keeper", 1, "每次真实合并，使剩余冷却最长的一张技能冷却-1。"),
        P(KaitPassive.BladeCovenant, "刃之契约", "Pact of the Blade", 1, "每次你击杀敌人，所有带冷却的技能冷却-1。"),
        P(KaitPassive.HexArmor, "咒术护甲", "Armor of Hexes", 1, "受到诅咒敌人攻击时，消耗其诅咒并抵挡伤害。"),
        P(KaitPassive.MasterHex, "咒术大师", "Master of Hexes", 1, "诅咒敌人死亡时，诅咒其四邻敌人。"),
        P(KaitPassive.MaddeningHex, "疯狂咒缚", "Maddening Hex", 1, "诅咒被伤害消耗时，对其四邻敌人各造成1伤。"),
        P(KaitPassive.LuckBlade, "幸运剑", "Luck Blade", 1, "每组选牌可重抽一次。"),
        P(KaitPassive.Lifedrinker, "饮命者", "Lifedrinker", 1, "每次连杀首次击杀诅咒敌人时，恢复1点生命。"),
        P(KaitPassive.BloodBookmark, "守卫刻文", "Glyph of Warding", 1, "连杀结束时留下刻文，使下一道被阻挡的裂隙转移至此。"),
        P(KaitPassive.MomentumResonance, "念动力", "Telekinesis", 1, "每次推拉敌人，其原格对应数字同向移动1格，可以合并。"),
        P(KaitPassive.Simulacrum, "拟像术", "Simulacrum", 2, "获得时，复制一张其他被动牌。"),
        P(KaitPassive.AccursedSpecter, "诅咒幽魂", "Accursed Specter", 1, "每条连斩首次击杀后，获得抵挡一次攻击的幽魂。"),
        P(KaitPassive.RepellingBlast, "斥力魔爆", "Repelling Blast", 2, "推动可以继续传递给挡路的敌人。"),
        P(KaitPassive.Trend, "鸦后预兆", "Raven Omen", 2, "新数字2优先在移动方向的反侧半区生成。"),
        P(KaitPassive.DisplacementCloak, "移位斗篷", "Cloak of Displacement", 1, "每个敌方阶段，抵挡首次对你的远程攻击。"),
        P(KaitPassive.OldNewsArchive, "远古奥秘之书", "Book of Ancient Secrets", 2, "至少有5枚2时，回合结束后合成最旧的两枚。"),
        P(KaitPassive.Passwall, "穿墙术", "Passwall", 1, "副盘数字可以穿过柱子，但不能停在柱格。"),
        P(KaitPassive.BagHolding, "异次元袋", "Bag of Holding", 1, "合并后，移除后方一个剩余的原料值数字。"),
        P(KaitPassive.ReverseGravity, "重力反转", "Reverse Gravity", 1, "副盘沿玩家输入的反方向移动。"),
        P(KaitPassive.Simplify, "化零为整", "Consolidation", 2, "每回合前两道同级裂隙，合为高一级裂隙。"),
        P(KaitPassive.WildMagic, "狂野魔法涌动", "Wild Magic Surge", 2, "裂隙随机偏移1格；新生敌人跳过首次行动。"),
        P(KaitPassive.ResidualSlash, "余势斩", "Residual Slash", 1, "斩击击杀后，剩余伤害继续命中前方敌人。", "yummn.N01"),
        P(KaitPassive.HexBlade, "咒刃", "Hex Blade", 0, "斩击时，诅咒目标四邻的其他敌人。", "yummn.R22"),
        P(KaitPassive.PiercingArrow, "穿透箭符", "Piercing Arrow Sigil", 0, "敌方箭矢穿过敌人，但仍被你和地形阻挡。", "yummn.M04"),
        P(KaitPassive.ProvokingWhispers, "煽乱低语", "Provoking Whispers", 2, "受到友伤的敌人，立即反击攻击者。", "yummn.S06"),
        PId("active.EldritchBlast",KaitPassive.EldritchBlast, "魔能爆", "Eldritch Blast", 0, 2, "冷却2。斩击后，向前方最近敌人发射1伤魔能爆并推开1格。", "RepeatableMagic", "yummn.R30"),
        P(KaitPassive.WarMage, "战争法师", "War Mage", 1, "斩击命中存活目标后，自动施放一张就绪的战争法术。", "yummn.N17"),
        P(KaitPassive.TwinSigil, "双生秘印", "Twin Sigil", 2, "可复制的指向法术额外生效一次。", "yummn.N10"),
        A(KaitSkill.HungerOfHadar, "哈达之饥", "Hunger of Hadar", 1, 4, "对目标格及四邻敌人各造成1伤，并诅咒存活目标。", "WarMageSpell", "RepeatableMagic", "yummn.S02"),
    };
    private static readonly string[] DefaultIds={
        "passive.SwiftBoots","active.DreadSlash","active.LesserPhantom","active.MistyStep","active.GraspHadar",
        "passive.EldritchSmite","active.RelentlessHex","active.LevistusTomb","passive.StaggeringSmite","passive.Devil",
        "passive.BladeCovenant","passive.HexArmor","passive.MasterHex","passive.MaddeningHex","passive.Lifedrinker",
        "passive.AccursedSpecter","passive.RepellingBlast","passive.DisplacementCloak","passive.BagHolding","passive.ResidualSlash",
        "passive.HexBlade","active.EldritchBlast","passive.TwinSigil","active.HungerOfHadar"};
    private static readonly string[] ExperimentalIds={
        "passive.CatAgility","active.IceTomb","passive.CheshireCat","passive.LuckBlade","passive.Passwall",
        "passive.ReverseGravity","passive.OldNewsArchive","passive.Simplify","passive.WildMagic","passive.ProvokingWhispers","passive.PiercingArrow"};
    static KaitAbilityCatalog(){foreach(var d in All)d.experimental=Array.IndexOf(ExperimentalIds,d.id)>=0;}
    private static KaitAbilityDef A(KaitSkill skill,string zh,string en,int rarity,int cd,string text,params string[] tags) =>
        new KaitAbilityDef { id="active."+skill, nameZh=zh,nameEn=en,rarity=(KaitRarity)rarity,cardText=text,
            cooldown=cd,skill=skill,kind=KaitAbilityKind.Active,
            origin=skill==KaitSkill.CatAgility||skill==KaitSkill.LesserPhantom?KaitAbilityOrigin.KaitOriginal:KaitAbilityOrigin.Dnd5e,
            effectTags=FilterEffectTags(tags),artSourceId=FindArtSource(tags),tags=new[]{ "Active", rarity==2?"BuildCore":"Component" }};
    private static KaitAbilityDef P(KaitPassive passive,string zh,string en,int rarity,string text,params string[] tags) =>
        new KaitAbilityDef { id="passive."+passive,nameZh=zh,nameEn=en,rarity=(KaitRarity)rarity,cardText=text,
            passive=passive,kind=KaitAbilityKind.Passive,copyable=passive!=KaitPassive.Simulacrum,
            experimental=passive==KaitPassive.WildMagic,
            effectTags=FilterEffectTags(tags),artSourceId=FindArtSource(tags),
            origin=passive==KaitPassive.Simplify||passive==KaitPassive.Trend?KaitAbilityOrigin.ProjectOriginal:KaitAbilityOrigin.Dnd5e,
            tags=new[]{ "Passive",rarity==2?"BuildCore":"Component" }};
    private static KaitAbilityDef PId(string id,KaitPassive passive,string zh,string en,int rarity,int cooldown,string text,params string[] tags)
    {
        var d=P(passive,zh,en,rarity,text,tags);d.id=id;d.cooldown=cooldown;return d;
    }
    private static string FindArtSource(string[] tags)=>Array.Find(tags,t=>t!=null&&t.StartsWith("yummn."));
    private static string[] FilterEffectTags(string[] tags)=>Array.FindAll(tags,t=>t!=null&&!t.StartsWith("yummn."));
    private static List<KaitAbilityDef> SelectIds(string[] ids){var list=new List<KaitAbilityDef>();foreach(var id in ids){var d=All.Find(x=>x.id==id);if(d!=null)list.Add(d);}return list;}
    public static List<KaitAbilityDef> DefaultPool()=>SelectIds(DefaultIds);
    public static List<KaitAbilityDef> ExperimentalPool()=>SelectIds(ExperimentalIds);
    public static List<KaitAbilityDef> LegacyPool()=>All.FindAll(d=>Array.IndexOf(DefaultIds,d.id)<0&&Array.IndexOf(ExperimentalIds,d.id)<0);
    public static List<KaitAbilityDef> CandidatePool()=>new List<KaitAbilityDef>(All);
    public static List<KaitAbilityDef> RecommendedFinalPool()=>DefaultPool();
    public static KaitAbilityDef Get(KaitSkill skill) => All.Find(d=>(d.kind==KaitAbilityKind.Active && d.skill==skill)||d.id=="active."+skill) ?? YummnCatalog.Cards.Find(d=>d.kind==KaitAbilityKind.Active && d.skill==skill);
    public static KaitAbilityDef Get(KaitPassive passive) => All.Find(d=>d.kind==KaitAbilityKind.Passive && d.passive==passive) ?? YummnCatalog.Cards.Find(d=>d.kind==KaitAbilityKind.Passive && d.passive==passive);
    public static string RarityName(KaitRarity rarity) => rarity==KaitRarity.Common?"普通":rarity==KaitRarity.Uncommon?"罕见":"稀有";
    public static Color RarityColor(KaitRarity rarity) => rarity==KaitRarity.Common?new Color(.82f,.87f,.92f):rarity==KaitRarity.Uncommon?new Color(.25f,.62f,1f):new Color(1f,.76f,.27f);
}
