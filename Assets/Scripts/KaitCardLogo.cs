using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Visual children only: the card itself remains the single drag/click target.
public sealed class KaitCardLogo : MonoBehaviour
{
    public const string ResourceRoot = "KaitVisuals/CardLogos/";
    private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    private HybridStyleGraphic picture;
    private HybridStyleGraphic frame, frameFill;
    private Text symbol;
    private Sprite current;
    public string AssetName { get; private set; }
    public string FlatSymbol => symbol.text;
    public Vector2 FlatFrameSize => frame.rectTransform.sizeDelta;

    public static KaitCardLogo Create(Transform parent, GlobalStyleSplit split, Font font, Vector2 position, float size)
    {
        var go = new GameObject("Card Logo", typeof(RectTransform), typeof(KaitCardLogo));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>(); rect.anchoredPosition = position; rect.sizeDelta = Vector2.one * size;
        var logo = go.GetComponent<KaitCardLogo>();
        // The flat badge has its own compact, wide outline. Both pieces use
        // the same cut as the card and are purely visual (no second button).
        Vector2 frameSize = new Vector2(Mathf.Max(76, size + 4), Mathf.Min(64, size - 10));
        logo.frame = FlatPlate(rect, split, "Simple Logo Frame", frameSize,
            new Color(.91f,.72f,.66f,.85f), 10);
        logo.frameFill = FlatPlate(rect, split, "Simple Logo Fill", frameSize - Vector2.one * 3,
            new Color(.24f,.22f,.28f,1), 8.5f);
        var art = new GameObject("Cartoon Logo", typeof(RectTransform), typeof(CanvasRenderer), typeof(HybridStyleGraphic));
        art.transform.SetParent(rect, false);
        logo.picture = art.GetComponent<HybridStyleGraphic>();
        logo.picture.rectTransform.sizeDelta = rect.sizeDelta;
        logo.picture.Configure(split, null, Color.white, Color.clear, Color.clear, 0, 0);
        logo.picture.raycastTarget = false;
        var label = new GameObject("Simple Logo", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(SunlitSplitText));
        label.transform.SetParent(rect, false);
        logo.symbol = label.GetComponent<Text>();
        logo.symbol.rectTransform.sizeDelta = frameSize - new Vector2(14,8);
        logo.symbol.font = font; logo.symbol.fontSize = Mathf.RoundToInt(Mathf.Min(32,size * .43f));
        logo.symbol.fontStyle = FontStyle.Bold; logo.symbol.alignment = TextAnchor.MiddleCenter;
        logo.symbol.color = new Color(1,.94f,.87f); logo.symbol.raycastTarget = false;
        logo.symbol.resizeTextForBestFit = true; logo.symbol.resizeTextMinSize = 18;
        logo.symbol.resizeTextMaxSize = logo.symbol.fontSize;
        var cut = label.GetComponent<SunlitSplitText>(); cut.Configure(split); cut.SetSides(false, true);
        return logo;
    }

    private static HybridStyleGraphic FlatPlate(Transform parent, GlobalStyleSplit split, string name, Vector2 size, Color tint, float radius)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(HybridStyleGraphic));
        go.transform.SetParent(parent, false);
        var plate = go.GetComponent<HybridStyleGraphic>();
        plate.rectTransform.sizeDelta = size;
        plate.Configure(split, null, Color.clear, tint, Color.clear, 0, radius);
        plate.raycastTarget = false;
        return plate;
    }

    public void Show(KaitSkill skill) => Set(skill.ToString(), KaitSkillCard.Sigil(skill));
    public void Show(KaitPassive passive) => Set(passive.ToString(), PassiveSymbol(passive));
    private void Set(string name, string text)
    {
        AssetName = name;
        current = Load(name); symbol.text = text;
        picture.SetVisualState(current, current != null ? Color.white : Color.clear, Color.clear);
    }
    public void SetTint(Color tint)
    {
        picture.SetVisualState(current, current != null ? tint : Color.clear, Color.clear);
        symbol.color = new Color(tint.r, tint.g * .94f, tint.b * .87f, tint.a);
        frame.SetVisualState(null, Color.clear, new Color(.91f * tint.r,.72f * tint.g,.66f * tint.b,.85f * tint.a));
        frameFill.SetVisualState(null, Color.clear, new Color(.24f * tint.r,.22f * tint.g,.28f * tint.b,tint.a));
    }
    public static Sprite Load(string name)
    {
        if (sprites.TryGetValue(name, out var cached) && cached != null) return cached;
        var texture = Resources.Load<Texture2D>(ResourceRoot + "Transparent/" + name)
            ?? Resources.Load<Texture2D>(ResourceRoot + name);
        if (texture == null) return null;
        var sprite = Sprite.Create(texture, new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
        sprites[name] = sprite;
        return sprite;
    }
    public static string PassiveSymbol(KaitPassive passive)
    {
        switch(passive)
        {
            case KaitPassive.BirdEye: return "眼";
            case KaitPassive.OldNewsArchive: return "归档";
            case KaitPassive.Simplify: return "合";
            case KaitPassive.BloodBookmark: return "签";
            case KaitPassive.MomentumResonance: return "联动";
            case KaitPassive.Devil: return "CD−1";
            case KaitPassive.CheshireCat: return "HP≥1";
            case KaitPassive.Squeeze: return "><";
            case KaitPassive.Follower: return "随";
            case KaitPassive.BladeCovenant: return "3杀";
            case KaitPassive.Trend: return "反侧";
            case KaitPassive.SweepTail: return "扫";
            default: return "";
        }
    }
}
