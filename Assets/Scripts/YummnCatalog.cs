using System.Collections.Generic;
public static class YummnCatalog
{
    public const string Version="0.8";
    public static readonly List<KaitAbilityDef> Cards=new List<KaitAbilityDef>{
        A("M01",KaitSkill.Flurry,"疾风连击","通用",1,1,"本次拳击对同一目标连续三拳。","三拳"),
        A("M02",KaitSkill.PatientDefense,"闪身防御","通用",0,1,"抵挡下一敌人阶段的第一次攻击伤害。","防"),
        A("M03",KaitSkill.StunningFist,"震慑拳","通用",1,1,"本次拳击使存活目标跳过下一次行动。","震"),
        P("M04",KaitPassive.DeflectMissiles,"拨挡飞弹","通用",0,"每个敌人阶段，挡住第一枚命中你的普通箭矢。","拨"),
        P("M05",KaitPassive.PerfectSelf,"完美自我","通用",0,"每次进入气竭时恢复1气，不跳过本次气竭。","气+1"),
        A("O01",KaitSkill.Palm,"推掌","散打",0,1,"拳击后，将存活目标击退1格。","推1"),
        P("O02",KaitPassive.OpenHand,"散打技巧","散打",1,"疾风连击后，将存活目标击退1格。","连推","M01"),
        P("O03",KaitPassive.FollowThrough,"追身步","散打",1,"近身成功推开敌人后，进入其原格。","跟进","push"),
        P("O04",KaitPassive.Tranquility,"宁静","散打",0,"气竭时未攻击的移动，抵挡本阶段第一次近战伤害。","静"),
        P("O05",KaitPassive.Wholeness,"混元体","散打",2,"从气竭恢复高速时，恢复1生命。","HP+1"),
        P("O06",KaitPassive.QuiveringPalm,"震颤掌","散打",2,"拳击留印，数量不限；掌印下方指向命中方向。下次异向命中额外2伤。","印+2"),
        A("E01",KaitSkill.WaterWhip,"水鞭","四象",0,0,"原地拉近前方最近敌人至多2格，造成1伤。","拉2","Water"),
        A("E02",KaitSkill.UnbrokenAir,"不坏气拳","四象",1,1,"拳击后，将存活目标沿攻击方向推到底。","推>"),
        A("E03",KaitSkill.ShapeIce,"塑流成冰","四象",1,1,"相邻空格升起冰柱，同时最多一根。","冰柱","Ice"),
        A("E04",KaitSkill.FrostBreath,"冬之吐息","四象",1,1,"原地攻击前方两格，各1伤并冻结存活目标。","霜2","Winter"),
        P("E05",KaitPassive.FireSnake,"火蛇之牙","四象",1,"拳击时，主目标身后一格的敌人额外受1伤。","后1"),
        P("E06",KaitPassive.ShatteringPalm,"碎冰掌","四象",2,"拳击已冻结敌人，解除冻结并使其四邻敌人各受1伤。","碎冰","E04"),
        A("S01",KaitSkill.YummnShadowStep,"暗影步","暗影",1,1,"传送到所选方向最近的空阴影格，不攻击。","影步","Shadow"),
        A("S02",KaitSkill.Darkness,"黑暗术","暗影",1,1,"相邻空格布置暗幕，阻箭至下一敌人阶段结束。","暗幕","Darkness"),
        P("S03",KaitPassive.ShadowCloak,"暗影斗篷","暗影",0,"未攻击并停在阴影中，使下一次针对你的瞄准失效。","隐"),
        P("S04",KaitPassive.PassWithoutTrace,"无踪步","暗影",2,"未攻击的移动不补新2，副盘仍正常移动合并。","无2"),
        P("S05",KaitPassive.ShadowAssault,"影袭","暗影",2,"从阴影格发动的拳击，无视目标正面格挡。","破盾"),
        P("S06",KaitPassive.Opportunist,"投机者","暗影",1,"敌人主动走到你身边时，原地反击1拳。","反1")};
    public static KaitAbilityDef Get(string id)=>Cards.Find(d=>d.id==id||d.id=="yummn."+id);
    public static bool IsActive(KaitSkill s)=>Cards.Exists(d=>d.kind==KaitAbilityKind.Active&&d.skill==s);
    public static bool IsMonk(KaitAbilityDef d)=>d!=null&&d.id.StartsWith("yummn.");
    public static List<KaitAbilityDef> Pool()
    {
        var pool=new List<KaitAbilityDef>(Cards);
        foreach(var p in new[]{KaitPassive.Passwall,KaitPassive.BagHolding,KaitPassive.ReverseGravity,KaitPassive.OldNewsArchive,KaitPassive.MomentumResonance,KaitPassive.LuckBlade,KaitPassive.BirdEye})pool.Add(KaitAbilityCatalog.Get(p));
        return pool;
    }
    private static KaitAbilityDef A(string id,KaitSkill s,string name,string tag,int rarity,int cost,string text,string sigil,string mode=null)=>new KaitAbilityDef{id="yummn."+id,kind=KaitAbilityKind.Active,skill=s,nameZh=name,nameEn=s.ToString(),rarity=(KaitRarity)rarity,kiExtraCost=cost,cardText=text,tags=new[]{"Yummn",tag},traditionTag=tag,allowedCharacters=new[]{"Yummn"},actionOverride=mode,sigil=sigil,copyable=false,origin=KaitAbilityOrigin.Dnd5e};
    private static KaitAbilityDef P(string id,KaitPassive p,string name,string tag,int rarity,string text,string sigil,params string[] prereqs)=>new KaitAbilityDef{id="yummn."+id,kind=KaitAbilityKind.Passive,passive=p,nameZh=name,nameEn=p.ToString(),rarity=(KaitRarity)rarity,cardText=text,tags=new[]{"Yummn",tag},traditionTag=tag,allowedCharacters=new[]{"Yummn"},prerequisiteIds=prereqs,sigil=sigil,copyable=false,origin=KaitAbilityOrigin.ProjectOriginal};
}
