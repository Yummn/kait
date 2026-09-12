using UnityEngine;
public static class YummnRepoolArt
{
    public static bool IsFrozenVisual(KaitEnemy enemy,bool yummn)=>yummn?enemy.yummnFrozen:enemy.frozenActions>0;
    private static readonly System.Collections.Generic.Dictionary<string,int> oldIcons=new System.Collections.Generic.Dictionary<string,int>{
        {"yummn.R02",7},{"yummn.O02",5},{"yummn.R06",1},{"yummn.R08",11},{"yummn.R09",12},
        {"yummn.E03",13},{"yummn.E04",14},{"yummn.E05",15},{"yummn.E06",16},
        {"yummn.S01",20},{"yummn.R17",17},{"yummn.R19",19},{"yummn.S02",18},{"yummn.S06",22},
        {"yummn.O05",9},{"yummn.M05",4},{"yummn.O06",10},{"yummn.R30",12},{"yummn.O03",7},{"yummn.O04",8},{"yummn.R40",22}};
    private static readonly System.Collections.Generic.Dictionary<string,Sprite> cache=new System.Collections.Generic.Dictionary<string,Sprite>();
    public static string NewIcon(KaitAbilityDef def)
    {
        if(def==null)return null;
        switch(def.id)
        {
            case "yummn.R05":return "Stun";

            case "yummn.R26":return "Heal";
            case "yummn.R39":return "Decoy";
            case "yummn.R22":return "Reflect";
            case "yummn.M04":return "Deflect";
            case "yummn.R01":return "TwinFists";
            case "yummn.R03":return "WindBoots";
            case "yummn.R13":return "FrostTouch";
            case "yummn.R15":return "IceFist";
            case "yummn.R23":return "BattleMeditation";
            case "yummn.R24":return "Stillness";
            case "yummn.R28":return "KiSea";
            case "yummn.R31":return "Passwall";
            case "yummn.R32":return "Bag";
            case "yummn.R33":return "Gravity";
            case "yummn.R34":return "Book";
            case "yummn.R37":return "SpiritGuard";
            case "yummn.R38":return "EmptyBody";
            default:return null;
        }
    }
    public static Sprite Icon(KaitAbilityDef def)
    {
        if(def==null)return null;
        if(cache.TryGetValue(def.id,out var cached))return cached;
        string name=NewIcon(def);
        if(name!=null)
        {
            var t=Resources.Load<Texture2D>("KaitVisuals/Yummn/Repool/Card_"+name);
            if(t!=null)return cache[def.id]=Sprite.Create(t,new Rect(0,0,t.width,t.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
        }
        return oldIcons.TryGetValue(def.id,out int index)?cache[def.id]=YummnV08Art.Icon(index):null;
    }
}
