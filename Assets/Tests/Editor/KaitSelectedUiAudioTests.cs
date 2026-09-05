using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class KaitSelectedUiAudioTests
{
    [TestCase("ButtonClick_A", .085f)]
    [TestCase("InvalidAction_Defeat_A", .93f)]
    [TestCase("Victory_B3", 4f)]
    [TestCase("Defeat_B3", 4f)]
    [TestCase("Merge_B", .82f)]
    public void SelectedUiSoundKeepsAuditionPcm(string name, float seconds)
    {
        string path = "Audio/UI/SelectedModel/" + name;
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

    [TestCase("clickClip", "ButtonClick_A")]
    [TestCase("invalidClip", "InvalidAction_Defeat_A")]
    [TestCase("winClip", "Victory_B3")]
    [TestCase("loseClip", "Defeat_B3")]
    public void RuntimeReferenceMatchesUserSelection(string field, string clip)
    {
        string source = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/GameAudio.cs"));
        StringAssert.Contains(field + " = Resources.Load<AudioClip>(SelectedUiPath + \"" + clip + "\");", source);
    }
}
