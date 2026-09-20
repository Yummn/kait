using UnityEngine;
using UnityEngine.UI;

public sealed partial class KaitGame
{
    private readonly Button[] yummnCastDirections=new Button[4];
    private Image yummnCastToast;
    private static readonly KaitDirection[] CastDirections={KaitDirection.Up,KaitDirection.Right,KaitDirection.Down,KaitDirection.Left};
    private void RefreshYummnSkillTargets()
    {
        bool visible=!run.ended&&(run.IsReynard?targetingSkill==KaitSkill.ReynardLightningBolt:run.IsYummn&&YummnCatalog.TargetsDirection(targetingSkill));
        for(int i=0;i<4;i++)
        {
            if(yummnCastDirections[i]==null&&visible)
            {
                int index=i;
                var b=MakeFlatButton(gameplayRoot.transform,Vector2.zero,new Vector2(84,72),new[]{"↑","→","↓","←"}[i]+"\n施放");
                b.name="Yummn Cast "+CastDirections[i];
                b.gameObject.AddComponent<KaitSkillTargetSurface>();
                // Keep a generous click surface for occupied cells, but let the
                // shared selector effect provide all visuals.
                b.transition=Selectable.Transition.None;
                b.targetGraphic.color=Color.clear;
                var label=b.GetComponentInChildren<Text>();label.gameObject.SetActive(false);
                b.onClick.AddListener(()=>HandleBattleCellClick(run.katePos+KaitRun.Delta(CastDirections[index])));
                yummnCastDirections[i]=b;
            }
            var button=yummnCastDirections[i];if(button==null)continue;
            button.gameObject.SetActive(visible);if(!visible)continue;
            button.GetComponent<RectTransform>().sizeDelta=Vector2.one*(120*gameContent.localScale.x);
            var d=KaitRun.Delta(CastDirections[i]);
            var origin=battleCells[run.katePos.x+run.katePos.y*KaitRun.BattleSize].rectTransform;
            // The hidden outer border has no UI cells; measure the playable grid.
            int anchor=2+2*KaitRun.BattleSize;
            var xStep=battleCells[anchor+1].rectTransform.position-battleCells[anchor].rectTransform.position;
            var yStep=battleCells[anchor+KaitRun.BattleSize].rectTransform.position-battleCells[anchor].rectTransform.position;
            button.transform.position=origin.position+xStep*d.x+yStep*d.y;
            button.transform.SetAsLastSibling();
        }
        RefreshSkillTargetMarkers();
    }

    private void RefreshSkillTargetMarkers()
    {
        if(battleSkillTargetMarkers==null||battleSkillTargetMarkers.Length!=KaitRun.BattleSize*KaitRun.BattleSize)
            battleSkillTargetMarkers=new YummnSkillTargetMarker[KaitRun.BattleSize*KaitRun.BattleSize];
        for(int y=1;y<KaitRun.BattleSize-1;y++)for(int x=1;x<KaitRun.BattleSize-1;x++)
        {
            int index=x+y*KaitRun.BattleSize;
            if(battleSkillTargetMarkers[index]==null)
            {
                var go=new GameObject("Skill Target "+x+","+y,typeof(RectTransform),typeof(YummnSkillTargetMarker));
                go.transform.SetParent(battleEffectLayer,false);
                var marker=go.GetComponent<YummnSkillTargetMarker>();marker.rectTransform.sizeDelta=new Vector2(108,108);
                battleSkillTargetMarkers[index]=marker;
            }
            var target=battleSkillTargetMarkers[index];
            Vector2Int cell=new Vector2Int(x,y);
            bool active=IsPreparedBattleTarget(cell);
            target.gameObject.SetActive(active);
            if(active){target.rectTransform.position=battleCells[index].rectTransform.position;target.transform.SetAsLastSibling();}
        }

        if(threatSkillTargetMarkers==null||threatSkillTargetMarkers.Length!=run.ThreatSize*run.ThreatSize)
            threatSkillTargetMarkers=new YummnSkillTargetMarker[run.ThreatSize*run.ThreatSize];
        for(int y=0;y<run.ThreatSize;y++)for(int x=0;x<run.ThreatSize;x++)
        {
            int index=x+y*run.ThreatSize;
            if(threatSkillTargetMarkers[index]==null)
            {
                var go=new GameObject("Threat Skill Target "+x+","+y,typeof(RectTransform),typeof(YummnSkillTargetMarker));
                go.transform.SetParent(threatCells[index].transform,false);
                var marker=go.GetComponent<YummnSkillTargetMarker>();marker.rectTransform.sizeDelta=new Vector2(100,100);
                marker.color=new Color(.62f,.88f,1f,.78f);threatSkillTargetMarkers[index]=marker;
            }
            bool active=run.IsYummn&&targetingSkill==KaitSkill.MageHand&&run.IsLegalSkillCell(targetingSkill,new Vector2Int(x,y));
            threatSkillTargetMarkers[index].gameObject.SetActive(active);
            if(active)threatSkillTargetMarkers[index].transform.SetAsLastSibling();
        }
    }

    private bool IsPreparedBattleTarget(Vector2Int cell)
    {
        if(targetingSkill==KaitSkill.None||run.ended)return false;
        if(run.IsYummn)
            return targetingSkill!=KaitSkill.MageHand&&run.IsLegalSkillCell(targetingSkill,cell);
        if(KaitRun.NeedsCellTarget(targetingSkill))return run.IsLegalSkillCell(targetingSkill,cell);
        if(KaitRun.NeedsEnemyTarget(targetingSkill))return run.EnemyAt(cell)!=null;
        return cell==run.katePos;
    }
    private void ShowYummnCastFailure(string reason)
    {
        string message="释放失败："+reason;
        statusText.text=message;
        if(yummnCastToast!=null)Destroy(yummnCastToast.gameObject);
        var toast=Rect("Yummn Cast Failure",canvas.transform,new Vector2(0,180),new Vector2(760,68),new Color(.16f,.13f,.18f,.96f));
        toast.sprite=roundedSprite;toast.type=Image.Type.Sliced;toast.raycastTarget=false;yummnCastToast=toast;
        var label=MakeText(message,toast.transform,Vector2.zero,new Vector2(720,58),27,Cream,TextAnchor.MiddleCenter,FontStyle.Bold,false);
        label.font=threatBoardFont;label.raycastTarget=false;
        StartCoroutine(HoldYummnCastFailure(toast));
    }
    private System.Collections.IEnumerator HoldYummnCastFailure(Image toast)
    {yield return new WaitForSecondsRealtime(1.8f);if(toast!=null)yield return FadeAndDestroy(toast.rectTransform,.35f);}
}
