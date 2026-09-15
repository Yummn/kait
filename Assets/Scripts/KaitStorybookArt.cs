using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class KaitStorybookArt
{
    static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
    public static Sprite Load(string name)
    {
        if(name=="Qi")return YummnKiWisp.Skin(0);
        if(cache.TryGetValue(name,out var sprite))return sprite;
        var t=(name.EndsWith("Backdrop")&&name!="LibraryBackdrop")?Resources.Load<Texture2D>("KaitVisuals/Storybook0915/"+name):null;
        if(t==null)t=Resources.Load<Texture2D>("KaitVisuals/Storybook0910/"+name);
        if(t==null)return null;
        return cache[name]=Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
    }
    public static Image Icon(Transform parent,string asset,Vector2 pos,Vector2 size)
        =>KaitStorybookDetails.Image(parent,asset,pos,size,Load(asset));
    // Fixed variations don't consume gameplay RNG or change when the UI refreshes.
    static readonly int[,] iceLayout={ {0,1,3,4,2},{3,4,0,1,4},{1,0,4,3,0},{4,3,1,0,3},{5,1,3,4,1} };
    public static Sprite Floor(bool snow,int x,int y)
    {
        int index=iceLayout[(y+4)%5,(x+4)%5];string key=(snow?"Ice0915_":"Grass0915_")+index;
        if(cache.TryGetValue(key,out var sprite))return sprite;
        var t=Resources.Load<Texture2D>("KaitVisuals/Storybook0915/"+(snow?"Ice":"Grass")+"FloorAtlas");
        if(t==null)return Load(snow?"IceA":"GrassA");
        float w=t.width/3f,h=t.height/2f;
        // Inset half a source pixel prevents adjacent atlas cells bleeding at seams.
        return cache[key]=Sprite.Create(t,new Rect(index%3*w+.5f,(1-index/3)*h+.5f,w-1,h-1),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
    }
    public static Sprite Wall(bool snow)=>Load(snow?"IceWall":"GrassWall");
    public static Sprite Detail(string name)
    {
        string key="0915_"+name;if(cache.TryGetValue(key,out var sprite))return sprite;
        var t=Resources.Load<Texture2D>("KaitVisuals/Storybook0915/"+name);
        return t==null?null:cache[key]=Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
    }
    public static Sprite GroundApron(bool snow)=>snow?SnowApron():Detail("GrassApron");
    public static Sprite SnowApron()
    {
        const string key="SnowApron0914";if(cache.TryGetValue(key,out var sprite))return sprite;
        var t=Resources.Load<Texture2D>("KaitVisuals/Storybook0914/SnowApron");
        return t==null?null:cache[key]=Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
    }
}
