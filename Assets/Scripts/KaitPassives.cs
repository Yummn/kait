using System;
using System.Collections.Generic;
using UnityEngine;

public enum KaitPassive
{
    None,
    BirdEye,
    OldNewsArchive,
    Simplify,
    BloodBookmark,
    MomentumResonance,
    Devil,
    CheshireCat,
    Squeeze,
    Follower,
    BladeCovenant,
    Trend,
    SweepTail
}

public enum KaitPassiveCategory
{
    Information,
    ThreatAutomation,
    SpawnCompression,
    SpawnPlacement,
    CrossBoard,
    SkillEngine,
    FriendlyFire,
    SpawnPush,
    SpawnRule,
    ChainSkill,
    ThreatRule
}

[Serializable]
public sealed class KaitPassiveTrigger
{
    public KaitPassive passive;
    public Vector2Int threatCell = new Vector2Int(-1, -1);
    public Vector2Int battleCell = new Vector2Int(-1, -1);
    public string message;
}

public static class KaitPassiveCatalog
{
    public static readonly KaitPassive[] All =
    {
        KaitPassive.BirdEye,
        KaitPassive.OldNewsArchive,
        KaitPassive.Simplify,
        KaitPassive.BloodBookmark,
        KaitPassive.MomentumResonance,
        KaitPassive.Devil,
        KaitPassive.CheshireCat,
        KaitPassive.Squeeze,
        KaitPassive.Follower,
        KaitPassive.BladeCovenant,
        KaitPassive.Trend,
        KaitPassive.SweepTail
    };

    public static string Name(KaitPassive passive)
    {
        switch (passive)
        {
            case KaitPassive.BirdEye: return "鸦后之眼";
            case KaitPassive.OldNewsArchive: return "旧闻归档";
            case KaitPassive.Simplify: return "化零为整";
            case KaitPassive.BloodBookmark: return "血色书签";
            case KaitPassive.MomentumResonance: return "念动力共振";
            case KaitPassive.Devil: return "魔手";
            case KaitPassive.CheshireCat: return "猫戏老鼠";
            case KaitPassive.Squeeze: return "挤压";
            case KaitPassive.Follower: return "尾随者";
            case KaitPassive.BladeCovenant: return "刃之魔契";
            case KaitPassive.Trend: return "定势";
            case KaitPassive.SweepTail: return "鸦羽扫尾";
            default: return "未知被动";
        }
    }

    public static string ShortName(KaitPassive passive)
    {
        switch (passive)
        {
            case KaitPassive.BirdEye: return "鸦眼";
            case KaitPassive.OldNewsArchive: return "归档";
            case KaitPassive.Simplify: return "化整";
            case KaitPassive.BloodBookmark: return "书签";
            case KaitPassive.MomentumResonance: return "共振";
            case KaitPassive.Devil: return "魔手";
            case KaitPassive.CheshireCat: return "戏鼠";
            case KaitPassive.Squeeze: return "挤压";
            case KaitPassive.Follower: return "尾随";
            case KaitPassive.BladeCovenant: return "魔契";
            case KaitPassive.Trend: return "定势";
            case KaitPassive.SweepTail: return "扫尾";
            default: return "空";
        }
    }

    public static string Description(KaitPassive passive)
    {
        switch (passive)
        {
            case KaitPassive.BirdEye: return "预览下一枚 2 的候选出生位置。";
            case KaitPassive.OldNewsArchive: return "第 5 枚 2 出现时，最早的两枚 2 自动合并。";
            case KaitPassive.Simplify: return "同回合两个同级出怪事件合成一个高一级事件。";
            case KaitPassive.BloodBookmark: return "连杀结束留下书签，下一次被阻挡的出怪改在书签处。";
            case KaitPassive.MomentumResonance: return "每回合第一次推动敌人时，对应威胁数字也前进一格。";
            case KaitPassive.Devil: return "使用主动技能后，随机令另一个冷却中的主动技能 CD -1。";
            case KaitPassive.CheshireCat: return "敌军友伤不能击杀敌人，最低保留 1 点生命。";
            case KaitPassive.Squeeze: return "出怪格被敌人占用时，先尝试沿本回合方向将其推开。";
            case KaitPassive.Follower: return "同回合后续敌人优先在第一名新敌人旁边生成。";
            case KaitPassive.BladeCovenant: return "一条连杀中每击杀 3 名敌人，所有主动技能 CD -1。";
            case KaitPassive.Trend: return "回合末的新 2 优先从本回合移动方向的反侧生成。";
            case KaitPassive.SweepTail: return "主动撞边界结束连杀时，清除对应位置的一枚 2。";
            default: return string.Empty;
        }
    }

    public static KaitPassiveCategory Category(KaitPassive passive)
    {
        switch (passive)
        {
            case KaitPassive.BirdEye: return KaitPassiveCategory.Information;
            case KaitPassive.OldNewsArchive: return KaitPassiveCategory.ThreatAutomation;
            case KaitPassive.Simplify: return KaitPassiveCategory.SpawnCompression;
            case KaitPassive.BloodBookmark: return KaitPassiveCategory.SpawnPlacement;
            case KaitPassive.MomentumResonance: return KaitPassiveCategory.CrossBoard;
            case KaitPassive.Devil: return KaitPassiveCategory.SkillEngine;
            case KaitPassive.CheshireCat: return KaitPassiveCategory.FriendlyFire;
            case KaitPassive.Squeeze: return KaitPassiveCategory.SpawnPush;
            case KaitPassive.Follower: return KaitPassiveCategory.SpawnRule;
            case KaitPassive.BladeCovenant: return KaitPassiveCategory.ChainSkill;
            case KaitPassive.Trend: return KaitPassiveCategory.ThreatRule;
            case KaitPassive.SweepTail: return KaitPassiveCategory.CrossBoard;
            default: return KaitPassiveCategory.Information;
        }
    }
}
