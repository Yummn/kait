using UnityEngine;

// Each piece stays independent of the board state and the shared UI controls.
public static class KaitSunlitTheme
{
    public const string ResourceRoot = "KaitVisuals/EmeraldCourtyard/";

    public static Sprite Load(string name, float borderFraction = 0f, float nominalWidth = 100f)
    {
        Texture2D texture = Resources.Load<Texture2D>(ResourceRoot + name);
        if (texture == null) return null;
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        float border = texture.width * borderFraction;
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), texture.width / nominalWidth * 100f, 0,
            SpriteMeshType.FullRect, new Vector4(border, border, border, border));
    }
}
