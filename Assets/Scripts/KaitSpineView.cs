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
    public const float WallStopTimeScale = 0.75f;
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
    public const string YummnFollowUpReady = "01_multi_idle_standBy";
    public const string YummnFollowUpAttack = "01_attack_skipQuest";
    public const string YummnKill = "01_standBy";
    public const string YummnKiGuard = "108201_skill0";
    public const string YummnRun = "01_run";
    public const string YummnWalk = "01_walk";
    public const string YummnBuff = "108201_skill1";
    public const string YummnHeal = "108201_joyResult";
    public const string YummnAttackSkill = "01_multi_standBy";
    public const string YummnVictory = "000000_smile";

    public string RestAnimation { get; set; } = Idle;
    private bool? yummnHasKi;
    private bool terminalPose;
    public void ResetTerminalPose() => terminalPose=false;
    private static bool IsTerminalAnimation(string animation) => animation==Die || animation=="01_die" || animation==Victory || animation==YummnVictory;
    public void SetYummnKiState(bool hasKi)
    {
        if(yummnHasKi==hasKi)return;
        yummnHasKi=hasKi;
        if(!IsReady)return;
        var current=CurrentAnimation;
        if(current==null)return;
        if(current.Loop && IsYummnRest(current.Animation.Name))PlayLoop(Idle);
    }
    private static bool IsYummnRest(string name) => name==Idle || name=="01_idle" || name==YummnFollowUpReady;

    public static string YummnSkillAnimation(KaitSkill skill)
    {
        return skill == KaitSkill.WindStep || skill == KaitSkill.PatientDefense
            ? YummnBuff : YummnAttackSkill;
    }

    private static Material sharedGraphicMaterial;
    private readonly SkeletonGraphic graphic;
    private readonly RectTransform root;
    private readonly RectTransform skeletonRect;
    private readonly Material flashMaterial;
    private readonly float rightFacingVisualX;
    private float[] weaponWorldVertices = new float[8];

    public RectTransform Root => root;
    public bool IsReady => graphic != null && graphic.Skeleton != null && graphic.AnimationState != null;
    public TrackEntry CurrentAnimation => IsReady ? graphic.AnimationState.GetCurrent(0) : null;

    public bool TryGetSwordTipWorldPosition(out Vector3 worldPosition)
    {
        worldPosition = root != null ? root.position : Vector3.zero;
        if (!IsReady || skeletonRect == null) return false;

        // Follow the visible attachment geometry rather than Bone.Data.Length.
        // Makoto's weapon bones have almost no authored length, while the
        // region itself contains the actual blade shape. The weapon vertex
        // farthest from the character root is the sword tip and remains
        // correct when the skeleton is mirrored.
        // Both attack clips explicitly render the blade through weaponMainF.
        // Other weapon slots can retain setup-pose attachments while hidden;
        // including them would make the sampled point stick near Kait's feet.
        string[] candidates = { "weaponMainF" };
        Vector2 characterRoot = graphic.Skeleton.RootBone == null
            ? Vector2.zero
            : new Vector2(graphic.Skeleton.RootBone.WorldX, graphic.Skeleton.RootBone.WorldY);
        Vector2 bestTip = Vector2.zero;
        float bestTipDistance = -1f;
        foreach (string slotName in candidates)
        {
            Slot slot = graphic.Skeleton.FindSlot(slotName);
            if (slot == null || slot.Attachment == null || slot.A <= 0.02f) continue;

            int vertexValueCount;
            if (slot.Attachment is RegionAttachment region)
            {
                vertexValueCount = 8;
                region.ComputeWorldVertices(slot.Bone, weaponWorldVertices, 0);
            }
            else if (slot.Attachment is VertexAttachment vertexAttachment)
            {
                vertexValueCount = vertexAttachment.WorldVerticesLength;
                if (vertexValueCount < 4) continue;
                if (weaponWorldVertices.Length < vertexValueCount)
                    weaponWorldVertices = new float[vertexValueCount];
                vertexAttachment.ComputeWorldVertices(slot, weaponWorldVertices);
            }
            else
            {
                continue;
            }

            for (int i = 0; i < vertexValueCount; i += 2)
            {
                Vector2 vertex = new Vector2(weaponWorldVertices[i], weaponWorldVertices[i + 1]);
                float distance = (vertex - characterRoot).sqrMagnitude;
                if (distance > bestTipDistance)
                {
                    bestTipDistance = distance;
                    bestTip = vertex;
                }
            }
        }
        if (bestTipDistance < 0f) return false;
        // SkeletonGraphic multiplies every generated mesh vertex by the
        // Canvas reference-pixels-per-unit value. ComputeWorldVertices returns
        // the original Spine-space value, so applying only TransformPoint left
        // the sampled tip about 100 times too close to the skeleton origin
        // (visually, at Kait's feet).
        float uiVertexScale = graphic.canvas != null ? graphic.canvas.referencePixelsPerUnit : 100f;
        Vector2 uiTip = bestTip * uiVertexScale;
        worldPosition = skeletonRect.TransformPoint(new Vector3(uiTip.x, uiTip.y, 0f));
        return true;
    }

    private KaitSpineView(RectTransform root, SkeletonGraphic graphic, RectTransform skeletonRect, float rightFacingVisualX, Material flashMaterial)
    {
        this.root = root;
        this.graphic = graphic;
        this.skeletonRect = skeletonRect;
        this.rightFacingVisualX = rightFacingVisualX;
        this.flashMaterial = flashMaterial;
        graphic.AnimationState.Start += entry => {
            if(yummnHasKi.HasValue && entry.Loop && IsYummnRest(entry.Animation.Name))
                PlayLoop(Idle);
        };
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
        skeletonGraphic.AnimationState.SetAnimation(0, skeletonGraphic.Skeleton.Data.FindAnimation(Idle)!=null?Idle:"01_idle", true);
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
        // The unarmed body fills the whole mesh, unlike Kait's wide sword silhouette.
        // Keep her body at the same visual scale in the selector, board and trails.
        bool yummn = skeletonGraphic.Skeleton.Data.FindAnimation("108201_skill0") != null;
        if (yummn) scale *= .82f;
        skeletonRect.localScale = Vector3.one * scale;
        Vector2 centeredPosition = new Vector2(-bodyBounds.center.x * scale, -meshBounds.center.y * scale);
        skeletonRect.anchoredPosition = centeredPosition;
        if (!name.Contains("Trail"))
            KaitContactShadow.Create(hostRect, skeletonGraphic,
                new Vector2(0, yummn ? -height * scale * .5f + size.y * .03f : -size.y * .38f),
                new Vector2(size.x * .36f, size.y * .09f));

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
        if (!IsReady || string.IsNullOrEmpty(animation) || terminalPose) return;
        if(IsTerminalAnimation(animation)){PlayOnce(animation,null);return;}
        animation = ResolveAnimation(animation);
        TrackEntry current = graphic.AnimationState.GetCurrent(0);
        if (current != null && current.Animation != null && current.Animation.Name == animation && current.Loop) return;
        graphic.timeScale = 1f;
        TrackEntry entry = graphic.AnimationState.SetAnimation(0, animation, true);
        if (animation == Run || animation == "01_run_gamestart" || animation == YummnRun || animation == YummnWalk) entry.MixDuration = 0.02f;
    }

    public void PlayOnce(string animation, string followUp = Idle)
    {
        if (!IsReady || string.IsNullOrEmpty(animation) || terminalPose) return;
        if(IsTerminalAnimation(animation)){terminalPose=true;followUp=null;}
        animation=ResolveAnimation(animation);if(!string.IsNullOrEmpty(followUp))followUp=ResolveAnimation(followUp);
        graphic.timeScale = 1f;
        TrackEntry entry = graphic.AnimationState.SetAnimation(0, animation, false);
        if (animation == WallStop) entry.TimeScale = WallStopTimeScale;
        if (graphic.Skeleton.Data.FindAnimation("108201_skill0") != null &&
            (animation == "01_attack" || animation == YummnFollowUpAttack || animation == YummnKill))
            entry.TimeScale = 2f;
        if (animation == YummnKiGuard)
        {
            entry.TimeScale = 2f;
            float fps=graphic.Skeleton.Data.Fps;
            entry.AnimationEnd=Mathf.Min(entry.Animation.Duration,50f/(fps>0?fps:30f));
        }
        if (!string.IsNullOrEmpty(followUp)) graphic.AnimationState.AddAnimation(0, followUp, true, 0f);
    }

    // Change the resting pose without cutting off a one-shot or blocking input.
    public void RefreshRestPose()
    {
        if(!IsReady)return;
        var current=CurrentAnimation;
        if(current==null||current.Loop){PlayLoop(Idle);return;}
    }

    public float Duration(string animation)
    {
        if (!IsReady || string.IsNullOrEmpty(animation)) return 0f;
        animation=ResolveAnimation(animation);
        Spine.Animation found = graphic.Skeleton.Data.FindAnimation(animation);
        return found == null ? 0f : found.Duration;
    }
    private string ResolveAnimation(string name)
    {
        if (graphic.Skeleton.Data.FindAnimation("108201_skill0") != null)
        {
            if(yummnHasKi.HasValue && IsYummnRest(name))
                return yummnHasKi.Value ? YummnFollowUpReady : "01_idle";
            if (name == Run) name = YummnRun;
            else if (name == Victory) name = YummnVictory;
            else if (name == Idle && RestAnimation != Idle) name = RestAnimation;
        }
        if(graphic.Skeleton.Data.FindAnimation(name)!=null)return name;
        string mapped=name.Replace("05_","01_").Replace("104301_","108201_");
        return graphic.Skeleton.Data.FindAnimation(mapped)!=null?mapped:"01_idle";
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
