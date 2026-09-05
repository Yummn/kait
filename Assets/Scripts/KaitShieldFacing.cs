using System;
using UnityEngine;
using UnityEngine.UI;

// Independent presentation: persistent facing never blocks a turn or actor animation.
public sealed class KaitShieldFacing : MonoBehaviour
{
    public Func<KaitEnemy> Enemy;
    public Func<Vector3> Position;
    private static Material sharedMaterial;
    private RawImage image;
    private float blockedUntil;
    public int Frame { get; private set; }

    public void Initialize()
    {
        image = gameObject.AddComponent<RawImage>();
        image.texture = Resources.Load<Texture2D>("KaitVisuals/Effects/ShieldFacingB");
        if (sharedMaterial == null)
            sharedMaterial = new Material(Resources.Load<Shader>("Shaders/UIWhiteGoldShatter"));
        image.material = sharedMaterial;
        image.raycastTarget = false;
        image.maskable = false;
        image.rectTransform.sizeDelta = Vector2.one * 100f;
        image.rectTransform.anchorMin = image.rectTransform.anchorMax = image.rectTransform.pivot = Vector2.one * .5f;
        Refresh();
    }

    public void Block() { blockedUntil = Time.unscaledTime + .24f; Refresh(); }
    private void LateUpdate() => Refresh();
    public void Refresh()
    {
        var enemy = Enemy?.Invoke();
        if (enemy == null || enemy.life == KaitEnemyLife.Dead || enemy.hp <= 0)
        { Destroy(gameObject); return; }
        var facing = enemy.facing;
        image.enabled = facing != Vector2Int.zero;
        if (!image.enabled) return;
        Vector3 center = transform.parent.InverseTransformPoint(Position());
        // The downward rim sits above the health bar, behind the actor's feet.
        transform.localPosition = center + (Vector3)(Vector2)facing * (facing.y < 0 ? 22f : 43f);
        transform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg);
        float remaining = blockedUntil - Time.unscaledTime;
        Frame = remaining > .16f ? 2 : remaining > .08f ? 3 : remaining > 0 ? 4 : 0;
        image.uvRect = KaitSwordAtlasView.FrameUv(Frame, image.texture.width, image.texture.height);
        image.color = new Color(1, 1, 1, remaining > 0 ? 1f : .58f);
    }
}
