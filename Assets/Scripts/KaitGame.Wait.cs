using UnityEngine;
using UnityEngine.UI;

public sealed partial class KaitGame
{
    private Button waitButton;
    private bool tapStartedOnYummn;
    private float tapStartedAt, tapTravel;
    private bool IsYummnTapPosition(Vector2 screen)
    {
        if(!run.IsYummn||busy||run.ended||TutorialBlocksInput())return false;
        int index=run.katePos.x+run.katePos.y*7;
        return battleCells[index]!=null&&RectTransformUtility.RectangleContainsScreenPoint(battleCells[index].rectTransform,screen,null);
    }
    private void HandleWait()
    {
        if(!run.IsYummn||busy||run.ended||TutorialBlocksInput())return;
        var start=run.katePos;var enemiesBefore=SnapshotEnemies();var spawnsBefore=SnapshotSpawns();
        var result=run.TryYummnWait();if(!result.valid)return;
        yummnBufferedDirection=null;yummnAcceptBuffer=false;
        GameAudio.PlayClick();AppendLog(start,result);
        StartCoroutine(PlayTurn(result,start,enemiesBefore,spawnsBefore));
    }
}
