using UnityEngine;
using UnityEngine.UI;

public class ScoreFireEffect : MonoBehaviour
{
    [SerializeField, Range(8, 32)] private int particleCount = 32;
    [SerializeField] private Color emberColor = new Color32(239, 96, 74, 255);
    [SerializeField] private Color flameColor = new Color32(250, 160, 103, 255);
    [SerializeField] private Color coreColor = new Color32(255, 230, 164, 255);
    [SerializeField] private Color heatColor = new Color32(123, 62, 67, 255);

    private Flame[] flames;
    private RectTransform rectTransform;
    private RectTransform flameRoot;
    private Image heatOverlay;
    private Sprite flameSprite;
    private Vector3 baseScale;
    private bool burning;
    private float spawnTimer;
    private int burstRemaining;
    private float intensity;

    private sealed class Flame
    {
        public RectTransform rect;
        public Image image;
        public Vector2 velocity;
        public Color color;
        public float life;
        public float maxLife;
        public float spin;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        baseScale = rectTransform.localScale;
        flameSprite = GetComponent<Image>()?.sprite;
        BuildHeatOverlay();
        BuildPool();
    }

    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        bool anyAlive = false;

        if (burning)
        {
            spawnTimer -= deltaTime;
            if (spawnTimer <= 0f || burstRemaining > 0)
            {
                SpawnFlame();
                spawnTimer = burstRemaining > 0 ? 0.03f : Mathf.Lerp(0.105f, 0.045f, intensity);
                burstRemaining = Mathf.Max(0, burstRemaining - 1);
            }

            float pulseAmount = Mathf.Lerp(0.018f, 0.055f, intensity);
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 15f) * pulseAmount;
            rectTransform.localScale = baseScale * pulse;
        }
        else
        {
            burning = false;
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale,
                baseScale, deltaTime * 10f);
        }

        if (flames == null) return;

        foreach (Flame flame in flames)
        {
            if (!flame.image.gameObject.activeSelf) continue;

            flame.life -= deltaTime;
            if (flame.life <= 0f)
            {
                flame.image.gameObject.SetActive(false);
                continue;
            }

            anyAlive = true;
            flame.rect.anchoredPosition += flame.velocity * deltaTime;
            flame.rect.localRotation *= Quaternion.Euler(0f, 0f, flame.spin * deltaTime);

            float normalized = flame.life / flame.maxLife;
            float width = Mathf.Lerp(0.35f, 1f, normalized);
            flame.rect.localScale = new Vector3(width, Mathf.Lerp(0.55f, 1f, normalized), 1f);

            Color color = flame.color;
            color.a = Mathf.SmoothStep(0f, 1f, Mathf.Min(normalized * 3f, (1f - normalized) * 5f));
            flame.image.color = color;
        }

        if (!burning && !anyAlive) {
            rectTransform.localScale = baseScale;
        }
    }

    public void SetCombo(int comboCount)
    {
        comboCount = Mathf.Clamp(comboCount, 0, 10);
        UpdateHeat(comboCount);

        if (comboCount < 5) return;

        intensity = Mathf.InverseLerp(5f, 10f, comboCount);
        burning = true;
        burstRemaining = Mathf.Clamp(8 + comboCount * 2, 14, particleCount);
        spawnTimer = 0f;
        flameRoot.localScale = Vector3.one * Mathf.Lerp(1f, 1.55f, intensity);
    }

    public void StopFire()
    {
        burning = false;
        burstRemaining = 0;
        intensity = 0f;
        UpdateHeat(0);

        if (flames != null)
        {
            foreach (Flame flame in flames) {
                flame.image.gameObject.SetActive(false);
            }
        }

        if (rectTransform != null) {
            rectTransform.localScale = baseScale;
        }
    }

    [ContextMenu("Preview Fire")]
    private void PreviewFire()
    {
        SetCombo(10);
    }

    private void BuildHeatOverlay()
    {
        GameObject overlay = new GameObject("ComboHeat", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image));
        overlay.layer = gameObject.layer;
        overlay.transform.SetParent(transform, false);

        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        overlayRect.SetAsFirstSibling();

        heatOverlay = overlay.GetComponent<Image>();
        heatOverlay.sprite = flameSprite;
        heatOverlay.type = Image.Type.Sliced;
        heatOverlay.raycastTarget = false;
        UpdateHeat(0);
    }

    private void UpdateHeat(int comboCount)
    {
        if (heatOverlay == null) return;

        float heat = Mathf.Clamp01(comboCount / 4f);
        Color color = heatColor;
        color.a = Mathf.Lerp(0f, 0.42f, heat);
        heatOverlay.color = color;
    }

    private void BuildPool()
    {
        GameObject root = new GameObject("FireEffect", typeof(RectTransform));
        root.layer = gameObject.layer;
        flameRoot = root.GetComponent<RectTransform>();
        flameRoot.SetParent(transform, false);
        flameRoot.anchorMin = Vector2.zero;
        flameRoot.anchorMax = Vector2.one;
        flameRoot.offsetMin = Vector2.zero;
        flameRoot.offsetMax = Vector2.zero;
        flameRoot.SetSiblingIndex(1);

        flames = new Flame[particleCount];
        for (int i = 0; i < flames.Length; i++)
        {
            GameObject particle = new GameObject("Flame", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image));
            particle.layer = gameObject.layer;
            particle.transform.SetParent(flameRoot, false);

            RectTransform particleRect = particle.GetComponent<RectTransform>();
            particleRect.anchorMin = particleRect.anchorMax = new Vector2(0.5f, 0.5f);
            particleRect.pivot = new Vector2(0.5f, 0.1f);

            Image image = particle.GetComponent<Image>();
            image.sprite = flameSprite;
            image.type = Image.Type.Simple;
            image.raycastTarget = false;
            particle.SetActive(false);

            flames[i] = new Flame { rect = particleRect, image = image };
        }
    }

    private void SpawnFlame()
    {
        Flame flame = null;
        foreach (Flame candidate in flames)
        {
            if (!candidate.image.gameObject.activeSelf) {
                flame = candidate;
                break;
            }
        }

        if (flame == null) return;

        float edgeBias = Random.value < 0.42f ? Mathf.Sign(Random.value - 0.5f) : Random.Range(-0.55f, 0.55f);
        float x = edgeBias * rectTransform.rect.width * 0.48f;

        flame.maxLife = flame.life = Random.Range(0.55f, 0.95f);
        float sizeMultiplier = Mathf.Lerp(1f, 1.7f, intensity);
        flame.velocity = new Vector2(Random.Range(-9f, 9f),
            Random.Range(62f, 105f) * Mathf.Lerp(1f, 1.32f, intensity));
        flame.spin = Random.Range(-70f, 70f);
        flame.color = Random.value < 0.28f ? coreColor :
            (Random.value < 0.62f ? flameColor : emberColor);

        // Start at the upper edge so the flames rise visibly above the score button.
        flame.rect.anchoredPosition = new Vector2(x, rectTransform.rect.height * 0.38f);
        flame.rect.sizeDelta = new Vector2(Random.Range(13f, 21f), Random.Range(28f, 48f)) * sizeMultiplier;
        flame.rect.localScale = Vector3.one;
        flame.rect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-12f, 12f));
        flame.image.color = flame.color;
        flame.image.gameObject.SetActive(true);
    }
}
