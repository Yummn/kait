using System.Collections;
using UnityEngine;

public sealed partial class KaitGame
{
    // Real framebuffer captures: do not route through the legacy fixed-16:9 camera.
    private IEnumerator CaptureMobile0912(string name)
    {
        yield return new WaitForEndOfFrame();
        var layout=gameContent.GetComponent<KaitStorybookLayout>();
        Debug.Log("MOBILE0912_FRAME "+name+" root="+layout.UiRoot.rect+" safe="+Screen.safeArea);
        foreach(var card in canvas.GetComponentsInChildren<KaitSkillCard>())if(!card.IsCandidate)CheckMobilePixels0912(card.Rect,"active");
        foreach(var card in canvas.GetComponentsInChildren<KaitPassiveCard>())if(!card.IsCandidate)CheckMobilePixels0912(card.Rect,"passive");
        CheckMobilePixels0912(layout.Help,"tutorial");CheckMobilePixels0912(layout.Settings,"settings");
        var texture=ScreenCapture.CaptureScreenshotAsTexture();
        // The first swapchain frame after launch can still be black. Don't
        // silently approve a file that contains no rendered game at all.
        bool rendered=false;
        for(int attempt=0;attempt<4;attempt++)
        {
            for(int sy=1;sy<=3;sy++)for(int sx=1;sx<=3;sx++)
            {var c=texture.GetPixel(texture.width*sx/4,texture.height*sy/4);if(c.r+c.g+c.b>.1f)rendered=true;}
            if(rendered)break;
            Destroy(texture);yield return new WaitForSecondsRealtime(.3f);yield return new WaitForEndOfFrame();
            texture=ScreenCapture.CaptureScreenshotAsTexture();
        }
        if(!rendered)Debug.LogError("MOBILE0912_QA: black capture "+name);
        System.IO.File.WriteAllBytes(Application.dataPath+"/../../Logs/mobile0912-"+name+".png",texture.EncodeToPNG());
        Debug.Log("MOBILE0912_CAPTURE "+name+" "+texture.width+"x"+texture.height);
        Destroy(texture);
    }
    private void CheckMobilePixels0912(RectTransform rect,string label)
    {
        var layout=gameContent.GetComponent<KaitStorybookLayout>();
        Rect safe=layout.SafeNormalizedOverride.HasValue?KaitStorybookLayout.MapSafeRect(new Rect(0,0,Screen.width,Screen.height),layout.SafeNormalizedOverride.Value):Screen.safeArea;
        safe.xMin-=1;safe.yMin-=1;safe.xMax+=1;safe.yMax+=1;
        var corners=new Vector3[4];rect.GetWorldCorners(corners);
        foreach(var corner in corners)if(!safe.Contains(RectTransformUtility.WorldToScreenPoint(null,corner)))
        {Debug.LogError("MOBILE0912_QA: rendered outside safe area "+label);break;}
    }
    private IEnumerator SetMobileSize0912(int width,int height)
    {
        Screen.SetResolution(width,height,false);
        // Resolution requests apply after rendering. Let the next canvas and UI update settle too.
        yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();gameContent.GetComponent<KaitStorybookLayout>().ApplyLayout();
        yield return new WaitForSecondsRealtime(.6f);
    }
    private void CheckMobile0912(RectTransform rect,string label)
    {
        var layout=gameContent.GetComponent<KaitStorybookLayout>();
        var corners=new Vector3[4];rect.GetWorldCorners(corners);
        var safe=layout.SafeRect;safe.xMin-=1;safe.xMax+=1;safe.yMin-=1;safe.yMax+=1;
        foreach(var corner in corners)
            if(!safe.Contains((Vector2)layout.UiRoot.InverseTransformPoint(corner)))
            {Debug.LogError("MOBILE0912_QA: outside safe area "+label);break;}
    }
    private IEnumerator VerifyMobileLayout0912()
    {
        var layout=gameContent.GetComponent<KaitStorybookLayout>();
        for(int y=0;y<5;y++)for(int x=0;x<5;x++)run.threat[x,y]=(x+y)%3==0?1<<((x+y)%4+1):0;
        run.enemies.Add(new KaitEnemy{id=9877,type=KaitEnemyType.Archer,pos=new Vector2Int(2,4),hp=2,maxHp=2,life=KaitEnemyLife.Active});
        RefreshAll();
        foreach(var size in new[]{new Vector2Int(2400,1080),new Vector2Int(1920,1080),new Vector2Int(1920,1200)})
        {
            yield return SetMobileSize0912(size.x,size.y);
            CheckMobile0912(layout.Help,"tutorial");CheckMobile0912(layout.Settings,"settings");
            CheckMobile0912((RectTransform)gameContent.Find("Battle Board"),"battle board");
            CheckMobile0912((RectTransform)gameContent.Find("Threat Panel"),"2048 board");
            CheckMobile0912(storybookPortrait.rectTransform,"portrait");
            foreach(var card in canvas.GetComponentsInChildren<KaitSkillCard>())if(!card.IsCandidate)CheckMobile0912(card.Rect,"active");
            foreach(var card in canvas.GetComponentsInChildren<KaitPassiveCard>())if(!card.IsCandidate)CheckMobile0912(card.Rect,"passive");
            yield return CaptureMobile0912(size.x+"x"+size.y);
        }
        yield return SetMobileSize0912(2400,1080);layout.SafeNormalizedOverride=new Rect(.03f,.025f,.95f,.95f);
        run.skills.Clear();run.passives.Clear();
        foreach(var def in YummnCatalog.Cards)if(def.kind==KaitAbilityKind.Active&&!run.skills.Contains(def.skill)&&run.skills.Count<6)run.skills.Add(def.skill);
        RefreshAll();yield return new WaitForSecondsRealtime(.8f);
        foreach(var card in skillDeck.Owned)if(card.gameObject.activeSelf)CheckMobile0912(card.Rect,"six active");
        yield return CaptureMobile0912("safe-six-active");
        var pointer=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        skillDeck.Owned[0].OnPointerEnter(pointer);yield return new WaitForSecondsRealtime(.5f);
        CheckMobile0912(skillDeck.Owned[0].Rect,"expanded active");yield return CaptureMobile0912("expanded");
        skillDeck.Owned[0].OnPointerExit(pointer);
        run.skills.Clear();
        foreach(var def in YummnCatalog.Cards)if(def.kind==KaitAbilityKind.Passive&&!run.passives.Contains(def.passive)&&run.passives.Count<6)run.passives.Add(def.passive);
        RefreshAll();yield return new WaitForSecondsRealtime(.8f);
        foreach(var card in canvas.GetComponentsInChildren<KaitPassiveCard>())if(!card.IsCandidate)CheckMobile0912(card.Rect,"six passives");
        yield return CaptureMobile0912("safe-six-passives");
        var pack=new KaitRewardPack{id=9901,characterId=KaitCharacter.Yummn};pack.choices.AddRange(YummnCatalog.Cards.GetRange(0,3));
        run.rewardQueue.Enqueue(pack);RefreshAll();yield return new WaitForSecondsRealtime(.6f);
        yield return CaptureMobile0912("rewards");
        Debug.Log("MOBILE0912_QA_COMPLETE");
    }
}
