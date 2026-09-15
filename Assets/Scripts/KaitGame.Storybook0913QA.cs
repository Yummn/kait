using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed partial class KaitGame
{
    private IEnumerator VerifyStorybook0913()
    {
        yield return SetMobileSize0912(2400,1080);
        var layout=gameContent.GetComponent<KaitStorybookLayout>();
        foreach(var skill in new[]{KaitSkill.FrostBreath,KaitSkill.PhantomSlide})
        {
            foreach(var direction in new[]{0,1,2,3})
            {
                run.enemies.Clear();run.spawns.Clear();run.rewardQueue.Clear();run.skills.Clear();run.passives.Clear();
                run.skills.Add(skill);typeof(KaitRun).GetProperty("katePos").SetValue(run,new Vector2Int(3,3));run.Yummn.ki=7;run.Yummn.phase=YummnPhase.Burst;
                typeof(KaitRun).GetProperty("chainActive").SetValue(run,false);
                if(skill==KaitSkill.FrostBreath)run.enemies.Add(new KaitEnemy{id=9900+direction,type=KaitEnemyType.Grunt,pos=run.katePos+KaitRun.Delta(CastDirections[direction]),hp=3,maxHp=3,life=KaitEnemyLife.Active});
                RefreshAll();HandleSkillCardCast(0);yield return null;
                var button=yummnCastDirections[direction];
                var point=RectTransformUtility.WorldToScreenPoint(null,button.transform.position);
                var e=new PointerEventData(EventSystem.current){position=point,button=PointerEventData.InputButton.Left};
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);
                if(hits.Count==0||hits[0].gameObject.GetComponentInParent<KaitSkillTargetSurface>()==null)
                    Debug.LogError("STORYBOOK0913_QA: direction hit area obscured "+skill+direction);
                skillDeck.SendMessage("HandlePreviewPress",point);
                yield return null;
                if(targetingSkill!=skill)Debug.LogError("STORYBOOK0913_QA: pointer down cancelled direction skill");
                var before=run.turn;
                ExecuteEvents.Execute(button.gameObject,e,ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute(button.gameObject,e,ExecuteEvents.pointerUpHandler);
                ExecuteEvents.Execute(button.gameObject,e,ExecuteEvents.pointerClickHandler);
                while(busy)yield return null;
                if(targetingSkill!=KaitSkill.None||run.turn!=before+1)Debug.LogError("STORYBOOK0913_QA: directional cast failed "+skill+direction+" "+statusText.text);
            }
        }
        typeof(KaitRun).GetProperty("katePos").SetValue(run,new Vector2Int(3,3));run.enemies.Clear();run.spawns.Clear();run.skills.Clear();run.passives.Clear();run.rewardQueue.Clear();
        run.skills.AddRange(new[]{KaitSkill.FrostBreath,KaitSkill.PhantomSlide,KaitSkill.MageHand});
        run.passives.AddRange(new[]{KaitPassive.SpellEcho,KaitPassive.BountyJar,KaitPassive.StunStrike});run.Yummn.ki=7;
        typeof(KaitRun).GetProperty("chainActive").SetValue(run,false);
        for(int y=0;y<5;y++)for(int x=0;x<5;x++)run.threat[x,y]=(x+y)%3==0?1<<((x+y)%7+1):0;
        RefreshAll();yield return new WaitForSecondsRealtime(.7f);
        yield return CaptureMobile0912("0913-wide");
        var pack=new KaitRewardPack{id=9913,characterId=KaitCharacter.Yummn};pack.choices.AddRange(YummnCatalog.Cards.GetRange(0,3));
        run.rewardQueue.Enqueue(pack);RefreshAll();yield return new WaitForSecondsRealtime(.3f);
        rewardDeck.SendMessage("Select",(object)0);yield return new WaitForSecondsRealtime(.5f);
        int slotCount=0;
        foreach(var logo in rewardDeck.GetComponentsInChildren<KaitCardLogo>())
        {
            if(logo.transform.parent.name.StartsWith("Reward Drop Slot"))
            {
                slotCount++;
                var picture=logo.transform.Find("Cartoon Logo") as RectTransform;
                var slot=(RectTransform)logo.transform.parent;var corners=new Vector3[4];picture.GetWorldCorners(corners);
                foreach(var p in corners)if(!slot.rect.Contains((Vector2)slot.InverseTransformPoint(p)))Debug.LogError("STORYBOOK0913_QA: slot illustration outside frame");
                CheckMobile0912(slot,"replacement slot");
            }
        }
        if(slotCount!=6)Debug.LogError("STORYBOOK0913_QA: expected six replacement slots, got "+slotCount+" allowed="+run.CanSelectReward+" busy="+busy);
        yield return CaptureMobile0912("0913-card-tray");
        yield return SetMobileSize0912(1920,1080);yield return CaptureMobile0912("0913-tray-16x9");
        var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
        var tray=(RectTransform)rewardDeck.transform.Find("Equipment Card Tray");
        var firstSlot=(RectTransform)tray.Find("Reward Drop Slot 1");
        bool began=(bool)typeof(KaitRewardDeck).GetMethod("BeginDrag",flags).Invoke(rewardDeck,new object[]{0});
        var drop=(Vector2)rewardDeck.transform.InverseTransformPoint(firstSlot.position);
        typeof(KaitRewardDeck).GetMethod("EndDrag",flags).Invoke(rewardDeck,new object[]{0,drop});
        if(!began||run.CurrentReward!=null||run.EquippedCardCount!=6)Debug.LogError("STORYBOOK0913_QA: tray drop failed");
        run.rewardQueue.Clear();RefreshAll();yield return new WaitForSecondsRealtime(.4f);
        run.Yummn.profile.maxKi=9;run.Yummn.ki=9;RefreshAll();yield return new WaitForSecondsRealtime(.2f);
        foreach(var pip in actionPips)
        {
            var corners=new Vector3[4];pip.rectTransform.GetWorldCorners(corners);
            foreach(var p in corners)if(!layout.Hud.rect.Contains((Vector2)layout.Hud.InverseTransformPoint(p)))Debug.LogError("STORYBOOK0913_QA: nine qi out of HUD");
        }
        yield return CaptureMobile0912("0913-nine-qi");
        atmosphereDanger=true;nextDangerCheck=Time.unscaledTime+10;dangerStrength=1;atmosphere.SetState(0,1,1);
        yield return CaptureMobile0912("0913-danger");
        Debug.Log("STORYBOOK0913_QA_COMPLETE");
    }
}
