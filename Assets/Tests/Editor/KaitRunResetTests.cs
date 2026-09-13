using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class KaitRunResetTests
{
    const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    static void Set(KaitGame game,string name,object value) => typeof(KaitGame).GetField(name,Hidden).SetValue(game,value);
    static object Get(KaitGame game,string name) => typeof(KaitGame).GetField(name,Hidden).GetValue(game);

    [Test]
    public void RepeatedResetDiscardsOldEnemySnapshotsAndPersistentEffects()
    {
        var host=new GameObject("Reset test");host.SetActive(false);
        var game=host.AddComponent<KaitGame>();
        var root=new GameObject("Test canvas",typeof(Canvas));
        Set(game,"canvas",root.GetComponent<Canvas>());
        try
        {
            for(int round=0;round<3;round++)
            {
                Set(game,"animatedEnemies",new List<KaitEnemy>{new KaitEnemy{id=1,pos=new Vector2Int(2,2)}});
                Set(game,"animatedSpawns",new List<KaitSpawnRequest>());
                Set(game,"displayedThreat",new int[5,5]);
                Set(game,"hideKate",true);Set(game,"hideThreatValues",true);
                Set(game,"yummnAcceptBuffer",true);Set(game,"greyStrength",1f);
                var effect=new GameObject("Old persistent terrain",typeof(RectTransform),typeof(YummnV08Effect));
                effect.transform.SetParent(root.transform,false);
                var combat=new GameObject("Old combat effect",typeof(RectTransform),typeof(KaitCombatEffectGraphic));
                combat.transform.SetParent(root.transform,false);
                ((List<KaitCombatEffectGraphic>)Get(game,"activeCombatEffects")).Add(combat.GetComponent<KaitCombatEffectGraphic>());
                typeof(KaitGame).GetMethod("ResetRunPresentation",Hidden).Invoke(game,null);
                Assert.IsNull(Get(game,"animatedEnemies"),"Old enemies must not be recreated by RefreshBattle");
                Assert.IsNull(Get(game,"animatedSpawns"));Assert.IsNull(Get(game,"displayedThreat"));
                Assert.AreEqual(false,Get(game,"hideKate"));Assert.AreEqual(false,Get(game,"hideThreatValues"));
                Assert.AreEqual(false,Get(game,"yummnAcceptBuffer"));Assert.AreEqual(0f,Get(game,"greyStrength"));
                Assert.AreEqual(0,root.GetComponentsInChildren<YummnV08Effect>(true).Length);
                Assert.AreEqual(0,root.GetComponentsInChildren<KaitCombatEffectGraphic>(true).Length);
                Assert.AreEqual(0,((List<KaitCombatEffectGraphic>)Get(game,"activeCombatEffects")).Count);
            }
        }
        finally {Object.DestroyImmediate(host);Object.DestroyImmediate(root);}
    }
}
