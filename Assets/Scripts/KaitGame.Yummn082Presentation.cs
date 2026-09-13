using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class KaitGame
{
    // Independent from decorative trails: walking cannot delete a logical marker.
    private readonly Dictionary<int,KaitSpineView> yummnLogicalGhosts=new Dictionary<int,KaitSpineView>();
    private readonly List<KaitSpineView> yummnFadingGhosts=new List<KaitSpineView>();
    private string YummnCostLabel(KaitDirection direction)
    {int n=run.PreviewYummnMovementCost(direction);return n<0?"—":n.ToString();}
    private void CreateYummnLogicalGhost(int id,Vector2Int cell,KaitDirection direction)
    {
        if(yummnLogicalGhosts.ContainsKey(id)||makotoSkeletonData==null)return;
        var view=KaitSpineView.Create(makotoSkeletonData,battleUnderEffectLayer,new Vector2(115,115),"Yummn Logical Afterimage Trail "+id);
        if(view==null)return;
        view.Root.position=YummnCellPosition(cell);view.Face(direction);view.PlayLoop(KaitSpineView.Idle);
        view.SetTint(new Color(.68f,.88f,1f,.34f));
        var sand=new GameObject("C Time Sand",typeof(RectTransform),typeof(YummnGhostSand));
        sand.transform.SetParent(view.Root,false);sand.GetComponent<RectTransform>().sizeDelta=new Vector2(115,115);
        if(view.CurrentAnimation!=null)view.CurrentAnimation.TimeScale=0;
        yummnLogicalGhosts.Add(id,view);
    }
    private void RemoveYummnLogicalGhost(int id,float duration)
    {
        if(!yummnLogicalGhosts.TryGetValue(id,out var view))return;
        var sand=view.Root.GetComponentInChildren<YummnGhostSand>();if(sand!=null)Destroy(sand.gameObject);
        yummnLogicalGhosts.Remove(id);yummnFadingGhosts.Add(view);StartCoroutine(FadeYummnLogicalGhost(view,duration));
    }
    private IEnumerator FadeYummnLogicalGhost(KaitSpineView view,float duration)
    {
        for(float t=0;t<duration&&view.Root!=null;t+=Time.unscaledDeltaTime)
        {view.SetTint(new Color(.68f,.88f,1f,.34f*(1-t/duration)));yield return null;}
        yummnFadingGhosts.Remove(view);view.Destroy();
    }
    private void ClearYummnLogicalGhosts()
    {foreach(var view in yummnLogicalGhosts.Values)view.Destroy();yummnLogicalGhosts.Clear();foreach(var view in yummnFadingGhosts)view.Destroy();yummnFadingGhosts.Clear();}
    private void SyncYummnLogicalGhosts()
    {
        if(!run.IsYummn||!run.Yummn.rules.Is082||run.ended){ClearYummnLogicalGhosts();return;}
        foreach(var marker in run.Yummn.afterimages)if(marker.alive)CreateYummnLogicalGhost(marker.id,marker.cell,marker.direction);
        foreach(int id in new List<int>(yummnLogicalGhosts.Keys))if(!run.Yummn.afterimages.Exists(m=>m.id==id&&m.alive))RemoveYummnLogicalGhost(id,.3f);
    }
    private void ShowYummnAfterimageKi(int amount)
    {
        if(yummnHud==null)return;
        var label=MakeText("+"+amount,yummnHud.transform,new Vector2(110,25),new Vector2(45,30),22,new Color(.45f,.9f,1),TextAnchor.MiddleCenter);
        label.gameObject.name="Floating Damage";
        label.raycastTarget=false;StartCoroutine(FadeAndDestroy(label.rectTransform,.65f));
    }
    private IEnumerator AnimateYummn082Supply(KaitTurnResult r,YummnCombatEvent ev)
    {
        bool counter=ev.status=="CounterSupplyReady";
        StopYummnThreatPulses();displayedThreat=counter?r.threatAfter:r.yummnThreatAfterSupply;RefreshThreat();
        var cells=new List<RectTransform>();int start=counter?ev.amount:0,end=counter?r.newThreatCells.Count:ev.amount;
        for(int i=start;i<end;i++){var p=r.newThreatCells[i];cells.Add(threatCells[p.x+p.y*run.ThreatSize].rectTransform);}
        if(cells.Count>0)yield return ScalePulseMany(cells,.1f,1.1f,.1f);
        if(!counter&&r.merges.Exists(m=>m.systemMerge))
        {
            var archive=new KaitTurnResult{threatBefore=r.yummnThreatAfterSupply,threatAfter=r.yummnThreatAfterArchive};
            archive.threatMotions.AddRange(r.threatMotions.GetRange(r.yummnInitialMotionCount,r.threatMotions.Count-r.yummnInitialMotionCount));
            archive.merges.AddRange(r.merges.FindAll(m=>m.systemMerge));yield return AnimateThreat(archive);
        }
    }
}
