using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Native, resolution-independent layout with cached nine-slice artwork.
// Only the UI skin changes: card identities, art and rule definitions stay intact.
public static class KaitStorybookTheme
{
    public static readonly Color Ink=new Color32(49,45,68,255);
    public static readonly Color Paper=new Color32(255,246,227,255);
    public static readonly Color Mint=new Color32(190,229,218,255);
    public static readonly Color Peach=new Color32(249,204,177,255);
    public static readonly Color Muted=new Color32(108,96,111,255);
    static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
    public static Sprite Panel=>Surface("panel",Paper,Ink,6);
    public static Sprite Button=>FramedButton(false);
    public static Sprite Pressed=>FramedButton(true);
    public static Sprite Hud=>InlaidSurface("hud-0914",Paper,Ink,false,15);
    public static Sprite Tray=>InlaidSurface("tray-0914",new Color32(224,199,179,255),new Color32(112,91,105,255),false,18);
    public static Sprite Tile(Color fill,bool empty=false)=>InlaidSurface("tile-0914-"+ColorUtility.ToHtmlStringRGBA(fill)+empty,fill,Color.Lerp(fill,new Color32(106,82,96,255),empty?.32f:.22f),empty,12);

    // Fixed-width corners, directional highlights, shallow inset cell edges.
    static Sprite InlaidSurface(string key,Color fill,Color ink,bool inset,float radius)
    {
        if(cache.TryGetValue(key,out var sprite))return sprite;
        const int size=96;var pixels=new Color[size*size];
        for(int y=0;y<size;y++)for(int x=0;x<size;x++)
        {
            float d=Distance(x+.5f,y+.5f,new Rect(2,2,92,92),radius);bool top=y>48;
            Color c=fill;
            if(d>-1.4f)c=ink;
            else if(d>-2.4f)c=Color.Lerp(fill,top!=inset?Color.white:ink,top!=inset?.28f:.17f);
            c.a*=Mathf.Clamp01(.5f-d);pixels[x+y*size]=c;
        }
        return cache[key]=SpriteFrom(key,size,size,pixels,new Vector4(19,19,19,19));
    }
    public static Sprite KeyLayer(bool face)
    {
        string key=face?"key-face-0914":"key-seat-0914";
        if(cache.TryGetValue(key,out var sprite))return sprite;
        const int n=192;var pixels=new Color[n*n];
        for(int y=0;y<n;y++)for(int x=0;x<n;x++)
        {
            var r=face?new Rect(3,3,186,186):new Rect(3,5,186,182);
            float d=Distance(x+.5f,y+.5f,r,face?18:24);bool lit=y>n*.5f||x<n*.25f;
            Color c;
            if(face)
            {
                c=Color.Lerp(new Color32(248,235,211,255),Paper,Mathf.Clamp01(y/50f));
                if(d>-2.8f)c=Color.Lerp(Ink,Paper,.18f);
                else if(d>-6f)c=lit?new Color32(255,253,241,255):new Color32(218,192,174,255);
                if(x>20&&x<64&&y>180&&d<-6)c=new Color32(255,255,247,255);
            }
            else
            {
                c=Color.Lerp(new Color32(171,144,151,255),new Color32(214,224,231,255),Mathf.SmoothStep(0,1,y/192f));
                if(d>-4)c=Ink;
                else if(d>-6&&lit)c=new Color32(228,233,229,255);
            }
            c.a*=Mathf.Clamp01(.5f-d);
            if(!face&&c.a<1)c=Over(c,new Color(Ink.r,Ink.g,Ink.b,.15f*Mathf.Clamp01(.5f-Distance(x+.5f,y+.5f,new Rect(3,2,186,182),24))));
            pixels[x+y*n]=c;
        }
        var t=new Texture2D(n,n,TextureFormat.RGBA32,false){name=key,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};t.SetPixels(pixels);t.Apply(false,true);
        return cache[key]=Sprite.Create(t,new Rect(0,0,n,n),Vector2.one*.5f,200,0,SpriteMeshType.FullRect,new Vector4(30,30,30,30));
    }

    static Sprite FramedButton(bool pressed)
    {
        string key=pressed?"key-down":"key-up";
        if(cache.TryGetValue(key,out var found))return found;
        const int size=96;var pixels=new Color[size*size];
        for(int y=0;y<size;y++)for(int x=0;x<size;x++)
        {
            float d=Distance(x+.5f,y+.5f,new Rect(2,5,92,89),14);
            Color c=d>-3?Ink:d>-7?new Color32(188,198,211,255):d>-9?Ink:pressed?Mint:Paper;
            c.a=Mathf.Clamp01(.5f-d);
            if(c.a<1)c=Over(c,new Color(Ink.r,Ink.g,Ink.b,Mathf.Clamp01(.5f-Distance(x+.5f,y+.5f,new Rect(2,1,92,89),14))*.4f));
            pixels[x+y*size]=c;
        }
        return cache[key]=SpriteFrom(key,size,size,pixels,new Vector4(24,24,24,24));
    }
    public static Sprite ThreatStone()
    {
        const string key="threat-stone-0914";
        if(cache.TryGetValue(key,out var found))return found;
        const int size=128;var pixels=new Color[size*size];
        var seeds=new[]{new Vector2(20,98),new Vector2(74,112),new Vector2(112,84),new Vector2(50,69),new Vector2(95,38),new Vector2(24,22),new Vector2(63,-8)};
        var colors=new[]{new Color32(84,75,98,255),new Color32(76,67,91,255),new Color32(87,74,98,255),new Color32(67,61,81,255),new Color32(77,65,86,255),new Color32(72,63,84,255),new Color32(56,51,70,255)};
        for(int y=0;y<size;y++)for(int x=0;x<size;x++)
        {
            float d=Distance(x+.5f,y+.5f,new Rect(2,2,124,124),15);
            var p=new Vector2(x+.5f,y+.5f);
            float nearest=99999,second=99999;int cell=0;
            for(int i=0;i<seeds.Length;i++){float ds=(p-seeds[i]).sqrMagnitude;if(ds<nearest){second=nearest;nearest=ds;cell=i;}else second=Mathf.Min(second,ds);}
            Color c=colors[cell];float seam=(second-nearest)/100f;
            if(seam<1.35f)c=Color.Lerp(new Color32(46,43,62,255),c,Mathf.Clamp01(seam));
            else if(seam<2.0f)c=Color.Lerp(c,Paper,.07f);
            // Small chipped corners, no snow or grass from the battle-map wall.
            if((x<16&&y>105)||(x>109&&y<18))c=new Color32(105,90,114,255);
            if(d>-3)c=Ink;
            c.a=Mathf.Clamp01(.5f-d);pixels[x+y*size]=c;
        }
        return cache[key]=SpriteFrom(key,size,size,pixels,Vector4.zero);
    }

    public static Sprite Surface(string key,Color fill,Color border,float stroke=5,float radius=18)
    {
        if(cache.TryGetValue(key,out var sprite))return sprite;
        const int size=96;var pixels=new Color[size*size];
        for(int y=0;y<size;y++)for(int x=0;x<size;x++)
        {
            float d=Distance(x+.5f,y+.5f,new Rect(2,5,92,89),radius);
            Color c=Color.Lerp(border,fill,Mathf.Clamp01(-d-stroke));
            c.a*=Mathf.Clamp01(.5f-d);
            if(c.a<1&&stroke>0){float shadow=Mathf.Clamp01(.5f-Distance(x+.5f,y+.5f,new Rect(2,1,92,89),radius))*.32f; c=Over(c,new Color(Ink.r,Ink.g,Ink.b,shadow));}
            pixels[x+y*size]=c;
        }
        sprite=SpriteFrom(key,size,size,pixels,new Vector4(24,24,24,24));cache[key]=sprite;return sprite;
    }
    public static Sprite Card(KaitAbilityDef def,bool flat=false,bool square=false,int compactHeight=0)
    {
        if(def==null)return null;
        string key="card-"+def.kind+"-"+def.rarity+"-"+flat+"-"+square+"-"+compactHeight;
        if(cache.TryGetValue(key,out var result))return result;
        const int w=400;int h=compactHeight>0?compactHeight*2:square?400:512;var pixels=new Color[w*h];
        Color rarity=def.rarity==KaitRarity.Common?new Color32(177,192,208,255):
            def.rarity==KaitRarity.Uncommon?new Color32(84,160,215,255):new Color32(231,181,91,255);
        Color fill=flat?new Color32(54,47,64,255):Paper;
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)
        {
            float d=Distance(x+.5f,y+.5f,new Rect(4,8,w-8,h-18),28);
            Color c=flat?rarity:Ink;
            if(d<-6)c=flat?fill:rarity;
            if(d<-16)c=fill;
            // Broad cel-shaded corner bindings, not fine metallic filigree.
            float cornerX=Mathf.Min(x,w-1-x),cornerY=Mathf.Min(y,h-1-y);
            if(!flat&&d<-6&&cornerX<74&&cornerY<74&&cornerX+cornerY<99)
            {
                c=cornerX+cornerY<95?rarity:Color.Lerp(rarity,Ink,.65f);
            }
            c.a=Mathf.Clamp01(.5f-d);
            if(def.kind==KaitAbilityKind.Active&&compactHeight==0)
            {
                float circle=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(w*.5f,h-22));
                Color crest=circle<11?Color.Lerp(rarity,Paper,.40f):circle<17?rarity:Ink;crest.a=Mathf.Clamp01(23.5f-circle);
                c=Over(crest,c);
            }
            pixels[x+y*w]=c;
        }
        result=SpriteFrom(key,w,h,pixels,Vector4.zero);cache[key]=result;return result;
    }
    static Sprite SpriteFrom(string name,int w,int h,Color[] pixels,Vector4 border)
    {
        var texture=new Texture2D(w,h,TextureFormat.RGBA32,false){name="Storybook "+name,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
        texture.SetPixels(pixels);texture.Apply(false,true);
        return Sprite.Create(texture,new Rect(0,0,w,h),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,border);
    }
    static Color Over(Color front,Color back)
    {float a=front.a+back.a*(1-front.a);if(a<=0)return Color.clear;return new Color((front.r*front.a+back.r*back.a*(1-front.a))/a,(front.g*front.a+back.g*back.a*(1-front.a))/a,(front.b*front.a+back.b*back.a*(1-front.a))/a,a);}
    static float Distance(float x,float y,Rect r,float radius)
    {var q=new Vector2(Mathf.Abs(x-r.center.x)-r.width*.5f+radius,Mathf.Abs(y-r.center.y)-r.height*.5f+radius);return new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0)-radius;}
    public static void PaperPanel(Image image){image.sprite=Panel;image.type=Image.Type.Sliced;image.color=Color.white;}
    public static void Wash(Transform parent,string name,Color tint)
    {
        var image=new GameObject(name,typeof(RectTransform),typeof(Image)).GetComponent<Image>();image.transform.SetParent(parent,false);
        image.rectTransform.anchorMin=Vector2.zero;image.rectTransform.anchorMax=Vector2.one;image.rectTransform.sizeDelta=Vector2.zero;
        image.color=tint;image.raycastTarget=false;
    }
}
