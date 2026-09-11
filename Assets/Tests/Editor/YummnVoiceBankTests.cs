using NUnit.Framework;
using UnityEngine;
using System.Reflection;

public sealed class YummnVoiceBankTests
{
    [TestCase("Battle_N_1")][TestCase("Battle_N_2")][TestCase("Battle_N_3")]
    [TestCase("Battle_N_4")][TestCase("Battle_N_5")][TestCase("Battle_N_6")]
    [TestCase("Battle_H_1")][TestCase("Battle_H_2")][TestCase("Battle_C_2")]
    [TestCase("Battle_Hit_1")][TestCase("Battle_Hit_3")][TestCase("Battle_Hit_5")]
    [TestCase("Battle_Hit_6")][TestCase("Go_1")][TestCase("Win_1")]
    [TestCase("Fail_1")][TestCase("Battle_Die_1")]
    public void PlayerVoiceUsesOneBankAndPreservesKait(string suffix)
    {
        var original=Resources.Load<AudioClip>("Audio/Voice/Gloria/Gloria_"+suffix);
        Assert.NotNull(original);
        Assert.AreSame(original,GameAudio.ResolvePlayerVoice(original,false));
        var yummn=GameAudio.ResolvePlayerVoice(original,true);
        Assert.NotNull(yummn);Assert.AreEqual("Bridget_"+suffix,yummn.name);
        Assert.Greater(yummn.length,0);
    }
    [TestCase("Battle_N_2")][TestCase("Battle_N_3")][TestCase("Battle_N_4")]
    [TestCase("Battle_N_1")][TestCase("Battle_N_5")][TestCase("Battle_Hit_1")]
    [TestCase("Battle_Hit_2")][TestCase("Battle_Hit_3")][TestCase("Battle_Hit_4")]
    [TestCase("Go_1")][TestCase("Battle_H_2")][TestCase("Battle_Die_1")][TestCase("Battle_C_1")]
    public void GuardReplacementHasCompleteBank(string suffix)
    {
        Assert.NotNull(Resources.Load<AudioClip>("Audio/Voice/Enemies/Coonya/Coonya_"+suffix));
    }
    [Test] public void MissingClipDoesNotFallBackToKait()
    {
        var dummy=AudioClip.Create("Gloria_Missing",100,1,48000,false);
        try{Assert.IsNull(GameAudio.ResolvePlayerVoice(dummy,true));}
        finally{Object.DestroyImmediate(dummy);}
    }
}
