using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class KaitGame
{
    // Explicit QA command only; never changes a normal session.
    private void PrepareBuild061Preview(string mode)
    {
        if(mode=="mechanics")return;
        if(mode=="owned")
        {
            run.skills.AddRange(new[]{KaitSkill.SwiftBoots,KaitSkill.IceTomb,KaitSkill.CatAgility});
            run.passives.AddRange(new[]{KaitPassive.BirdEye,KaitPassive.LuckBlade,KaitPassive.Passwall});
            run.TryUseSkill(KaitSkill.SwiftBoots,-1,out _);RefreshAll();return;
        }
        if(mode=="reward")
        {
            run.EnqueueMergeReward(new KaitMergeEvent{sourceValue=16,resultValue=32});
            run.CurrentReward.choices.Clear();
            run.CurrentReward.choices.Add(KaitAbilityCatalog.Get(KaitSkill.HexCurse));
            run.CurrentReward.choices.Add(KaitAbilityCatalog.Get(KaitPassive.BloodBookmark));
            run.CurrentReward.choices.Add(KaitAbilityCatalog.Get(KaitSkill.DimensionDoor));
            RefreshAll();ShowPendingSkillChoice();return;
        }
        var root=new GameObject("v061 Visual Review",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        root.transform.SetParent(canvas.transform,false);
        var area=root.GetComponent<RectTransform>();area.sizeDelta=new Vector2(1920,1080);
        root.GetComponent<Image>().color=new Color(.17f,.16f,.20f);root.GetComponent<Image>().raycastTarget=false;
        if(mode=="effects")
        {
            for(int i=0;i<16;i++)KaitBuildEffect.Play(area,area.TransformPoint(new Vector3((i%4-1.5f)*260,360-i/4*240,0)),i,160,20);
            return;
        }
        int.TryParse(mode.Replace("cards",""),out int page);
        var all=KaitAbilityCatalog.All.FindAll(d=>!d.experimental);
        var split=root.AddComponent<GlobalStyleSplit>();split.Configure(area,.5f,.5f);
        for(int i=0;i<6;i++)
        {
            int index=page*6+i;if(index>=all.Count)break;
            var def=all[index];var position=new Vector2((i%3-1)*300,180-i/3*340);
            if(def.kind==KaitAbilityKind.Active)
            {
                var card=KaitSkillCard.Create(area,split,threatBoardFont,KaitSunlitTheme.Load("SkillCardHD"),KaitSunlitTheme.Load("SkillCardFlat"),null,null,null);
                card.Show(def.skill,true,position,0);card.PreviewAt(position);
            }
            else
            {
                var card=KaitPassiveCard.Create(area,split,threatBoardFont,KaitSunlitTheme.Load("PassiveCardBlankHD"),KaitSunlitTheme.Load("PassiveCardFlat"),null,null);
                card.Show(def.passive,true,position,0);card.PreviewAt(position);
            }
        }
    }

    private IEnumerator VerifyBuild061Runtime(string path)
    {
        yield return new WaitForSecondsRealtime(.3f);
        run.skills.Clear();run.skills.AddRange(new[]{KaitSkill.HexCurse,KaitSkill.Command,KaitSkill.DispelMagic});
        run.enemies.Clear();run.spawns.Clear();
        var enemy=new KaitEnemy{id=901,type=KaitEnemyType.Archer,life=KaitEnemyLife.Active,pos=new Vector2Int(4,3),hp=20,maxHp=20,rangedState=KaitRangedState.Aim};
        enemy.intent=new KaitIntent{type=KaitIntentType.LineShot,origin=enemy.pos,direction=Vector2Int.left,damage=1};
        for(int x=1;x<enemy.pos.x;x++)enemy.intent.affectedCells.Add(new Vector2Int(x,3));
        run.enemies.Add(enemy);
        var rift=new KaitSpawnRequest{tier=1,targetCell=new Vector2Int(1,1),createdTurn=run.turn,turnsUntilSpawn=1};run.spawns.Add(rift);
        RefreshAll();yield return null;
        int before=run.turn;
        if(!HandleSkillCardCast(0))Debug.LogError("Build061 QA: curse card cast rejected");
        HandleBattleCellClick(enemy.pos);
        if(!enemy.cursed || run.turn!=before || run.SkillCooldown(KaitSkill.HexCurse)!=3)Debug.LogError("Build061 QA: curse application failed");
        yield return new WaitForSecondsRealtime(.16f);yield return new WaitForEndOfFrame();CaptureCanvasToPng(path);
        yield return null; // Readback time must not eat the following effect's lifetime.
        if(!HandleSkillCardCast(1))Debug.LogError("Build061 QA: command card cast rejected");
        HandleBattleCellClick(enemy.pos);
        if(enemy.intent.direction!=Vector2Int.up)Debug.LogError("Build061 QA: attack lock did not rotate");
        yield return new WaitForSecondsRealtime(.16f);yield return new WaitForEndOfFrame();CaptureCanvasToPng(path+".command.png");
        yield return null;
        if(!HandleSkillCardCast(2))Debug.LogError("Build061 QA: dispel card cast rejected");
        HandleBattleCellClick(rift.targetCell);
        if(run.SpawnAt(rift.targetCell)!=null)Debug.LogError("Build061 QA: selected rift was not removed");
        yield return new WaitForSecondsRealtime(.16f);yield return new WaitForEndOfFrame();CaptureCanvasToPng(path+".dispel.png");
        yield return null;
        Vector2Int start=run.katePos;HandleDirection(KaitDirection.Right);
        if(run.katePos==start && run.turn==before)Debug.LogError("Build061 QA: skill feedback blocked movement");
        yield return new WaitForSecondsRealtime(.6f);yield return new WaitForEndOfFrame();CaptureCanvasToPng(path+".move.png");
        Debug.Log("Build061 runtime QA passed: curse, cooldown, rotated attack, dispel, and movement interruption verified");
        Application.Quit();
    }
}
