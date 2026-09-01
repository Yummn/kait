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

    [TestCase("100161", "01_")]
    [TestCase("105731", "04_")]
    [TestCase("106331", "08_")]
    [TestCase("112731", "06_")]
    [TestCase("111031", "26_")]
    [TestCase("104731", "05_")]
    public void EnemySpineExport_LoadsGameplayAnimations(string assetId, string prefix)
    {
        SkeletonDataAsset asset = Resources.Load<SkeletonDataAsset>($"Characters/Enemies/{assetId}/{assetId}_SkeletonData");
        Assert.IsNotNull(asset, $"Enemy SkeletonDataAsset is missing: {assetId}");
        Spine.SkeletonData data = asset.GetSkeletonData(false);
        Assert.IsNotNull(data, $"Enemy Spine export could not be parsed: {assetId}");
        foreach (string suffix in new[] { EnemySpineView.LandingSuffix, EnemySpineView.IdleSuffix, EnemySpineView.AttackSuffix, EnemySpineView.DamageSuffix })
        {
            Spine.Animation animation = data.FindAnimation(prefix + suffix);
            Assert.IsNotNull(animation, $"Required enemy animation is missing: {assetId}/{prefix + suffix}");
            Assert.Greater(animation.Duration, 0f);
        }
    }

    [Test]
    public void AttackWarningStripeTexture_IsPackagedForPlayerBuilds()
    {
        Texture2D texture = Resources.Load<Texture2D>("KaitVisuals/AttackWarningStripes");
        Assert.IsNotNull(texture);
        Assert.Greater(texture.width, 0);
        Assert.Greater(texture.height, 0);
    }

    [TestCase("KaitVisuals/DungeonFloor")]
    [TestCase("KaitVisuals/DungeonWall")]
    public void DungeonTileTexture_IsPackagedForPlayerBuilds(string resourcePath)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        Assert.IsNotNull(texture);
        Assert.AreEqual(16, texture.width);
        Assert.AreEqual(16, texture.height);
    }
}
