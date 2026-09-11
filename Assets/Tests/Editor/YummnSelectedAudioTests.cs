using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class YummnSelectedAudioTests
{
    [TestCase("Air")] [TestCase("Block")] [TestCase("Exhaust")]
    [TestCase("Fire")] [TestCase("Frost")] [TestCase("Ice")]
    [TestCase("Ki")] [TestCase("Kill")] [TestCase("Move")]
    [TestCase("PalmSeal")] [TestCase("Punch")] [TestCase("Ready")]
    [TestCase("Recover")] [TestCase("Shadow")] [TestCase("Shatter")]
    [TestCase("Water")]
    public void ApprovedClipOverridesLegacyWithoutLossyImport(string cue)
    {
        var clip = YummnAudio.LoadClip(cue);
        Assert.NotNull(clip);
        Assert.AreEqual("Assets/Resources/Audio/Yummn/SelectedModel/" + (cue == "Ki" ? "KiGainB" : cue) + ".wav", AssetDatabase.GetAssetPath(clip));
        Assert.AreEqual(48000, clip.frequency);
        Assert.AreEqual(2, clip.channels);
        var importer = (AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip));
        Assert.AreEqual(AudioCompressionFormat.PCM, importer.defaultSampleSettings.compressionFormat);
        Assert.IsFalse(importer.forceToMono);
        Assert.AreEqual(cue == "PalmSeal" ? 1.25f : 1f, YummnAudio.CueVolume(cue));
    }
    [Test] public void UnreplacedLegacyCueRemainsAvailable()
    {
        Assert.AreSame(Resources.Load<AudioClip>("Audio/Yummn/Skill"), YummnAudio.LoadClip("Skill"));
        Assert.IsNull(YummnAudio.LoadClip("MissingCue"));
    }
}
