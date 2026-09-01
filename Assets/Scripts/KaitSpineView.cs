using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public sealed class KaitSpineView
{
    private const float VisualFill = 1.16f;
    public const string Idle = "05_idle";
    public const string Run = "05_run_gamestart";
    public const string ChainDirectionChoice = "000000_rarityup_posing";
    public const string WallStop = "05_joy_long_return";
    public const string Attack = "05_attack";
    public const string ChainAttack = "05_attack_skipQuest";
    public const string Damage = "05_damage";
    public const string Die = "05_die";
    public const string JoyShort = "05_joy_short";
    public const string JoyLong = "05_joy_long";
    public const string LargeAttack = "104301_skill0";
    public const string SmallAttack = "104301_skill1";
    public const string OtherSkill = "104301_skill2";
    public const string Victory = "000000_mana_jump";
    public const string ShadowStep = "000000_run_jump";

    private static Material sharedGraphicMaterial;
    private readonly SkeletonGraphic graphic;
    private readonly RectTransform root;
    private readonly RectTransform skeletonRect;
    private readonly Material flashMaterial;
    private readonly float rightFacingVisualX;

    public RectTransform Root => root;
    public bool IsReady => graphic != null && graphic.Skeleton != null && graphic.AnimationState != null;

    private KaitSpineView(RectTransform root, SkeletonGraphic graphic, RectTransform skeletonRect, float rightFacingVisualX, Material flashMaterial)
    {
        this.root = root;
        this.graphic = graphic;
        this.skeletonRect = skeletonRect;
        this.rightFacingVisualX = rightFacingVisualX;
        this.flashMaterial = flashMaterial;
    }

    public static KaitSpineView Create(SkeletonDataAsset data, Transform parent, Vector2 size, string name = "Kait Spine")
    {
        if (data == null || parent == null) return null;

        var host = new GameObject(name, typeof(RectTransform));
        host.transform.SetParent(parent, false);
        RectTransform hostRect = host.GetComponent<RectTransform>();
        hostRect.anchorMin = hostRect.anchorMax = hostRect.pivot = new Vector2(0.5f, 0.5f);
        hostRect.sizeDelta = size;
        hostRect.anchoredPosition = Vector2.zero;

        if (sharedGraphicMaterial == null)
        {
            Material packagedMaterial = Resources.Load<Material>("Characters/Makoto/KaitSkeletonGraphic");
            if (packagedMaterial != null)
                sharedGraphicMaterial = packagedMaterial;
            else
            {
                Shader shader = Shader.Find("Spine/SkeletonGraphic");
                if (shader == null)
                {
                    Object.Destroy(host);
                    Debug.LogError("Kait Spine: Spine/SkeletonGraphic shader was not found.");
                    return null;
                }
                sharedGraphicMaterial = new Material(shader) { name = "Kait Spine UI Material" };
            }
        }

        SkeletonGraphic skeletonGraphic = SkeletonGraphic.NewSkeletonGraphicGameObject(data, hostRect, sharedGraphicMaterial);
        skeletonGraphic.name = "Makoto Skeleton";
        skeletonGraphic.raycastTarget = false;
        // Kait's sword intentionally extends beyond a board cell during several
        // animations. Ignore both Unity UI masks and Spine clipping so those
        // vertices are never trimmed when the character changes parent or facing.
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
            Debug.LogError("Kait Spine: Makoto skeleton data could not be initialized.");
            return null;
        }
        Material flashMaterial = CreateFlashMaterial(name + " Hit Flash");
        if (flashMaterial != null) skeletonGraphic.material = flashMaterial;
        skeletonGraphic.AnimationState.SetAnimation(0, Idle, true);
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
        float scale = Mathf.Min(size.x * VisualFill / width, size.y * VisualFill / height);
        skeletonRect.localScale = Vector3.one * scale;
        Vector2 centeredPosition = new Vector2(-bodyBounds.center.x * scale, -meshBounds.center.y * scale);
        skeletonRect.anchoredPosition = centeredPosition;

        return new KaitSpineView(hostRect, skeletonGraphic, skeletonRect, centeredPosition.x, flashMaterial);
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

    public void Face(KaitDirection direction)
    {
        if (!IsReady) return;
        if (direction == KaitDirection.Left)
        {
            graphic.Skeleton.ScaleX = -Mathf.Abs(graphic.Skeleton.ScaleX);
            skeletonRect.anchoredPosition = new Vector2(-rightFacingVisualX, skeletonRect.anchoredPosition.y);
        }
        else if (direction == KaitDirection.Right)
        {
            graphic.Skeleton.ScaleX = Mathf.Abs(graphic.Skeleton.ScaleX);
            skeletonRect.anchoredPosition = new Vector2(rightFacingVisualX, skeletonRect.anchoredPosition.y);
        }
    }

    public void SetOpacity(float opacity)
    {
        if (graphic != null) graphic.color = new Color(1f, 1f, 1f, Mathf.Clamp01(opacity));
    }

    public void SetTint(Color color)
    {
        if (graphic != null) graphic.color = color;
    }

    public void SetHitFlash(float amount)
    {
        if (flashMaterial != null) flashMaterial.SetFloat("_FillPhase", Mathf.Clamp01(amount));
    }

    public void PlayLoop(string animation)
    {
        if (!IsReady || string.IsNullOrEmpty(animation)) return;
        TrackEntry current = graphic.AnimationState.GetCurrent(0);
        if (current != null && current.Animation != null && current.Animation.Name == animation && current.Loop) return;
        graphic.timeScale = 1f;
        TrackEntry entry = graphic.AnimationState.SetAnimation(0, animation, true);
        if (animation == Run) entry.MixDuration = 0.02f;
    }

    public void PlayOnce(string animation, string followUp = Idle)
    {
        if (!IsReady || string.IsNullOrEmpty(animation)) return;
        graphic.timeScale = 1f;
        TrackEntry entry = graphic.AnimationState.SetAnimation(0, animation, false);
        if (animation == WallStop) entry.TimeScale = 2f;
        if (!string.IsNullOrEmpty(followUp)) graphic.AnimationState.AddAnimation(0, followUp, true, 0f);
    }

    public float Duration(string animation)
    {
        if (!IsReady || string.IsNullOrEmpty(animation)) return 0f;
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
