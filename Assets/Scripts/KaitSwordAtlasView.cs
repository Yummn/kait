using Spine;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Approved eight-frame slash, aligned to the live weapon rather than the target cell.</summary>
public sealed class KaitSwordAtlasView : MonoBehaviour
{
    private static Material sharedMaterial;
    private KaitSpineView source;
    private TrackEntry attack;
    private RawImage image;
    private RectTransform rect;
    private float lead;
    private bool finisher;
    // Bright-core centers measured from the approved frames (normalized cell coordinates).
    private static readonly Vector2[] NormalCenters = {
        new Vector2(.2000f,.1487f), new Vector2(.1837f,.1022f), new Vector2(.2446f,.0149f),
        new Vector2(.2369f,-.0441f), new Vector2(.2179f,-.0235f), new Vector2(.1948f,-.0365f),
        new Vector2(.1857f,-.1962f), new Vector2(.1857f,-.1962f) };
    private static readonly Vector2[] FinisherCenters = {
        new Vector2(.2052f,.1293f), new Vector2(.1993f,.0794f), new Vector2(.2120f,.0172f),
        new Vector2(.1907f,-.0713f), new Vector2(.1966f,-.0147f), new Vector2(.1593f,-.0065f),
        new Vector2(.0337f,.0186f), new Vector2(-.0302f,.0697f) };
    private const float SweepDuration = 0.28f;

    public static Rect FrameUv(int frame, int width, int height)
    {
        frame = Mathf.Clamp(frame, 0, 7);
        float x = frame % 4 * 0.25f, y = 1f - (frame / 4 + 1) * 0.5f;
        return new Rect(x + 0.5f / width, y + 0.5f / height,
            0.25f - 1f / width, 0.5f - 1f / height);
    }

    public static KaitSwordAtlasView Create(KaitSpineView source, RectTransform parent, bool finisher)
    {
        if (source == null || !source.IsReady || parent == null) return null;
        Texture2D atlas = Resources.Load<Texture2D>("KaitVisuals/Effects/WhiteGoldSlash" + (finisher ? "Finisher" : "Normal"));
        if (atlas == null) return null;
        if (sharedMaterial == null)
        {
            Shader shader = Resources.Load<Shader>("Shaders/UIWhiteGoldShatter");
            if (shader == null) return null;
            sharedMaterial = new Material(shader) { name = "White Gold Sword Atlas" };
        }
        var go = new GameObject("Kait White Gold Sword", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        go.transform.SetParent(parent, false);
        var view = go.AddComponent<KaitSwordAtlasView>();
        view.source = source;
        view.finisher = finisher;
        view.attack = source.CurrentAnimation;
        view.lead = finisher ? 0.08f : 0.46f;
        view.rect = go.GetComponent<RectTransform>();
        view.rect.anchorMin = view.rect.anchorMax = view.rect.pivot = Vector2.one * 0.5f;
        view.image = go.GetComponent<RawImage>();
        view.image.texture = atlas;
        view.image.material = sharedMaterial;
        view.image.maskable = false;
        view.image.raycastTarget = false;
        view.image.color = Color.clear;
        return view;
    }

    private void LateUpdate()
    {
        // No coroutine, input lock or delayed replay. A new move/skill cancels this attack's slash.
        if (source == null || source.Root == null || attack == null || source.CurrentAnimation != attack)
        { Destroy(gameObject); return; }
        float t = (attack.TrackTime - lead) / SweepDuration;
        if (t >= 1f) { Destroy(gameObject); return; }
        if (t < 0f || !source.TryGetSwordTipWorldPosition(out Vector3 tip))
        { image.color = Color.clear; return; }
        Transform parent = rect.parent;
        Vector2 center = parent.InverseTransformPoint(source.Root.position);
        Vector2 direction = (Vector2)parent.InverseTransformPoint(tip) - center;
        if (direction.sqrMagnitude < 64f) { image.color = Color.clear; return; }
        int frame = Mathf.Clamp(Mathf.FloorToInt(t * 8f), 0, 7);
        Vector2 core = (finisher ? FinisherCenters : NormalCenters)[frame];
        float size = Mathf.Clamp(direction.magnitude / 0.30f, 130f, finisher ? 190f : 180f);
        float angle = Mathf.Atan2(direction.y, direction.x) - Mathf.Atan2(core.y, core.x);
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
        // Attack clips translate the skeleton inside its host. Anchor the visible
        // bright core to the blade, not to that stationary host's center.
        Vector2 localTip = parent.InverseTransformPoint(tip);
        rect.localPosition = localTip - direction.normalized * 8f - (Vector2)(rotation * (Vector3)(core * size));
        rect.sizeDelta = Vector2.one * size;
        rect.localRotation = rotation;
        image.uvRect = FrameUv(frame, image.texture.width, image.texture.height);
        image.color = new Color(1f, 1f, 1f, 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.80f, 1f, t)));
    }
}
