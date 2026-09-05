using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class KaitSelectedRangedAudioTests
{
    [TestCase("ArrowFlight_A2", .70f)]
    [TestCase("ArrowImpact_A", .46f)]
    [TestCase("MagicCast_B2", .91f)]
    [TestCase("MagicImpact_B2", .88f)]
    public void SelectedRangedSoundPreservesAuditionImport(string name, float seconds)
    {
        string path = "Audio/Ranged/SelectedModel/" + name;
        AudioClip clip = Resources.Load<AudioClip>(path);
        Assert.NotNull(clip);
        Assert.AreEqual(48000, clip.frequency);
        Assert.AreEqual(2, clip.channels);
        Assert.That(clip.length, Is.EqualTo(seconds).Within(.001f));
        var importer = (AudioImporter)AssetImporter.GetAtPath("Assets/Resources/" + path + ".wav");
        Assert.AreEqual(AudioCompressionFormat.PCM, importer.defaultSampleSettings.compressionFormat);
        Assert.AreEqual(AudioSampleRateSetting.PreserveSampleRate, importer.defaultSampleSettings.sampleRateSetting);
        Assert.AreEqual(AudioClipLoadType.DecompressOnLoad, importer.defaultSampleSettings.loadType);
        Assert.IsFalse(importer.forceToMono);
        Assert.IsFalse(new SerializedObject(importer).FindProperty("m_Normalize").boolValue);
    }
}
