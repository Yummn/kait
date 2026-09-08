using UnityEngine;
using UnityEngine.UI;

// Presentation only: never owns the turn lock or modifies board state.
public sealed class KaitBuildEffect : MonoBehaviour
{
    private Image art;
    private float started, duration;
    private static readonly Sprite[] sprites=new Sprite[16];
    public static KaitBuildEffect Play(Transform layer,Vector3 worldPosition,int motif,float size=80,float seconds=.55f)
    {
        var texture=Resources.Load<Texture2D>(KaitCardSkin.Root+"Effects");
        if(texture==null || layer==null)return null;
        motif=Mathf.Clamp(motif,0,15);
        if(sprites[motif]==null)
        {
            float w=texture.width/4f,h=texture.height/4f;
            sprites[motif]=Sprite.Create(texture,new Rect(motif%4*w,(3-motif/4)*h,w,h),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
        }
        var go=new GameObject("Build Effect "+motif,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image),typeof(KaitBuildEffect));
        go.transform.SetParent(layer,false);go.transform.position=worldPosition;
        var effect=go.GetComponent<KaitBuildEffect>();effect.art=go.GetComponent<Image>();
        effect.art.sprite=sprites[motif];effect.art.raycastTarget=false;
        effect.art.rectTransform.sizeDelta=Vector2.one*size;
        effect.started=Time.unscaledTime;effect.duration=seconds;
        return effect;
    }
    private void Update()
    {
        float t=(Time.unscaledTime-started)/duration;
        if(t>=1){Destroy(gameObject);return;}
        float pop=1-Mathf.Pow(1-Mathf.Clamp01(t/.3f),3);
        transform.localScale=Vector3.one*Mathf.Lerp(.65f,1.05f,pop);
        art.color=new Color(1,1,1,Mathf.Min(t*12,1)*Mathf.Clamp01((1-t)*2));
    }
    public static int Motif(KaitSkill skill)
    {
        switch(skill)
        {
            case KaitSkill.HexCurse:return 0;
            case KaitSkill.DispelMagic:return 1;
            case KaitSkill.MistyStep:case KaitSkill.RelentlessHex:case KaitSkill.DimensionDoor:return 2;
            case KaitSkill.LevistusTomb:return 6;
            case KaitSkill.Command:return 13;
            case KaitSkill.GraspHadar:return 5;
            case KaitSkill.EldritchSmite:return 10;
            default:return -1;
        }
    }
    public static int Motif(KaitPassive passive)
    {
        switch(passive)
        {
            case KaitPassive.BloodBookmark:case KaitPassive.ArcaneLock:return 3;
            case KaitPassive.HexArmor:case KaitPassive.AccursedSpecter:case KaitPassive.DisplacementCloak:return 7;
            case KaitPassive.MasterHex:case KaitPassive.MaddeningHex:return 0;
            case KaitPassive.Lifedrinker:return 8;
            case KaitPassive.Devil:case KaitPassive.BladeCovenant:return 9;
            case KaitPassive.BagHolding:return 11;
            case KaitPassive.Passwall:return 12;
            case KaitPassive.OldNewsArchive:return 14;
            case KaitPassive.Simplify:return 15;
            case KaitPassive.StaggeringSmite:case KaitPassive.RepellingBlast:return 4;
            default:return -1;
        }
    }
}
