using System.Reflection;
using NUnit.Framework;

public sealed class YummnHealthRulesTests
{
    [Test] public void CurrentRulesDefaultToSixAndCanSelectThree()
    {
        Assert.AreEqual(6,YummnRulesSnapshot.Current().MaxHp);
        Assert.AreEqual(3,YummnRulesSnapshot.Current(maxHp:3).MaxHp);
        Assert.AreNotEqual(YummnRulesSnapshot.Current().ScoreKey,YummnRulesSnapshot.Current(maxHp:3).ScoreKey);
    }

    [Test] public void NewYummnRunUsesRuleHealthWhileKaitKeepsThree()
    {
        var run=new KaitRun();
        run.SelectCharacter(KaitCharacter.Yummn,614,YummnRulesSnapshot.Current());
        Assert.AreEqual(6,run.KateMaxHp);Assert.AreEqual(6,run.kateHp);
        run.SelectCharacter(KaitCharacter.Yummn,614,YummnRulesSnapshot.Current(maxHp:3));
        Assert.AreEqual(3,run.KateMaxHp);Assert.AreEqual(3,run.kateHp);
        run.SelectCharacter(KaitCharacter.Kait,614);
        Assert.AreEqual(3,run.KateMaxHp);Assert.AreEqual(3,run.kateHp);
    }

    [Test] public void MissingHealthFieldKeepsHistoricalThreeHpReplayRule()
    {
        var rules=YummnRulesSnapshot.Current();
        typeof(YummnRulesSnapshot).GetField("maxHp090",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(rules,0);
        Assert.IsTrue(rules.Valid);Assert.AreEqual(3,rules.MaxHp);
    }
}
