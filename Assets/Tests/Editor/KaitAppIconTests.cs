using NUnit.Framework;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

public class KaitAppIconTests
{
    [Test] public void PortraitIsSquareAndHighResolution()
    {
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(KaitAppIconSettings.PortraitPath);
        Assert.NotNull(texture);
        Assert.AreEqual(texture.width,texture.height);
        Assert.GreaterOrEqual(texture.width,1024);
    }

    [Test] public void AdaptiveInsetPreservesResourceAndBackground()
    {
        var doc=XDocument.Parse("<adaptive-icon xmlns:android='http://schemas.android.com/apk/res/android'><background android:drawable='@mipmap/bg'/><foreground android:drawable='@mipmap/fg'/></adaptive-icon>");
        XNamespace android="http://schemas.android.com/apk/res/android";
        Assert.IsTrue(KaitAdaptiveIconInset.InsetForeground(doc));
        Assert.AreEqual("@mipmap/bg",(string)doc.Root.Element("background").Attribute(android+"drawable"));
        var inset=doc.Root.Element("foreground").Element("inset");
        Assert.AreEqual("@mipmap/fg",(string)inset.Attribute(android+"drawable"));
        Assert.AreEqual("16.6667%",(string)inset.Attribute(android+"inset"));
        Assert.IsFalse(KaitAdaptiveIconInset.InsetForeground(doc));
    }
}
