using NUnit.Framework;
using Spine.Unity;
using UnityEngine;

public class EnemyPoseLoopTests
{
    [TestCase("100161","01_")]
    [TestCase("105731","04_")]
    [TestCase("106331","08_")]
    [TestCase("112731","06_")]
    [TestCase("111031","26_")]
    [TestCase("104731","05_")]
    public void LockedEnemyLoopsWithoutQueuedIdle(string id,string prefix)
    {
        var host=new GameObject("Pose Test",typeof(Canvas));EnemySpineView view=null;
        try
        {
            var data=Resources.Load<SkeletonDataAsset>($"Characters/Enemies/{id}/{id}_SkeletonData");
            view=EnemySpineView.Create(data,prefix,host.transform,new Vector2(115,115),"Test");
            view.PlayPrepareAttack();var entry=view.CurrentAnimation;
            Assert.IsTrue(entry.Loop);Assert.IsNull(entry.Next);
            view.SyncPreparation(true);Assert.AreSame(entry,view.CurrentAnimation);
            view.SyncPreparation(false);Assert.AreEqual(prefix+"idle",view.CurrentAnimation.Animation.Name);
            view.PlayDamage();entry=view.CurrentAnimation;view.SyncPreparation(true);
            Assert.AreSame(entry,view.CurrentAnimation);
        }
        finally{view?.Destroy();Object.DestroyImmediate(host);}
    }
}
