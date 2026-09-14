using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public sealed class YummnDefaultPresetTests
{
    const BindingFlags Hidden=BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    static readonly string[] Keys={"MaxHp","MaxKi","FixedMove","KillKi","AttackPhase","FullRecovery","SupplyMode","FromEight","MovePhase","AttackOne","KiGuard","Reward32","ExitExhaustionEarly","FreeFastAttack","DisableKiGuard"};

    [Test] public void EmptyPreferencesUseApprovedDefaultRules()
    {
        var saved=Keys.ToDictionary(k=>k,k=>PlayerPrefs.HasKey("Kait.Yummn082."+k)?(int?)PlayerPrefs.GetInt("Kait.Yummn082."+k):null);
        try
        {
            foreach(var k in Keys)PlayerPrefs.DeleteKey("Kait.Yummn082."+k);
            var preset=(YummnRulesSnapshot)typeof(KaitGame).GetMethod("YummnPreset",Hidden).Invoke(null,null);
            Assert.AreEqual(7,preset.MaxKi);Assert.AreEqual(YummnMovementCostMode.PerCell,preset.MovementCostMode);
            Assert.AreEqual(3,preset.KillKi);Assert.AreEqual(YummnTileSupplyMode.EffectiveMove,preset.Supply);
            Assert.IsFalse(preset.AttackAdvancesEnemyPhase);Assert.IsTrue(preset.ExhaustionNeedsFullKi);
            Assert.IsTrue(preset.AttackCostsOne);Assert.IsFalse(preset.MovementAdvancesEnemyPhase);
            Assert.IsFalse(preset.SpawnFromEight);Assert.IsTrue(preset.KiGuard);Assert.AreEqual(16,preset.RewardMergeValue);
            Assert.AreEqual(6,preset.MaxHp);
            Assert.AreEqual(preset.ScoreKey,YummnRulesSnapshot.Current().ScoreKey);
        }
        finally
        {
            foreach(var pair in saved)if(pair.Value.HasValue)PlayerPrefs.SetInt("Kait.Yummn082."+pair.Key,pair.Value.Value);else PlayerPrefs.DeleteKey("Kait.Yummn082."+pair.Key);
            PlayerPrefs.Save();
        }
    }

    [Test] public void YummnRuleSwitchesAreUncheckedByDefaultAndCheckedMeansVariant()
    {
        var saved=Keys.ToDictionary(k=>k,k=>PlayerPrefs.HasKey("Kait.Yummn082."+k)?(int?)PlayerPrefs.GetInt("Kait.Yummn082."+k):null);
        var root=new GameObject("Variant settings",typeof(RectTransform),typeof(Canvas));root.SetActive(false);
        try
        {
            foreach(var k in Keys)PlayerPrefs.DeleteKey("Kait.Yummn082."+k);
            var game=root.AddComponent<KaitGame>();var font=Resources.Load<Font>("NotoSansCJKsc-Regular");
            typeof(KaitGame).GetField("uiFont",Hidden).SetValue(game,font);typeof(KaitGame).GetField("threatBoardFont",Hidden).SetValue(game,font);
            var run=(KaitRun)typeof(KaitGame).GetField("run",Hidden).GetValue(game);run.SelectCharacter(KaitCharacter.Yummn,914,YummnRulesSnapshot.Current());
            typeof(KaitGame).GetMethod("BuildSettingsOverlay",Hidden).Invoke(game,new object[]{root.transform});
            var switches=root.GetComponentsInChildren<Toggle>(true).Where(t=>t.gameObject.name=="Flat Toggle").ToArray();
            foreach(var toggle in switches)Assert.IsFalse(toggle.isOn,toggle.GetComponentInChildren<Text>(true)?.text);
            var free=switches.Single(t=>t.GetComponentInChildren<Text>(true).text.Contains("高速攻击不耗气"));free.isOn=true;
            var preset=(YummnRulesSnapshot)typeof(KaitGame).GetMethod("YummnPreset",Hidden).Invoke(null,null);
            Assert.IsFalse(preset.AttackCostsOne);
            foreach(var label in root.GetComponentsInChildren<Text>(true))
            {Assert.IsTrue(label.enabled,label.name);Assert.Greater(label.color.a,.99f,label.text);Assert.NotNull(label.font,label.text);}
        }
        finally
        {
            Object.DestroyImmediate(root);
            foreach(var pair in saved)if(pair.Value.HasValue)PlayerPrefs.SetInt("Kait.Yummn082."+pair.Key,pair.Value.Value);else PlayerPrefs.DeleteKey("Kait.Yummn082."+pair.Key);
            PlayerPrefs.Save();
        }
    }

    [Test] public void RemovedPreferencesCannotChangeFixedRulesAndOnlyNamedControlsAreGone()
    {
        var saved=Keys.ToDictionary(k=>k,k=>PlayerPrefs.HasKey("Kait.Yummn082."+k)?(int?)PlayerPrefs.GetInt("Kait.Yummn082."+k):null);
        bool holdExisted=PlayerPrefs.HasKey("Kait.Input.HoldRepeat");int hold=PlayerPrefs.GetInt("Kait.Input.HoldRepeat");
        var root=new GameObject("Default settings",typeof(RectTransform),typeof(Canvas));root.SetActive(false);
        try
        {
            PlayerPrefs.SetInt("Kait.Yummn082.AttackPhase",1);PlayerPrefs.SetInt("Kait.Yummn082.SupplyMode",(int)YummnTileSupplyMode.EveryAction);
            PlayerPrefs.SetInt("Kait.Yummn082.FromEight",1);PlayerPrefs.SetInt("Kait.Yummn082.Reward32",1);PlayerPrefs.SetInt("Kait.Input.HoldRepeat",0);
            var game=root.AddComponent<KaitGame>();var font=Resources.Load<Font>("NotoSansCJKsc-Regular");
            typeof(KaitGame).GetField("uiFont",Hidden).SetValue(game,font);typeof(KaitGame).GetField("threatBoardFont",Hidden).SetValue(game,font);
            var run=(KaitRun)typeof(KaitGame).GetField("run",Hidden).GetValue(game);run.SelectCharacter(KaitCharacter.Yummn,913,YummnRulesSnapshot.Current());
            typeof(KaitGame).GetMethod("BuildSettingsOverlay",Hidden).Invoke(game,new object[]{root.transform});
            var preset=(YummnRulesSnapshot)typeof(KaitGame).GetMethod("YummnPreset",Hidden).Invoke(null,null);
            Assert.AreEqual(YummnTileSupplyMode.EveryAction,preset.Supply);Assert.IsFalse(preset.AttackAdvancesEnemyPhase);
            Assert.IsFalse(preset.SpawnFromEight);Assert.AreEqual(16,preset.RewardMergeValue);
            Assert.IsTrue((bool)typeof(KaitGame).GetProperty("HoldRepeatEnabled",Hidden).GetValue(game));
            string all=string.Join("|",root.GetComponentsInChildren<Text>(true).Select(t=>t.text));
            Assert.IsFalse(all.Contains("长按连续输入"));Assert.IsFalse(all.Contains("攻击也推进敌方回合"));
            Assert.IsFalse(all.Contains("从8开始出怪"));Assert.IsFalse(all.Contains("技能获得阈值"));
            Assert.IsNotNull(root.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.name=="Yummn Rule MaxHp"));
            Assert.IsNotNull(root.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.name=="Yummn Rule SupplyMode"));
            var toggles=root.GetComponentsInChildren<Toggle>(true).Where(t=>t.gameObject.activeSelf).Select(t=>(RectTransform)t.transform).OrderByDescending(t=>t.anchoredPosition.y).ToArray();
            Assert.AreEqual(5,toggles.Length);
            for(int i=1;i<toggles.Length;i++)Assert.GreaterOrEqual(toggles[i-1].anchoredPosition.y-toggles[i-1].rect.height/2,toggles[i].anchoredPosition.y+toggles[i].rect.height/2);
        }
        finally
        {
            Object.DestroyImmediate(root);
            foreach(var pair in saved)if(pair.Value.HasValue)PlayerPrefs.SetInt("Kait.Yummn082."+pair.Key,pair.Value.Value);else PlayerPrefs.DeleteKey("Kait.Yummn082."+pair.Key);
            if(holdExisted)PlayerPrefs.SetInt("Kait.Input.HoldRepeat",hold);else PlayerPrefs.DeleteKey("Kait.Input.HoldRepeat");PlayerPrefs.Save();
        }
    }
}
