using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class KaitGame
{
    private bool yummnMoving;
    private Vector2Int yummnMoveFrom,yummnMoveTo;
    private float yummnMoveProgress;
    private Coroutine yummnThreatPulseRoutine;
    private Vector3 KaitVisualPosition(Vector2Int cell)
    {
        return run.IsYummn&&yummnMoving
            ? Vector3.Lerp(YummnCellPosition(yummnMoveFrom),YummnCellPosition(yummnMoveTo),yummnMoveProgress)
            : YummnCellPosition(cell);
    }
    private void StopYummnThreatPulses()
    {
        if(yummnThreatPulseRoutine==null)return;
        StopCoroutine(yummnThreatPulseRoutine);yummnThreatPulseRoutine=null;
        if(threatCells!=null)foreach(var cell in threatCells)if(cell!=null)cell.rectTransform.localScale=Vector3.one;
    }
    private Vector2Int yummnVisualIce=YummnRun.NoCell,yummnVisualDarkness=YummnRun.NoCell;
    private Image yummnIce,yummnDarkness,yummnPalm;
    private readonly YummnV08Effect[] yummnShadows=new YummnV08Effect[49];
    private void OnApplicationFocus(bool focused){if(!focused){yummnBufferedDirection=null;yummnAcceptBuffer=false;}}
    private Vector3 YummnCellPosition(Vector2Int p)=>battleCells[p.x+p.y*KaitRun.BattleSize].rectTransform.position;
    private Image YummnTerrainImage(string name,int effect,float size)
    {
        if(effect==2||effect==7||effect==8)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(YummnV08Effect));
            go.transform.SetParent(effect==8?battleEnemyHitLayer:battleUnderEffectLayer,false);
            var animated=go.GetComponent<YummnV08Effect>();animated.rectTransform.sizeDelta=Vector2.one*size;
            animated.InitializePersistent(effect==8?12:effect,effect!=2);return animated;
        }
        var im=Rect(name,battleUnderEffectLayer,Vector2.zero,Vector2.one*size,Color.clear);
        im.sprite=YummnV08Art.Effect(effect);im.material=YummnV08Art.Material;
        im.preserveAspect=true;im.raycastTarget=false;im.maskable=false;
        im.color=im.sprite!=null?Color.white:Color.clear;
        return im;
    }
    private void PlaceYummnTerrain(Image image,Vector2Int p)
    {
        int index=p.x+p.y*7;var cell=battleCells[index].transform;
        image.rectTransform.SetParent(cell,false);image.rectTransform.anchoredPosition=Vector2.zero;
        int warningIndex=battleWarningLines[index].transform.parent.GetSiblingIndex();
        if(image.transform.GetSiblingIndex()>warningIndex)image.transform.SetSiblingIndex(warningIndex);
    }
    private void RefreshYummnTerrain()
    {
        if(battleUnderEffectLayer==null)return;
        if(run.IsYummn)
        {
            if(yummnIce==null)yummnIce=YummnTerrainImage("Ice Pillar",2,110);
            if(yummnDarkness==null)yummnDarkness=YummnTerrainImage("Hollow Darkness",7,130);
            if(yummnPalm==null)yummnPalm=YummnTerrainImage("Quivering Palm Direction",8,48);
        }
        var ice=busy?yummnVisualIce:run.Yummn.icePillar;
        var dark=busy?yummnVisualDarkness:run.Yummn.darkness;
        if(yummnIce!=null){yummnIce.gameObject.SetActive(run.IsYummn&&InsideBattle(ice));if(InsideBattle(ice))PlaceYummnTerrain(yummnIce,ice);}
        if(yummnDarkness!=null){yummnDarkness.gameObject.SetActive(run.IsYummn&&InsideBattle(dark));if(InsideBattle(dark))PlaceYummnTerrain(yummnDarkness,dark);}
        var marked=(animatedEnemies??run.enemies).Find(e=>e.id==run.Yummn.palmEnemyId&&e.life!=KaitEnemyLife.Dead);
        if(yummnPalm!=null)
        {
            yummnPalm.gameObject.SetActive(run.IsYummn&&marked!=null);
            if(marked!=null){yummnPalm.rectTransform.position=YummnCellPosition(marked.pos);yummnPalm.rectTransform.localEulerAngles=new Vector3(0,0,HalfArrowAngle(run.Yummn.palmDirection));}
        }
        for(int y=1;y<=5;y++)for(int x=1;x<=5;x++)
        {
            int i=x+y*7;var p=new Vector2Int(x,y);
            bool shadow=run.IsYummn&&run.IsYummnShadow(p);
            if(shadow&&yummnShadows[i]==null)
            {
                var go=new GameObject("Shadow Corners",typeof(RectTransform),typeof(YummnV08Effect));go.transform.SetParent(battleUnderEffectLayer,false);
                var g=go.GetComponent<YummnV08Effect>();g.rectTransform.sizeDelta=Vector2.one*140;
                g.InitializePersistent(14,true);yummnShadows[i]=g;
            }
            if(yummnShadows[i]!=null){yummnShadows[i].gameObject.SetActive(shadow);if(shadow)PlaceYummnTerrain(yummnShadows[i],p);}
        }
    }
    private void PlayV08Fx(Vector2Int p,int index,KaitDirection direction,float size)
    {
        if(!InsideBattle(p))return;
        var go=new GameObject("Yummn V08 "+index,typeof(RectTransform),typeof(YummnV08Effect));
        go.transform.SetParent(index==6||index==7||index==10?battleUnderEffectLayer:battleEnemyHitLayer,false);
        var fx=go.GetComponent<YummnV08Effect>();fx.rectTransform.sizeDelta=Vector2.one*size;fx.rectTransform.position=YummnCellPosition(p);
        if(index==0||index==1||index==3||index==4)
        {
            // All four approved sheets face right. Keep the trail between attacker and target.
            fx.rectTransform.sizeDelta=Vector2.one*116;
            fx.rectTransform.anchoredPosition-=(Vector2)KaitRun.Delta(direction)*30;
            fx.rectTransform.anchoredPosition+=new Vector2(0,8);
        }
        // Aim denial is a head cue, distinct from the teleport at the character's center.
        if(index==6)fx.rectTransform.sizeDelta=Vector2.one*140;
        if(index==9){fx.rectTransform.sizeDelta=Vector2.one*85;fx.rectTransform.anchoredPosition+=new Vector2(0,78);}
        if(index==10){fx.rectTransform.sizeDelta=Vector2.one*125;fx.rectTransform.anchoredPosition+=new Vector2(0,-14);}
        if(index==11)fx.rectTransform.sizeDelta=Vector2.one*140;
        if(index==13){fx.rectTransform.sizeDelta=Vector2.one*46;fx.rectTransform.anchoredPosition+=new Vector2(0,64);}
        if(index==0||index==1||index==3||index==4||index==8)fx.rectTransform.localEulerAngles=new Vector3(0,0,HalfArrowAngle(KaitRun.Delta(direction)));
        fx.Initialize(index,index==10?.48f:.32f);
    }
    private IEnumerator AnimateYummnMove(Vector2Int from,Vector2Int to,YummnMoveCause cause,KaitDirection direction,bool preserveAttack=false,YummnPhase? phaseAtStart=null)
    {
        if(from==to)yield break;
        // Rules have already finished resolving; use the starting phase even
        // when this step restores full Ki or spends the final point of Ki.
        var movementPhase=phaseAtStart??run.KiPhase;
        if(movementPhase==YummnPhase.Exhausted)ClearAllTrailVisuals();
        kaitSpine?.Face(direction);
        if(cause==YummnMoveCause.Teleport){PlayV08Fx(from,6,direction,104);PlayV08Fx(to,6,direction,104);YummnAudio.Play("Shadow");}
        else {if(!preserveAttack)kaitSpine?.PlayLoop(YummnMovementStyle.Animation(movementPhase));YummnAudio.Play("Move");}
        float duration=cause==YummnMoveCause.Teleport?.12f:Mathf.Min(.3f,.14f+Vector2Int.Distance(from,to)*.025f);
        yummnMoving=true;yummnMoveFrom=from;yummnMoveTo=to;yummnMoveProgress=0;
        int ghostCount=YummnMovementStyle.GhostCount(movementPhase,cause,Vector2Int.Distance(from,to)),ghosts=0;
        try
        {
            for(float t=0;t<duration;t+=Time.unscaledDeltaTime)
            {
                yummnMoveProgress=EaseOutCubic(t/duration);
                if(kaitSpine!=null)kaitSpine.Root.position=KaitVisualPosition(from);
                while(ghosts<ghostCount&&yummnMoveProgress>(ghosts+1f)/(ghostCount+1f))
                {ghosts++;var ghost=CreateGhostToken(Vector3.Lerp(YummnCellPosition(from),YummnCellPosition(to),ghosts/(ghostCount+1f)),Mathf.Max(1,run.Ki),direction,.62f);StartCoroutine(FadeAndDestroyTrail(ghost,.16f));}
                yield return null;
            }
            displayKate=to;
            if(kaitSpine!=null)kaitSpine.Root.position=YummnCellPosition(to);
        }
        finally {yummnMoving=false;}
    }
    private IEnumerator AnimateYummnEnemyMove(KaitEnemy actor,Vector2Int from,Vector2Int to)
    {
        var view=EnemySpine(actor);if(view==null)yield break;
        view.Face(to-from);var a=YummnCellPosition(from);var b=YummnCellPosition(to);
        for(float t=0;t<.18f;t+=Time.unscaledDeltaTime){view.Root.position=Vector3.Lerp(a,b,EaseOutCubic(t/.18f));yield return null;}
        view.Root.position=b;
    }
}

public static class YummnMovementStyle
{
    public static string Animation(YummnPhase phase)=>phase==YummnPhase.Exhausted?KaitSpineView.YummnWalk:KaitSpineView.YummnRun;
    public static int GhostCount(YummnPhase phase,YummnMoveCause cause,float distance)=>
        phase==YummnPhase.Exhausted||cause==YummnMoveCause.Teleport?0:Mathf.Max(0,Mathf.CeilToInt(distance*2)-1);
}

// Geometry rather than another filled tile: warnings and feet remain readable.
public sealed class YummnShadowCorners:MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();var r=rectTransform.rect;
        foreach(int x in new[]{-1,1})foreach(int y in new[]{-1,1})
        {
            float px=x<0?r.xMin:r.xMax,py=y<0?r.yMin:r.yMax;
            Quad(vh,new Rect(x<0?px:px-14,y<0?py:py-2,14,2));
            Quad(vh,new Rect(x<0?px:px-2,y<0?py:py-14,2,14));
        }
    }
    private void Quad(VertexHelper vh,Rect r)
    {int n=vh.currentVertCount;vh.AddVert(new Vector3(r.xMin,r.yMin),color,Vector2.zero);vh.AddVert(new Vector3(r.xMin,r.yMax),color,Vector2.zero);vh.AddVert(new Vector3(r.xMax,r.yMax),color,Vector2.zero);vh.AddVert(new Vector3(r.xMax,r.yMin),color,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
}
