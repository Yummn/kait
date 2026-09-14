using UnityEngine;

public sealed partial class KaitGame
{
    // Called after stopping old coroutines, before selecting or rendering a new run.
    // Destroying actors alone is insufficient: RefreshBattle prefers animation snapshots.
    private void ResetRunPresentation()
    {
        victoryPresentationStarted = victoryPresentationComplete = false;
        animatedEnemies = null;
        animatedSpawns = null;
        displayedThreat = null;
        hideThreatValues = hideKate = false;
        displayKate = null;
        impactCells.Clear();
        firingWarlocks.Clear();
        yummnBufferedDirection = null;
        yummnAcceptBuffer = false;
        yummnBufferedTurn = yummnBufferedAction = -1;
        yummnPunchPose.Reset();
        ClearYummnLogicalGhosts();
        ClearAllCombatEffects();

        // These views own their Update/animation lifecycle, not this component's coroutines.
        // Hide synchronously before deferred destruction so neither a callback nor the next
        // frame can display a previous run's terrain or hit effect.
        if (canvas != null)
        {
            foreach (var effect in canvas.GetComponentsInChildren<YummnV08Effect>(true))
                RemoveRunVisual(effect.gameObject);
            foreach (var effect in canvas.GetComponentsInChildren<KaitCombatEffectGraphic>(true))
                RemoveRunVisual(effect.gameObject);
        }
        if (yummnIce != null) RemoveRunVisual(yummnIce.gameObject);
        if (yummnDarkness != null) RemoveRunVisual(yummnDarkness.gameObject);
        if (yummnDecoy != null) RemoveRunVisual(yummnDecoy.gameObject);
        foreach (var mark in yummnPalmImages) if (mark != null) RemoveRunVisual(mark.gameObject);
        yummnPalmImages.Clear();
        yummnIce = yummnDarkness = yummnDecoy = yummnPalm = null;
        System.Array.Clear(yummnShadows, 0, yummnShadows.Length);
        repoolPillar = null;
        repoolPillarCell = yummnVisualIce = yummnVisualDarkness = yummnVisualDecoy = YummnRun.NoCell;
        chainIdleSeconds = greyStrength = dangerStrength = nextDangerCheck = 0;
        atmosphereDanger = false;
        atmosphereTurn = atmosphereStep = -1;
        if (atmosphere != null) atmosphere.SetState(0, 0, Time.unscaledTime, 0);
        GameAudio.InterruptActionSounds();
    }

    private static void RemoveRunVisual(GameObject visual)
    {
        if (visual == null) return;
        visual.SetActive(false);
        if (Application.isPlaying) Destroy(visual);
        else DestroyImmediate(visual);
    }
}
