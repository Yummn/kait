using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class YummnV08Art
{
    public const string Root="KaitVisuals/Yummn/V08/";
    private static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
    public static Material Material=>YummnSnowCourtyard.StoneMaterial;
    // Generated sheet spacing is hand-drawn, not a perfect tile grid. Keep each
    // complete emblem and exclude neighboring strokes without editing the bitmap.
    private static readonly Rect[] iconBounds={
        new Rect(20,30,254,236),new Rect(289,30,220,236),new Rect(515,30,230,236),new Rect(748,30,241,236),new Rect(995,30,249,236),
        new Rect(20,274,236,231),new Rect(258,274,265,231),new Rect(526,274,220,231),new Rect(750,274,235,231),new Rect(986,274,259,231),
        new Rect(20,514,244,235),new Rect(265,514,228,235),new Rect(494,514,255,235),new Rect(750,514,242,235),new Rect(993,514,256,235),
        new Rect(17,750,247,232),new Rect(265,750,244,232),new Rect(511,750,235,232),new Rect(748,750,244,232),new Rect(994,750,255,232),
        new Rect(25,990,226,250),new Rect(253,990,260,250),new Rect(515,990,230,250)};
    public static Sprite Icon(int index)
    {
        string key="Icon"+index;if(cache.TryGetValue(key,out var cached))return cached;
        var texture=Resources.Load<Texture2D>(Root+"CardIcons");if(texture==null||index<0||index>=iconBounds.Length)return null;
        var b=iconBounds[index];float s=texture.width/1254f;
        return cache[key]=Sprite.Create(texture,new Rect(b.x*s,texture.height-(b.y+b.height)*s,b.width*s,b.height*s),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
    }
    public static Sprite Effect(int index)=>Slice("Effects",index,4,3);
    private static Sprite Slice(string name,int index,int cols,int rows)
    {
        string key=name+index;if(cache.TryGetValue(key,out var sprite))return sprite;
        var texture=Resources.Load<Texture2D>(Root+name);if(texture==null||index<0||index>=cols*rows)return null;
        float w=texture.width/(float)cols,h=texture.height/(float)rows;
        return cache[key]=Sprite.Create(texture,new Rect(index%cols*w,(rows-1-index/cols)*h,w,h),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
    }
}
// Initializes sprite/material synchronously; never draws the default white quad.
public sealed class YummnV08Effect : Image
{
    private float age,duration;private Vector2 baseSize;
    private static readonly Dictionary<int,Sprite[]> clips=new Dictionary<int,Sprite[]>();
    private Sprite[] frames;
    private bool selected,persistent,loop;
    public int FrameIndex {get;private set;}
    public static string SelectedClip(int index)=>index==0?"WaterC":index==1?"AirA":index==2?"PillarC":index==3?"WinterA":index==4?"FireA":index==5?"ShatterA":index==6?"TeleportB":index==7?"DarknessB":index==8?"PalmBurstA":index==9?"KiA":index==10?"ExhaustC":index==11?"RecoverB":index==12?"PalmMarkB":index==13?"AimDeniedC":index==14?"ShadowA":index==15?"GhostHitC":index==16?"RepoolStun_A":index==17?"RepoolIceGuard_B":index==18?"RepoolHeal_A":index==19?"RepoolDecoy_B":index==20?"RepoolReflect_A":index==21?"RepoolDeflect_A":null;
    public static float ClipDuration(int index)=>index==2||index==7||index==12||index==14?.8f:index==0||index==3||index==6||index==9||index==10||index==11||index==13?.48f:.4f;
    protected override void OnEnable(){base.OnEnable();age=0;}
    public void InitializePersistent(int index,bool looping)
    {Initialize(index,ClipDuration(index));persistent=true;loop=looping;}
    protected override void Awake(){base.Awake();color=Color.clear;raycastTarget=false;maskable=false;}
    public void Initialize(int index,float seconds)
    {
        selected=SelectedClip(index)!=null;frames=null;FrameIndex=0;persistent=false;loop=false;
        if(selected)
        {
            if(!clips.TryGetValue(index,out frames))
            {
                var sheet=Resources.Load<Texture2D>("KaitVisuals/Yummn/Frames/"+SelectedClip(index));
                if(sheet!=null)
                {
                    frames=new Sprite[8];
                    for(int i=0;i<8;i++)
                    {
                        int x0=Mathf.RoundToInt(i%4*sheet.width/4f),x1=Mathf.RoundToInt((i%4+1)*sheet.width/4f);
                        int y0=Mathf.RoundToInt((1-i/4)*sheet.height/2f),y1=Mathf.RoundToInt((2-i/4)*sheet.height/2f);
                        frames[i]=Sprite.Create(sheet,new Rect(x0,y0,x1-x0,y1-y0),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
                    }
                    clips[index]=frames;
                }
            }
            // New sheets have native alpha; the old chroma-key material would alter their colors.
            sprite=frames!=null?frames[0]:null;material=null;preserveAspect=true;
            duration=ClipDuration(index);age=0;baseSize=rectTransform.sizeDelta;
            color=sprite!=null?Color.white:Color.clear;return;
        }
        sprite=YummnV08Art.Effect(index);material=YummnV08Art.Material;preserveAspect=true;
        duration=seconds;age=0;baseSize=rectTransform.sizeDelta;color=sprite!=null?Color.white:Color.clear;
    }
    private void Update()
    {
        age+=Time.unscaledDeltaTime;float t=loop?Mathf.Repeat(age,duration)/duration:Mathf.Clamp01(age/duration);
        if(selected)
        {
            if(sprite!=null&&frames!=null){FrameIndex=Mathf.Min(7,Mathf.FloorToInt(t*8));sprite=frames[FrameIndex];}
            rectTransform.sizeDelta=baseSize;color=sprite!=null?Color.white:Color.clear;
            if(t>=1&&!persistent)Destroy(gameObject);return;
        }
        rectTransform.sizeDelta=baseSize*Mathf.Lerp(.8f,1.08f,1-Mathf.Pow(1-t,3));
        color=new Color(1,1,1,sprite==null?0:Mathf.Min(1,(1-t)*3));
        if(t>=1)Destroy(gameObject);
    }
    protected override void OnPopulateMesh(VertexHelper vh){if(sprite==null){vh.Clear();return;}base.OnPopulateMesh(vh);}
}
