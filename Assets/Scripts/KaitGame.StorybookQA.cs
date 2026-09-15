using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class KaitGame
{
    private IEnumerator VerifyStorybook098()
    {
        mainMenu.Select(KaitCharacter.Yummn);
        mainMenu.gameObject.SetActive(true);gameplayRoot.SetActive(false);
        yield return null;CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/storybook098-home.png");
        mainMenu.LibraryButton.onClick.Invoke();yield return null;
        var library=canvas.GetComponentInChildren<KaitCardLibrary>();
        foreach(var character in new[]{KaitCharacter.Kait,KaitCharacter.Yummn})
        {
            library.Select(character,-1);yield return null;
            CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/storybook098-library-"+character+".png");
            for(int rarity=0;rarity<3;rarity++)
            {
                library.Select(character,rarity);
                for(int page=0;page<(library.Total+5)/6;page++)
                {
                    yield return null;Canvas.ForceUpdateCanvases();
                    foreach(var label in library.GetComponentsInChildren<Text>())
                        if(!string.IsNullOrWhiteSpace(label.text)&&label.cachedTextGenerator.vertexCount==0)Debug.LogError("STORYBOOK098_QA: missing library text "+label.text);
                    library.ChangePage(1);
                }
            }
        }
        library.Select(KaitCharacter.Yummn,2);yield return null;CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/storybook098-library-rare.png");
        Destroy(library.gameObject);yield return null;
        mainMenu.SettingsButton.onClick.Invoke();yield return null;
        foreach(var label in settingsOverlay.GetComponentsInChildren<Text>())
            if(!string.IsNullOrWhiteSpace(label.text)&&label.cachedTextGenerator.vertexCount==0)Debug.LogError("STORYBOOK098_QA: missing settings text "+label.text);
        CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/storybook098-settings.png");
        settingsOverlay.SetActive(false);mainMenu.TutorialButton.onClick.Invoke();yield return null;
        CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/storybook098-tutorial.png");
        tutorialOverlay.SetActive(false);mainMenu.gameObject.SetActive(false);gameplayRoot.SetActive(true);
        for(int y=0;y<5;y++)for(int x=0;x<5;x++)run.threat[x,y]=(x+y)%3==0?1<<((x+y)%4+1):0;
        run.enemies.Add(new KaitEnemy{id=9877,type=KaitEnemyType.Archer,pos=new Vector2Int(2,4),hp=2,maxHp=2,life=KaitEnemyLife.Active});
        run.enemies.Add(new KaitEnemy{id=9878,type=KaitEnemyType.Swordsman,pos=new Vector2Int(4,1),hp=3,maxHp=3,life=KaitEnemyLife.Active});
        RefreshAll();yield return new WaitForSecondsRealtime(.6f);
        CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/storybook098-battle-Yummn.png");
        int oldMaxKi=run.Yummn.profile.maxKi;run.Yummn.profile.maxKi=9;RefreshYummnKiDisplay();yield return null;
        foreach(var pip in actionPips)
            if(Mathf.Abs(pip.rectTransform.anchoredPosition.x)+pip.rectTransform.rect.width*.5f>121)Debug.LogError("STORYBOOK098_QA: nine qi do not fit");
        if(storybookPortrait.rectTransform.anchoredPosition.x-storybookPortrait.rectTransform.rect.width*.5f>=-300)Debug.LogError("STORYBOOK098_QA: portrait should overlap outer left edge");
        foreach(var pip in actionPips)if(pip.rectTransform.sizeDelta.y!=storybookHearts[0].rectTransform.sizeDelta.y)Debug.LogError("STORYBOOK098_QA: unequal health and qi heights");
        foreach(var key in controlsPanel.GetComponentsInChildren<Button>())if(((RectTransform)key.transform).rect.width<76)Debug.LogError("STORYBOOK098_QA: small control key");
        CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/storybook0910-nine-qi.png");
        run.Yummn.profile.maxKi=oldMaxKi;RefreshYummnKiDisplay();
        // Inspect the actual reward state, not only an empty battlefield.
        var pack=new KaitRewardPack{id=9901,characterId=KaitCharacter.Yummn};
        foreach(var def in YummnCatalog.Cards)
        {
            if(pack.choices.Count==0&&def.kind==KaitAbilityKind.Active&&def.rarity==KaitRarity.Uncommon)pack.choices.Add(def);
            else if(pack.choices.Count==1&&def.kind==KaitAbilityKind.Passive)pack.choices.Add(def);
            else if(pack.choices.Count==2&&def.kind==KaitAbilityKind.Active&&def.rarity==KaitRarity.Rare){pack.choices.Add(def);break;}
        }
        if(pack.choices.Count<3){pack.choices.Clear();pack.choices.AddRange(YummnCatalog.Cards.GetRange(0,3));}
        run.rewardQueue.Clear();run.rewardQueue.Enqueue(pack);RefreshAll();yield return new WaitForSecondsRealtime(.6f);
        CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/storybook099-rewards.png");
        foreach(var card in canvas.GetComponentsInChildren<KaitSkillCard>())if(card.IsCandidate)
        {
            foreach(var label in card.GetComponentsInChildren<Text>())
                if(!string.IsNullOrWhiteSpace(label.text)&&label.cachedTextGenerator.vertexCount==0)Debug.LogError("STORYBOOK098_QA: reward text missing "+label.text);
        }
        run.rewardQueue.Clear();RefreshAll();yield return null;
        foreach(var card in skillDeck.Owned)if(card.gameObject.activeSelf&&!card.ShouldPreviewAt(Time.unscaledTime)&&card.GetComponentInChildren<KaitCardLogo>(true).gameObject.activeSelf)Debug.LogError("STORYBOOK098_QA: folded active icon should be hidden");
        foreach(var label in controlsPanel.GetComponentsInChildren<Text>())if(label.text.Contains("重开"))Debug.LogError("STORYBOOK098_QA: restart button remains");
        foreach(var heart in storybookHearts)if(heart!=null&&heart.gameObject.activeSelf&&Mathf.Abs(heart.rectTransform.anchoredPosition.x)+heart.rectTransform.sizeDelta.x*.5f>290)Debug.LogError("STORYBOOK098_QA: heart outside HUD");
        foreach(var card in canvas.GetComponentsInChildren<KaitSkillCard>())
        {styleSplit.GetLocalSplits(card.Rect,out float bottom,out float top);if(bottom<=1||top<=1)Debug.LogError("STORYBOOK098_QA: card illustration still cut");}
        var preview=skillDeck.Owned[0];preview.PreviewAt(new Vector2(0,-180));yield return new WaitForSecondsRealtime(.3f);
        CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/storybook098-card-split.png");
        preview.DismissPreview();
        ResetRunPresentation();run.SelectCharacter(KaitCharacter.Kait,988);run.StateCommitted=null;
        run.skills.AddRange(new[]{KaitSkill.SwiftBoots,KaitSkill.IceTomb,KaitSkill.CatAgility});
        run.passives.AddRange(new[]{KaitPassive.BirdEye,KaitPassive.BloodBookmark,KaitPassive.SweepTail});
        ConfigureCharacterVisuals();EnsureKaitSpine();RefreshAll();yield return new WaitForSecondsRealtime(.6f);
        CaptureCanvasToPng("C:/Users/yummn/Downloads/kait/Logs/storybook098-battle-Kait.png");
        // Verify the board footprint remains equal and fits landscape 16:10 and 20:9.
        var parent=(RectTransform)gameContent.parent;var oldMin=parent.anchorMin;var oldMax=parent.anchorMax;var oldSize=parent.sizeDelta;
        parent.anchorMin=parent.anchorMax=Vector2.one*.5f;
        foreach(var size in new[]{new Vector2(1728,1080),new Vector2(2400,1080)})
        {
            parent.sizeDelta=size;yield return null;yield return null;
            float scale=gameContent.localScale.x;
            if(1500*scale>size.x||786*scale>size.y)Debug.LogError("STORYBOOK098_QA: content does not fit "+size);
        }
        parent.anchorMin=oldMin;parent.anchorMax=oldMax;parent.sizeDelta=oldSize;
        Debug.Log("STORYBOOK098_QA_COMPLETE");
    }
}
