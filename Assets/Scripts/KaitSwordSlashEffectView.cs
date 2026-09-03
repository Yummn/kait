using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Self-timed material slash. It deliberately does not use a KaitGame coroutine,
/// so accepting the next chain direction cannot erase the previous hit feedback.
/// </summary>
public sealed class KaitSwordSlashEffectView : MonoBehaviour
{
    private RawImage image;
    private float duration;
    private float elapsed;
    private Quaternion baseRotation;
    private Color tint = Color.white;

    public void Configure(RawImage target, float playbackDuration, Color effectTint)
    {
        image = target;
        duration = Mathf.Max(0.12f, playbackDuration);
        elapsed = 0f;
        baseRotation = transform.localRotation;
        tint = effectTint;
        transform.localScale = Vector3.one * 0.62f;
    }

    private void Update()
    {
        if (image == null)
        {
            Destroy(gameObject);
            return;
        }

        elapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        float appear = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t * 8f));
        float fade = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.52f, 1f, t));
        Color frameColor = tint;
        frameColor.a *= appear * fade * 0.96f;
        image.color = frameColor;
        float scale = t < 0.22f
            ? Mathf.Lerp(0.62f, 1.06f, Mathf.SmoothStep(0f, 1f, t / 0.22f))
            : Mathf.Lerp(1.06f, 1.16f, Mathf.SmoothStep(0f, 1f, (t - 0.22f) / 0.78f));
        transform.localScale = Vector3.one * scale;
        transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, Mathf.Lerp(-7f, 7f, t));
        if (t >= 1f) Destroy(gameObject);
    }
}
