using System.Collections;
using UnityEngine;

public sealed partial class KaitGame
{
    private IEnumerator PlayPool083Missile(Vector2Int from,Vector2Int to)
    {
        var go=new GameObject("Yummn V08 Merge Missile",typeof(RectTransform),typeof(CanvasRenderer),typeof(YummnV08Effect));
        go.transform.SetParent(battleEnemyHitLayer,false);
        var fx=go.GetComponent<YummnV08Effect>();fx.rectTransform.sizeDelta=Vector2.one*38;fx.Initialize(21,.4f);
        var start=YummnCellPosition(from);var end=YummnCellPosition(to);
        for(float t=0;t<.16f;t+=Time.unscaledDeltaTime)
        {if(fx==null)yield break;fx.rectTransform.position=Vector3.Lerp(start,end,t/.16f);yield return null;}
        if(go!=null)Destroy(go);
    }
}
