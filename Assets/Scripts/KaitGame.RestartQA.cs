using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class KaitGame
{
    // Hosted on a separate QA component: NewRun intentionally stops this game's coroutines.
    private IEnumerator VerifyRestartRuntime()
    {
        yield return new WaitForSecondsRealtime(.5f);
        mainMenu.gameObject.SetActive(false);gameplayRoot.SetActive(true);
        for (int cycle = 0; cycle < 6; cycle++)
        {
            run.SelectCharacter(cycle % 2 == 0 ? KaitCharacter.Yummn : KaitCharacter.Kait, 913 + cycle);
            ConfigureCharacterVisuals();EnsureKaitSpine();
            var enemy = new KaitEnemy { id=9000+cycle, type=KaitEnemyType.Grunt,
                pos=new Vector2Int(2,2), hp=2, maxHp=2, life=KaitEnemyLife.Active };
            run.enemies.Add(enemy);
            animatedEnemies=new List<KaitEnemy>{enemy};animatedSpawns=SnapshotSpawns();
            displayedThreat=new int[5,5];busy=true;
            RefreshAll();
            PlayCombatEffectAtCell(KaitCombatEffectKind.Ice,enemy.pos,Vector2.one*120,20);
            PlayV08Fx(enemy.pos,0,KaitDirection.Right,120);
            typeof(KaitRun).GetProperty("kateHp").SetValue(run,0);
            typeof(KaitRun).GetProperty("ended").SetValue(run,true);
            kaitSpine?.PlayOnce(KaitSpineView.Die);
            yield return null;
            NewRun();
            yield return null;
            for(int frame=0;frame<4;frame++){RefreshAll();yield return null;}
            if(animatedEnemies!=null||animatedSpawns!=null||displayedThreat!=null||busy||hideKate||hideThreatValues)
                Debug.LogError("RESTART_QA: previous presentation survived reset");
            foreach(var id in enemySpines.Keys)
                if(!run.enemies.Exists(e=>e.id==id))Debug.LogError("RESTART_QA: old enemy recreated "+id);
            if(activeCombatEffects.Count!=0)Debug.LogError("RESTART_QA: old combat effect survived");
            foreach(var effect in canvas.GetComponentsInChildren<YummnV08Effect>(true))
                if(effect.name.StartsWith("Yummn V08"))Debug.LogError("RESTART_QA: old burst survived");
            Debug.Log("RESTART_QA_CYCLE "+cycle+" "+run.Character);
        }
        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(Application.dataPath,"../../Logs/restart-final.png"));
        yield return new WaitForSecondsRealtime(.4f);
        Debug.Log("RESTART_QA_COMPLETE");Application.Quit();
    }
}
