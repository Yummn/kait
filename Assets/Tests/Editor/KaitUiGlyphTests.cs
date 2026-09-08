using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public sealed class KaitUiGlyphTests
{
    [Test]
    public void EverySymbolHasVisibleBoundedGeometryWithoutRaycast()
    {
        var root=new GameObject("Glyph Test",typeof(RectTransform));
        try
        {
            foreach(KaitUiGlyph.Symbol symbol in Enum.GetValues(typeof(KaitUiGlyph.Symbol)))
            {
                var g=KaitUiGlyph.Create(root.transform,symbol,Vector2.zero,32);
                using(var vh=new VertexHelper())
                {
                    typeof(KaitUiGlyph).GetMethod("OnPopulateMesh",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.DeclaredOnly).Invoke(g,new object[]{vh});
                    Assert.Greater(vh.currentVertCount,0,symbol.ToString());
                    var v=new UIVertex();
                    for(int i=0;i<vh.currentVertCount;i++){vh.PopulateUIVertex(ref v,i);Assert.LessOrEqual(Mathf.Abs(v.position.x),16);Assert.LessOrEqual(Mathf.Abs(v.position.y),16);}
                }
                Assert.IsFalse(g.raycastTarget);UnityEngine.Object.DestroyImmediate(g.gameObject);
            }
        }
        finally{UnityEngine.Object.DestroyImmediate(root);}
    }
}
