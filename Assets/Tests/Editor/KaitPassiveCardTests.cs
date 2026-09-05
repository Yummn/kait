using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class KaitPassiveCardTests
{
    [Test]
    public void DockingSeparatesCardsAndReservesMenus()
    {
        var area = new Rect(-960, -540, 1920, 1080);
        float[] x = KaitPassiveDeck.ResolveDockPositions(new[] { 600f, 620f, 700f }, area);
        Assert.GreaterOrEqual(x[0], -720);
        Assert.LessOrEqual(x[2], 640);
        Assert.GreaterOrEqual(x[1] - x[0], KaitPassiveCard.Size.x + 14f - 0.01f);
        Assert.GreaterOrEqual(x[2] - x[1], KaitPassiveCard.Size.x + 14f - 0.01f);
    }

    [Test]
    public void OwnedDockLeavesExactlyHalfVisibleAndCandidatesStayInside()
    {
        var area = new Rect(-960, -540, 1920, 1080);
        Vector2 owned = KaitPassiveCard.ClampToScreen(new Vector2(9000, 9000), area, false);
        Vector2 candidate = KaitPassiveCard.ClampToScreen(new Vector2(9000, 9000), area, true);
        Assert.AreEqual(area.yMax, owned.y);
        Assert.AreEqual(KaitPassiveCard.Size.y * 0.5f, area.yMax - (owned.y - KaitPassiveCard.Size.y * 0.5f));
        Assert.Less(candidate.y + KaitPassiveCard.Size.y * 0.5f, area.yMax);
        Assert.Less(candidate.x + KaitPassiveCard.Size.x * 0.5f, area.xMax);
    }

    [Test]
    public void GlobalSplitIsNotClampedAtMovingCardCorners()
    {
        var root = new GameObject("Root", typeof(RectTransform), typeof(GlobalStyleSplit));
        var child = new GameObject("Card", typeof(RectTransform));
        try
        {
            var area = root.GetComponent<RectTransform>();
            area.sizeDelta = new Vector2(200, 200);
            var target = child.GetComponent<RectTransform>();
            target.SetParent(area, false);
            target.sizeDelta = new Vector2(100, 100);
            target.anchoredPosition = new Vector2(70, 0);
            var split = root.GetComponent<GlobalStyleSplit>();
            split.Configure(area, 0.4f, 0.6f);
            split.GetLocalSplits(target, out float bottom, out float top);
            Assert.AreEqual(-0.3f, bottom, 0.001f);
            Assert.AreEqual(-0.1f, top, 0.001f);
        }
        finally { Object.DestroyImmediate(root); }
    }

    [TestCase(-500f, true, false)]
    [TestCase(0f, true, true)]
    [TestCase(500f, false, true)]
    public void MovingOneGraphicSelectsTheCorrectTwoTextures(float x, bool hasLeft, bool hasRight)
    {
        var root = new GameObject("Root", typeof(RectTransform), typeof(GlobalStyleSplit));
        var go = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(HybridStyleGraphic));
        Sprite hd = KaitSunlitTheme.Load("PassiveCardHD");
        Sprite flat = KaitSunlitTheme.Load("PassiveCardFlat");
        try
        {
            var area = root.GetComponent<RectTransform>();
            area.sizeDelta = new Vector2(1920, 1080);
            go.transform.SetParent(area, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = KaitPassiveCard.Size;
            rect.anchoredPosition = new Vector2(x, 0);
            var split = root.GetComponent<GlobalStyleSplit>();
            split.Configure(area, 0.447f, 0.563f);
            var surface = go.GetComponent<HybridStyleGraphic>();
            surface.Configure(split, hd, Color.white, Color.white, Color.clear, 0, 8);
            surface.SetRightSprite(flat);
            Assert.AreSame(flat.texture, surface.material.GetTexture("_RightTex"));
            using (var mesh = new VertexHelper())
            {
                typeof(HybridStyleGraphic).GetMethod("OnPopulateMesh", BindingFlags.Instance | BindingFlags.NonPublic,
                    null, new[] { typeof(VertexHelper) }, null)
                    .Invoke(surface, new object[] { mesh });
                bool left = false, right = false;
                for (int i = 0; i < mesh.currentVertCount; i++)
                {
                    var vertex = UIVertex.simpleVert;
                    mesh.PopulateUIVertex(ref vertex, i);
                    left |= vertex.uv1.z == 0;
                    right |= vertex.uv1.z == 2;
                }
                Assert.AreEqual(hasLeft, left);
                Assert.AreEqual(hasRight, right);
                Assert.AreEqual(1, go.GetComponents<Graphic>().Length);
            }
        }
        finally
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(hd);
            Object.DestroyImmediate(flat);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void DragDoesNotClickOrChooseAndOwnedReturnsToDock(bool candidate)
    {
        var root = new GameObject("Root", typeof(RectTransform), typeof(GlobalStyleSplit));
        var events = new GameObject("Events", typeof(EventSystem));
        Sprite hd = KaitSunlitTheme.Load("PassiveCardHD"), flat = KaitSunlitTheme.Load("PassiveCardFlat");
        try
        {
            var area = root.GetComponent<RectTransform>();
            area.sizeDelta = new Vector2(1920, 1080);
            var split = root.GetComponent<GlobalStyleSplit>();
            split.Configure(area, 0.447f, 0.563f);
            int choices = 0, docks = 0;
            var card = KaitPassiveCard.Create(area, split, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),
                hd, flat, c => choices++, (c, x) => { docks++; c.SetDock(x); });
            card.Show(KaitPassive.BirdEye, candidate, Vector2.zero, 0);
            var data = new PointerEventData(events.GetComponent<EventSystem>())
                { pointerId = -1, button = PointerEventData.InputButton.Left, position = Vector2.zero };
            card.OnPointerDown(data);
            card.OnBeginDrag(data);
            data.position = new Vector2(300, 150);
            card.OnDrag(data);
            card.OnPointerUp(data);
            card.OnEndDrag(data);
            card.OnPointerClick(data);
            Assert.IsFalse(card.IsDragging);
            Assert.IsTrue(card.SuppressedClick);
            Assert.AreEqual(0, choices);
            Assert.AreEqual(candidate ? 0 : 1, docks);
            card.OnPointerDown(data);
            card.OnPointerUp(data);
            card.OnPointerClick(data);
            Assert.AreEqual(candidate ? 1 : 0, choices);
        }
        finally
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(events);
            Object.DestroyImmediate(hd);
            Object.DestroyImmediate(flat);
        }
    }

    [Test]
    public void SeamOutsideCardKeepsOutsideCoordinatesForShaderClipping()
    {
        var go = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(HybridStyleGraphic));
        try
        {
            go.GetComponent<RectTransform>().sizeDelta = KaitPassiveCard.Size;
            var surface = go.GetComponent<HybridStyleGraphic>();
            surface.Configure(null, null, Color.white, Color.white, Color.white, 3, 8);
            surface.SetFallbackSplit(2, 3);
            using (var mesh = new VertexHelper())
            {
                typeof(HybridStyleGraphic).GetMethod("OnPopulateMesh", BindingFlags.Instance | BindingFlags.NonPublic,
                    null, new[] { typeof(VertexHelper) }, null).Invoke(surface, new object[] { mesh });
                int seamVertices = 0;
                for (int i = 0; i < mesh.currentVertCount; i++)
                {
                    var vertex = UIVertex.simpleVert;
                    mesh.PopulateUIVertex(ref vertex, i);
                    if (vertex.uv1.z != 1) continue;
                    seamVertices++;
                    Assert.Greater(vertex.uv1.x, 1f, "An off-card seam must be clipped, not clamped onto the card edge.");
                }
                Assert.AreEqual(4, seamVertices);
            }
        }
        finally { Object.DestroyImmediate(go); }
    }

    [Test]
    public void DeckShowsOnlyOwnedCardsResetsAndDoesNotAffectTurns()
    {
        var root = new GameObject("Deck", typeof(RectTransform), typeof(GlobalStyleSplit), typeof(KaitPassiveDeck));
        try
        {
            var area = root.GetComponent<RectTransform>();
            area.sizeDelta = new Vector2(1920, 1080);
            var split = root.GetComponent<GlobalStyleSplit>();
            split.Configure(area, 0.447f, 0.563f);
            var deck = root.GetComponent<KaitPassiveDeck>();
            deck.Initialize(area, split, Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), i => {});
            var run = new KaitRun();
            run.Reset(42);
            run.passives.Add(KaitPassive.BirdEye);
            int turn = run.turn;
            deck.Sync(run);
            Assert.IsTrue(deck.Owned[0].gameObject.activeSelf);
            Assert.IsFalse(deck.Owned[1].gameObject.activeSelf);
            Assert.AreEqual(turn, run.turn);
            deck.ResetDeck();
            foreach (var card in deck.Owned) Assert.IsFalse(card.gameObject.activeSelf);
        }
        finally { Object.DestroyImmediate(root); }
    }
}
