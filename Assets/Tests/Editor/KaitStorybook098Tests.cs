using NUnit.Framework;
using UnityEngine;

public sealed class KaitStorybook098Tests
{
    [Test] public void FramesAreCachedAndKeepRoleAndRarityDistinct()
    {
        var active=new KaitAbilityDef{kind=KaitAbilityKind.Active,rarity=KaitRarity.Common};
        var passive=new KaitAbilityDef{kind=KaitAbilityKind.Passive,rarity=KaitRarity.Common};
        var rare=new KaitAbilityDef{kind=KaitAbilityKind.Active,rarity=KaitRarity.Rare};
        Assert.AreSame(KaitStorybookTheme.Card(active),KaitStorybookTheme.Card(active));
        Assert.AreNotSame(KaitStorybookTheme.Card(active),KaitStorybookTheme.Card(passive));
        Assert.AreNotSame(KaitStorybookTheme.Card(active),KaitStorybookTheme.Card(rare));
        Assert.AreNotSame(KaitStorybookTheme.Card(active),KaitStorybookTheme.Card(active,true));
        Assert.Greater(KaitStorybookTheme.Panel.border.x,0);
    }
    [Test] public void ThemeDoesNotReplaceEitherCharactersPool()
    {
        Assert.AreEqual(39,KaitCardLibrary.Cards(KaitCharacter.Kait,-1).Count);
        Assert.AreEqual(56,KaitCardLibrary.Cards(KaitCharacter.Yummn,-1).Count);
        Assert.AreEqual("Kait.Run.Yummn.0.9.7",KaitVersion.YummnSaveKey);
    }
}
