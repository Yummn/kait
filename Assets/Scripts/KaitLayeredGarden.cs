using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// Decorations are real UI scene objects. Only the ground shadow is flattened
// into a low-cost, shared alpha field, regenerated from their live silhouettes.
[DefaultExecutionOrder(80)]
public sealed class KaitLayeredGarden : MonoBehaviour
{
    private sealed class Caster
    {
        public Image image;
        public float height, flatten, strength, sway, phase;
        public MaterialPropertyBlock properties;
    }
    private readonly List<Caster> casters = new List<Caster>();
    private readonly Vector3[] corners = new Vector3[4];
    private RectTransform mapping, actorLayer;
    private RenderTexture field;
    private Material projection;
    private Mesh quad;
    private CommandBuffer commands;
    private float nextRender;
    public RenderTexture ShadowField => field;
    public int DecorationCount => casters.Count;
    public static bool ArtReady => Resources.Load<Texture2D>(KaitSunlitTheme.ResourceRoot + "GrassBase") != null
        && Resources.Load<Texture2D>(KaitSunlitTheme.ResourceRoot + "TreeTrunk") != null
        && Resources.Load<Texture2D>(KaitSunlitTheme.ResourceRoot + "TreeCanopy") != null
        && Resources.Load<Texture2D>(KaitSunlitTheme.ResourceRoot + "FlowerClump") != null;
    public static readonly Vector2 LightOffset = new Vector2(.72f,-1);

    public void Initialize(RectTransform ground)
    {
        mapping = ground;
        field = new RenderTexture(1024,1024,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear)
            { name="Live Garden Shadow Field", filterMode=FilterMode.Bilinear, wrapMode=TextureWrapMode.Clamp };
        field.Create();
        projection = new Material(Resources.Load<Shader>("Shaders/UIDecorProjection"))
            { name="Decor Silhouette Projection", hideFlags=HideFlags.HideAndDontSave };
        quad = new Mesh { name="Garden Shadow Quad" };
        quad.vertices = new[] { new Vector3(-.5f,-.5f),new Vector3(.5f,-.5f),new Vector3(.5f,.5f),new Vector3(-.5f,.5f) };
        quad.uv = new[] { Vector2.zero,Vector2.right,Vector2.one,Vector2.up };
        quad.triangles = new[] { 0,1,2,0,2,3 };
        commands = new CommandBuffer { name="Kait Live Garden Shadows" };
        commands.SetRenderTarget(field);
        commands.ClearRenderTarget(false, true, Color.clear);
        Graphics.ExecuteCommandBuffer(commands);
    }

    public void BuildDecorations(RectTransform parent, RectTransform protectedActors)
    {
        actorLayer = protectedActors;
        // Root remains entirely outside the 5x5 board. Only the crown's fringe
        // reaches the upper edge; it is drawn above paving, never underneath it.
        Add(parent,"Tree Trunk", "TreeTrunk",new Vector2(-930,399),new Vector2(116,174),35,.55f,.9f,0);
        Add(parent,"Tree Crown", "TreeCanopy",new Vector2(-874,532),new Vector2(640,426.6667f),135,.88f,.92f,.65f);
        Add(parent,"Left Daisies 1", "FlowerClump",new Vector2(-938,121),new Vector2(50,54),12,.8f,.8f,.7f);
        Add(parent,"Left Daisies 2", "FlowerClump",new Vector2(-940,-182),new Vector2(54,60),12,.8f,.8f,.9f);
        Add(parent,"Lower Daisies 1", "FlowerClump",new Vector2(-900,-442),new Vector2(110,98),16,.8f,.8f,.8f);
        Add(parent,"Lower Daisies 2", "FlowerClump",new Vector2(-718,-486),new Vector2(90,84),14,.8f,.8f,1.1f);
        Add(parent,"Lower Daisies 3", "FlowerClump",new Vector2(-494,-468),new Vector2(60,60),10,.8f,.8f,.8f);
    }

    private void Add(RectTransform parent,string name,string art,Vector2 position,Vector2 size,float height,float flatten,float strength,float sway)
    {
        var sprite = KaitSunlitTheme.Load(art);
        if (sprite == null) { Debug.LogWarning("Missing garden decoration: " + art); return; }
        var go = new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
        go.transform.SetParent(parent,false);
        var image=go.GetComponent<Image>(); image.sprite=sprite; image.color=Color.white;
        image.raycastTarget=false; image.maskable=false;
        image.rectTransform.anchoredPosition=position; image.rectTransform.sizeDelta=size;
        var properties=new MaterialPropertyBlock();
        properties.SetTexture("_MainTex",image.sprite.texture); properties.SetFloat("_Strength",strength);
        properties.SetFloat("_Softness",art=="TreeCanopy"?.007f:.012f);
        casters.Add(new Caster { image=image,height=height,flatten=flatten,strength=strength,sway=sway,phase=casters.Count*1.47f,properties=properties });
    }

    private void LateUpdate()
    {
        foreach(var caster in casters)
        {
            if(caster.image == null) continue;
            var rect=caster.image.rectTransform;
            rect.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.unscaledTime*.65f+caster.phase)*caster.sway);
            float alpha=1;
            if(caster.height>100 && actorLayer!=null)
            {
                foreach(Transform actor in actorLayer)
                {
                    if(!actor.gameObject.activeInHierarchy) continue;
                    Vector2 local=rect.InverseTransformPoint(actor.position);
                    if(rect.rect.Contains(local)) { alpha=.28f; break; }
                }
            }
            Color tint=caster.image.color; tint.a=Mathf.Lerp(tint.a,alpha,1-Mathf.Exp(-12*Time.unscaledDeltaTime)); caster.image.color=tint;
        }
        if(Time.unscaledTime < nextRender) return;
        nextRender=Time.unscaledTime+1f/30;
        RenderShadows();
    }

    public static Matrix4x4 ProjectionMatrix(Vector2 centre,Vector2 size,float angle,float height,float flatten)
    {
        return Matrix4x4.TRS(centre+LightOffset*height,Quaternion.Euler(0,0,angle),new Vector3(size.x,size.y*flatten,1));
    }

    public void RenderShadows()
    {
        if(mapping==null || mapping.rect.width<=0 || mapping.rect.height<=0 || commands==null) return;
        Rect r=mapping.rect;
        commands.Clear(); commands.SetRenderTarget(field); commands.ClearRenderTarget(false,true,Color.clear);
        // RawImage samples bottom-left UVs, unlike a camera's screen image.
        // Applying the camera-to-RT Y flip here puts top-left tree shadows at
        // the bottom of the board on D3D. Keep this UV-space field upright.
        commands.SetViewProjectionMatrices(Matrix4x4.identity,GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(r.xMin,r.xMax,r.yMin,r.yMax,-1,1),false));
        foreach(var caster in casters)
        {
            if(caster.image==null || !caster.image.gameObject.activeInHierarchy) continue;
            var rect=caster.image.rectTransform; rect.GetWorldCorners(corners);
            Vector2 bl=mapping.InverseTransformPoint(corners[0]), tl=mapping.InverseTransformPoint(corners[1]), br=mapping.InverseTransformPoint(corners[3]);
            Vector2 centre=mapping.InverseTransformPoint(rect.TransformPoint(rect.rect.center));
            var size=new Vector2(Vector2.Distance(bl,br),Vector2.Distance(bl,tl));
            float angle=Mathf.Atan2(br.y-bl.y,br.x-bl.x)*Mathf.Rad2Deg;
            // Scale the light displacement with the same Canvas coordinate space.
            float unitScale=size.x/Mathf.Max(1,rect.rect.width);
            commands.DrawMesh(quad,ProjectionMatrix(centre,size,angle,caster.height*unitScale,caster.flatten),projection,0,0,caster.properties);
        }
        Graphics.ExecuteCommandBuffer(commands);
    }

    private void OnDestroy()
    {
        commands?.Release();
        foreach (var caster in casters)
        {
            if (caster.image == null || caster.image.sprite == null) continue;
            if (Application.isPlaying) Destroy(caster.image.sprite); else DestroyImmediate(caster.image.sprite);
        }
        if(field!=null) field.Release();
        if(Application.isPlaying) { Destroy(field); Destroy(projection); Destroy(quad); }
        else { DestroyImmediate(field); DestroyImmediate(projection); DestroyImmediate(quad); }
    }
}
