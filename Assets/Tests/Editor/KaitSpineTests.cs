using NUnit.Framework;
using Spine.Unity;
using UnityEngine;

public sealed class KaitSpineTests
{
    [Test]
    public void MakotoExport_LoadsEveryGameplayAnimation()
    {
        SkeletonDataAsset asset = Resources.Load<SkeletonDataAsset>("Characters/Makoto/Makoto_SkeletonData");
        Assert.IsNotNull(asset, "Makoto SkeletonDataAsset is missing from Resources.");
        Spine.SkeletonData data = asset.GetSkeletonData(false);
        Assert.IsNotNull(data, "The Spine 3.8.75 Makoto export could not be parsed.");

        string[] required =
        {
            KaitSpineView.Idle, KaitSpineView.Run, KaitSpineView.StandBy,
            KaitSpineView.Attack, KaitSpineView.ChainAttack, KaitSpineView.Damage,
            KaitSpineView.Die, KaitSpineView.JoyShort, KaitSpineView.JoyLong,
            KaitSpineView.SmallAttack, KaitSpineView.LargeAttack, KaitSpineView.OtherSkill,
            KaitSpineView.Victory, KaitSpineView.ShadowStep
        };
        foreach (string animation in required)
        {
            Spine.Animation found = data.FindAnimation(animation);
            Assert.IsNotNull(found, $"Required animation is missing: {animation}");
            Assert.Greater(found.Duration, 0f, $"Animation has no duration: {animation}");
        }
    }

    [Test]
    public void SkeletonGraphicMaterial_IsPackagedForPlayerBuilds()
    {
        Material material = Resources.Load<Material>("Characters/Makoto/KaitSkeletonGraphic");
        Assert.IsNotNull(material);
        Assert.IsNotNull(material.shader);
        Assert.AreEqual("Spine/SkeletonGraphic", material.shader.name);
    }

    [TestCase("100161")]
    [TestCase("105731")]
    [TestCase("106331")]
    [TestCase("112731")]
    [TestCase("111031")]
    [TestCase("104731")]
    public void SpecifiedEnemyPortrait_IsPackagedForPlayerBuilds(string assetId)
    {
        Texture2D portrait = Resources.Load<Texture2D>("EnemyPortraits/" + assetId);
        Assert.IsNotNull(portrait, "Enemy portrait is missing: " + assetId);
        Assert.Greater(portrait.width, 0);
        Assert.Greater(portrait.height, 0);
    }
}
