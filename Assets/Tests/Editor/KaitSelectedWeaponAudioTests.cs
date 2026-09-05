using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class KaitSelectedWeaponAudioTests
{
    private const string Root = "Audio/Combat/SelectedModel/";

    [TestCase("SwordSwing_A_OriginalLevel", 0.48f)]
    [TestCase("Push_A_OriginalLevel", 0.202f)]
    [TestCase("Block_A_OriginalLevel", 0.4f)]
    [TestCase("Hit_A", 0.94f)]
    [TestCase("Kill_A", 0.9f)]
    [TestCase("Chain_A", 0.86f)]
    public void SelectedClipKeepsOriginalPcmImport(string name, float length)
    {
        AudioClip clip = Resources.Load<AudioClip>(Root + name);
        Assert.NotNull(clip);
        Assert.AreEqual(48000, clip.frequency);
        Assert.AreEqual(2, clip.channels);
        Assert.That(clip.length, Is.EqualTo(length).Within(0.001f));
        var importer = (AudioImporter)AssetImporter.GetAtPath("Assets/Resources/" + Root + name + ".wav");
        Assert.AreEqual(AudioCompressionFormat.PCM, importer.defaultSampleSettings.compressionFormat);
        Assert.AreEqual(AudioSampleRateSetting.PreserveSampleRate, importer.defaultSampleSettings.sampleRateSetting);
        Assert.AreEqual(AudioClipLoadType.DecompressOnLoad, importer.defaultSampleSettings.loadType);
        Assert.IsFalse(importer.forceToMono);
        var serialized = new SerializedObject(importer);
        Assert.IsFalse(serialized.FindProperty("m_Normalize").boolValue);
    }

    [TestCase(0, "Kill_A")]
    [TestCase(1, "Kill_A")]
    [TestCase(2, "Chain_A")]
    [TestCase(3, "Chain_A")]
    [TestCase(10, "Chain_A")]
    public void SecondConsecutiveKillSwitchesToChain(int kills, string expected)
    {
        var select = typeof(GameAudio).GetMethod("SelectedKillClip", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(select);
        var actual = (AudioClip)select.Invoke(null, new object[] { kills });
        Assert.AreSame(Resources.Load<AudioClip>(Root + expected), actual);
    }
}
