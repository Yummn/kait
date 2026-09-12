using NUnit.Framework;
using UnityEngine;

public class YummnRewardThresholdTests
{
    [TestCase(16,16,1)] [TestCase(16,32,0)]
    [TestCase(32,16,0)] [TestCase(32,32,1)]
    public void OnlyChosenMergeCreatesReward(int threshold,int merged,int count)
    {
        var run=new KaitRun();
        run.SelectCharacter(KaitCharacter.Yummn,412,YummnRulesSnapshot.Current(rewardValue:threshold));
        typeof(KaitRun).GetMethod("HandleMilestoneMerge",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)
            .Invoke(run,new object[]{new KaitMergeEvent{sourceValue=merged/2,resultValue=merged}});
        Assert.AreEqual(count,run.rewardQueue.Count);
    }
    [Test] public void DefaultAndSavedRulesAndTutorialAgree()
    {
        var rules=YummnRulesSnapshot.Current();Assert.AreEqual(16,rules.RewardMergeValue);
        foreach(int threshold in new[]{16,32})
        {
            rules=JsonUtility.FromJson<YummnRulesSnapshot>(JsonUtility.ToJson(YummnRulesSnapshot.Current(rewardValue:threshold)));
            Assert.IsTrue(rules.Valid);Assert.AreEqual(threshold,rules.RewardMergeValue);
            StringAssert.Contains("合成"+threshold+"获得三选一",YummnComicTutorial.Appendix(rules));
        }
        Assert.AreNotEqual(YummnRulesSnapshot.Current().ScoreKey,YummnRulesSnapshot.Current(rewardValue:32).ScoreKey);
    }
    [Test] public void MissingSaveFieldAndKaitKeep32()
    {
        var json=JsonUtility.ToJson(YummnRulesSnapshot.Current()).Replace("\"rewardMergeValue\":16","\"rewardMergeValue\":0");
        Assert.AreEqual(32,JsonUtility.FromJson<YummnRulesSnapshot>(json).RewardMergeValue);
        var run=new KaitRun();run.SelectCharacter(KaitCharacter.Kait,412);
        run.EnqueueMergeReward(new KaitMergeEvent{sourceValue=8,resultValue=16});Assert.AreEqual(0,run.rewardQueue.Count);
        run.EnqueueMergeReward(new KaitMergeEvent{sourceValue=16,resultValue=32});Assert.AreEqual(1,run.rewardQueue.Count);
    }
}
