using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class KaitAudioTimingTests
{
    [TestCase(KaitIntentType.Melee, 0, true)]
    [TestCase(KaitIntentType.CrossBlast, 0, true)]
    [TestCase(KaitIntentType.LineShot, 0, false)]
    [TestCase(KaitIntentType.LineShot, 1, true)]
    [TestCase(KaitIntentType.Melee, 1, false)]
    [TestCase(KaitIntentType.CrossBlast, 1, false)]
    [TestCase(KaitIntentType.Move, 0, false)]
    public void ContactPhaseDoesNotWaitForArrows(KaitIntentType type, int phase, bool expected)
    {
        var method = typeof(KaitGame).GetMethod("MatchesImpactPhase", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.AreEqual(expected, method.Invoke(null, new object[] { type, phase }));
    }

    [TestCase(1f, 1f, false)]
    [TestCase(1.02f, 1f, false)]
    [TestCase(1.05f, 1f, true)]
    public void ActionCueSuppressesSimultaneousDuplicates(float now, float previous, bool expected)
    {
        var method = typeof(GameAudio).GetMethod("AcceptActionCue", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.AreEqual(expected, method.Invoke(null, new object[] { now, previous }));
    }

    [Test]
    public void MergeSoundStartsBeforePulse()
    {
        string source = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/KaitGame.cs"));
        int sound = source.IndexOf("GameAudio.PlayMerge(strongest);");
        Assert.Greater(sound, 0);
        Assert.Greater(source.IndexOf("yield return ScalePulseMany(mergeCells", sound), sound);
    }

    [Test]
    public void InterruptPreservesImpactAndVoiceChannels()
    {
        string source = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts/GameAudio.cs"));
        int start = source.IndexOf("public static void InterruptActionSounds()");
        string body = source.Substring(start, source.IndexOf("private static bool AcceptActionCue", start) - start);
        StringAssert.Contains("swingSource?.Stop()", body);
        StringAssert.Contains("magicActionSource?.Stop()", body);
        StringAssert.DoesNotContain("impactSource?.Stop()", body);
        StringAssert.DoesNotContain("killSource?.Stop()", body);
        StringAssert.DoesNotContain("kaitVoiceSource?.Stop()", body);
    }
}
