using NUnit.Framework;
using UnityEngine;
using System.Linq;

// v0.7 two-action expectations were superseded by the approved v0.8 guide.
// The complete old source is retained in Backups/before-yummn-v0.8-20260909-155731.
// Kait regression expectations remain unchanged in their original test files.
public class YummnRulesTests
{
    [Test] public void CornerPillarsUseRowColumnOnBothBoards()
    {
        var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,42,new YummnRulesSnapshot());
        Assert.IsTrue(r.walls[1,5]);Assert.IsTrue(r.walls[5,1]);Assert.IsTrue(r.threatPillars[0,4]);Assert.IsTrue(r.threatPillars[4,0]);Assert.IsFalse(r.walls[1,2]);
    }
    [TestCase(KaitCharacter.Kait)] [TestCase(KaitCharacter.Yummn)]
    public void CharacterReplayRestoresIndependentRulesAndSettings(KaitCharacter character)
    {
        var r=new KaitRun(new KaitBalanceConfig());r.SelectCharacter(character,6021);
        for(int i=0;i<8&&!r.ended;i++)r.TryGlobalInput((KaitDirection)(i%4));
        r.config.playerInvincible=true;r.RecordSettingsChange();
        for(int i=0;i<4&&!r.ended;i++)r.TryGlobalInput((KaitDirection)((i+1)%4));
        var copy=new KaitRun(new KaitBalanceConfig());Assert.IsTrue(copy.RestoreReplay(r.SaveReplay()));
        Assert.AreEqual(character,copy.Character);Assert.AreEqual(r.SaveReplay(),copy.SaveReplay());Assert.AreEqual(r.katePos,copy.katePos);Assert.AreEqual(r.kateHp,copy.kateHp);Assert.AreEqual(r.Ki,copy.Ki);Assert.AreEqual(r.KiPhase,copy.KiPhase);CollectionAssert.AreEqual(r.threat,copy.threat);
    }
    [Test] public void OldYummnReplayIsRejectedBeforeMutatingRun()
    {
        var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,42,new YummnRulesSnapshot());var current=r.SaveReplay();
        Assert.IsFalse(r.RestoreReplay(current.Replace(YummnRulesProfile.Version,"Yummn.FixedFist.0.7")));Assert.AreEqual(current,r.SaveReplay());
    }
    [Test] public void LegacyActionAndCooldownCardsAreNotInNewPool()
    {
        var pool=YummnCatalog.Pool();
        Assert.IsFalse(pool.Any(d=>d.skill==KaitSkill.WindStep||d.passive==KaitPassive.ReturnMissile||d.passive==KaitPassive.EmptyBody||d.passive==KaitPassive.Unarmored));
        Assert.AreEqual(30,pool.Count);
    }
}
