using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public sealed class KaitCombatEffectTests
{
    [TestCase(KaitCombatEffectKind.NormalHit)]
    [TestCase(KaitCombatEffectKind.SwordArc)]
    [TestCase(KaitCombatEffectKind.Block)]
    [TestCase(KaitCombatEffectKind.Kill)]
    [TestCase(KaitCombatEffectKind.ChainKill)]
    [TestCase(KaitCombatEffectKind.EnemyHit)]
    [TestCase(KaitCombatEffectKind.MagicCast)]
    [TestCase(KaitCombatEffectKind.MagicImpact)]
    [TestCase(KaitCombatEffectKind.Ice)]
    [TestCase(KaitCombatEffectKind.Phantom)]
    [TestCase(KaitCombatEffectKind.Speed)]
    [TestCase(KaitCombatEffectKind.DreadSlash)]
    public void EffectProducesLocalGeometry(KaitCombatEffectKind kind)
    {
        var root = new GameObject("Effect Test Canvas", typeof(Canvas));
        var effect = new GameObject("Effect", typeof(RectTransform), typeof(CanvasRenderer),
            typeof(KaitCombatEffectGraphic));
        effect.transform.SetParent(root.transform, false);
        RectTransform rect = effect.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200f, 200f);

        KaitCombatEffectGraphic graphic = effect.GetComponent<KaitCombatEffectGraphic>();
        graphic.Configure(kind, Color.red, Color.white, 1f);
        graphic.SetProgress(0.35f);
        graphic.Rebuild(CanvasUpdate.PreRender);

        Mesh mesh = graphic.canvasRenderer.GetMesh();
        Assert.Greater(mesh.vertexCount, 0, kind + " should render visible geometry");
        Assert.LessOrEqual(mesh.bounds.extents.x, 135f, kind + " escaped its local effect area");
        Assert.LessOrEqual(mesh.bounds.extents.y, 135f, kind + " escaped its local effect area");

        Object.DestroyImmediate(root);
    }

    [Test]
    public void SwordSlashMaterialIsPackagedAsTexture()
    {
        Texture2D texture = Resources.Load<Texture2D>("KaitVisuals/Effects/KaitSwordSlashSheet");
        Assert.NotNull(texture);
        Assert.AreEqual(256, texture.width);
        Assert.AreEqual(256, texture.height);
    }
}
