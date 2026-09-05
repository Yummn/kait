using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class HybridStyleButton : Button
{
    private HybridStyleGraphic surface;
    private Sprite normalSprite;
    private Sprite pressedSprite;
    private Color accent = Color.white;
    private SelectionState currentState = SelectionState.Normal;
    private Coroutine scaleRoutine;

    public void Configure(HybridStyleGraphic graphic, Sprite normalLeftSprite, Sprite pressedLeftSprite, Color normalAccent)
    {
        surface = graphic;
        normalSprite = normalLeftSprite;
        pressedSprite = pressedLeftSprite != null ? pressedLeftSprite : normalLeftSprite;
        transition = Transition.None;
        targetGraphic = surface;
        SetAccent(normalAccent);
    }

    public void SetAccent(Color color)
    {
        accent = color;
        ApplyVisualState(currentState);
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        currentState = state;
        base.DoStateTransition(state, true);
        ApplyVisualState(state);

        float targetScale = state == SelectionState.Pressed ? 0.94f : 1f;
        if (!Application.isPlaying || !isActiveAndEnabled || instant)
        {
            transform.localScale = Vector3.one * targetScale;
            return;
        }
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(TweenScale(targetScale, state == SelectionState.Pressed ? 0.045f : 0.075f));
    }

    protected override void OnDisable()
    {
        if (scaleRoutine != null)
        {
            StopCoroutine(scaleRoutine);
            scaleRoutine = null;
        }
        transform.localScale = Vector3.one;
        base.OnDisable();
    }

    private void ApplyVisualState(SelectionState state)
    {
        if (surface == null) return;
        Sprite sprite = state == SelectionState.Pressed ? pressedSprite : normalSprite;
        // The painted left skin already owns its palette; semantic accent
        // colors belong to the flat half, not a second multiply over blue/gold.
        Color left = Color.white;
        Color right = accent;
        switch (state)
        {
            case SelectionState.Highlighted:
            case SelectionState.Selected:
                left = new Color(1.08f, 1.08f, 1.08f, 1f);
                right = Color.Lerp(accent, Color.white, 0.14f);
                break;
            case SelectionState.Pressed:
                left = MultiplyRgb(Color.white, 0.76f);
                right = MultiplyRgb(accent, 0.76f);
                break;
            case SelectionState.Disabled:
                left = new Color(0.64f, 0.67f, 0.73f, 1f);
                right = Color.Lerp(accent, new Color(0.38f, 0.38f, 0.38f, accent.a), 0.62f);
                left.a = right.a = 0.72f;
                break;
        }
        surface.SetVisualState(sprite, left, right);
    }

    private IEnumerator TweenScale(float target, float duration)
    {
        Vector3 from = transform.localScale;
        Vector3 to = Vector3.one * target;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / duration), 3f);
            transform.localScale = Vector3.LerpUnclamped(from, to, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.localScale = to;
        scaleRoutine = null;
    }

    private static Color MultiplyRgb(Color color, float multiplier)
    {
        return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier, color.a);
    }
}
