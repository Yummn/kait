using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public sealed class YummnSnowCourtyardTests
{
    [TestCase("Ground")]
    [TestCase("Floor")]
    [TestCase("Pillar")]
    [TestCase("SnowMask")]
    public void AllSnowAssetsArePackagedWithoutTextureCompression(string name)
    {
        var texture=Resources.Load<Texture2D>(YummnSnowCourtyard.ResourceRoot+name);
        Assert.NotNull(texture);
        var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
        Assert.IsTrue(importer.mipmapEnabled);
        Assert.AreEqual(TextureImporterCompression.Uncompressed,importer.textureCompression);
        Assert.AreEqual(TextureImporterNPOTScale.None,importer.npotScale);
        Assert.AreEqual(FilterMode.Trilinear,importer.filterMode);
        Assert.AreEqual(name!="SnowMask",importer.sRGBTexture);
    }

    [Test] public void OnlySixteenPerimeterCellsGetTwentySnowEdges()
    {
        var root=new GameObject("Grid",typeof(RectTransform));
        try
        {
            int decorated=0,bands=0;
            for(int x=1;x<=5;x++)for(int y=1;y<=5;y++)
            {
                var cell=new GameObject("Cell",typeof(RectTransform));cell.transform.SetParent(root.transform,false);
                var edge=YummnSnowCourtyard.CreateSnowEdges(cell.transform,x,y);
                if(x>1&&x<5&&y>1&&y<5){Assert.IsNull(edge);continue;}
                Assert.NotNull(edge);decorated++;
                foreach(var image in edge.GetComponentsInChildren<RawImage>())
                {
                    Assert.IsFalse(image.raycastTarget);
                    Assert.IsFalse(image.maskable);
                    Assert.NotNull(image.texture);
                    Assert.AreEqual("UI/Yummn Snow Coverage",image.material.shader.name);
                    if(image.name=="Painted Snow")bands++;
                }
            }
            Assert.AreEqual(16,decorated);Assert.AreEqual(20,bands);
        }
        finally{Object.DestroyImmediate(root);}
    }

    [Test] public void GroundingRestoresOriginalKaitWallGeometry()
    {
        var root=new GameObject("Root",typeof(RectTransform));
        try
        {
            var obstacle=new GameObject("Stone",typeof(RectTransform),typeof(Image)).GetComponent<Image>();
            obstacle.transform.SetParent(root.transform,false);
            var shadow=KaitSoftShadow.Create(root.transform,"Contact");
            YummnSnowCourtyard.SetWallGrounding(obstacle,shadow,true);
            Assert.AreSame(YummnSnowCourtyard.StoneMaterial,obstacle.material);
            Assert.AreEqual(1f,obstacle.material.GetFloat("_UsePaintedColor"));
            Assert.AreEqual(Vector2.one*114,obstacle.rectTransform.sizeDelta);
            Assert.Less(shadow.rectTransform.anchoredPosition.magnitude,3);
            YummnSnowCourtyard.SetWallGrounding(obstacle,shadow,false);
            Assert.AreSame(obstacle.defaultMaterial,obstacle.material);
            Assert.AreEqual(Vector2.one*96,obstacle.rectTransform.sizeDelta);
            Assert.AreEqual(Vector2.one*105,shadow.rectTransform.sizeDelta);
            Assert.AreEqual(new Vector2(3,-4),shadow.rectTransform.anchoredPosition);
            Assert.AreEqual(new Color(.12f,.18f,.15f,.26f),shadow.color);
        }
        finally{Object.DestroyImmediate(root);}
    }

    [Test] public void DecorativeSnowDoesNotChangeCollisionCoordinates()
    {
        var run=new KaitRun(new KaitBalanceConfig());run.SelectCharacter(KaitCharacter.Yummn,77,new YummnRulesSnapshot());
        Assert.IsTrue(run.walls[1,5]);Assert.IsTrue(run.walls[5,1]);
        int walls=0;for(int x=1;x<=5;x++)for(int y=1;y<=5;y++)if(run.walls[x,y])walls++;
        Assert.AreEqual(2,walls);
    }
}
