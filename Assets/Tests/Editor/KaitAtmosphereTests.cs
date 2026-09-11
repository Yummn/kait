using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class KaitAtmosphereTests
{
    private KaitRun Fresh() { var r=new KaitRun();r.Reset(42);r.enemies.Clear();r.spawns.Clear();return r; }
    private KaitEnemy Aiming(KaitRun r)
    {
        var e=new KaitEnemy{id=1,type=KaitEnemyType.Warlock,life=KaitEnemyLife.Active,rangedState=KaitRangedState.Aim,hp=2,pos=r.katePos+Vector2Int.right};
        e.intent.type=KaitIntentType.CrossBlast;e.intent.damage=1;e.intent.affectedCells.Add(r.katePos);r.enemies.Add(e);return e;
    }
    [Test] public void AimWarnsButFrozenDeadAndInvincibleDoNot()
    {
        var r=Fresh();var e=Aiming(r);Assert.IsTrue(r.IsKateInImminentDanger());
        e.frozenActions=1;Assert.IsFalse(r.IsKateInImminentDanger());e.frozenActions=0;
        e.life=KaitEnemyLife.Dead;Assert.IsFalse(r.IsKateInImminentDanger());e.life=KaitEnemyLife.Active;
        r.config.playerInvincible=true;Assert.IsFalse(r.IsKateInImminentDanger());
    }
    [Test] public void PreviewDoesNotConsumeCloakOrHexArmor()
    {
        var r=Fresh();var e=Aiming(r);r.passives.Add(KaitPassive.DisplacementCloak);
        Assert.IsFalse(r.IsKateInImminentDanger());Assert.IsFalse(r.IsKateInImminentDanger());
        r.passives.Clear();r.passives.Add(KaitPassive.HexArmor);e.cursed=true;
        Assert.IsFalse(r.IsKateInImminentDanger());Assert.IsFalse(e.hexArmorSpent);
    }
    [Test] public void RiftRespectsTimingSettingAndArcaneLock()
    {
        var r=Fresh();var s=new KaitSpawnRequest{targetCell=r.katePos,turnsUntilSpawn=2};r.spawns.Add(s);
        Assert.IsFalse(r.IsKateInImminentDanger());s.turnsUntilSpawn=1;Assert.IsTrue(r.IsKateInImminentDanger());
        r.config.enableRiftDamage=false;Assert.IsFalse(r.IsKateInImminentDanger());r.config.enableRiftDamage=true;
        r.passives.Add(KaitPassive.ArcaneLock);Assert.IsFalse(r.IsKateInImminentDanger());s.lockDelayed=true;Assert.IsTrue(r.IsKateInImminentDanger());
    }
    [Test] public void OverlayNeverBlocksInput()
    {
        var go=new GameObject("fx",typeof(RectTransform),typeof(KaitAtmosphereGraphic));
        try{var g=go.GetComponent<KaitAtmosphereGraphic>();g.SetState(1,1,3);Assert.IsFalse(g.raycastTarget);}
        finally{Object.DestroyImmediate(go);}
    }
}
