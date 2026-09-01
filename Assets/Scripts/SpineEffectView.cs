using Spine.Unity;
using UnityEngine;

public sealed class SpineEffectView
{
    private static Material sharedGraphicMaterial;
    private readonly SkeletonGraphic graphic;
    private readonly RectTransform root;
    private readonly RectTransform skeletonRect;
    private readonly string animationName;

    public RectTransform Root => root;
    public bool IsReady => graphic != null && graphic.Skeleton != null && graphic.AnimationState != null;

    private SpineEffectView(RectTransform root, SkeletonGraphic graphic, RectTransform skeletonRect, string animationName)
    {
        this.root = root;
        this.graphic = graphic;
        this.skeletonRect = skeletonRect;
        this.animationName = animationName;
    }

    public static SpineEffectView Create(SkeletonDataAsset data, Transform parent, Vector2 size, string animationName, string name)
    {
        if (data == null || parent == null || string.IsNullOrEmpty(animationName)) return null;

        var host = new GameObject(name, typeof(RectTransform));
        host.transform.SetParent(parent, false);
        RectTransform hostRect = host.GetComponent<RectTransform>();
        hostRect.anchorMin = hostRect.anchorMax = hostRect.pivot = new Vector2(0.5f, 0.5f);
        hostRect.sizeDelta = size;
        hostRect.anchoredPosition = Vector2.zero;

        if (sharedGraphicMaterial == null)
            sharedGraphicMaterial = Resources.Load<Material>("Characters/Makoto/KaitSkeletonGraphic");
        if (sharedGraphicMaterial == null)
        {
            Shader shader = Shader.Find("Spine/SkeletonGraphic");
            if (shader == null)
            {
                Object.Destroy(host);
                return null;
            }
            sharedGraphicMaterial = new Material(shader) { name = "Spine Effect UI Material" };
        }

        SkeletonGraphic skeletonGraphic = SkeletonGraphic.NewSkeletonGraphicGameObject(data, hostRect, sharedGraphicMaterial);
        skeletonGraphic.name = name + " Skeleton";
        skeletonGraphic.raycastTarget = false;
        skeletonGraphic.unscaledTime = true;
        skeletonGraphic.timeScale = 1f;
        skeletonGraphic.Initialize(false);
        if (skeletonGraphic.Skeleton == null || skeletonGraphic.AnimationState == null ||
            skeletonGraphic.Skeleton.Data.FindAnimation(animationName) == null)
        {
            Object.Destroy(host);
            return null;
        }

        skeletonGraphic.AnimationState.SetAnimation(0, animationName, true);
        skeletonGraphic.Update(0.35f);
        skeletonGraphic.MatchRectTransformWithBounds();

        RectTransform graphicRect = skeletonGraphic.rectTransform;
        Mesh mesh = skeletonGraphic.GetLastMesh();
        Bounds meshBounds = mesh != null ? mesh.bounds : new Bounds(Vector3.zero, graphicRect.sizeDelta);
        graphicRect.anchorMin = graphicRect.anchorMax = graphicRect.pivot = new Vector2(0.5f, 0.5f);
        graphicRect.sizeDelta = meshBounds.size;
        float width = Mathf.Max(0.01f, meshBounds.size.x);
        float height = Mathf.Max(0.01f, meshBounds.size.y);
        float scale = Mathf.Min(size.x * 0.94f / width, size.y * 0.94f / height);
        graphicRect.localScale = Vector3.one * scale;
        graphicRect.anchoredPosition = -new Vector2(meshBounds.center.x, meshBounds.center.y) * scale;
        skeletonGraphic.AnimationState.SetAnimation(0, animationName, true);
        skeletonGraphic.Update(0f);

        return new SpineEffectView(hostRect, skeletonGraphic, graphicRect, animationName);
    }

    public void SetParent(Transform parent, int siblingIndex = -1)
    {
        if (root == null || parent == null) return;
        root.SetParent(parent, false);
        root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
        root.anchoredPosition = Vector2.zero;
        root.localScale = Vector3.one;
        if (siblingIndex >= 0) root.SetSiblingIndex(Mathf.Min(siblingIndex, parent.childCount - 1));
    }

    public void SetVisible(bool visible)
    {
        if (root == null) return;
        if (root.gameObject.activeSelf != visible) root.gameObject.SetActive(visible);
        if (visible)
        {
            var current = graphic.AnimationState.GetCurrent(0);
            if (current == null || current.Animation == null || current.Animation.Name != animationName || !current.Loop)
                graphic.AnimationState.SetAnimation(0, animationName, true);
        }
    }

    public void Destroy()
    {
        if (root != null) Object.Destroy(root.gameObject);
    }
}
