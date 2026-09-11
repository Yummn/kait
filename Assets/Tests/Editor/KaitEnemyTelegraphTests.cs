using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class KaitEnemyTelegraphTests
{
    static KaitEnemy Enemy(KaitEnemyType kind,Vector2Int cell)
    {var e=new KaitEnemy{id=(int)kind,hp=3,type=kind,life=KaitEnemyLife.Active,pos=Vector2Int.one};e.intent.type=kind==KaitEnemyType.Archer?KaitIntentType.LineShot:kind==KaitEnemyType.Warlock?KaitIntentType.CrossBlast:KaitIntentType.Melee;e.intent.target=cell;e.intent.direction=Vector2Int.right;e.intent.affectedCells.Add(cell);return e;}
    [TestCase(KaitEnemyType.Grunt)][TestCase(KaitEnemyType.Swordsman)][TestCase(KaitEnemyType.Guard)][TestCase(KaitEnemyType.Archer)][TestCase(KaitEnemyType.Warlock)][TestCase(KaitEnemyType.ShieldKnight)]
    public void CoversOnlyActualIntent(KaitEnemyType kind)
    {var p=new Vector2Int(2,3);var e=Enemy(kind,p);Assert.AreEqual(1,KaitTelegraphPlan.At(p,new[]{e}).count);Assert.AreEqual(0,KaitTelegraphPlan.At(new Vector2Int(4,4),new[]{e}).count);}
    [Test] public void OverlapDeduplicatesDirectionsAndSingleGraphic()
    {var p=new Vector2Int(2,3);var a=Enemy(KaitEnemyType.Archer,p);var b=Enemy(KaitEnemyType.Archer,p);var plan=KaitTelegraphPlan.At(p,new[]{a,b});Assert.AreEqual(2,plan.count);Assert.AreEqual(1,plan.arrows);var go=new GameObject("test",typeof(RectTransform),typeof(KaitEnemyTelegraph));try{var g=go.GetComponent<KaitEnemyTelegraph>();g.Configure(plan,false);Assert.AreEqual(1,go.GetComponents<Graphic>().Length);Assert.IsFalse(g.raycastTarget);Assert.IsFalse(g.maskable);}finally{Object.DestroyImmediate(go);}}
    [Test] public void DeadOrDisabledEnemyNoWarning()
    {var p=Vector2Int.one;var e=Enemy(KaitEnemyType.Grunt,p);e.hp=0;Assert.AreEqual(0,KaitTelegraphPlan.At(p,new[]{e}).count);e.hp=3;e.yummnFrozen=true;Assert.AreEqual(0,KaitTelegraphPlan.At(p,new[]{e}).count);e.yummnFrozen=false;e.intent.type=KaitIntentType.None;Assert.AreEqual(0,KaitTelegraphPlan.At(p,new[]{e}).count);}
    [Test] public void MageCenterAndArmRemainDistinct()
    {var p=new Vector2Int(3,3);var e=Enemy(KaitEnemyType.Warlock,p);var arm=p+Vector2Int.right;e.intent.affectedCells.Add(arm);Assert.IsTrue(KaitTelegraphPlan.At(p,new[]{e}).center);var plan=KaitTelegraphPlan.At(arm,new[]{e});Assert.IsTrue(plan.mage);Assert.IsFalse(plan.center);Assert.AreEqual(4,plan.towardCenter);}
}
