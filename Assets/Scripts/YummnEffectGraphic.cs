using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Selected eight-frame clips. This component never controls gameplay timing.
public sealed class YummnEffectGraphic : Image
{
    public int kind { get => effectKind; set => Initialize(value); }
    private int effectKind;
    private float age;
    private bool initialized;
    private static readonly string[] Names={"Punch","Flurry","Palm","Stun","Ward","Frost"};
    private static readonly Dictionary<int,Sprite> Cache=new Dictionary<int,Sprite>();
    private static readonly Dictionary<int,Sprite[]> Clips=new Dictionary<int,Sprite[]>();
    private Sprite[] frames;
    public int FrameIndex { get; private set; }
    public static string SelectedClip(int value)=>value==0?"PunchB":value==1?"FlurryB":value==3?"StunB":value==4?"BlockA":null;
    public static float Duration(int value)=>value==1?.48f:value==3?.64f:value==4?.4f:.32f;
    protected override void Awake()
    {
        base.Awake();raycastTarget=false;maskable=false;preserveAspect=true;
        color=Color.clear;
    }
    public void Initialize(int value)
    {
        effectKind=Mathf.Clamp(value,0,Names.Length-1);
        age=0f;initialized=true;
        frames=null;FrameIndex=0;
        var selected=SelectedClip(effectKind);
        if(selected!=null)
        {
            if(!Clips.TryGetValue(effectKind,out frames))
            {
                var sheet=Resources.Load<Texture2D>("KaitVisuals/Yummn/Frames/"+selected);
                if(sheet!=null)
                {
                    frames=new Sprite[8];
                    for(int i=0;i<8;i++)
                    {
                        // Integer boundaries, no resizing of the selected art.
                        int x0=Mathf.RoundToInt(i%4*sheet.width/4f),x1=Mathf.RoundToInt((i%4+1)*sheet.width/4f);
                        int y0=Mathf.RoundToInt((1-i/4)*sheet.height/2f),y1=Mathf.RoundToInt((2-i/4)*sheet.height/2f);
                        frames[i]=Sprite.Create(sheet,new Rect(x0,y0,x1-x0,y1-y0),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
                    }
                    Clips[effectKind]=frames;
                }
            }
            sprite=frames!=null?frames[0]:null;
            rectTransform.localScale=Vector3.one;
            color=sprite!=null?Color.white:Color.clear;
            return;
        }
        if(!Cache.TryGetValue(effectKind,out var art)||art==null)
        {
            var t=Resources.Load<Texture2D>("KaitVisuals/Yummn/Effects/"+Names[effectKind]);
            if(t!=null)Cache[effectKind]=art=Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f,100);
        }
        // Coroutines may spawn us after Update but before the canvas renders.
        // Bind art and the initial pose now, never on the following frame.
        sprite=art;
        rectTransform.localScale=Vector3.one*.66f;
        color=art!=null?Color.white:Color.clear;
    }
    protected override void OnPopulateMesh(VertexHelper vertices)
    {
        // A default Image draws a white rectangle with no sprite. Missing art
        // must remain invisible, even if another caller changes our tint.
        if(!initialized||sprite==null){vertices.Clear();return;}
        base.OnPopulateMesh(vertices);
    }
    private void Update()
    {
        if(!initialized)return;
        age+=Time.unscaledDeltaTime;
        if(SelectedClip(effectKind)!=null)
        {
            // Missing/cleared art must stay invisible, never show Image's white quad.
            if(sprite==null||frames==null){color=Color.clear;if(age>=Duration(effectKind))Destroy(gameObject);return;}
            float duration=Duration(effectKind);
            FrameIndex=Mathf.Clamp(Mathf.FloorToInt(age/duration*8),0,7);
            sprite=frames[FrameIndex];
            rectTransform.localScale=Vector3.one;
            color=Color.white;
            if(age>=duration)Destroy(gameObject);
            return;
        }
        float life=kind==4?.5f:.32f,p=Mathf.Clamp01(age/life);
        rectTransform.localScale=Vector3.one*Mathf.Lerp(.66f,1,1-Mathf.Pow(1-p,3));
        color=new Color(1,1,1,sprite!=null?Mathf.Clamp01((1-p)*1.5f):0f);
        if(p>=1)Destroy(gameObject);
    }
}
