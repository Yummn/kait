using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class KaitGame
{
    private IEnumerator VerifyYummnDefaultsRuntime(string path)
    {
        yield return new WaitForSecondsRealtime(.5f);
        var preset=YummnPreset();
        run.SelectCharacter(KaitCharacter.Yummn,9137,preset);
        ConfigureCharacterVisuals();EnsureKaitSpine();
        mainMenu.gameObject.SetActive(false);gameplayRoot.SetActive(true);
        settingsOverlay.SetActive(true);settingsOverlay.transform.SetAsLastSibling();RefreshCharacterSettings();
        yield return null;Canvas.ForceUpdateCanvases();
        if(preset.MaxKi!=7||preset.MovementCostMode!=YummnMovementCostMode.PerCell||preset.KillKi!=3||
           preset.Supply!=YummnTileSupplyMode.EffectiveMove||preset.AttackAdvancesEnemyPhase||
           !preset.ExhaustionNeedsFullKi||!preset.AttackCostsOne||preset.MovementAdvancesEnemyPhase||
           preset.SpawnFromEight||!preset.KiGuard||preset.RewardMergeValue!=16)
            Debug.LogError("YUMMN_DEFAULTS_QA: default rules mismatch");
        string all="";
        foreach(var label in settingsOverlay.GetComponentsInChildren<Text>(true))
        {
            all+="|"+label.text;
            if(label.gameObject.activeInHierarchy&&label.verticalOverflow==VerticalWrapMode.Truncate&&label.preferredHeight>label.rectTransform.rect.height+2)
                Debug.LogError("YUMMN_DEFAULTS_QA: overflowing text "+label.text);
        }
        foreach(var removed in new[]{"长按连续输入","攻击也推进敌方回合","从8开始出怪","技能获得阈值"})
            if(all.Contains(removed))Debug.LogError("YUMMN_DEFAULTS_QA: removed option remains "+removed);
        if(!all.Contains("补给：有效移动至少一格补一个2"))Debug.LogError("YUMMN_DEFAULTS_QA: unified supply copy missing");
        CaptureCanvasToPng(path+".settings.png");
        foreach(var button in yummnRuleButtons)
        {
            var label=button.GetComponentInChildren<Text>();
            var mesh=label.canvasRenderer.GetMesh();
            if(mesh.vertexCount==0)Debug.LogError("YUMMN_DEFAULTS_QA: empty label mesh "+label.text);
            Debug.Log("SETTINGS_LABEL: "+label.text+" rect="+label.rectTransform.rect.size+" vertices="+mesh.vertexCount+" bounds="+mesh.bounds.size);
            var legacy=label.GetGenerationSettings(new Vector2(label.rectTransform.rect.width,22));
            legacy.verticalOverflow=VerticalWrapMode.Truncate;
            legacy.horizontalOverflow=HorizontalWrapMode.Wrap;
            var generator=new TextGenerator();generator.Populate(label.text,legacy);
            Debug.Log("SETTINGS_LEGACY_LAYOUT: "+label.text+" vertices="+generator.vertexCount);
            string before=label.text;button.onClick.Invoke();
            yield return null;Canvas.ForceUpdateCanvases();
            mesh=label.canvasRenderer.GetMesh();
            if(label.text==before||mesh.vertexCount==0)Debug.LogError("YUMMN_DEFAULTS_QA: clicked label not rendered "+label.text);
        }
        yield return new WaitForSecondsRealtime(.3f);
        Debug.Log("YUMMN_DEFAULTS_QA_COMPLETE");Application.Quit();
    }
}
