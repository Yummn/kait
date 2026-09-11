using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class DangerStyleSplitTests
{
    [TestCase(1920,1080)]
    [TestCase(2400,1080)]
    [TestCase(1080,2400)]
    public void DangerUsesSharedCutAndPreservesColor(int width, int height)
    {
        var root = new GameObject("Viewport", typeof(RectTransform), typeof(Canvas), typeof(GlobalStyleSplit));
        try
        {
            var space = root.GetComponent<RectTransform>(); space.sizeDelta = new Vector2(width,height);
            var split = root.GetComponent<GlobalStyleSplit>(); split.Configure(space,.45f,.57f);
            var go = new GameObject("Atmosphere",typeof(RectTransform),typeof(KaitAtmosphereGraphic));
            go.transform.SetParent(root.transform,false);
            var atmosphere = go.GetComponent<KaitAtmosphereGraphic>();
            atmosphere.rectTransform.sizeDelta = space.sizeDelta;
            atmosphere.ConfigureSplit(split); atmosphere.SetState(0,1,0);
            foreach (bool left in new[]{true,false})
            {
                var graphic = go.transform.Find(left ? "Danger C" : "Minimal Danger").GetComponent<Graphic>();
                Assert.IsFalse(graphic.raycastTarget);
                var rect = graphic.rectTransform.rect;
                using(var mesh = new VertexHelper())
                {
                    Color32 ink = new Color(.84f,.22f,.24f,.48f);
                    mesh.AddVert(new Vector2(rect.xMin,rect.yMin),ink,Vector2.zero);
                    mesh.AddVert(new Vector2(rect.xMin,rect.yMax),ink,Vector2.zero);
                    mesh.AddVert(new Vector2(rect.xMax,rect.yMax),ink,Vector2.one);
                    mesh.AddVert(new Vector2(rect.xMax,rect.yMin),ink,Vector2.one);
                    mesh.AddTriangle(0,1,2); mesh.AddTriangle(0,2,3);
                    graphic.GetComponent<SunlitSplitText>().ModifyMesh(mesh);
                    Assert.Greater(mesh.currentVertCount,0);
                    split.GetLocalSplits(graphic.rectTransform,out float bottom,out float top);
                    for(int i=0;i<mesh.currentVertCount;i++)
                    {
                        UIVertex v = new UIVertex(); mesh.PopulateUIVertex(ref v,i);
                        float line = rect.xMin+rect.width*Mathf.Lerp(bottom,top,Mathf.InverseLerp(rect.yMin,rect.yMax,v.position.y));
                        Assert.LessOrEqual((v.position.x-line)*(left?1:-1),.01f);
                        Assert.AreEqual(ink,v.color);
                    }
                }
            }
            atmosphere.SetState(0,0,1);
            Assert.AreEqual(0,go.transform.Find("Minimal Danger").GetComponent<Graphic>().color.a);
            Assert.AreEqual(0,go.transform.Find("Danger C").GetComponent<Graphic>().color.a);
        }
        finally { Object.DestroyImmediate(root); }
    }
}
