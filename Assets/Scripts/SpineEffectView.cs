using Spine.Unity;
using UnityEngine;

public sealed class SpineEffectView
{
    private static Material sharedGraphicMaterial;
    private static Material sharedHybridGraphicMaterial;
    private readonly SkeletonGraphic graphic;
    private readonly RectTransform root;
    private readonly RectTransform skeletonRect;
    private readonly string animationName;

    public RectTransform Root => root;
    public bool IsReady => graphic != null && graphic.Skeleton != null && graphic.AnimationState != null;
    public float Duration { get; private set; }

    private SpineEffectView(RectTransform root, SkeletonGraphic graphic, RectTransform skeletonRect, string animationName)
    {
        this.root = root;
        this.graphic = graphic;
        this.skeletonRect = skeletonRect;
        this.animationName = animationName;
    }

    public static SpineEffectView Create(SkeletonDataAsset data, Transform parent, Vector2 size,
        string animationName, string name, float playbackSpeed = 1f, float fit = 0.94f,
        bool hybridStyle = true)
    {
        if (data == null || parent == null || string.IsNullOrEmpty(animationName)) return null;

        var host = new GameObject(name, typeof(RectTransform));
        host.transform.SetParent(parent, false);
        RectTransform hostRect = host.GetComponent<RectTransform>();
        hostRect.anchorMin = hostRect.anchorMax = hostRect.pivot = new Vector2(0.5f, 0.5f);
        hostRect.sizeDelta = size;
        hostRect.anchoredPosition = Vector2.zero;

        Material graphicMaterial = hybridStyle ? HybridMaterial() : DefaultMaterial();
        if (graphicMaterial == null)
        {
            Object.Destroy(host);
            return null;
        }

        SkeletonGraphic skeletonGraphic = SkeletonGraphic.NewSkeletonGraphicGameObject(data, hostRect, graphicMaterial);
        skeletonGraphic.name = name + " Skeleton";
        skeletonGraphic.raycastTarget = false;
        skeletonGraphic.unscaledTime = true;
        skeletonGraphic.timeScale = Mathf.Max(0.01f, playbackSpeed);
        skeletonGraphic.Initialize(false);
        if (skeletonGraphic.Skeleton == null || skeletonGraphic.AnimationState == null ||
            skeletonGraphic.Skeleton.Data.FindAnimation(animationName) == null)
        {
            Object.Destroy(host);
            return null;
        }

        Spine.Animation animation = skeletonGraphic.Skeleton.Data.FindAnimation(animationName);
        skeletonGraphic.AnimationState.SetAnimation(0, animationName, false);
        skeletonGraphic.Update(Mathf.Min(0.35f, animation.Duration * 0.45f));
        skeletonGraphic.MatchRectTransformWithBounds();

        RectTransform graphicRect = skeletonGraphic.rectTransform;
        Mesh mesh = skeletonGraphic.GetLastMesh();
        Bounds meshBounds = mesh != null ? mesh.bounds : new Bounds(Vector3.zero, graphicRect.sizeDelta);
        graphicRect.anchorMin = graphicRect.anchorMax = graphicRect.pivot = new Vector2(0.5f, 0.5f);
        graphicRect.sizeDelta = meshBounds.size;
        float width = Mathf.Max(0.01f, meshBounds.size.x);
        float height = Mathf.Max(0.01f, meshBounds.size.y);
        float scale = Mathf.Min(size.x * fit / width, size.y * fit / height);
        graphicRect.localScale = Vector3.one * scale;
        graphicRect.anchoredPosition = -new Vector2(meshBounds.center.x, meshBounds.center.y) * scale;
        skeletonGraphic.AnimationState.SetAnimation(0, animationName, false);
        skeletonGraphic.Update(0f);

        var view = new SpineEffectView(hostRect, skeletonGraphic, graphicRect, animationName)
        {
            Duration = animation.Duration / Mathf.Max(0.01f, playbackSpeed)
        };
        return view;
    }

    private static Material DefaultMaterial()
    {
        if (sharedGraphicMaterial != null) return sharedGraphicMaterial;
        sharedGraphicMaterial = Resources.Load<Material>("Characters/Makoto/KaitSkeletonGraphic");
        if (sharedGraphicMaterial != null) return sharedGraphicMaterial;
        Shader shader = Shader.Find("Spine/SkeletonGraphic");
        if (shader != null) sharedGraphicMaterial = new Material(shader) { name = "Spine Effect UI Material" };
        return sharedGraphicMaterial;
    }

    private static Material HybridMaterial()
    {
        if (sharedHybridGraphicMaterial != null) return sharedHybridGraphicMaterial;
        Shader shader = Resources.Load<Shader>("Shaders/SpineEffectHybrid");
        if (shader == null) shader = Shader.Find("Spine/SkeletonGraphic Hybrid Effect");
        if (shader == null) return DefaultMaterial();
        sharedHybridGraphicMaterial = new Material(shader) { name = "Spine Hybrid Effect UI Material" };
        sharedHybridGraphicMaterial.SetFloat("_SplitBottom", 0.447f);
        sharedHybridGraphicMaterial.SetFloat("_SplitTop", 0.563f);
        sharedHybridGraphicMaterial.SetFloat("_PixelSize", 3f);
        return sharedHybridGraphicMaterial;
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
            if (current == null || current.Animation == null || current.Animation.Name != animationName)
                graphic.AnimationState.SetAnimation(0, animationName, false);
        }
    }

    public void SetTint(Color color)
    {
        if (graphic != null) graphic.color = color;
    }

    public void SetRotation(float degrees)
    {
        if (root != null) root.localRotation = Quaternion.Euler(0f, 0f, degrees);
    }

    public void SetScale(float scale)
    {
        if (root != null) root.localScale = Vector3.one * scale;
    }

    public void Destroy()
    {
        if (root != null) Object.Destroy(root.gameObject);
    }
}
