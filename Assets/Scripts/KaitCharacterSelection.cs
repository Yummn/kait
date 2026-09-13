using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// A self-contained native UI: no web view, Spine previews, or gameplay style masks.
public sealed class KaitCharacterSelection : MonoBehaviour
{
    public Action<KaitCharacter> StartCharacter;
    public Action<string> ContinueCharacter;
    public Action Back;
    public KaitCharacter Selected { get; private set; }
    public readonly List<Button> StartButtons = new List<Button>();
    public readonly List<Button> ContinueButtons = new List<Button>();
    RectTransform layout;
    Font font;
    Text status;
    readonly List<Image> highlights=new List<Image>();
    static Color Purple=new Color(.93f,.74f,.87f), Cyan=new Color(.61f,.91f,.94f);

    public static KaitCharacterSelection Create(Transform parent, Font font, Func<string,bool> hasSave)
    {
        var root=new GameObject("Character Selection",typeof(RectTransform),typeof(Image),typeof(KaitCharacterSelection));
        root.transform.SetParent(parent,false);
        var rt=(RectTransform)root.transform;rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;
        root.GetComponent<Image>().color=new Color(.035f,.04f,.075f);
        var ui=root.GetComponent<KaitCharacterSelection>();ui.font=font;ui.Build(hasSave);ui.Fit();return ui;
    }
    void Build(Func<string,bool> hasSave)
    {
        layout=Box("1920x1080 composition",transform,0,0,1920,1080);
        for(int i=0;i<2;i++)
        {
            var c=(KaitCharacter)i;
            var art=Box(c+" CG",layout,0,0,1920,1080).gameObject.AddComponent<KaitSelectionArt>();
            art.Configure(i==1,Resources.Load<Texture2D>("KaitVisuals/CharacterSelection/"+c));
            var hit=art.gameObject.AddComponent<Button>();hit.transition=Selectable.Transition.None;hit.targetGraphic=art;
            hit.onClick.AddListener(()=>Select(c));
        }
        // Underlay first, typography afterward: no decorative line can cover labels.
        for(int i=5;i>=0;i--)
        {
            var line=Panel("Seam "+i,layout,960,540,3+i*5,1123,new Color(.77f,.87f,1,i==0?.95f:.025f));
            line.rectTransform.localEulerAngles=new Vector3(0,0,-15.88f);line.raycastTarget=false;
        }
        var diamond=Panel("Seam diamond",layout,960,540,40,40,new Color(.08f,.09f,.16f));diamond.rectTransform.localEulerAngles=new Vector3(0,0,45);
        Label("✦",layout,938,519,44,44,28,Color.white,TextAnchor.MiddleCenter);
        Label("KAIT",layout,64,42,145,45,30,Color.white,TextAnchor.MiddleLeft,FontStyle.Bold);
        Label("/  DUAL FATES",layout,216,48,250,34,15,new Color(.84f,.8f,.88f));
        Label("CHOOSE YOUR PATH",layout,610,103,700,25,15,new Color(.92f,.86f,.88f),TextAnchor.MiddleCenter);
        Label("选择你的战斗方式",layout,570,134,780,54,34,Color.white,TextAnchor.MiddleCenter);
        ButtonAt("返回",layout,1720,44,134,50,new Color(.2f,.19f,.26f),()=>Back?.Invoke());
        for(int i=0;i<2;i++)
        {
            var c=(KaitCharacter)i;float x=i==0?94:1220;Color accent=i==0?Purple:Cyan;
            Label(i==0?"01":"02",layout,x,514,180,134,90,new Color(1,1,1,.055f),TextAnchor.MiddleLeft,FontStyle.Bold);
            Label(i==0?"逐影剑锋 · 蓄势连斩":"流风拳影 · 气息轮转",layout,x,628,610,35,22,accent);
            Label(c.ToString().ToUpperInvariant(),layout,x,654,610,118,78,Color.white,TextAnchor.MiddleLeft,FontStyle.Bold);
            string[] tags=i==0?new[]{"大剑","助跑蓄势","击杀转向"}:new[]{"拳斗","气与残影","三宗派构筑"};
            float tx=x;
            foreach(var tag in tags)
            {
                float width=tag.Length*19+30;var tagPanel=Panel(tag,layout,tx+width/2,794,width,34,new Color(1,1,1,.075f));
                Label(tag,tagPanel.transform,-width/2,-17,width,34,18,accent,TextAnchor.MiddleCenter);tx+=width+12;
            }
            Label(i==0?"以助跑积蓄斩击力量，击破敌人后自由转向。\n读准路线，让每一次击杀成为下一斩的起点。":"在高速滑行与气竭恢复之间掌握战斗节奏。\n自由混搭散打、四象与暗影，打出自己的流派。",layout,x,824,610,72,21,new Color(.88f,.86f,.92f));
            var button=ButtonAt("选择 "+c+"  →",layout,x,911,340,62,new Color(accent.r*.22f,accent.g*.22f,accent.b*.22f),()=>StartCharacter?.Invoke(c));
            StartButtons.Add(button);highlights.Add(button.GetComponent<Image>());
            var hover=button.gameObject.AddComponent<KaitSelectionHover>();hover.Entered=()=>Select(c);
            var saves=new List<string>();foreach(var key in SaveKeys(c))if(hasSave(key))saves.Add(key);
            for(int j=0;j<saves.Count;j++)
            {
                string key=saves[j];
                string title=key.EndsWith("0.8.2")?"继续 v0.8.2":key.EndsWith("0.8.1")?"旧版 v0.8.1":key.EndsWith("0.8")?"旧版 v0.8":"继续上次";
                var resume=ButtonAt(title,layout,x+j*174,987,166,44,new Color(.13f,.14f,.2f),()=>ContinueCharacter?.Invoke(key));
                ContinueButtons.Add(resume);
            }
        }
        status=Label("选择角色开启新旅程",layout,710,1042,500,25,16,new Color(.9f,.82f,.8f),TextAnchor.MiddleCenter);
        Label("两种战斗节奏 · 同一个双盘战场",layout,64,1042,610,25,15,new Color(.7f,.69f,.77f));
        Select(KaitCharacter.Kait);
    }
    public static string[] SaveKeys(KaitCharacter c)=>c==KaitCharacter.Yummn?new[]{"Kait.Run.Yummn.0.9.0"}:new[]{"Kait.Run.Kait"};
    public void Select(KaitCharacter c)
    {
        Selected=c;
        for(int i=0;i<highlights.Count;i++)
        {
            Color a=i==0?Purple:Cyan;bool selected=i==(int)c;
            highlights[i].color=selected?new Color(a.r*.4f,a.g*.4f,a.b*.4f):new Color(a.r*.16f,a.g*.16f,a.b*.16f);
        }
        if(status!=null)status.text="选择 "+c+" · 开始新的旅程";
    }
    public void Fit()
    {
        if(layout==null)return;var r=((RectTransform)transform).rect;
        layout.localScale=Vector3.one*Mathf.Min(r.width/1920f,r.height/1080f);
    }
    void LateUpdate()=>Fit();
    void OnRectTransformDimensionsChange()=>Fit();
    RectTransform Box(string name,Transform parent,float x,float y,float w,float h)
    {
        var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var rt=(RectTransform)go.transform;
        rt.sizeDelta=new Vector2(w,h);rt.anchoredPosition=new Vector2(x,-y);return rt;
    }
    Image Panel(string name,Transform parent,float x,float y,float w,float h,Color color)
    {
        var rt=Box(name,parent,parent==layout?x-960:x,parent==layout?y-540:y,w,h);
        var image=rt.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return image;
    }
    Text Label(string value,Transform parent,float x,float y,float w,float h,int size,Color color,TextAnchor align=TextAnchor.MiddleLeft,FontStyle style=FontStyle.Normal)
    {
        var rt=Box(value,parent,parent==layout?x+w/2-960:x+w/2,parent==layout?y+h/2-540:y+h/2,w,h);
        var t=rt.gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.fontStyle=style;t.color=color;t.alignment=align;t.raycastTarget=false;
        t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;
    }
    Button ButtonAt(string label,Transform parent,float x,float y,float w,float h,Color color,Action action)
    {
        var image=Panel(label,parent,x+w/2,y+h/2,w,h,color);image.raycastTarget=true;
        var outline=image.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.82f,.86f,.95f,.55f);outline.effectDistance=new Vector2(1,-1);
        var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;b.navigation=new Navigation{mode=Navigation.Mode.None};
        var colors=b.colors;colors.highlightedColor=new Color(1.2f,1.2f,1.2f);colors.pressedColor=new Color(.65f,.65f,.7f);colors.fadeDuration=.12f;b.colors=colors;
        Label(label,image.transform,-w/2+12,-h/2,w-24,h,h>50?24:20,Color.white,TextAnchor.MiddleCenter);
        b.onClick.AddListener(()=>action());return b;
    }
}

public sealed class KaitSelectionHover : MonoBehaviour, IPointerEnterHandler
{
    public Action Entered;
    public void OnPointerEnter(PointerEventData e)=>Entered?.Invoke();
}

// UVs match the approved HTML's cover crop. Both halves share the same seam equation.
public sealed class KaitSelectionArt : MaskableGraphic
{
    public bool Right { get; private set; }
    Texture2D art;
    public override Texture mainTexture=>art!=null?art:Texture2D.whiteTexture;
    public void Configure(bool right,Texture2D texture){Right=right;art=texture;SetAllDirty();}
    public static float Seam(float y)=>Mathf.Lerp(.42f,.58f,y);
    public override bool Raycast(Vector2 sp,Camera cam)
    {
        if(!base.Raycast(sp,cam)||!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,sp,cam,out var p))return false;
        var r=rectTransform.rect;return Right?(p.x-r.xMin)/r.width>=Seam((p.y-r.yMin)/r.height):(p.x-r.xMin)/r.width<=Seam((p.y-r.yMin)/r.height);
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();var r=rectTransform.rect;
        float[] ys={0,.12f,.37f,.61f,.8f,1};float[] brightness={.075f,.1f,.42f,1,1,.5f};
        for(int k=0;k<ys.Length-1;k++)
        {
            int start=vh.currentVertCount;
            Add(vh,r,ys[k],false,brightness[k]);Add(vh,r,ys[k],true,brightness[k]);
            Add(vh,r,ys[k+1],true,brightness[k+1]);Add(vh,r,ys[k+1],false,brightness[k+1]);
            vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);
        }
    }
    void Add(VertexHelper vh,Rect r,float y,bool edge,float brightness)
    {
        float x=Right?(edge?1:Seam(y)):(edge?Seam(y):0);
        float origin=Right?437.4f:-387.2f;
        var v=UIVertex.simpleVert;v.position=new Vector3(r.xMin+x*r.width,r.yMin+y*r.height);v.uv0=new Vector2((x*1920-origin)/1920,y);
        // This project's linear canvas does not convert UI vertex colours for us.
        float shade=QualitySettings.activeColorSpace==ColorSpace.Linear?Mathf.GammaToLinearSpace(brightness):brightness;
        v.color=new Color(shade,shade,shade,1);vh.AddVert(v);
    }
}
