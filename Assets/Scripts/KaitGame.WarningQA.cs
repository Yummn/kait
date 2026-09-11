using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class KaitGame
{
    private IEnumerator VerifyApprovedWarnings(string path)
    {
        yield return new WaitForSecondsRealtime(.4f);
        run.SelectCharacter(KaitCharacter.Yummn,8201);ConfigureCharacterVisuals();run.StateCommitted=null;EnsureKaitSpine();
        run.enemies.Clear();run.spawns.Clear();
        typeof(KaitRun).GetProperty("katePos").SetValue(run,new Vector2Int(1,3));
        foreach(KaitEnemyType kind in System.Enum.GetValues(typeof(KaitEnemyType)))
        {
            run.enemies.Clear();var enemy=new KaitEnemy{id=9900+(int)kind,type=kind,hp=3,maxHp=3,life=KaitEnemyLife.Active,pos=new Vector2Int(2,3),facing=Vector2Int.right};
            enemy.intent.type=kind==KaitEnemyType.Archer?KaitIntentType.LineShot:kind==KaitEnemyType.Warlock?KaitIntentType.CrossBlast:KaitIntentType.Melee;
            enemy.intent.target=new Vector2Int(3,3);enemy.intent.direction=Vector2Int.right;
            enemy.intent.affectedCells.Add(enemy.intent.target);
            if(kind==KaitEnemyType.Archer||kind==KaitEnemyType.ShieldKnight){enemy.intent.affectedCells.Add(new Vector2Int(4,3));enemy.intent.affectedCells.Add(new Vector2Int(5,3));}
            if(kind==KaitEnemyType.Warlock)foreach(var d in new[]{Vector2Int.right,Vector2Int.left,Vector2Int.up,Vector2Int.down})enemy.intent.affectedCells.Add(enemy.intent.target+d);
            run.enemies.Add(enemy);RefreshAll();yield return new WaitForSecondsRealtime(.18f);
            CaptureCanvasToPng(path+".enemy-"+kind+".png");
            int count=0;foreach(var g in selectedTelegraphs)if(g!=null&&g.Plan.count>0)count++;
            if(count!=enemy.intent.affectedCells.Count)Debug.LogError("WARNINGS_QA: selected range mismatch "+kind);
            foreach(var p in enemy.intent.affectedCells)impactCells.Add(p);RefreshBattle();yield return null;CaptureCanvasToPng(path+".impact-"+kind+".png");impactCells.Clear();
        }
        run.enemies.Clear();
        foreach(var kind in new[]{KaitEnemyType.Grunt,KaitEnemyType.Swordsman,KaitEnemyType.Guard})
        {
            var source=kind==KaitEnemyType.Grunt?new Vector2Int(2,3):kind==KaitEnemyType.Swordsman?new Vector2Int(3,4):new Vector2Int(4,3);
            var e=new KaitEnemy{id=9950+(int)kind,type=kind,hp=3,maxHp=3,life=KaitEnemyLife.Active,pos=source};e.intent.type=KaitIntentType.Melee;e.intent.target=new Vector2Int(3,3);e.intent.direction=e.intent.target-source;e.intent.affectedCells.Add(e.intent.target);run.enemies.Add(e);
        }
        RefreshAll();yield return new WaitForSecondsRealtime(.18f);CaptureCanvasToPng(path+".overlap.png");
        if(selectedTelegraphs[24].Plan.count!=3)Debug.LogError("WARNINGS_QA: overlap count");
        foreach(var e in run.enemies){e.hp=0;e.life=KaitEnemyLife.Dead;}RefreshAll();yield return null;
        foreach(var badge in selectedEnemyBadges)if(badge!=null&&badge.gameObject.activeSelf)Debug.LogError("WARNINGS_QA: dead melee badge remains");
        run.enemies.Clear();
        var boss=new KaitEnemy{id=9991,type=KaitEnemyType.ShieldKnight,hp=8,maxHp=8,life=KaitEnemyLife.Active,pos=new Vector2Int(3,3)};
        run.enemies.Add(boss);
        foreach(var direction in new[]{Vector2Int.right,Vector2Int.down,Vector2Int.left,Vector2Int.up})
        {
            boss.facing=direction;boss.intent.type=KaitIntentType.Melee;boss.intent.direction=direction;boss.intent.damage=1;
            boss.intent.affectedCells.Clear();
            for(var p=boss.pos+direction; p.x>=1&&p.x<=5&&p.y>=1&&p.y<=5; p+=direction)
                if(!run.walls[p.x,p.y])boss.intent.affectedCells.Add(p);else break;
            RefreshAll();yield return new WaitForSecondsRealtime(.18f);
            int visible=0;
            foreach(var fx in selectedTelegraphs)if(fx!=null&&fx.gameObject.activeSelf&&fx.Plan.boss!=0)visible++;
            if(visible!=boss.intent.affectedCells.Count)Debug.LogError("WARNINGS_QA: threatened-cell count mismatch");
            CaptureCanvasToPng(path+".boss-"+direction.x+"-"+direction.y+".png");
        }
        boss.hp=0;boss.life=KaitEnemyLife.Dead;boss.intent.affectedCells.Clear();RefreshAll();yield return null;
        foreach(var fx in selectedTelegraphs)if(fx!=null&&fx.gameObject.activeSelf&&fx.Plan.boss!=0)Debug.LogError("WARNINGS_QA: dead boss warning remains");
        UpdateAtmosphere();
        atmosphere.SetState(1,0,.16f);CaptureCanvasToPng(path+".clock.png");
        atmosphere.SetState(0,1,.16f);CaptureCanvasToPng(path+".danger.png");
        atmosphere.SetState(1,1,.46f);CaptureCanvasToPng(path+".both.png");
        atmosphere.SetState(0,0,0);CaptureCanvasToPng(path+".clear.png");
        foreach(var graphic in atmosphere.GetComponentsInChildren<Graphic>())if(graphic.raycastTarget)Debug.LogError("WARNINGS_QA: input blocked");
        Debug.Log("WARNINGS_QA_COMPLETE: four ground directions, death cleanup, clock/danger/combined/clear");
        Application.Quit();
    }
}
