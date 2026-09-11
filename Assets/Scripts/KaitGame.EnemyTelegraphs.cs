using UnityEngine;

public sealed partial class KaitGame
{
    private readonly KaitEnemyTelegraph[] selectedTelegraphs=new KaitEnemyTelegraph[49];
    private readonly KaitEnemyTelegraph[] selectedEnemyBadges=new KaitEnemyTelegraph[49];
    private KaitEnemyTelegraph NewTelegraph(Transform parent,string name)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(KaitEnemyTelegraph));go.transform.SetParent(parent,false);
        var g=go.GetComponent<KaitEnemyTelegraph>();g.rectTransform.sizeDelta=new Vector2(109,109);return g;
    }
    private void RefreshSelectedTelegraph(int index,Vector2Int p,bool impact)
    {
        if(selectedTelegraphs[index]==null)selectedTelegraphs[index]=NewTelegraph(battleWarningLines[index].transform,"Selected enemy telegraph");
        var plan=run.walls[p.x,p.y]?new KaitTelegraphPlan():KaitTelegraphPlan.At(p,animatedEnemies??run.enemies);
        var graphic=selectedTelegraphs[index];graphic.Configure(plan,impact);
        battleWarningLines[index].color=Color.clear;
        battleWarningLines[index].gameObject.SetActive(graphic.gameObject.activeSelf);
        if(selectedEnemyBadges[index]!=null)selectedEnemyBadges[index].gameObject.SetActive(false);
        // Source weapon emblems removed; retain only actual threatened cells.
    }
    private void StrikeSelectedTelegraphs(System.Collections.Generic.IEnumerable<Vector2Int> cells)
    {
        foreach(var p in cells)
        {
            if(!InsideBattle(p)||run.walls[p.x,p.y])continue;int index=p.x+p.y*7;
            if(selectedTelegraphs[index]==null)RefreshSelectedTelegraph(index,p,true);
            battleWarningLines[index].gameObject.SetActive(true);selectedTelegraphs[index].Strike();
        }
    }
}
