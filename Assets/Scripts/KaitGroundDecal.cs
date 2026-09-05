using UnityEngine;
using UnityEngine.UI;

// A single authored shadow field sampled by grass and every pavement cell.
// White in the authored mask is empty; black is coverage, not a painted tile.
[RequireComponent(typeof(RawImage))]
public sealed class KaitGroundDecal : MonoBehaviour
{
    private static Material sharedMaterial;
    private static Material alphaMaterial;
    private RawImage image;
    private RectTransform mapping;

    public static Material MaskMaterial
    {
        get
        {
            if (sharedMaterial == null)
            {
                var shader = Resources.Load<Shader>("Shaders/UIGroundMask");
                if (shader != null) sharedMaterial = new Material(shader)
                    { name = "Ground Mask UI", hideFlags = HideFlags.HideAndDontSave };
            }
            return sharedMaterial;
        }
    }

    public static Material AlphaMaterial
    {
        get
        {
            if(alphaMaterial == null && MaskMaterial != null)
            {
                alphaMaterial = new Material(MaskMaterial) { name = "Live Ground Shadow UI", hideFlags = HideFlags.HideAndDontSave };
                alphaMaterial.SetFloat("_UseAlpha",1);
            }
            return alphaMaterial;
        }
    }

    public static KaitGroundDecal Create(Transform parent, RectTransform mapping, Texture texture, Color tint, string name, bool alphaCoverage = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(KaitGroundDecal));
        go.transform.SetParent(parent, false);
        var decal = go.GetComponent<KaitGroundDecal>();
        decal.mapping = mapping;
        decal.image = go.GetComponent<RawImage>();
        decal.image.texture = texture;
        decal.image.material = alphaCoverage ? AlphaMaterial : MaskMaterial;
        decal.image.color = tint;
        decal.image.raycastTarget = false;
        var rect = decal.image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        decal.UpdateMapping();
        return decal;
    }

    private void LateUpdate() => UpdateMapping();

    private void UpdateMapping()
    {
        if (image == null || mapping == null) return;
        Rect uv = CalculateUvRect(image.rectTransform, mapping);
        if (image.uvRect != uv) image.uvRect = uv;
    }

    public static Rect CalculateUvRect(RectTransform target, RectTransform source)
    {
        Rect r = target.rect, s = source.rect;
        if (s.width <= 0 || s.height <= 0) return new Rect(0, 0, 1, 1);
        Vector3 min = source.InverseTransformPoint(target.TransformPoint(new Vector3(r.xMin, r.yMin)));
        Vector3 max = source.InverseTransformPoint(target.TransformPoint(new Vector3(r.xMax, r.yMax)));
        return new Rect((min.x - s.xMin) / s.width, (min.y - s.yMin) / s.height,
            (max.x - min.x) / s.width, (max.y - min.y) / s.height);
    }
}
