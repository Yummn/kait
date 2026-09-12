using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class KaitWarningFramesTests
{
    [TestCase("ClockA")][TestCase("DangerC")][TestCase("BossB")]
    public void ApprovedSheetsHaveEightLoopingFrames(string name)
    {
        var seen = new System.Collections.Generic.HashSet<Sprite>();
        for(int i=0;i<8;i++) { var s=KaitWarningFrames.Frame(name,i*.15f+.01f); Assert.NotNull(s); seen.Add(s); }
        Assert.AreEqual(8,seen.Count);
        Assert.AreSame(KaitWarningFrames.Frame(name,.01f),KaitWarningFrames.Frame(name,1.21f));
    }
    [Test] public void AtmosphereHasNoInputOrOpaqueMissingSprites()
    {
        var go=new GameObject("fx",typeof(RectTransform),typeof(KaitAtmosphereGraphic));
        try {
            var fx=go.GetComponent<KaitAtmosphereGraphic>(); fx.SetState(1,1,.16f);
            var images=go.GetComponentsInChildren<Image>(); Assert.AreEqual(KaitAtmosphereGraphic.EdgeClockCount+1,images.Length);
            foreach(var im in images){Assert.IsFalse(im.raycastTarget);Assert.NotNull(im.sprite);}
            fx.SetState(0,0,.16f); foreach(var im in images)Assert.AreEqual(0,im.color.a);
        } finally {Object.DestroyImmediate(go);}
    }
    [Test] public void BossWarningOnlyCoversLiveUnfrozenYummnIntent()
    {
        var run=new KaitRun();run.SelectCharacter(KaitCharacter.Yummn,8201);
        var p=new Vector2Int(3,3);
        var e=new KaitEnemy{type=KaitEnemyType.ShieldKnight,life=KaitEnemyLife.Active,hp=8};
        e.intent.type=KaitIntentType.Melee;e.intent.affectedCells.Add(p);
        Assert.IsTrue(KaitWarningFrames.Covers(run,e,p));Assert.IsFalse(KaitWarningFrames.Covers(run,e,p+Vector2Int.up));
        e.frozenActions=1;Assert.IsFalse(KaitWarningFrames.Covers(run,e,p));e.frozenActions=0;
        e.hp=0;Assert.IsFalse(KaitWarningFrames.Covers(run,e,p));e.hp=8;
        run.SelectCharacter(KaitCharacter.Kait,8201);Assert.IsFalse(KaitWarningFrames.Covers(run,e,p));
    }
    [Test] public void BossCanHideAndRotateWithoutChangingCellSize()
    {
        var go=new GameObject("warning",typeof(RectTransform),typeof(KaitBossWarningFrames));
        try {
            var fx=go.GetComponent<KaitBossWarningFrames>();fx.Configure(Vector2Int.down,false);
            var image=go.GetComponentInChildren<Image>();Assert.NotNull(image.sprite);Assert.IsFalse(image.raycastTarget);
            Assert.AreEqual(Vector2.zero,image.rectTransform.sizeDelta);
            Assert.That(image.rectTransform.eulerAngles.z,Is.EqualTo(270).Within(.01));
            fx.Clear();Assert.IsFalse(image.gameObject.activeSelf);
        }finally {Object.DestroyImmediate(go);}
    }
}
