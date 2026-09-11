using UnityEngine;
using UnityEngine.UI;

// Rule diagrams are live UI geometry, so labels stay sharp at every resolution.
public sealed class YummnTutorialDiagram : MonoBehaviour
{
    private Font font;
    private readonly Color ink=new Color(.96f,.95f,.9f), blue=new Color(.3f,.7f,.9f), orange=new Color(.95f,.57f,.42f);
    public void Show(int page,Font f,bool legacy=false,int maxKi=5,YummnRulesSnapshot rules=null)
    {
        font=f;
        for(int i=transform.childCount-1;i>=0;i--)
        {
            var child=transform.GetChild(i).gameObject;
            child.SetActive(false);
            if(Application.isPlaying)Destroy(child);else DestroyImmediate(child);
        }
        bool current=rules!=null&&rules.Is082;
        if(current&&page==1)
        {
            Caption("残影不挡路，只承接攻击",new Vector2(0,235));Grid(new Vector2(-210,20));Grid(new Vector2(210,20));
            Unit(new Vector2(-320,20),true);Unit(new Vector2(-100,20),false);
            Box(new Vector2(100,20),Vector2.one*69,new Color(blue.r,blue.g,blue.b,.3f));
            Unit(new Vector2(100,130),true);Unit(new Vector2(320,20),false);
            Label("<",new Vector2(210,20),42,orange);Label("+1 气",new Vector2(100,-90),30,blue);
            Caption("移动起点留一个残影",new Vector2(-210,-210));Caption("可多次受击，回合末消退",new Vector2(210,-210));
            return;
        }
        if(page==0)
        {
            Caption("高速：滑行出拳",new Vector2(-210,235));Caption("气竭：一步恢复",new Vector2(210,235));
            Grid(new Vector2(-210,30));Grid(new Vector2(210,30));
            Unit(new Vector2(-320,30),true);Unit(new Vector2(-210,30),false);
            Icon(0,new Vector2(-265,30),54);Label("−1 HP",new Vector2(-210,-40),30,orange);
            Unit(new Vector2(100,140),true);Unit(new Vector2(210,30),false);
            KaitUiGlyph.Create(transform,KaitUiGlyph.Symbol.Up,new Vector2(100,30),40).color=blue;
            Label(current?(rules.AttackAdvancesEnemyPhase?"攻击：敌人行动":"未击杀：敌人暂停"):legacy?"−1 气 · 敌人暂停":"未击杀：敌人暂停",new Vector2(-210,-210),26,ink);Label("敌人行动 → +1 气",new Vector2(210,-210),26,ink);
            int count=legacy?3:maxKi;
            for(int i=0;i<count;i++)Box(new Vector2((i-(count-1)*.5f)*34,-285),Vector2.one*22,blue);
        }
        else if(page==1)
        {
            Caption("预警锁定，不追着转向",new Vector2(0,235));Grid(new Vector2(-210,20));Grid(new Vector2(210,20));
            Box(new Vector2(-320,20),Vector2.one*92,new Color(.8f,.25f,.2f,.7f));
            Box(new Vector2(100,20),Vector2.one*92,new Color(.8f,.25f,.2f,.7f));
            Unit(new Vector2(-320,20),true);Unit(new Vector2(-210,20),false);
            Unit(new Vector2(100,130),true);Unit(new Vector2(210,20),false);
            Label("!",new Vector2(-320,100),45,orange);KaitUiGlyph.Create(transform,KaitUiGlyph.Symbol.Up,new Vector2(100,20),40).color=blue;
            Caption("留在红格：挨打",new Vector2(-210,-220));Caption("离开红格：避开",new Vector2(210,-220));
        }
        else if(page==2)
        {
            Caption("准备 → 校验方向与总气耗 → 执行",new Vector2(0,230));
            int[] icons={0,2,5,7};string[] labels={"三拳","震慑","推掌","追身"};
            for(int i=0;i<4;i++){float x=(i-1.5f)*195;Icon(icons[i],new Vector2(x,95),132);Caption(labels[i],new Vector2(x,-5));if(i<3)Label(">",new Vector2(x+98,90),28,blue);}
            Icon(14,new Vector2(-190,-160),105);Icon(17,new Vector2(190,-160),105);
            Caption("吐息",new Vector2(-190,-250));Caption("或",new Vector2(0,-150));Caption("暗影步",new Vector2(190,-250));
        }
        else
        {
            Label("16 + 16  →  32",new Vector2(0,225),38,ink);
            for(int i=0;i<3;i++)
            {
                Box(new Vector2((i-1)*150,65),new Vector2(116,165),i==0?new Color(.72f,.78f,.82f):i==1?blue:new Color(.88f,.68f,.3f));
                Icon(new[]{2,0,1}[i],new Vector2((i-1)*150,65),93);
            }
            Caption("三选一 · 拖入上 / 下卡槽",new Vector2(0,-75));
            Caption(current?"敌方阶段前出生":legacy?"击杀 / 气竭后检查":"敌方阶段后检查",new Vector2(-195,-180));Caption("占格：保留裂隙",new Vector2(195,-180));
            Caption("256 → 盾骑士 → 击败获胜",new Vector2(0,-235));
        }
    }
    private void Grid(Vector2 center)
    {for(int y=-1;y<=1;y++)for(int x=-1;x<=1;x++)Box(center+new Vector2(x*110,y*110),Vector2.one*104,new Color(.26f,.34f,.4f));}
    private void Unit(Vector2 at,bool player)
    {Box(at,Vector2.one*69,player?blue:orange);Label(player?"Y":"敌",at,34,new Color(.1f,.17f,.22f));}
    private void Caption(string text,Vector2 at)=>Label(text,at,24,ink);
    private void Label(string value,Vector2 at,int size,Color color)
    {
        var go=new GameObject("Diagram Label",typeof(RectTransform),typeof(Text));go.transform.SetParent(transform,false);
        var t=go.GetComponent<Text>();t.rectTransform.anchoredPosition=at;t.rectTransform.sizeDelta=new Vector2(780,Mathf.Max(55,size*1.5f+6));t.font=font;t.fontSize=size;t.color=color;t.text=value;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;
    }
    private Image Box(Vector2 at,Vector2 size,Color color)
    {
        var go=new GameObject("Diagram Shape",typeof(RectTransform),typeof(Image));go.transform.SetParent(transform,false);
        var i=go.GetComponent<Image>();i.rectTransform.anchoredPosition=at;i.rectTransform.sizeDelta=size;i.color=color;i.sprite=KaitCardSkin.RoundRect();i.type=Image.Type.Sliced;i.raycastTarget=false;return i;
    }
    private void Icon(int index,Vector2 at,float size)
    {var i=Box(at,Vector2.one*size,Color.white);i.type=Image.Type.Simple;i.sprite=KaitCardSkin.Icon(YummnCatalog.Cards[index]);i.material=YummnV08Art.Material;i.preserveAspect=true;}
}
