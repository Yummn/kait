using UnityEngine;
using UnityEngine.UI;

// One resource value and transform; two clipped skins of the approved B wisp.
[RequireComponent(typeof(Image))]
public sealed class YummnKiWisp : MonoBehaviour
{
    private static Sprite[] sprites;
    private static Sprite circle;
    public static readonly Color Cyan = new Color(.25f,.76f,.82f,1f);

    private static Sprite Circle()
    {
        if(circle!=null)return circle;
        const int size=64;
        var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
        texture.wrapMode=TextureWrapMode.Clamp;
        var pixels=new Color[size*size];
        for(int y=0;y<size;y++)for(int x=0;x<size;x++)
        {
            float distance=Vector2.Distance(new Vector2(x+.5f,y+.5f),Vector2.one*(size*.5f));
            pixels[y*size+x]=new Color(1,1,1,Mathf.Clamp01(31.5f-distance));
        }
        texture.SetPixels(pixels);texture.Apply();
        circle=Sprite.Create(texture,new Rect(0,0,size,size),Vector2.one*.5f,100);
        return circle;
    }
    private Image paintedEmpty, flatEmpty, paintedFill, flatFill;
    public float Amount { get; private set; }
    public bool Exhausted { get; private set; }

    public static Sprite Skin(int index)
    {
        if(sprites==null)
        {
            var texture=Resources.Load<Texture2D>("KaitVisuals/Yummn/UI/KiWispB");
            if(texture==null)return null;
            sprites=new Sprite[4];
            // Matched padded bounds measured on the generated RGBA sheet (1287x1222).
            // Sprite rects only: the source pixels and alpha are not modified.
            for(int i=0;i<4;i++)
                sprites[i]=Sprite.Create(texture,new Rect(i%2==0?210:775,i<2?618:35,328,536),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
        }
        return sprites[index];
    }

    public void Configure(GlobalStyleSplit split)
    {
        paintedEmpty=GetComponent<Image>();
        Setup(paintedEmpty,2,split,true);
        flatEmpty=Child("Minimal Empty",3,split,false);
        paintedFill=Child("Painted Fill",0,split,true);
        flatFill=Child("Minimal Fill",1,split,false);
        foreach(var dot in new[]{flatEmpty,flatFill})
        {
            dot.sprite=Circle();
            dot.rectTransform.anchorMin=dot.rectTransform.anchorMax=Vector2.one*.5f;
            dot.rectTransform.sizeDelta=Vector2.one*19f;
            dot.rectTransform.anchoredPosition=Vector2.zero;
        }
        flatEmpty.color=new Color(Cyan.r,Cyan.g,Cyan.b,.18f);
        foreach(var fill in new[]{paintedFill,flatFill})
        {fill.type=Image.Type.Filled;fill.fillMethod=Image.FillMethod.Vertical;fill.fillOrigin=0;}
    }
    private Image Child(string name,int skin,GlobalStyleSplit split,bool left)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image));
        go.transform.SetParent(transform,false);
        var rt=(RectTransform)go.transform;rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;
        var img=go.GetComponent<Image>();Setup(img,skin,split,left);return img;
    }
    private static void Setup(Image img,int skin,GlobalStyleSplit split,bool left)
    {
        img.sprite=Skin(skin);img.type=Image.Type.Simple;img.color=Color.white;img.raycastTarget=false;
        var clip=img.gameObject.AddComponent<SunlitSplitText>();clip.PreserveColors();clip.SetSides(left,!left);clip.Configure(split);
        // Without a split context, only use the painted skin (e.g. isolated HUD tests).
        img.enabled=split!=null||left;
    }
    public void SetState(float amount,bool exhausted)
    {
        Amount=Mathf.Clamp01(amount);Exhausted=exhausted;
        var tint=exhausted?new Color(.66f,.74f,.83f,1):Color.white;
        paintedFill.color=tint;
        flatFill.color=exhausted?new Color(.49f,.60f,.69f,1):Cyan;
        paintedFill.fillAmount=flatFill.fillAmount=Amount;
    }
}
