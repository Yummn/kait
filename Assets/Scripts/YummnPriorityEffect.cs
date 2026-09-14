using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Eight-frame 4x2 approved effect sheets. Kept separate from the older V08
// atlas so both art systems can coexist without chroma-key materials.
public sealed class YummnPriorityEffect : Image
{
    private static readonly Dictionary<string,Sprite[]> Cache=new Dictionary<string,Sprite[]>();
    private Sprite[] frames;
    private float age,duration;
    public string ClipName {get;private set;}
    public int FrameIndex {get;private set;}

    protected override void Awake()
    {
        base.Awake();raycastTarget=false;maskable=false;preserveAspect=true;color=Color.clear;
    }

    public void Initialize(string clip,float seconds=.56f)
    {
        ClipName=clip;duration=Mathf.Max(.08f,seconds);age=0;FrameIndex=0;
        if(!Cache.TryGetValue(clip,out frames))
        {
            var sheet=Resources.Load<Texture2D>("KaitVisuals/Yummn/PriorityFx/"+clip);
            if(sheet!=null)
            {
                frames=new Sprite[8];
                for(int i=0;i<8;i++)
                {
                    int x0=Mathf.RoundToInt(i%4*sheet.width/4f),x1=Mathf.RoundToInt((i%4+1)*sheet.width/4f);
                    int y0=Mathf.RoundToInt((1-i/4)*sheet.height/2f),y1=Mathf.RoundToInt((2-i/4)*sheet.height/2f);
                    frames[i]=Sprite.Create(sheet,new Rect(x0,y0,x1-x0,y1-y0),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
                }
            }
            Cache[clip]=frames;
        }
        sprite=frames!=null?frames[0]:null;color=sprite!=null?Color.white:Color.clear;
    }

    private void Update()
    {
        age+=Time.unscaledDeltaTime;
        float t=Mathf.Clamp01(age/duration);
        if(frames!=null){FrameIndex=Mathf.Min(7,Mathf.FloorToInt(t*8));sprite=frames[FrameIndex];}
        color=sprite!=null?new Color(1,1,1,Mathf.Min(1,(1-t)*5)):Color.clear;
        if(t>=1)Destroy(gameObject);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {if(sprite==null){vh.Clear();return;}base.OnPopulateMesh(vh);}
}
