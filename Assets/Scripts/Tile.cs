using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public TileState state { get; private set; }
    public TileCell cell { get; private set; }
    public bool locked { get; set; }

    private Image background;
    private TextMeshProUGUI text;
    private RectTransform rectTransform;
    private Coroutine scaleRoutine;
    private bool moving;
    private bool scaling;
    private float idlePhase;
    private Vector2 restAnchoredPosition;
    private bool hasRestPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        background = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        idlePhase = Random.Range(0f, Mathf.PI * 2f);

        Shadow shadow = GetComponent<Shadow>();
        if (shadow == null) {
            shadow = gameObject.AddComponent<Shadow>();
        }

        shadow.effectColor = new Color32(70, 59, 66, 90);
        shadow.effectDistance = new Vector2(3f, -4f);
        shadow.useGraphicAlpha = true;
    }

    private void Update()
    {
        if (cell == null || moving || scaling) return;

        float time = Time.unscaledTime;
        float offsetX = Mathf.Round(Mathf.Sin(time * 1.75f + idlePhase) * 2f);
        float offsetY = Mathf.Round(Mathf.Sin(time * 1.35f + idlePhase * 0.73f) * 2f);

        rectTransform.anchoredPosition = restAnchoredPosition + new Vector2(offsetX, offsetY);
        text.rectTransform.localRotation = Quaternion.Euler(0f, 0f,
            Mathf.Sin(time * 1.9f + idlePhase) * 2.4f);
    }

    public void SetState(TileState state)
    {
        this.state = state;

        background.color = state.backgroundColor;
        text.color = state.textColor;
        text.text = state.number.ToString();
    }

    public void Spawn(TileCell cell)
    {
        if (this.cell != null) {
            this.cell.tile = null;
        }

        this.cell = cell;
        this.cell.tile = this;

        transform.position = cell.transform.position;
        CaptureRestPosition();
        ResetIdlePose();
        StartScaleAnimation(0f, 1f, 0.24f, true);
    }

    public void MoveTo(TileCell cell)
    {
        ResetIdlePose();

        if (this.cell != null) {
            this.cell.tile = null;
        }

        this.cell = cell;
        this.cell.tile = this;

        StartCoroutine(AnimateMove(cell.transform.position, false));
    }

    public void Merge(TileCell cell)
    {
        ResetIdlePose();

        if (this.cell != null) {
            this.cell.tile = null;
        }

        this.cell = null;
        cell.tile.locked = true;

        StartCoroutine(AnimateMove(cell.transform.position, true));
    }

    public void PlayMergePulse()
    {
        rectTransform.SetAsLastSibling();
        StartScaleAnimation(rectTransform.localScale.x, 1f, 0.22f, true, 1.28f);
    }

    private IEnumerator AnimateMove(Vector3 to, bool merging)
    {
        if (scaleRoutine != null)
        {
            StopCoroutine(scaleRoutine);
            scaleRoutine = null;
            scaling = false;
            rectTransform.localScale = Vector3.one;
        }

        moving = true;
        float elapsed = 0f;
        float duration = merging ? 0.14f : 0.16f;

        Vector3 from = transform.position;
        while (elapsed < duration)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - progress, 3f);

            transform.position = Vector3.LerpUnclamped(from, to, eased);

            if (merging && progress > 0.55f) {
                float shrink = Mathf.InverseLerp(1f, 0.55f, progress);
                rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, 0.72f, shrink);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = to;
        rectTransform.localRotation = Quaternion.identity;
        text.rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
        CaptureRestPosition();
        moving = false;

        if (merging) {
            Destroy(gameObject);
        }
    }

    private void StartScaleAnimation(float from, float to, float duration,
        bool overshoot, float peak = 1.14f)
    {
        if (scaleRoutine != null) {
            StopCoroutine(scaleRoutine);
        }

        scaleRoutine = StartCoroutine(AnimateScale(from, to, duration, overshoot, peak));
    }

    private IEnumerator AnimateScale(float from, float to, float duration,
        bool overshoot, float peak)
    {
        scaling = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            float scale;

            if (overshoot && progress < 0.65f) {
                scale = Mathf.Lerp(from, peak, EaseOutCubic(progress / 0.65f));
            } else if (overshoot) {
                scale = Mathf.Lerp(peak, to, SmoothStep((progress - 0.65f) / 0.35f));
            } else {
                scale = Mathf.Lerp(from, to, SmoothStep(progress));
            }

            rectTransform.localScale = Vector3.one * scale;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        rectTransform.localScale = Vector3.one * to;
        rectTransform.localRotation = Quaternion.identity;
        text.rectTransform.localRotation = Quaternion.identity;
        scaling = false;
        scaleRoutine = null;
    }

    private void CaptureRestPosition()
    {
        restAnchoredPosition = rectTransform.anchoredPosition;
        hasRestPosition = true;
    }

    private void ResetIdlePose()
    {
        if (hasRestPosition) {
            rectTransform.anchoredPosition = restAnchoredPosition;
        }

        rectTransform.localRotation = Quaternion.identity;
        text.rectTransform.localRotation = Quaternion.identity;
    }

    private static float EaseOutCubic(float value)
    {
        return 1f - Mathf.Pow(1f - value, 3f);
    }

    private static float SmoothStep(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * (3f - 2f * value);
    }

}
