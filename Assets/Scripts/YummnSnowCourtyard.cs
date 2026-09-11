using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Ground, playable paving and frost are separate: no screenshot is baked
// underneath the units, and the gameplay grid remains exactly 5 by 5.
public static class YummnSnowCourtyard
{
    public const string ResourceRoot = "KaitVisuals/Yummn/SnowCourtyard/";
    private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
    private static Material snowMaterial;
    private static Material stoneMaterial;

    public static Material StoneMaterial
    {
        get
        {
            if(stoneMaterial==null)
            {
                var shader=Resources.Load<Shader>("Shaders/UISnowCoverage");
                if(shader==null)return null;
                stoneMaterial=new Material(shader){name="Yummn Slate Cutout",hideFlags=HideFlags.HideAndDontSave};
                stoneMaterial.SetFloat("_UsePaintedColor",1);
            }
            return stoneMaterial;
        }
    }

    public static Sprite Load(string name)
    {
        if (Sprites.TryGetValue(name, out var sprite) && sprite != null) return sprite;
        var texture = Resources.Load<Texture2D>(ResourceRoot + name);
        if (texture == null) return null;
        return Sprites[name] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
            Vector2.one * .5f, 100, 0, SpriteMeshType.FullRect);
    }

    public static GameObject CreateSnowEdges(Transform cell, int x, int y)
    {
        if (x != 1 && x != 5 && y != 1 && y != 5) return null;
        var texture = Resources.Load<Texture2D>(ResourceRoot + "SnowMask");
        var shader = Resources.Load<Shader>("Shaders/UISnowCoverage");
        if (texture == null || shader == null) return null;
        if (snowMaterial == null) snowMaterial = new Material(shader)
            { name = "Yummn Painted Snow Coverage", hideFlags = HideFlags.HideAndDontSave };
        var root = new GameObject("Snow Edge Integration", typeof(RectTransform));
        root.transform.SetParent(cell, false);
        // Above the paving and below obstacle, warning, rift and actor layers.
        root.transform.SetSiblingIndex(2);
        if (x == 1) AddBand(root.transform, texture, new Vector2(-60, 0), 90, x + y);
        if (x == 5) AddBand(root.transform, texture, new Vector2(60, 0), 90, x + y + 1);
        if (y == 1) AddBand(root.transform, texture, new Vector2(0, -60), 0, x + y);
        if (y == 5) AddBand(root.transform, texture, new Vector2(0, 60), 0, x + y + 1);
        return root;
    }

    private static void AddBand(Transform parent, Texture texture, Vector2 position, float angle, int variant)
    {
        // Only the middle ~18% of the authored coverage is snow. Its canvas
        // is intentionally taller than the visible strip, avoiding a hard cut.
        for (int layer = 0; layer < 2; layer++)
        {
            var go = new GameObject(layer == 0 ? "Frost Contact" : "Painted Snow", typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<RawImage>();
            image.texture = texture; image.material = snowMaterial;
            image.raycastTarget = false; image.maskable = false;
            image.color = layer == 0 ? new Color(.63f, .74f, .83f, .45f) : new Color(.88f, .94f, .98f, .82f + variant % 3 * .06f);
            image.uvRect = new Rect(variant % 2, (variant / 2) % 2,
                variant % 2 == 0 ? 1 : -1, (variant / 2) % 2 == 0 ? 1 : -1);
            // Leave breaks between uneven patches instead of drawing a white frame.
            image.rectTransform.sizeDelta = new Vector2(102 + variant % 3 * 9, 78 + variant % 3 * 10);
            image.rectTransform.anchoredPosition = position + (layer == 0 ? new Vector2(0, -1.1f) : Vector2.zero);
            image.rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public static void SetWallGrounding(Image obstacle, KaitSoftShadow shadow, bool snowTheme)
    {
        obstacle.material = snowTheme ? StoneMaterial : null;
        obstacle.rectTransform.sizeDelta = Vector2.one * (snowTheme ? 114 : 96);
        shadow.rectTransform.sizeDelta = Vector2.one * (snowTheme ? 100 : 105);
        shadow.rectTransform.anchoredPosition = snowTheme ? new Vector2(1.5f, -2) : new Vector2(3, -4);
        shadow.Shape(snowTheme ? 4 : 3, snowTheme ? 3 : 5);
        shadow.color = snowTheme ? new Color(.12f, .17f, .23f, .34f) : new Color(.12f, .18f, .15f, .26f);
    }
}
