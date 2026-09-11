using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class YummnV081ComparisonTests
{
    [Test] public void FourModesTenSeedsFourBuilds_ReportActualReachability()
    {
        string root=Directory.GetParent(Application.dataPath).FullName;
        var csv=new StringBuilder("group,seed,build,actions,kills,phases,requested,inserted,dropped,spawnedHP,damageTaken,highest,first32,first128,bossAction,starved,ended,reason\n");
        var rows=new List<string>();
        using(var logs=new StreamWriter(Path.Combine(root,"Logs/v081-comparison-actions.jsonl"),false,Encoding.UTF8))
        for(int group=0;group<4;group++)
        {
            int deaths=0,starved=0,wins=0,reach32=0,reach128=0,totalActions=0,totalKills=0,totalPhases=0,spawnHp=0,damage=0,max=0;
            for(int build=0;build<4;build++)for(int seed=8101;seed<=8110;seed++)
            {
var r=new KaitRun();r.SelectCharacter(KaitCharacter.Yummn,seed,new YummnRulesSnapshot(group%2==1?YummnTileSupplyMode.SkipStationaryPunch:YummnTileSupplyMode.KillOnly,group>=2,false,fiveKi:false,bossLine:false,actualMoveSupply:false));
                // Deliberately equipped from action zero for controlled card comparisons,
                // not claimed as a normally acquired or optimized player build.
                if(build==1){r.skills.Add(KaitSkill.Flurry);r.passives.Add(KaitPassive.FireSnake);}
                if(build==2){r.skills.Add(KaitSkill.FrostBreath);r.passives.Add(KaitPassive.ShatteringPalm);}
                if(build==3){r.skills.Add(KaitSkill.Darkness);r.passives.Add(KaitPassive.Opportunist);}
                var random=new System.Random(seed);int localHp=0,localDamage=0,highest=2;
                for(int step=0;step<200&&!r.ended;step++)
                {
                    var dirs=Enum.GetValues(typeof(KaitDirection)).Cast<KaitDirection>().OrderBy(_=>random.Next()).ToList();
                    var near=r.enemies.Where(e=>e.life!=KaitEnemyLife.Dead).OrderBy(e=>(e.pos-r.katePos).sqrMagnitude).FirstOrDefault();
                    if(near!=null)dirs=dirs.OrderBy(d=>(r.katePos+KaitRun.Delta(d)-near.pos).sqrMagnitude).ToList();
                    bool acted=false;
                    foreach(var d in dirs)
                    {
                        var ahead=r.EnemyAt(r.katePos+KaitRun.Delta(d));
                        int baseKi=r.KiPhase==YummnPhase.Burst?1:0;
                        KaitSkill prep=build==1?KaitSkill.Flurry:build==2?KaitSkill.FrostBreath:KaitSkill.None;
                        if(prep!=KaitSkill.None&&ahead!=null&&r.Ki>=baseKi+1&&!r.IsYummnPrepared(prep))r.TryUseSkill(prep,-1,out _);
                        int hpBefore=r.kateHp;var result=r.TryGlobalInput(d);if(!result.valid)continue;
                        acted=true;localDamage+=Math.Max(0,hpBefore-r.kateHp);
                        foreach(var p in result.spawnedEnemyCells){var e=r.EnemyAt(p);if(e!=null)localHp+=e.maxHp;}
                        highest=Math.Max(highest,r.threat.Cast<int>().Max());
                        logs.WriteLine(r.YummnActionJson(result));
                        while(r.CurrentReward!=null)r.SkipReward(); // Keep controlled loadout stable.
                        break;
                    }
                    if(!acted||r.YummnSupplyStarved)break;
                }
                var m=r.Yummn.metrics;int h=r.threat.Cast<int>().Max();
                csv.AppendLine($"{(char)('A'+group)},{seed},{build},{r.turn},{r.kills},{m.enemyPhases},{m.requestedTwos},{m.insertedTwos},{m.droppedTwos},{localHp},{localDamage},{highest},{m.first32Action},{m.first128Action},{m.bossCreatedAction},{r.YummnSupplyStarved},{r.ended},{r.endReason}");
                if(r.YummnSupplyStarved)starved++;if(r.ended&&!r.won)deaths++;if(r.won)wins++;if(m.first32Action>=0)reach32++;if(m.first128Action>=0)reach128++;
                totalActions+=r.turn;totalKills+=r.kills;totalPhases+=m.enemyPhases;spawnHp+=localHp;damage+=localDamage;max=Math.Max(max,highest);
                Assert.AreEqual(m.requestedTwos,m.insertedTwos+m.droppedTwos);
                Assert.LessOrEqual(m.enemyPhases,r.turn);
            }
            rows.Add($"| {(char)('A'+group)} | 40 | {totalActions} | {totalKills} | {totalPhases} | {starved} | {deaths} | {wins} | {reach32} | {reach128} | {max} | {spawnHp} | {damage} |");
        }
        File.WriteAllText(Path.Combine(root,"Logs/v081-comparison.csv"),csv.ToString(),Encoding.UTF8);
        File.WriteAllText(Path.Combine(root,"Docs/v0.8.1-四组对照实测.md"),
            "# 四组对照：自动化冒烟样本\n\n固定种子8101–8110，每组测试无构筑、连击＋火蛇、吐息＋碎冰、暗幕＋借机反击，共40局。构筑直接注入用于受控对比，并非正常选牌进程。人物3HP、初始3枚2，未开无敌、未增加补给、未改变墙体。每局最多200次输入；最近敌人优先的简单策略，不躲预警，不代表熟练玩家。暗幕组仅测试反击被动，策略不会施放暗幕。奖励跳过以保持固定构筑。\n\n| 组 | 局数 | 总行动 | 击杀 | 敌方阶段 | 断供局 | 失败局 | 胜利局 | 到32局 | 到128局 | 最高数字 | 生成总HP | 受到伤害 |\n|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|\n"+string.Join("\n",rows)+
            "\n\n原始数据：`Logs/v081-comparison.csv`；逐行动日志：`Logs/v081-comparison-actions.jsonl`。最高数字包含中途达到后被牌效果删除的数字。到32、128按实际合成记录统计。未到达记-1；没有结束并不等于胜利。\n\n这些数据用于检查规则执行和暴露资源断供，不能用于证明平衡性或通关率。A/C的初始资源问题另见实施记录中的理想供给推导。\n",Encoding.UTF8);
    }
}
