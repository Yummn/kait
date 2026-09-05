using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class KaitSelectedSkillAudioTests
{
    private const string Root = "Audio/Skills/SelectedModel/";

    [TestCase("SpeedBuff_A", 1f)]
    [TestCase("FrostBind_B", 1f)]
    [TestCase("ShadowStep_B", .3f)]
    [TestCase("MagicCharge_A", 1f)]
    [TestCase("DreadSlash_B", .94f)]
    [TestCase("Phantom_B", 1f)]
    [TestCase("BodyHurt_B", .43f)]
    public void SelectedSkillClipPreservesAudition(string name, float length)
    {
        AudioClip clip = Resources.Load<AudioClip>(Root + name);
        Assert.NotNull(clip);
        Assert.AreEqual(48000, clip.frequency);
        Assert.AreEqual(2, clip.channels);
        Assert.That(clip.length, Is.EqualTo(length).Within(.001f));
        var importer = (AudioImporter)AssetImporter.GetAtPath("Assets/Resources/" + Root + name + ".wav");
        Assert.AreEqual(AudioCompressionFormat.PCM, importer.defaultSampleSettings.compressionFormat);
        Assert.AreEqual(AudioSampleRateSetting.PreserveSampleRate, importer.defaultSampleSettings.sampleRateSetting);
        Assert.AreEqual(AudioClipLoadType.DecompressOnLoad, importer.defaultSampleSettings.loadType);
        Assert.IsFalse(importer.forceToMono);
        Assert.IsFalse(new SerializedObject(importer).FindProperty("m_Normalize").boolValue);
    }

    [TestCase(KaitSkill.SwiftBoots, Root + "SpeedBuff_A")]
    [TestCase(KaitSkill.CatAgility, Root + "SpeedBuff_A")]
    [TestCase(KaitSkill.IceTomb, Root + "FrostBind_B")]
    [TestCase(KaitSkill.ShadowStep, Root + "ShadowStep_B")]
    [TestCase(KaitSkill.DreadSlash, "Audio/UI/SkillUse_01")]
    [TestCase(KaitSkill.LesserPhantom, Root + "Phantom_B")]
    [TestCase(KaitSkill.None, "Audio/UI/SkillUse_01")]
    public void EachSkillSelectsOnlyItsAssignedClip(KaitSkill skill, string expected)
    {
        MethodInfo select = typeof(GameAudio).GetMethod("SelectedSkillClip", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(select);
        AudioClip clip = (AudioClip)select.Invoke(null, new object[] { skill });
        Assert.NotNull(clip);
        Assert.AreSame(Resources.Load<AudioClip>(expected), clip);
    }

    [Test]
    public void CastSitesPassTheirSkillAndChargeUsesSelectedAudio()
    {
        string game = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/KaitGame.cs"));
        Assert.IsFalse(game.Contains("GameAudio.PlaySkillUse();"));
        Assert.AreEqual(3, System.Text.RegularExpressions.Regex.Matches(game,
            @"GameAudio\.PlaySkillUse\(skill\);").Count);
        string audio = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/GameAudio.cs"));
        StringAssert.Contains("magicChargeClip = Resources.Load<AudioClip>(SelectedSkillPath + \"MagicCharge_A\");", audio);
    }

    [TestCase(KaitIntentType.Melee, true, 1, false, true)]
    [TestCase(KaitIntentType.Melee, true, 0, false, false)]
    [TestCase(KaitIntentType.Melee, true, 1, true, false)]
    [TestCase(KaitIntentType.Melee, false, 1, false, false)]
    [TestCase(KaitIntentType.LineShot, true, 1, false, false)]
    [TestCase(KaitIntentType.CrossBlast, true, 1, false, false)]
    [TestCase(KaitIntentType.Move, true, 1, false, false)]
    public void BodyImpactDoesNotDoubleRangedHitsOrPlayForInvulnerability(
        KaitIntentType type, bool hitKait, int damage, bool invincible, bool expected)
    {
        var action = new KaitEnemyAction { type = type, hitKate = hitKait, damage = damage };
        var method = typeof(KaitGame).GetMethod("ShouldPlayBodyHurt", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        Assert.AreEqual(expected, method.Invoke(null, new object[] { action, invincible }));
    }

    [Test]
    public void SlashAudioPlaysOnceAtWaveReleaseNotPerCell()
    {
        string game = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/KaitGame.cs"));
        Assert.AreEqual(1, System.Text.RegularExpressions.Regex.Matches(game,
            @"GameAudio\.PlayDreadSlash\(\);").Count);
        int wave = game.IndexOf("if (result.dreadSlash)");
        int audioCall = game.IndexOf("GameAudio.PlayDreadSlash();", wave);
        int loop = game.IndexOf("yield return AnimateAllEnemyActions", wave);
        Assert.Greater(audioCall, wave);
        Assert.Less(audioCall, loop);
    }
}
