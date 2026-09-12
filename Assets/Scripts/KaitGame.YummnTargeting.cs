using UnityEngine;
using UnityEngine.UI;

public sealed partial class KaitGame
{
    private readonly Button[] yummnCastDirections=new Button[4];
    private Image yummnCastToast;
    private static readonly KaitDirection[] CastDirections={KaitDirection.Up,KaitDirection.Right,KaitDirection.Down,KaitDirection.Left};
    private void RefreshYummnSkillTargets()
    {
        bool visible=run.IsYummn&&!run.ended&&YummnCatalog.TargetsDirection(targetingSkill);
        for(int i=0;i<4;i++)
        {
            if(yummnCastDirections[i]==null&&visible)
            {
                int index=i;
                var b=MakeFlatButton(gameplayRoot.transform,Vector2.zero,new Vector2(84,72),new[]{"↑","→","↓","←"}[i]+"\n施放");
                b.name="Yummn Cast "+CastDirections[i];
                var label=b.GetComponentInChildren<Text>();label.text=new[]{"↑","→","↓","←"}[i];label.fontSize=30;label.resizeTextForBestFit=false;
                b.onClick.AddListener(()=>HandleBattleCellClick(run.katePos+KaitRun.Delta(CastDirections[index])));
                yummnCastDirections[i]=b;
            }
            var button=yummnCastDirections[i];if(button==null)continue;
            button.gameObject.SetActive(visible);if(!visible)continue;
            var d=KaitRun.Delta(CastDirections[i]);
            var origin=battleCells[run.katePos.x+run.katePos.y*KaitRun.BattleSize].rectTransform;
            // The hidden outer border has no UI cells; measure the playable grid.
            int anchor=2+2*KaitRun.BattleSize;
            var xStep=battleCells[anchor+1].rectTransform.position-battleCells[anchor].rectTransform.position;
            var yStep=battleCells[anchor+KaitRun.BattleSize].rectTransform.position-battleCells[anchor].rectTransform.position;
            button.transform.position=origin.position+xStep*d.x+yStep*d.y;
            button.transform.SetAsLastSibling();
        }
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
