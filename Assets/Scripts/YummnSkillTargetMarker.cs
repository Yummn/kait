using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// A looping edge-only selector. It lives on the board effect overlay so actors
// never hide it, while its transparent centre keeps the selected unit readable.
public sealed class YummnSkillTargetMarker : Image
{
    private const string ResourcePath="KaitVisuals/Yummn/SkillTargetSelector";
    private static Sprite[] cachedFrames;
    private Sprite[] frames;
    private float age;

    protected override void Awake()
    {
        base.Awake();
        raycastTarget=false;
        maskable=false;
        preserveAspect=true;
        frames=LoadFrames();
        if(frames.Length>0)sprite=frames[0];
    }

    private static Sprite[] LoadFrames()
    {
        if(cachedFrames!=null)return cachedFrames;
        var sheet=Resources.Load<Texture2D>(ResourcePath);
        if(sheet==null)return cachedFrames=new Sprite[0];
        int width=sheet.width/4,height=sheet.height/2;
        var result=new List<Sprite>(8);
        for(int row=0;row<2;row++)for(int column=0;column<4;column++)
        {
            // Sprite sheet is read left-to-right, top row first.
            int y=(1-row)*height;
            result.Add(Sprite.Create(sheet,new Rect(column*width,y,width,height),Vector2.one*.5f,100));
        }
        return cachedFrames=result.ToArray();
    }

    private void Update()
    {
        if(frames==null||frames.Length==0)return;
        age+=Time.unscaledDeltaTime;
        int frame=Mathf.FloorToInt(age*12f)%frames.Length;
        if(sprite!=frames[frame])sprite=frames[frame];
        float pulse=1f+Mathf.Sin(age*7f)*.025f;
        rectTransform.localScale=Vector3.one*pulse;
    }
}
