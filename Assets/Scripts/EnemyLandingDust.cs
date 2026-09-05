using Spine;
using UnityEngine;

/// <summary>Waits for the existing landing track's contact frame without locking input.</summary>
public sealed class EnemyLandingDust : MonoBehaviour
{
    // All six shipped landing clips reach the ground at their 0.2667 body key.
    public const float ContactTime = .2667f;
    public const float DustDuration = .34f;
    private EnemySpineView source;
    private TrackEntry landing;
    private Vector3 spawnPosition;
    private KaitCombatEffectGraphic graphic;
    public bool Emitted { get; private set; }
    public KaitCombatEffectGraphic Graphic => graphic;

    public static EnemyLandingDust Create(EnemySpineView actor, RectTransform layer)
    {
        TrackEntry track = actor?.CurrentAnimation;
        if (layer == null || track?.Animation == null || !track.Animation.Name.EndsWith(EnemySpineView.LandingSuffix)) return null;
        var go = new GameObject("Enemy Landing Dust A", typeof(RectTransform), typeof(CanvasRenderer),
            typeof(KaitCombatEffectGraphic), typeof(EnemyLandingDust));
        go.transform.SetParent(layer, false);
        var view = go.GetComponent<EnemyLandingDust>();
        view.source = actor; view.landing = track; view.spawnPosition = actor.Root.position;
        view.graphic = go.GetComponent<KaitCombatEffectGraphic>();
        view.graphic.enabled = false;
        view.graphic.rectTransform.sizeDelta = Vector2.one * 120f;
        return view;
    }

    private void Update()
    {
        if (Emitted) return;
        if (source == null || source.Root == null || !source.Root.gameObject.activeInHierarchy ||
            source.CurrentAnimation != landing || (source.Root.position-spawnPosition).sqrMagnitude > 1f)
        { Destroy(gameObject); return; }
        if (landing.TrackTime < ContactTime) return;
        Emitted = true;
        graphic.rectTransform.position = source.GroundPosition;
        graphic.enabled = true;
        graphic.Configure(KaitCombatEffectKind.LandingDust, Color.white, Color.white, .5f);
        graphic.Play(DustDuration);
    }
}
