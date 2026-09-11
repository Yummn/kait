using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class KaitSelectedWorldAudioTests
{
    [Test] public void BossEntranceUsesWorldChannelEvenWithCharacterVoices()
    {
        var code = System.IO.File.ReadAllText(System.IO.Path.Combine(Application.dataPath, "Scripts/GameAudio.cs"));
        StringAssert.Contains("bossRoarClip = Resources.Load<AudioClip>(SelectedWorldPath + \"Boss_A\");", code);
        StringAssert.Contains("PlayOneShot(instance?.worldSource, instance?.bossRoarClip, 0.72f);", code);
        StringAssert.DoesNotContain("instance.enemyCharacterVoiceBanks.ContainsKey(KaitEnemyType.ShieldKnight)) return", code);
    }
    [TestCase("RiftOpen_B2", 1f)]
    [TestCase("SpawnLanding_A", .905f)]
    [TestCase("WallStop_A2", .48f)]
    [TestCase("SkillReady_A2", .93f)]
    [TestCase("Boss_A", 1f)]
    public void SelectedWorldSoundPreservesAuditionImport(string name, float seconds)
    {
        string path = "Audio/World/SelectedModel/" + name;
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
