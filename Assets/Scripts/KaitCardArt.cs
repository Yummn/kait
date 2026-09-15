using System.Collections.Generic;
using UnityEngine;

// Fit visible artwork rather than the transparent canvas around each illustration.
public static class KaitCardArt
{
    static readonly Dictionary<Sprite,Sprite> fitted=new Dictionary<Sprite,Sprite>();
    public static Sprite Fit(Sprite source)
    {
        if(source==null)return null;
        if(fitted.TryGetValue(source,out var cached))return cached;
        var area=source.rect;int w=Mathf.RoundToInt(area.width),h=Mathf.RoundToInt(area.height);
        var rt=RenderTexture.GetTemporary(w,h,0,RenderTextureFormat.ARGB32);
        var previous=RenderTexture.active;
        Texture2D copy=null;
        try
        {
            var texture=source.texture;
            Graphics.Blit(texture,rt,new Vector2(area.width/texture.width,area.height/texture.height),new Vector2(area.x/texture.width,area.y/texture.height));
            RenderTexture.active=rt;
            copy=new Texture2D(w,h,TextureFormat.RGBA32,false);
            copy.ReadPixels(new Rect(0,0,w,h),0,0);copy.Apply();
            var pixels=copy.GetPixels32();int left=w,right=-1,bottom=h,top=-1;
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)if(pixels[y*w+x].a>40)
            {left=Mathf.Min(left,x);right=Mathf.Max(right,x);bottom=Mathf.Min(bottom,y);top=Mathf.Max(top,y);}
            if(right<left)return fitted[source]=source;
            int pad=Mathf.CeilToInt(Mathf.Max(right-left,top-bottom)*.045f);
            left=Mathf.Max(0,left-pad);right=Mathf.Min(w-1,right+pad);bottom=Mathf.Max(0,bottom-pad);top=Mathf.Min(h-1,top+pad);
            return fitted[source]=Sprite.Create(texture,new Rect(area.x+left,area.y+bottom,right-left+1,top-bottom+1),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
        }
        finally {RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);if(copy!=null){if(Application.isPlaying)Object.Destroy(copy);else Object.DestroyImmediate(copy);}}
    }
}
