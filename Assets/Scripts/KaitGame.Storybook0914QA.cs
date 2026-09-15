using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public sealed partial class KaitGame
{
    private IEnumerator VerifyStorybook0914()
    {
        run.skills.Clear();run.passives.Clear();run.rewardQueue.Clear();
        run.skills.AddRange(new[]{KaitSkill.FrostBreath,KaitSkill.PhantomSlide,KaitSkill.MageHand});
        run.passives.AddRange(new[]{KaitPassive.SpellEcho,KaitPassive.BountyJar,KaitPassive.StunStrike});
        run.Yummn.profile.maxKi=7;run.Yummn.ki=5;run.Yummn.phase=YummnPhase.Burst;
        foreach(var enemy in new[]{new KaitEnemy{id=9891,type=KaitEnemyType.Archer,pos=new Vector2Int(2,4),hp=2,maxHp=2,life=KaitEnemyLife.Active},new KaitEnemy{id=9892,type=KaitEnemyType.Warlock,pos=new Vector2Int(2,2),hp=2,maxHp=2,life=KaitEnemyLife.Active}})run.enemies.Add(enemy);
        int[,] numbers={{0,2,0,16,0},{4,8,2,0,0},{0,0,4,0,2},{8,4,0,32,0},{0,0,0,0,0}};
        for(int y=0;y<5;y++)for(int x=0;x<5;x++)run.threat[x,y]=numbers[y,x];
        RefreshAll();
        foreach(var size in new[]{new Vector2Int(2400,1080),new Vector2Int(1920,1080)})
        {
            yield return SetMobileSize0912(size.x,size.y);
            var hud=gameContent.GetComponent<KaitStorybookLayout>().Hud;
            foreach(var pip in actionPips)
            {
                var r=pip.rectTransform.rect;
                if(Mathf.Abs(r.width/r.height-328f/536)>.001f||!pip.preserveAspect)Debug.LogError("STORYBOOK0914_QA: stretched qi");
                AssertInHud0914(pip.rectTransform,hud);
            }
            foreach(var heart in storybookHearts)AssertInHud0914(heart.rectTransform,hud);
            AssertInHud0914((RectTransform)hud.Find("Fist"),hud);
            CheckMobile0912(storybookPortrait.rectTransform,"portrait");
            yield return CaptureMobile0912("0914-yummn-"+size.x);
        }
        var key=waitButton as KaitStorybookButton;
        var before=key.transform.localScale;var rootPos=key.transform.position;
        var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,key.transform.position)};
        key.OnPointerEnter(pointer);key.OnPointerDown(pointer);yield return new WaitForSecondsRealtime(.15f);
        if(key.transform.localScale!=before||key.transform.position!=rootPos||key.Face.anchoredPosition.y>=0)Debug.LogError("STORYBOOK0914_QA: button rim moved or face did not depress");
        yield return CaptureMobile0912("0914-button-down");key.OnPointerUp(pointer);key.OnPointerExit(pointer);
        OpenMenuSettings();yield return new WaitForSecondsRealtime(.2f);
        foreach(var label in settingsOverlay.GetComponentsInChildren<Text>())
            if(!string.IsNullOrWhiteSpace(label.text)&&label.cachedTextGenerator.vertexCount==0)Debug.LogError("STORYBOOK0914_QA: missing settings label "+label.text);
        yield return CaptureMobile0912("0914-settings");settingsOverlay.SetActive(false);
        var original=actionPips[0].rectTransform.sizeDelta;
        run.Yummn.profile.maxKi=9;run.Yummn.ki=9;RefreshAll();yield return null;
        if(actionPips.Length!=9||actionPips[0].rectTransform.sizeDelta!=original)Debug.LogError("STORYBOOK0914_QA: nondefault count squeezed qi");
        yield return CaptureMobile0912("0914-nine-qi-allowed-overflow");
        run.Yummn.profile.maxKi=7;run.Yummn.ki=7;RefreshAll();
        var pack=new KaitRewardPack{id=9914,characterId=KaitCharacter.Yummn};pack.choices.AddRange(YummnCatalog.Cards.GetRange(0,3));
        run.rewardQueue.Enqueue(pack);RefreshAll();yield return new WaitForSecondsRealtime(.3f);
        rewardDeck.SendMessage("Select",(object)0);yield return new WaitForSecondsRealtime(.4f);
        yield return CaptureMobile0912("0914-card-tray");
        run.SelectCharacter(KaitCharacter.Kait,9914);ConfigureCharacterVisuals();run.StateCommitted=null;EnsureKaitSpine();RefreshAll();
        yield return SetMobileSize0912(2400,1080);yield return CaptureMobile0912("0914-kait-wide");
        Debug.Log("STORYBOOK0914_QA_COMPLETE");
    }
    private void AssertInHud0914(RectTransform child,RectTransform hud)
    {
        var corners=new Vector3[4];child.GetWorldCorners(corners);
        var safe=hud.rect;safe.xMin+=8;safe.xMax-=8;safe.yMin+=8;safe.yMax-=8;
        foreach(var p in corners)if(!safe.Contains((Vector2)hud.InverseTransformPoint(p)))Debug.LogError("STORYBOOK0914_QA: HUD inner padding "+child.name);
    }
}
