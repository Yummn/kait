using NUnit.Framework;
using UnityEngine;

public sealed class YummnPriorityFeedbackTests
{
    private static readonly string[] EffectNames=
    {
        "BountyJar","CommandAct","GravityPendulum","LastingIllusion","MageHand","MagicMissile",
        "ManaPearl","MirrorCreate","MirrorResonance","Misdirection","ResonanceCrystal","ShadowBladeEcho",
        "SonicBurst","SpellEcho","StoneCollision","SweepPursuit","ThunderWave","WardingGlyph"
    };

    private static readonly string[] AudioNames=
    {
        "CommandAct","MageHand","MagicMissile","MirrorCreate","SonicBurst","StoneCollision","WardingGlyph"
    };

    [Test]
    public void PriorityEffectSheets_AreReadableFourByTwoAtlases()
    {
        foreach(string name in EffectNames)
        {
            var texture=Resources.Load<Texture2D>("KaitVisuals/Yummn/PriorityFx/"+name);
            Assert.That(texture,Is.Not.Null,name+" effect sheet was not imported");
            int frameWidth=texture.width/4;
            int frameHeight=texture.height/2;
            Assert.That(frameWidth,Is.GreaterThanOrEqualTo(64),name+" frames are unexpectedly small");
            Assert.That(frameHeight,Is.GreaterThanOrEqualTo(64),name+" frames are unexpectedly small");
            Assert.That(Mathf.Abs(frameWidth-frameHeight),Is.LessThanOrEqualTo(1),name+" must contain eight near-square frames");
        }
    }

    [Test]
    public void PriorityAudioClips_AreLoadable()
    {
        foreach(string name in AudioNames)
            Assert.That(Resources.Load<AudioClip>("Audio/Yummn/Priority/"+name),Is.Not.Null,name+" audio was not imported");
    }
}
