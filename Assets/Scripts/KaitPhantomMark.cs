using System;
using UnityEngine;

public sealed class KaitPhantomMark : MonoBehaviour
{
    public Func<bool> IsMarked;
    public Func<Vector3> HeadPosition;
    public KaitCombatEffectGraphic Graphic { get; private set; }
    public bool Releasing { get; private set; }
    private float elapsed;

    public void Initialize(KaitCombatEffectGraphic graphic)
    {
        Graphic = graphic;
        graphic.SetHeldProgress(.01f);
    }

    private void LateUpdate() => Advance(Time.unscaledDeltaTime);

    public void Advance(float delta)
    {
        if (Graphic == null) { Destroy(gameObject); return; }
        if (!Releasing && (IsMarked == null || !IsMarked()))
        {
            Releasing = true;
            elapsed = 0;
        }
        if (!Releasing && HeadPosition != null) transform.position = HeadPosition();
        elapsed += Mathf.Max(0, delta);
        if (Releasing)
        {
            Graphic.SetHeldProgress(Mathf.Lerp(.625f, 1f, elapsed / .24f));
            if (elapsed >= .24f) Destroy(gameObject);
        }
        else Graphic.SetHeldProgress(Mathf.Lerp(.01f, .4f, Mathf.Clamp01(elapsed / .2f)));
    }
}
