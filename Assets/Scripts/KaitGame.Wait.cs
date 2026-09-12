using UnityEngine;
using UnityEngine.UI;

public sealed partial class KaitGame
{
    private Button waitButton;
    private void HandleWait()
    {
        if(!run.IsYummn||busy||run.ended||TutorialBlocksInput()||targetingSkill!=KaitSkill.None)return;
        var start=run.katePos;var enemiesBefore=SnapshotEnemies();var spawnsBefore=SnapshotSpawns();
        var result=run.TryYummnWait();if(!result.valid)return;
        yummnBufferedDirection=null;yummnAcceptBuffer=false;
        GameAudio.PlayClick();AppendLog(start,result);
        StartCoroutine(PlayTurn(result,start,enemiesBefore,spawnsBefore));
    }
}
