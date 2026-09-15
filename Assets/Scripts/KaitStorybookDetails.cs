using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Decorative UI only. Never changes actor masks, board coordinates or input.
public static class KaitStorybookDetails
{
    static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
    public static Image Image(Transform parent,string name,Vector2 pos,Vector2 size,Sprite sprite)
    {
        var image=new GameObject(name,typeof(RectTransform),typeof(Image)).GetComponent<Image>();
        image.transform.SetParent(parent,false);image.rectTransform.anchoredPosition=pos;image.rectTransform.sizeDelta=size;
        image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;return image;
    }
    public static Sprite Decoration(int cell)
    {
        string key="decoration"+cell;if(sprites.TryGetValue(key,out var sprite))return sprite;
        var texture=Resources.Load<Texture2D>("KaitVisuals/Storybook099/Decorations");if(texture==null)return null;
        float w=texture.width/2f,h=texture.height/2f;
        return sprites[key]=Sprite.Create(texture,new Rect(cell%2*w,(1-cell/2)*h,w,h),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
    }
    static Sprite Portrait(KaitCharacter character)
    {
        return KaitStorybookArt.Load(character+"Portrait");
    }
    public static Image PortraitBadge(Transform parent,KaitCharacter character,Vector2 pos,float size)
    {
        return Image(parent,"Character Portrait",pos,Vector2.one*size,Portrait(character));
    }
    public static void SetPortrait(Image image,KaitCharacter character){if(image!=null)image.sprite=Portrait(character);}
    public static GameObject SnowFrame(Transform parent)
    {
        var root=new GameObject("Storybook Snow Edge Details",typeof(RectTransform));root.transform.SetParent(parent,false);
        Image(root.transform,"Upper Fir",new Vector2(-715,423),new Vector2(185,195),Decoration(0));
        Image(root.transform,"Lower Fir",new Vector2(-710,-387),new Vector2(182,168),Decoration(0));
        Image(root.transform,"Snow Lantern",new Vector2(-194,-375),new Vector2(108,136),Decoration(1));
        return root;
    }
    public static void LibraryFrame(Transform parent)
    {
        Image(parent,"Storybook Stack",new Vector2(-790,-239),Vector2.one*275,Decoration(2));
        Image(parent,"Library Pennant",new Vector2(794,-445),Vector2.one*165,Decoration(3));
        for(int i=0;i<3;i++)
        {
            var line=Image(parent,"Page Edge "+i,new Vector2(-948+i*5,0),new Vector2(2,1000),null);
            line.color=new Color32(218,202,189,255);
        }
    }
}
