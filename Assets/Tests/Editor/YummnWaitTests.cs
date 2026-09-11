using NUnit.Framework;
using UnityEngine;

public class YummnWaitTests
{
    [Test]
    public void WaitAdvancesExactlyOneEnemyPhaseWithoutPlayerAction()
    {
        var run=new KaitRun();run.SelectCharacter(KaitCharacter.Yummn,123);
        run.enemies.Clear();var pos=run.katePos;var ki=run.Ki;var turn=run.turn;
        var board=(int[,])run.threat.Clone();var phases=run.Yummn.metrics.enemyPhases;
        var result=run.TryYummnWait();
        Assert.IsTrue(result.valid);Assert.IsTrue(result.yummnAction.isWait);
        Assert.AreEqual(pos,run.katePos);Assert.AreEqual(ki,run.Ki);
        Assert.AreEqual(turn+1,run.turn);Assert.AreEqual(phases+1,run.Yummn.metrics.enemyPhases);
        CollectionAssert.AreEqual(board,run.threat);
        Assert.IsEmpty(result.katePath);Assert.IsEmpty(result.merges);
        Assert.AreEqual(0,result.yummnAction.totalKiCost);
        Assert.IsFalse(result.yummnAction.didAttack);
    }
    [Test]
    public void WaitingDoesNotCommitPreparedSkill()
    {
        var run=new KaitRun();run.SelectCharacter(KaitCharacter.Yummn,123);
        run.Yummn.prepared.Add("yummn.M02");
        run.TryYummnWait();Assert.IsTrue(run.Yummn.prepared.Contains("yummn.M02"));
    }
    [Test]
    public void KaitCannotWait()
    {
        var run=new KaitRun();run.SelectCharacter(KaitCharacter.Kait,123);
        int turn=run.turn;Assert.IsFalse(run.TryYummnWait().valid);Assert.AreEqual(turn,run.turn);
    }
}
