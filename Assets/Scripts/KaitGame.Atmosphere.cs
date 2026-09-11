using UnityEngine;

public sealed partial class KaitGame
{
    private KaitAtmosphereGraphic atmosphere;
    private float chainIdleSeconds, greyStrength, dangerStrength, nextDangerCheck;
    private int atmosphereTurn = -1, atmosphereStep = -1;
    private bool atmosphereDanger;

    private void UpdateAtmosphere()
    {
        if (canvas == null) return;
        if (atmosphere == null)
        {
            var go = new GameObject("Time Stop and Danger Edges", typeof(RectTransform), typeof(KaitAtmosphereGraphic));
            go.transform.SetParent(canvas.transform, false);
            atmosphere = go.GetComponent<KaitAtmosphereGraphic>();
            var rect = atmosphere.rectTransform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            atmosphere.raycastTarget = false;
        }
        bool visible = !run.ended && !TutorialBlocksInput();
        bool waiting = visible && !run.IsYummn && run.chainActive && !busy;
        if (!waiting || atmosphereTurn != run.turn || atmosphereStep != run.chainStepCount) chainIdleSeconds = 0;
        atmosphereTurn = run.turn; atmosphereStep = run.chainStepCount;
        if (waiting) chainIdleSeconds += Time.unscaledDeltaTime;
        float target = Mathf.SmoothStep(0, 1, Mathf.Clamp01((chainIdleSeconds - 1.2f) / 1.8f));
        greyStrength = Mathf.MoveTowards(greyStrength, target, Time.unscaledDeltaTime * 4);
        if (Time.unscaledTime >= nextDangerCheck)
        {
            nextDangerCheck = Time.unscaledTime + .1f;
            atmosphereDanger = run.IsKateInImminentDanger();
        }
        bool enemyClockActive=!run.IsYummn||!run.Yummn.rules.Legacy||run.KiPhase==YummnPhase.Exhausted;
        dangerStrength = Mathf.MoveTowards(dangerStrength, visible && !busy && enemyClockActive && atmosphereDanger ? 1 : 0, Time.unscaledDeltaTime * 5);
        atmosphere.ConfigureSplit(styleSplit);
        atmosphere.SetState(greyStrength, dangerStrength, Time.unscaledTime);
    }
}
