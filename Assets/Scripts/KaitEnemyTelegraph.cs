using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// One graphic per legal cell: overlapping attackers never add another fill.
public struct KaitTelegraphPlan
{
    public int count, arrows, boss, towardCenter, entries;
    public bool mage, center;
    public static bool Ready(KaitEnemy e)=>e!=null&&e.hp>0&&e.life==KaitEnemyLife.Active&&e.frozenActions==0&&!e.yummnFrozen&&!e.yummnStunned&&e.intent!=null&&e.intent.type!=KaitIntentType.None&&e.intent.type!=KaitIntentType.Move;
    public static int Direction(Vector2Int d)=>d.x>0?1:d.y>0?2:d.x<0?4:d.y<0?8:0;
    public static KaitTelegraphPlan At(Vector2Int p,IEnumerable<KaitEnemy> enemies)
    {
        var plan=new KaitTelegraphPlan();
        foreach(var e in enemies)
        {
            if(!Ready(e))continue;
            bool covered=e.intent.affectedCells.Contains(p)||(e.intent.type==KaitIntentType.Melee&&e.intent.affectedCells.Count==0&&e.intent.target==p);
            if(!covered)continue;plan.count++;
            var direction=e.intent.direction==Vector2Int.zero?p-e.pos:e.intent.direction;
            plan.entries|=Direction(direction);
            if(e.intent.type==KaitIntentType.CrossBlast){plan.mage=true;plan.center|=e.intent.target==p;plan.towardCenter|=Direction(e.intent.target-p);}
            else if(e.type==KaitEnemyType.ShieldKnight)plan.boss|=Direction(direction);
            else if(e.intent.type==KaitIntentType.LineShot)plan.arrows|=Direction(direction);
        }
        return plan;
    }
}

public sealed class KaitEnemyTelegraph : MaskableGraphic
{
    public KaitTelegraphPlan Plan {get;private set;}
    public bool Badge {get;private set;}
    public KaitEnemyType Kind {get;private set;}
    private KaitTelegraphPlan drawn;
    private bool forcedImpact;
    private float strikeUntil;
    private static readonly Vector2[] Dirs={Vector2.right,Vector2.up,Vector2.left,Vector2.down};
    protected override void Awake(){base.Awake();raycastTarget=false;maskable=false;}
    public void Configure(KaitTelegraphPlan plan,bool impact)
    {
        Badge=false;Plan=plan;forcedImpact=impact;
        if(plan.count>0)drawn=plan;
        else if(!impact&&Time.unscaledTime>=strikeUntil)drawn=plan;
        gameObject.SetActive(plan.count>0||impact||Time.unscaledTime<strikeUntil);SetVerticesDirty();
    }
    public void ConfigureBadge(KaitEnemyType kind){Badge=true;Kind=kind;gameObject.SetActive(true);SetVerticesDirty();}
    public void Strike(){strikeUntil=Time.unscaledTime+.24f;gameObject.SetActive(true);SetVerticesDirty();}
    private void Update()
    {
        if(!Badge&&Plan.count==0&&!forcedImpact&&Time.unscaledTime>=strikeUntil){gameObject.SetActive(false);return;}
        SetVerticesDirty();
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();bool hit=forcedImpact||Time.unscaledTime<strikeUntil;
        float pulse=.72f+.18f*(Mathf.Sin(Time.unscaledTime*5f)+1)*.5f;
        Color c=hit?new Color(1,.84f,.36f,.96f):new Color(.70f,.28f,.19f,pulse);
        if(Badge){DrawBadge(vh);return;}
        // Fixed alpha regardless of overlap count; the geometry stays inside 109px.
        Border(vh,new Color(c.r,c.g,c.b,.14f),7);
        Border(vh,c,2.3f);
        if(drawn.mage)
        {
            if(drawn.center){float r=17+2*Mathf.Sin(Time.unscaledTime*5);Line(vh,new Vector2(0,r),new Vector2(r,0),2,c);Line(vh,new Vector2(r,0),new Vector2(0,-r),2,c);Line(vh,new Vector2(0,-r),new Vector2(-r,0),2,c);Line(vh,new Vector2(-r,0),new Vector2(0,r),2,c);foreach(var d in Dirs)Line(vh,d*27,d*38,2,c);}
            else ForDirs(drawn.towardCenter,d=>Chevron(vh,d*10,d,c,9));
        }
        else if(drawn.boss!=0)
            ForDirs(drawn.boss,d=>{var n=new Vector2(-d.y,d.x);foreach(float side in new[]{-29f,29f})Line(vh,-d*44+n*side,d*44+n*side,3,c);Chevron(vh,d*7,d,c,13);});
        else if(drawn.arrows!=0)
            ForDirs(drawn.arrows,d=>{var n=new Vector2(-d.y,d.x);foreach(float side in new[]{-19f,19f})Line(vh,-d*44+n*side,d*44+n*side,1.4f,c);float t=Mathf.Repeat(Time.unscaledTime*20,20);foreach(float off in new[]{-20f,15f})Chevron(vh,d*(off+t-10),d,c,9);});
        else ForDirs(drawn.entries,d=>Chevron(vh,-d*36,d,c,5));
    }
    private void DrawBadge(VertexHelper vh)
    {
        Color ink=new Color(.72f,.32f,.19f,.88f),back=new Color(.18f,.22f,.28f,.91f);
        // Full-cell rounded outline, below the character; no opaque backing.
        Border(vh,ink,1.4f);
        int weaponStart=vh.currentVertCount;
        float bob=0; // Ground marking: no floating badge or vertical bob.
        if(Kind==KaitEnemyType.Archer)
        {
            Line(vh,new Vector2(-7,-14),new Vector2(5,-7),2,ink);
            Line(vh,new Vector2(5,-7),new Vector2(8,0),2,ink);
            Line(vh,new Vector2(8,0),new Vector2(5,7),2,ink);
            Line(vh,new Vector2(5,7),new Vector2(-7,14),2,ink);
            Line(vh,new Vector2(-7,-14),new Vector2(-7,14),1,ink);
            Line(vh,new Vector2(-11,0),new Vector2(12,0),2,ink);
            Chevron(vh,new Vector2(12,0),Vector2.right,ink,4);
        }
        else if(Kind==KaitEnemyType.Warlock)
        {
            Line(vh,new Vector2(0,-16),new Vector2(0,4),3,ink);
            Polygon(vh,new[]{new Vector2(0,17),new Vector2(7,9),new Vector2(0,2),new Vector2(-7,9)},ink);
        }
        else if(Kind==KaitEnemyType.ShieldKnight)
        {
            Polygon(vh,new[]{new Vector2(-10,12),new Vector2(0,16),new Vector2(10,12),new Vector2(8,-5),new Vector2(0,-15),new Vector2(-8,-5)},ink);
            Line(vh,new Vector2(0,-8),new Vector2(0,10),2,back);
        }
        else if(Kind==KaitEnemyType.Guard)
        {
            Polygon(vh,new[]{new Vector2(-10,9),new Vector2(-5,13),new Vector2(-3,7),new Vector2(3,7),new Vector2(5,13),new Vector2(10,9),new Vector2(7,2),new Vector2(6,-11),new Vector2(-6,-11),new Vector2(-7,2)},ink);
            Line(vh,new Vector2(-4,0),new Vector2(4,0),1.4f,back);Line(vh,new Vector2(-4,-5),new Vector2(4,-5),1.4f,back);
        }
        else
        {
            bool sword=Kind==KaitEnemyType.Swordsman;
            Polygon(vh,new[]{new Vector2(-2,0+bob),new Vector2(-3,sword?10:6),new Vector2(0,sword?16:12),new Vector2(3,sword?10:6),new Vector2(2,0+bob)},ink);
            Line(vh,new Vector2(-6,-1+bob),new Vector2(6,-1+bob),2,ink);Line(vh,new Vector2(0,-1+bob),new Vector2(0,sword?-13:-9),2.5f,ink);
        }
        // Fit the weapon itself, not its old small badge rectangle.
        UIVertex vertex=default;
        Vector2 lo=new Vector2(float.MaxValue,float.MaxValue),hi=new Vector2(float.MinValue,float.MinValue);
        for(int i=weaponStart;i<vh.currentVertCount;i++)
        {vh.PopulateUIVertex(ref vertex,i);lo=Vector2.Min(lo,vertex.position);hi=Vector2.Max(hi,vertex.position);}
        Vector2 size=hi-lo,centre=(lo+hi)*.5f;
        float scale=Mathf.Min(rectTransform.rect.width*.86f/Mathf.Max(1,size.x),rectTransform.rect.height*.86f/Mathf.Max(1,size.y));
        for(int i=weaponStart;i<vh.currentVertCount;i++)
        {vh.PopulateUIVertex(ref vertex,i);vertex.position=((Vector2)vertex.position-centre)*scale;vh.SetUIVertex(vertex,i);}
    }
    private static void ForDirs(int bits,System.Action<Vector2> draw){for(int i=0;i<4;i++)if((bits&(1<<i))!=0)draw(Dirs[i]);}
    private static void Chevron(VertexHelper vh,Vector2 p,Vector2 d,Color c,float size){var n=new Vector2(-d.y,d.x);Line(vh,p-d*size+n*size,p,2,c);Line(vh,p,p-d*size-n*size,2,c);}
    private static void Border(VertexHelper vh,Color c,float width)
    {
        const float h=50,r=6;
        Line(vh,new Vector2(-h+r,h),new Vector2(h-r,h),width,c);Line(vh,new Vector2(-h+r,-h),new Vector2(h-r,-h),width,c);
        Line(vh,new Vector2(h,-h+r),new Vector2(h,h-r),width,c);Line(vh,new Vector2(-h,-h+r),new Vector2(-h,h-r),width,c);
        for(int corner=0;corner<4;corner++){float angle=corner*Mathf.PI/2;var center=new Vector2(corner==0||corner==3?h-r:-h+r,corner<2?h-r:-h+r);for(int k=0;k<6;k++){float a=angle+k*Mathf.PI/12,b=a+Mathf.PI/12;Line(vh,center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,center+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*r,width,c);}}
    }
    private static void Polygon(VertexHelper vh,Vector2[] points,Color c){int start=vh.currentVertCount;Vector2 center=Vector2.zero;foreach(var p in points)center+=p;center/=points.Length;vh.AddVert(center,c,Vector2.zero);foreach(var p in points)vh.AddVert(p,c,Vector2.zero);for(int i=0;i<points.Length;i++)vh.AddTriangle(start,start+1+i,start+1+(i+1)%points.Length);}
    private static void Line(VertexHelper vh,Vector2 a,Vector2 b,float w,Color c){var n=new Vector2(-(b-a).y,(b-a).x).normalized*w*.5f;Quad(vh,a-n,b-n,b+n,a+n,c);}
    private static void Quad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color tint){int n=vh.currentVertCount;vh.AddVert(a,tint,Vector2.zero);vh.AddVert(b,tint,Vector2.zero);vh.AddVert(c,tint,Vector2.zero);vh.AddVert(d,tint,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
}
