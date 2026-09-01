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
    public const string DeathSuffix = "die";
    public const string PrepareAttackSuffix = "joy_short";

    private static Material sharedGraphicMaterial;
    private readonly SkeletonGraphic graphic;
    private readonly RectTransform root;
    private readonly RectTransform skeletonRect;
    private readonly Material flashMaterial;
    private readonly float rightFacingVisualX;
    private readonly string prefix;

    public RectTransform Root => root;
    public bool IsReady => graphic != null && graphic.Skeleton != null && graphic.AnimationState != null;

    private EnemySpineView(RectTransform root, SkeletonGraphic graphic, RectTransform skeletonRect, float rightFacingVisualX, string prefix, Material flashMaterial)
    {
        this.root = root;
        this.graphic = graphic;
        this.skeletonRect = skeletonRect;
        this.rightFacingVisualX = rightFacingVisualX;
        this.prefix = prefix;
        this.flashMaterial = flashMaterial;
    }

    public static EnemySpineView Create(SkeletonDataAsset data, string animationPrefix, Transform parent, Vector2 size, string name, float visualScale = 1f)
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
        // Enemy weapons and effects are allowed to extend beyond their home cell.
        // Ignore both Unity UI masks and Spine clipping attachments, matching Kait.
        skeletonGraphic.maskable = false;
        skeletonGraphic.canvasRenderer.cullTransparentMesh = false;
        MeshGenerator.Settings meshSettings = skeletonGraphic.MeshGenerator.settings;
        meshSettings.useClipping = false;
        skeletonGraphic.MeshGenerator.settings = meshSettings;
        skeletonGraphic.unscaledTime = true;
        skeletonGraphic.timeScale = 1f;
        skeletonGraphic.Initialize(false);
        if (skeletonGraphic.Skeleton == null || skeletonGraphic.AnimationState == null)
        {
            Object.Destroy(host);
            return null;
        }

        Material flashMaterial = CreateFlashMaterial(name + " Hit Flash");
        if (flashMaterial != null) skeletonGraphic.material = flashMaterial;

        string idle = animationPrefix + IdleSuffix;
        skeletonGraphic.AnimationState.SetAnimation(0, idle, true);
        skeletonGraphic.Update(0f);
        skeletonGraphic.MatchRectTransformWithBounds();

        RectTransform skeletonRect = skeletonGraphic.rectTransform;
        Mesh mesh = skeletonGraphic.GetLastMesh();
        Bounds meshBounds = mesh != null ? mesh.bounds : new Bounds(Vector3.zero, skeletonRect.sizeDelta);
        Bounds bodyBounds = BodyBounds(skeletonGraphic, meshBounds);
        skeletonRect.anchorMin = skeletonRect.anchorMax = new Vector2(0.5f, 0.5f);
        skeletonRect.pivot = new Vector2(0.5f, 0.5f);
        skeletonRect.sizeDelta = meshBounds.size;
        float width = Mathf.Max(0.01f, meshBounds.size.x);
        float height = Mathf.Max(0.01f, meshBounds.size.y);
        float scale = Mathf.Min(size.x * VisualFill / width, size.y * VisualFill / height) * Mathf.Max(0.01f, visualScale);
        skeletonRect.localScale = Vector3.one * scale;
        Vector2 centeredPosition = new Vector2(-bodyBounds.center.x * scale, -meshBounds.center.y * scale);
        skeletonRect.anchoredPosition = centeredPosition;

        return new EnemySpineView(hostRect, skeletonGraphic, skeletonRect, centeredPosition.x, animationPrefix, flashMaterial);
    }

    private static Material CreateFlashMaterial(string name)
    {
        Material template = Resources.Load<Material>("KaitVisuals/SpineHitFlash");
        Shader fillShader = template != null ? template.shader : Shader.Find("Spine/Skeleton Fill");
        if (fillShader == null) return null;
        var material = template != null ? new Material(template) : new Material(fillShader);
        material.name = name;
        material.SetColor("_FillColor", Color.white);
        material.SetFloat("_FillPhase", 0f);
        return material;
    }

    private static Bounds BodyBounds(SkeletonGraphic skeletonGraphic, Bounds fallback)
    {
        Slot centerSlot = skeletonGraphic.Skeleton.FindSlot("Center");
        BoundingBoxAttachment bodyBox = centerSlot?.Attachment as BoundingBoxAttachment;
        if (bodyBox == null || bodyBox.WorldVerticesLength < 4) return fallback;
        float[] vertices = new float[bodyBox.WorldVerticesLength];
        bodyBox.ComputeWorldVertices(centerSlot, vertices);
        float minX = vertices[0], maxX = vertices[0], minY = vertices[1], maxY = vertices[1];
        for (int i = 2; i < vertices.Length; i += 2)
        {
            minX = Mathf.Min(minX, vertices[i]); maxX = Mathf.Max(maxX, vertices[i]);
            minY = Mathf.Min(minY, vertices[i + 1]); maxY = Mathf.Max(maxY, vertices[i + 1]);
        }
        return new Bounds(new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f), new Vector3(maxX - minX, maxY - minY, 0f));
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

    public void SetOpacity(float opacity)
    {
        if (graphic == null) return;
        Color color = graphic.color;
        color.a = Mathf.Clamp01(opacity);
        graphic.color = color;
    }

    public void SetHitFlash(float amount)
    {
        if (flashMaterial != null) flashMaterial.SetFloat("_FillPhase", Mathf.Clamp01(amount));
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
    public void PlayDeath() => PlayOnce(prefix + DeathSuffix, false);
    public void PlayPrepareAttack() => PlayOnce(prefix + PrepareAttackSuffix);

    public float LandingDuration => Duration(prefix + LandingSuffix);
    public float AttackDuration => Duration(prefix + AttackSuffix);
    public float DamageDuration => Duration(prefix + DamageSuffix);
    public float DeathDuration => Duration(prefix + DeathSuffix);
    public float PrepareAttackDuration => Duration(prefix + PrepareAttackSuffix);

    private void PlayOnce(string animation, bool returnToIdle = true)
    {
        if (!IsReady || graphic.Skeleton.Data.FindAnimation(animation) == null) return;
        graphic.AnimationState.SetAnimation(0, animation, false);
        if (returnToIdle) graphic.AnimationState.AddAnimation(0, prefix + IdleSuffix, true, 0f);
    }

    private float Duration(string animation)
    {
        if (!IsReady) return 0f;
        Spine.Animation found = graphic.Skeleton.Data.FindAnimation(animation);
        return found == null ? 0f : found.Duration;
    }

    public void Destroy()
    {
        if (Application.isPlaying)
        {
            if (flashMaterial != null) Object.Destroy(flashMaterial);
            if (root != null) Object.Destroy(root.gameObject);
        }
        else
        {
            if (flashMaterial != null) Object.DestroyImmediate(flashMaterial);
            if (root != null) Object.DestroyImmediate(root.gameObject);
        }
    }
}
