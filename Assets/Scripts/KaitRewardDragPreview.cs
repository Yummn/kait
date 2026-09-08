using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed partial class KaitGame
{
    // Invoked only by the explicit QA command. Uses the production pointer handlers.
    private IEnumerator VerifyRewardDragRuntime(string path)
    {
        yield return new WaitForSecondsRealtime(.3f);
        run.skills.Clear();run.skills.AddRange(new[]{KaitSkill.SwiftBoots,KaitSkill.CatAgility,KaitSkill.Command});
        run.passives.Clear();run.passives.AddRange(new[]{KaitPassive.BirdEye,KaitPassive.CheshireCat,KaitPassive.Trend});
        run.EnqueueMergeReward(new KaitMergeEvent{sourceValue=16,resultValue=32});
        run.CurrentReward.choices.Clear();
        run.CurrentReward.choices.Add(KaitAbilityCatalog.Get(KaitSkill.HexCurse));
        run.CurrentReward.choices.Add(KaitAbilityCatalog.Get(KaitPassive.Simulacrum));
        run.CurrentReward.choices.Add(KaitAbilityCatalog.Get(KaitSkill.DimensionDoor));
        RefreshAll();yield return new WaitForSecondsRealtime(.4f);
        var toolbar=rewardDeck.transform.Find("Reward Actions") as RectTransform;
        var handle=toolbar.Find("Drag Handle").gameObject;
        Vector2 toolbarHome=toolbar.anchoredPosition;
        foreach(var destination in new[]{new Vector2(-540,300),new Vector2(540,300),new Vector2(0,300)})
        {
            var drag=RewardPointer(handle,29);var start=drag.position;
            ExecuteEvents.Execute(handle,drag,ExecuteEvents.beginDragHandler);
            Vector2 delta=destination-toolbar.anchoredPosition;
            drag.position=start+(Vector2)rewardDeck.transform.TransformVector(delta);
            ExecuteEvents.Execute(handle,drag,ExecuteEvents.dragHandler);ExecuteEvents.Execute(handle,drag,ExecuteEvents.endDragHandler);
            RefreshAll();yield return null;
            if(Vector2.Distance(toolbar.anchoredPosition,destination)>1||run.CurrentReward==null)Debug.LogError("RewardDrag QA: toolbar drag was reset or consumed reward");
            yield return new WaitForEndOfFrame();CaptureCanvasToPng(path+".toolbar-"+(destination.x<0?"left":destination.x>0?"right":"split")+".png");
        }
        toolbar.anchoredPosition=toolbarHome;yield return null;
        yield return new WaitForEndOfFrame();CaptureCanvasToPng(path+".offer.png");yield return null;
        var card=System.Array.Find(rewardDeck.GetComponentsInChildren<KaitSkillCard>(),c=>c.Skill==KaitSkill.HexCurse);
        var pointer=RewardPointer(card.gameObject,17);
        ExecuteEvents.Execute(card.gameObject,pointer,ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(card.gameObject,pointer,ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(card.gameObject,pointer,ExecuteEvents.pointerClickHandler);
        if(run.CurrentReward==null||run.skills.Contains(KaitSkill.HexCurse))Debug.LogError("RewardDrag QA: a tap consumed a reward");
        // A missed drag must spring back, without acting as a later click.
        BeginRewardPointer(card.gameObject,pointer);
        pointer.position=RectTransformUtility.WorldToScreenPoint(null,rewardDeck.transform.TransformPoint(new Vector3(650,-120,0)));
        ExecuteEvents.Execute(card.gameObject,pointer,ExecuteEvents.dragHandler);EndRewardPointer(card.gameObject,pointer);
        if(run.CurrentReward==null||run.skills.Contains(KaitSkill.HexCurse))Debug.LogError("RewardDrag QA: missed drag consumed a reward");
        yield return new WaitForSecondsRealtime(.4f);
        BeginRewardPointer(card.gameObject,pointer);MoveRewardPointer(card.gameObject,pointer,1);
        yield return null;yield return new WaitForEndOfFrame();CaptureCanvasToPng(path+".replace.png");yield return null;
        EndRewardPointer(card.gameObject,pointer);
        if(run.skills[1]!=KaitSkill.HexCurse||run.CurrentReward!=null||!run.IsAbilityPending(KaitAbilityCatalog.Get(KaitSkill.HexCurse)))Debug.LogError("RewardDrag QA: replacement or staging failed");
        yield return new WaitForSecondsRealtime(.4f);yield return new WaitForEndOfFrame();CaptureCanvasToPng(path+".equipped.png");yield return null;
        run.EnqueueMergeReward(new KaitMergeEvent{sourceValue=16,resultValue=32});run.CurrentReward.choices.Clear();
        run.CurrentReward.choices.Add(KaitAbilityCatalog.Get(KaitPassive.Simulacrum));
        run.CurrentReward.choices.Add(KaitAbilityCatalog.Get(KaitSkill.IceTomb));
        run.CurrentReward.choices.Add(KaitAbilityCatalog.Get(KaitPassive.BloodBookmark));
        RefreshAll();yield return new WaitForSecondsRealtime(.35f);
        var mirror=System.Array.Find(rewardDeck.GetComponentsInChildren<KaitPassiveCard>(),c=>c.Passive==KaitPassive.Simulacrum);
        pointer=RewardPointer(mirror.gameObject,-1);BeginRewardPointer(mirror.gameObject,pointer);MoveRewardPointer(mirror.gameObject,pointer,0);
        yield return null;yield return new WaitForEndOfFrame();CaptureCanvasToPng(path+".copy.png");yield return null;
        EndRewardPointer(mirror.gameObject,pointer);
        if(run.CurrentReward==null||run.passives.Contains(KaitPassive.Simulacrum))Debug.LogError("RewardDrag QA: first copy drop equipped prematurely");
        yield return new WaitForSecondsRealtime(.4f);yield return new WaitForEndOfFrame();CaptureCanvasToPng(path+".copy-ready.png");yield return null;
        BeginRewardPointer(mirror.gameObject,pointer);MoveRewardPointer(mirror.gameObject,pointer,2);EndRewardPointer(mirror.gameObject,pointer);
        if(run.passives[2]!=KaitPassive.Simulacrum||run.copiedPassive!=KaitPassive.BirdEye||run.CurrentReward!=null)Debug.LogError("RewardDrag QA: final copy replacement failed");
        yield return new WaitForSecondsRealtime(.4f);yield return new WaitForEndOfFrame();CaptureCanvasToPng(path+".done.png");
        Debug.Log("RewardDrag runtime QA passed: tap preview, missed release, touch replacement, pending activation, two-step passive copy.");
        Application.Quit();
    }
    private PointerEventData RewardPointer(GameObject card,int id)
    {
        var p=new PointerEventData(EventSystem.current){pointerId=id,button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(null,card.transform.position)};
        var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(p,hits);
        if(hits.Count==0||hits[0].gameObject!=card)Debug.LogError("RewardDrag QA: card is not the top input surface");
        return p;
    }
    private void BeginRewardPointer(GameObject card,PointerEventData p)
    {
        p.position=RectTransformUtility.WorldToScreenPoint(null,card.transform.position);
        ExecuteEvents.Execute(card,p,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(card,p,ExecuteEvents.beginDragHandler);
    }
    private void MoveRewardPointer(GameObject card,PointerEventData p,int slot)
    {
        var target=rewardDeck.transform.Find("Reward Drop Slot "+(slot+1));
        p.position=RectTransformUtility.WorldToScreenPoint(null,target.position);ExecuteEvents.Execute(card,p,ExecuteEvents.dragHandler);
    }
    private static void EndRewardPointer(GameObject card,PointerEventData p)
    {
        ExecuteEvents.Execute(card,p,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(card,p,ExecuteEvents.endDragHandler);
        ExecuteEvents.Execute(card,p,ExecuteEvents.pointerClickHandler);
    }
}
