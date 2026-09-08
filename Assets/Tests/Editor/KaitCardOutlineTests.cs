using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public sealed class KaitCardOutlineTests
{
    private GameObject root;
    private RectTransform area;
    private GlobalStyleSplit split;
    [SetUp]public void Setup()
    {
        root=new GameObject("Outline test",typeof(RectTransform));area=root.GetComponent<RectTransform>();area.sizeDelta=new Vector2(1920,1080);
        split=root.AddComponent<GlobalStyleSplit>();split.Configure(area,.45f,.55f);
    }
    [TearDown]public void Cleanup()=>Object.DestroyImmediate(root);
    private KaitCardOutline Card(bool passive,float x)
    {
        var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        GameObject card;
        if(passive){var c=KaitPassiveCard.Create(area,split,font,null,null,null,null);c.Show(KaitPassive.BirdEye,true,new Vector2(x,0),0);card=c.gameObject;}
        else{var c=KaitSkillCard.Create(area,split,font,null,null,null,null,null);c.Show(KaitSkill.HexCurse,true,new Vector2(x,0),0);card=c.gameObject;}
        return card.GetComponentInChildren<KaitCardOutline>();
    }
    private static void Populate(KaitCardOutline border,VertexHelper vh)=>typeof(KaitCardOutline).GetMethod("OnPopulateMesh",BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly).Invoke(border,new object[]{vh});
    [TestCase(false,-400,false)][TestCase(true,-400,false)]
    [TestCase(false,400,true)][TestCase(true,400,true)]
    public void FlatCardOutlineIsAbsentOnCartoonSide(bool passive,float x,bool visible)
    {
        var border=Card(passive,x);using(var vh=new VertexHelper())
        {Populate(border,vh);Assert.AreEqual(visible,vh.currentVertCount>0);}
        Assert.IsFalse(border.raycastTarget);
    }
    [TestCase(false,0f,1f)][TestCase(true,0f,1.18f)]
    [TestCase(false,12f,1.18f)][TestCase(true,-12f,.85f)]
    public void DiagonalCutClipsEveryBorderVertexEvenWithScaleAndRotation(bool passive,float rotation,float scale)
    {
        var border=Card(passive,0);border.transform.parent.localScale=Vector3.one*scale;
        border.transform.parent.localRotation=Quaternion.Euler(0,0,rotation);
        using(var vh=new VertexHelper())
        {
            Populate(border,vh);Assert.Greater(vh.currentVertCount,0);
            for(int i=0;i<vh.currentVertCount;i++)
            {
                var vertex=new UIVertex();vh.PopulateUIVertex(ref vertex,i);
                var point=area.InverseTransformPoint(border.transform.TransformPoint(vertex.position));
                Assert.GreaterOrEqual(point.x-point.y*(192f/1080),-.002f);
            }
        }
    }
    [Test]public void SameCardCanCrossBackAndForthWithoutKeepingTheOldBorder()
    {
        var border=Card(false,-400);
        using(var vh=new VertexHelper())
        {
            Populate(border,vh);Assert.AreEqual(0,vh.currentVertCount);
            ((RectTransform)border.transform.parent).anchoredPosition=new Vector2(400,0);
            Populate(border,vh);Assert.Greater(vh.currentVertCount,0);
            ((RectTransform)border.transform.parent).anchoredPosition=new Vector2(-400,0);
            Populate(border,vh);Assert.AreEqual(0,vh.currentVertCount);
        }
    }
    [Test]public void DropTargetOutlineIsNotStyleClipped()
    {
        var go=new GameObject("Target",typeof(RectTransform),typeof(KaitCardOutline));go.transform.SetParent(area,false);
        var border=go.GetComponent<KaitCardOutline>();border.rectTransform.sizeDelta=new Vector2(236,164);border.rectTransform.anchoredPosition=new Vector2(-400,0);
        using(var vh=new VertexHelper()){Populate(border,vh);Assert.Greater(vh.currentVertCount,0);}
    }
}
