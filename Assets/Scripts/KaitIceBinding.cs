using System;
using UnityEngine;

// Status lifetime is owned by the rules, not the length of the sprite sheet.
public sealed class KaitIceBinding : MonoBehaviour
{
    public Func<bool> IsFrozen;
    public Func<Vector3> GroundPosition;
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
        if (!Releasing && (IsFrozen == null || !IsFrozen()))
        {
            Releasing = true;
            elapsed = 0;
        }
        if (!Releasing && GroundPosition != null) transform.position = GroundPosition();
        elapsed += Mathf.Max(0, delta);
        if (Releasing)
        {
            Graphic.SetHeldProgress(Mathf.Lerp(.625f, 1f, elapsed / .26f));
            if (elapsed >= .26f) Destroy(gameObject);
        }
        else
            Graphic.SetHeldProgress(Mathf.Lerp(.01f, .4f, Mathf.Clamp01(elapsed / .22f)));
    }
}
