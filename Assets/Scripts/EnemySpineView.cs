using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public sealed class EnemySpineView
{
    private const float VisualFill = 0.88f;
    public const string LandingSuffix = "landing";
    public const string IdleSuffix = "idle";
    public const string AttackSuffix = "attack";
    public const string DamageSuffix = "damage";

    private static Material sharedGraphicMaterial;
    private readonly SkeletonGraphic graphic;
    private readonly RectTransform root;
    private readonly RectTransform skeletonRect;
    private readonly float rightFacingVisualX;
    private readonly string prefix;

    public RectTransform Root => root;
    public bool IsReady => graphic != null && graphic.Skeleton != null && graphic.AnimationState != null;

    private EnemySpineView(RectTransform root, SkeletonGraphic graphic, RectTransform skeletonRect, float rightFacingVisualX, string prefix)
    {
        this.root = root;
        this.graphic = graphic;
        this.skeletonRect = skeletonRect;
        this.rightFacingVisualX = rightFacingVisualX;
        this.prefix = prefix;
    }

    public static EnemySpineView Create(SkeletonDataAsset data, string animationPrefix, Transform parent, Vector2 size, string name)
    {
        if (data == null || parent == null || string.IsNullOrEmpty(animationPrefix)) return null;

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
            sharedGraphicMaterial = new Material(shader) { name = "Enemy Spine UI Material" };
        }

        SkeletonGraphic skeletonGraphic = SkeletonGraphic.NewSkeletonGraphicGameObject(data, hostRect, sharedGraphicMaterial);
        skeletonGraphic.name = name + " Skeleton";
        skeletonGraphic.raycastTarget = false;
        skeletonGraphic.unscaledTime = true;
        skeletonGraphic.timeScale = 1f;
        skeletonGraphic.Initialize(false);
        if (skeletonGraphic.Skeleton == null || skeletonGraphic.AnimationState == null)
        {
            Object.Destroy(host);
            return null;
        }

        string idle = animationPrefix + IdleSuffix;
        skeletonGraphic.AnimationState.SetAnimation(0, idle, true);
        skeletonGraphic.Update(0f);
        skeletonGraphic.MatchRectTransformWithBounds();

        RectTransform skeletonRect = skeletonGraphic.rectTransform;
        Mesh mesh = skeletonGraphic.GetLastMesh();
        Bounds meshBounds = mesh != null ? mesh.bounds : new Bounds(Vector3.zero, skeletonRect.sizeDelta);
        skeletonRect.anchorMin = skeletonRect.anchorMax = new Vector2(0.5f, 0.5f);
        skeletonRect.pivot = new Vector2(0.5f, 0.5f);
        skeletonRect.sizeDelta = meshBounds.size;
        float width = Mathf.Max(0.01f, meshBounds.size.x);
        float height = Mathf.Max(0.01f, meshBounds.size.y);
        float scale = Mathf.Min(size.x * VisualFill / width, size.y * VisualFill / height);
        skeletonRect.localScale = Vector3.one * scale;
        Vector2 centeredPosition = new Vector2(-meshBounds.center.x * scale, -meshBounds.center.y * scale);
        skeletonRect.anchoredPosition = centeredPosition;

        return new EnemySpineView(hostRect, skeletonGraphic, skeletonRect, centeredPosition.x, animationPrefix);
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
        if (root != null && root.gameObject.activeSelf != visible) root.gameObject.SetActive(visible);
    }

    public void Face(Vector2Int direction)
    {
        if (!IsReady) return;
        if (direction.x < 0)
        {
            graphic.Skeleton.ScaleX = -Mathf.Abs(graphic.Skeleton.ScaleX);
            skeletonRect.anchoredPosition = new Vector2(-rightFacingVisualX, skeletonRect.anchoredPosition.y);
        }
        else if (direction.x > 0)
        {
            graphic.Skeleton.ScaleX = Mathf.Abs(graphic.Skeleton.ScaleX);
            skeletonRect.anchoredPosition = new Vector2(rightFacingVisualX, skeletonRect.anchoredPosition.y);
        }
    }

    public void SetTint(Color color)
    {
        if (graphic != null) graphic.color = color;
    }

    public void PlayIdle()
    {
        if (!IsReady) return;
        string animation = prefix + IdleSuffix;
        TrackEntry current = graphic.AnimationState.GetCurrent(0);
        if (current != null && current.Animation != null && current.Animation.Name == animation && current.Loop) return;
        graphic.AnimationState.SetAnimation(0, animation, true);
    }

    public void PlayLanding() => PlayOnce(prefix + LandingSuffix);
    public void PlayAttack() => PlayOnce(prefix + AttackSuffix);
    public void PlayDamage() => PlayOnce(prefix + DamageSuffix);

    public float LandingDuration => Duration(prefix + LandingSuffix);
    public float AttackDuration => Duration(prefix + AttackSuffix);
    public float DamageDuration => Duration(prefix + DamageSuffix);

    private void PlayOnce(string animation)
    {
        if (!IsReady || graphic.Skeleton.Data.FindAnimation(animation) == null) return;
        graphic.AnimationState.SetAnimation(0, animation, false);
        graphic.AnimationState.AddAnimation(0, prefix + IdleSuffix, true, 0f);
    }

    private float Duration(string animation)
    {
        if (!IsReady) return 0f;
        Spine.Animation found = graphic.Skeleton.Data.FindAnimation(animation);
        return found == null ? 0f : found.Duration;
    }

    public void Destroy()
    {
        if (root != null) Object.Destroy(root.gameObject);
    }
}
