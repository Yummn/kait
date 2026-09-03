using NUnit.Framework;
using UnityEngine;

public sealed class KaitVoiceTests
{
    [TestCase("Gloria_Battle_N_1")]
    [TestCase("Gloria_Battle_N_2")]
    [TestCase("Gloria_Battle_N_3")]
    [TestCase("Gloria_Battle_N_4")]
    [TestCase("Gloria_Battle_N_5")]
    [TestCase("Gloria_Battle_N_6")]
    [TestCase("Gloria_Battle_H_1")]
    [TestCase("Gloria_Battle_H_2")]
    [TestCase("Gloria_Battle_C_2")]
    [TestCase("Gloria_Battle_Hit_1")]
    [TestCase("Gloria_Battle_Hit_3")]
    [TestCase("Gloria_Battle_Hit_5")]
    [TestCase("Gloria_Battle_Hit_6")]
    [TestCase("Gloria_Go_1")]
    [TestCase("Gloria_Win_1")]
    [TestCase("Gloria_Fail_1")]
    [TestCase("Gloria_Battle_Die_1")]
    public void GloriaVoiceClip_IsPackagedAsAResource(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/Voice/Gloria/" + clipName);
        Assert.IsNotNull(clip, clipName + " should be available to GameAudio at runtime.");
    }

    [TestCase("April_Battle_N_1")]
    [TestCase("April_Battle_N_2")]
    [TestCase("April_Battle_N_3")]
    [TestCase("April_Battle_N_4")]
    [TestCase("April_Battle_N_5")]
    [TestCase("April_Battle_H_2")]
    [TestCase("April_Battle_Hit_1")]
    [TestCase("April_Battle_Hit_2")]
    [TestCase("April_Battle_Hit_3")]
    [TestCase("April_Battle_Hit_4")]
    [TestCase("April_Battle_Die_1")]
    [TestCase("April_Go_1")]
    [TestCase("April_Battle_C_1")]
    public void AprilGruntVoiceClip_IsPackagedAsAResource(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/Voice/Enemies/April/" + clipName);
        Assert.IsNotNull(clip, clipName + " should be available to the grunt voice bank at runtime.");
    }

    [TestCase("Olivia_Battle_N_1")]
    [TestCase("Olivia_Battle_N_2")]
    [TestCase("Olivia_Battle_N_3")]
    [TestCase("Olivia_Battle_N_4")]
    [TestCase("Olivia_Battle_N_5")]
    [TestCase("Olivia_Battle_H_2")]
    [TestCase("Olivia_Battle_Hit_1")]
    [TestCase("Olivia_Battle_Hit_2")]
    [TestCase("Olivia_Battle_Hit_3")]
    [TestCase("Olivia_Battle_Hit_4")]
    [TestCase("Olivia_Battle_Die_1")]
    [TestCase("Olivia_Go_1")]
    [TestCase("Olivia_Battle_C_1")]
    public void OliviaSwordsmanVoiceClip_IsPackagedAsAResource(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/Voice/Enemies/Olivia/" + clipName);
        Assert.IsNotNull(clip, clipName + " should be available to the swordsman voice bank at runtime.");
    }

    [TestCase("Monica_Battle_N_1")]
    [TestCase("Monica_Battle_N_2")]
    [TestCase("Monica_Battle_N_3")]
    [TestCase("Monica_Battle_N_4")]
    [TestCase("Monica_Battle_N_5")]
    [TestCase("Monica_Battle_H_2")]
    [TestCase("Monica_Battle_Hit_1")]
    [TestCase("Monica_Battle_Hit_2")]
    [TestCase("Monica_Battle_Hit_3")]
    [TestCase("Monica_Battle_Hit_4")]
    [TestCase("Monica_Battle_Die_1")]
    [TestCase("Monica_Go_1")]
    [TestCase("Monica_Battle_C_1")]
    public void MonicaArcherVoiceClip_IsPackagedAsAResource(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/Voice/Enemies/Monica/" + clipName);
        Assert.IsNotNull(clip, clipName + " should be available to the archer voice bank at runtime.");
    }

    [TestCase("Bridget_Battle_N_1")]
    [TestCase("Bridget_Battle_N_2")]
    [TestCase("Bridget_Battle_N_3")]
    [TestCase("Bridget_Battle_N_4")]
    [TestCase("Bridget_Battle_N_5")]
    [TestCase("Bridget_Battle_H_2")]
    [TestCase("Bridget_Battle_Hit_1")]
    [TestCase("Bridget_Battle_Hit_2")]
    [TestCase("Bridget_Battle_Hit_3")]
    [TestCase("Bridget_Battle_Hit_4")]
    [TestCase("Bridget_Battle_Die_1")]
    [TestCase("Bridget_Go_1")]
    [TestCase("Bridget_Battle_C_1")]
    public void BridgetGuardVoiceClip_IsPackagedAsAResource(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/Voice/Enemies/Bridget/" + clipName);
        Assert.IsNotNull(clip, clipName + " should be available to the guard voice bank at runtime.");
    }

    [TestCase("Aloe_Battle_N_1")]
    [TestCase("Aloe_Battle_N_2")]
    [TestCase("Aloe_Battle_N_3")]
    [TestCase("Aloe_Battle_N_4")]
    [TestCase("Aloe_Battle_N_5")]
    [TestCase("Aloe_Battle_H_2")]
    [TestCase("Aloe_Battle_Hit_1")]
    [TestCase("Aloe_Battle_Hit_2")]
    [TestCase("Aloe_Battle_Hit_3")]
    [TestCase("Aloe_Battle_Hit_4")]
    [TestCase("Aloe_Battle_Die_1")]
    [TestCase("Aloe_Go_1")]
    [TestCase("Aloe_Battle_C_1")]
    public void AloeWarlockVoiceClip_IsPackagedAsAResource(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/Voice/Enemies/Aloe/" + clipName);
        Assert.IsNotNull(clip, clipName + " should be available to the warlock voice bank at runtime.");
    }

    [TestCase("Ursula_Battle_N_1")]
    [TestCase("Ursula_Battle_N_2")]
    [TestCase("Ursula_Battle_N_3")]
    [TestCase("Ursula_Battle_N_4")]
    [TestCase("Ursula_Battle_N_5")]
    [TestCase("Ursula_Battle_H_2")]
    [TestCase("Ursula_Battle_Hit_1")]
    [TestCase("Ursula_Battle_Hit_2")]
    [TestCase("Ursula_Battle_Hit_3")]
    [TestCase("Ursula_Battle_Hit_4")]
    [TestCase("Ursula_Battle_Die_1")]
    [TestCase("Ursula_Go_1")]
    [TestCase("Ursula_Battle_C_1")]
    public void UrsulaShieldKnightVoiceClip_IsPackagedAsAResource(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/Voice/Enemies/Ursula/" + clipName);
        Assert.IsNotNull(clip, clipName + " should be available to the shield knight voice bank at runtime.");
    }
}
