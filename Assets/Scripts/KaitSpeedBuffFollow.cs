using System;
using UnityEngine;

/// <summary>Short ankle feedback follows the live actor, without owning any animation or input lock.</summary>
public sealed class KaitSpeedBuffFollow : MonoBehaviour
{
    public Func<RectTransform> Actor;
    public Func<Color> Tint;
    public Func<Vector2Int> Direction;
    private KaitCombatEffectGraphic effect;
    public void Refresh()
    {
        if (effect == null) effect = GetComponent<KaitCombatEffectGraphic>();
        RectTransform actor = Actor?.Invoke();
        if (actor == null) { Destroy(gameObject); return; }
        transform.position = actor.TransformPoint(new Vector3(0,-actor.rect.height*.38f,0));
        if (Tint != null) effect.color = Tint();
        if (Direction != null) effect.SetBoundaryDirection(Direction());
    }
    private void LateUpdate() => Refresh();
}
