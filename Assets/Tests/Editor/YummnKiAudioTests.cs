using NUnit.Framework;
using UnityEngine;

public class YummnKiAudioTests
{
    [TestCase(0,0,false)]
    [TestCase(1,0,true)]
    [TestCase(0,10,true)]
    [TestCase(0,1,true)]
    public void OnlyPaidActionsPlaySpend(int cost,int attack,bool expected)
    {
        Assert.AreEqual(expected,YummnAudio.HasKiExpenditure(new YummnActionContext
            {totalKiCost=cost,attackCostTenths=attack,kiBefore=3,kiAfter=3}));
    }
    [Test] public void ApprovedReversedPairLoads()
    {
        var gain=YummnAudio.LoadClip("Ki");
        var spend=YummnAudio.LoadClip("KiSpendB");
        Assert.NotNull(gain);Assert.NotNull(spend);
        Assert.AreEqual("KiGainB",gain.name);Assert.AreEqual("KiSpendB",spend.name);
        Assert.AreNotSame(gain,spend);
        Assert.That(gain.length,Is.EqualTo(1f).Within(.02f));
        Assert.That(spend.length,Is.EqualTo(1f).Within(.02f));
    }
}
