using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Approved eight-frame sheets, read top-to-bottom without resampling the source.
public static class KaitWarningFrames
{
    private static readonly Dictionary<string, Sprite[]> clips = new Dictionary<string, Sprite[]>();
    public static Sprite Frame(string name, float time)
    {
        if (!clips.TryGetValue(name, out var frames))
        {
            var sheet = Resources.Load<Texture2D>("KaitVisuals/Yummn/Frames/" + name);
            if (sheet == null) return null;
            frames = new Sprite[8];
            for (int i = 0; i < 8; i++)
            {
                int x = Mathf.RoundToInt(i % 4 * sheet.width / 4f);
                int x1 = Mathf.RoundToInt((i % 4 + 1) * sheet.width / 4f);
                int y = Mathf.RoundToInt((1 - i / 4) * sheet.height / 2f);
                int y1 = Mathf.RoundToInt((2 - i / 4) * sheet.height / 2f);
                var border = name == "DangerC" ? new Vector4((x1-x)*.4f,(y1-y)*.4f,(x1-x)*.4f,(y1-y)*.4f) : Vector4.zero;
                frames[i] = Sprite.Create(sheet, new Rect(x, y, x1-x, y1-y), Vector2.one*.5f, 100, 0, SpriteMeshType.FullRect, border);
            }
            clips[name] = frames;
        }
        return frames[Mathf.FloorToInt(Mathf.Repeat(time, 1.2f) / .15f) % 8];
    }
    public static Image Image(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.raycastTarget = false; image.maskable = false; image.color = Color.clear;
        return image;
    }
    public static bool Covers(KaitRun run, KaitEnemy enemy, Vector2Int cell)
    {
        return run.IsYummn && run.Yummn.rules.BossLine && enemy.type == KaitEnemyType.ShieldKnight &&
            enemy.life == KaitEnemyLife.Active && enemy.hp > 0 && enemy.frozenActions == 0 &&
            enemy.intent.type == KaitIntentType.Melee && enemy.intent.affectedCells.Contains(cell);
    }
}

public sealed class KaitBossWarningFrames : MonoBehaviour
{
    private Image image;
    private bool impact;
    public void Configure(Vector2Int direction, bool striking)
    {
        if (image == null)
        {
            image = KaitWarningFrames.Image("Boss B ground warning", transform);
            image.rectTransform.anchorMin = Vector2.zero; image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.offsetMin = image.rectTransform.offsetMax = Vector2.zero;
        }
        impact = striking;
        // Authored left-to-right; board and UI both use positive y upwards.
        image.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x)*Mathf.Rad2Deg);
        enabled = true; image.gameObject.SetActive(true); LateUpdate();
    }
    public void Clear() { enabled = false; if (image != null) image.gameObject.SetActive(false); }
    private void LateUpdate()
    {
        if (image == null) return;
        image.sprite = KaitWarningFrames.Frame("BossB", Time.unscaledTime);
        image.color = image.sprite == null ? Color.clear : impact ? new Color(1f, .85f, .3f, .95f) : new Color(1,1,1,.82f);
    }
}
