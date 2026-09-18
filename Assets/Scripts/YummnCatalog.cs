using System.Collections.Generic;
public static class YummnCatalog
{
    public const string Version="0.9.17-card-balance";
    public const string PreviousVersion="0.9.7-palm-kick";
    public static readonly List<KaitAbilityDef> LegacyCards=new List<KaitAbilityDef>{
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
    public static readonly List<KaitAbilityDef> Cards=new List<KaitAbilityDef>{
        P("R01",KaitPassive.TwinPunch,"疾风连击","",1,"每次拳击消耗2气，连续出拳两次。","双"),
        A("R02",KaitSkill.PreciseStep,"疾步如风·收势","",0,1,"消耗1气，向指定方向移动1格。","步","Precise"),
        P("R03",KaitPassive.FrugalStride,"风行之靴","",2,"高速移动固定消耗1气；自主移动补两个2。","渡"),
        P("O02",KaitPassive.OpenHand,"散打技巧·推掌","",0,"拳击命中后，将存活敌人推开1格。","推"),
        P("R05",KaitPassive.StunStrike,"震慑拳","",1,"拳击命中后，消耗1气震慑存活敌人1回合。","震"),
        P("R06",KaitPassive.WaitingGuard,"无甲防御","",0,"等待时，抵挡本次敌方回合的首次伤害。","候"),
        P("M04",KaitPassive.DeflectMissiles,"拨挡飞弹","",0,"受到箭矢攻击时，消耗1气抵挡。","拨"),
        P("R08",KaitPassive.DistantPull,"水鞭","",1,"攻击方向上较远的敌人，造成1点伤害并拉至身前。","牵"),
        P("R09",KaitPassive.EndlessPush,"不坏气拳","",1,"拳击命中后，将存活敌人推至尽头。","远推"),
        A("E03",KaitSkill.ShapeIce,"塑造流水","",0,2,"在空格生成一根永久冰柱。","柱","Ice"),
        A("E04",KaitSkill.FrostBreath,"冬之吐息","",0,2,"对前方4格造成1点伤害，冻结存活敌人。","霜4","Winter"),
        P("E05",KaitPassive.FireSnake,"火蛇之牙","",1,"拳击时，对目标身后一格造成1点伤害。","贯"),
        P("R13",KaitPassive.FreezePush,"寒霜之触","",1,"强制移动敌人后，将其冻结。","封"),
        P("E06",KaitPassive.ShatteringPalm,"碎冰掌","",2,"破冰时，对目标四邻造成1点伤害。","碎"),
        P("R15",KaitPassive.FreezePunch,"冰封拳","",2,"拳击命中后，冻结存活敌人。","凝"),
        A("S01",KaitSkill.YummnShadowStep,"暗影步","",0,1,"传送至选定的任一空暗影格。","影","Shadow"),
        A("R17",KaitSkill.EchoStep,"影遁术","",1,2,"消耗指定方向的一个残影，传送至该格。","逐","Echo"),
        P("R19",KaitPassive.AllEchoWard,"暗影斗篷","",1,"残影可抵挡同一敌方回合内的所有攻击。","万"),
        A("S02",KaitSkill.Darkness,"黑暗术","",0,2,"在指定格生成永久黑雾，阻挡箭矢并禁止格内敌人攻击。","幕","Darkness"),
        P("S06",KaitPassive.Opportunist,"伺机而动","",1,"敌人进入攻击范围时，消耗1气踢击一次。","迎"),
        P("R40",KaitPassive.OpportunityAttack,"借机攻击","",1,"敌人离开攻击范围时，消耗1气踢击一次。","离"),
        P("R22",KaitPassive.EchoReprisal,"暗影反击","",1,"残影或诱饵受击时，对攻击者造成1点伤害。","反"),
        P("R23",KaitPassive.KillSupply,"斗战冥想","",1,"自主移动不再补2；每次击杀补两个2。","杀2"),
        P("R24",KaitPassive.WaitSupply,"静谧心境","",1,"自主移动不再补2；每次等待补两个2。","候2"),
        P("O05",KaitPassive.Wholeness,"混元体","",1,"退出气竭时，恢复1点生命。","生"),
        A("R26",KaitSkill.MendWait,"疗伤冥想","",1,3,"恢复1点生命，随后推进敌方回合。","疗","Heal"),
        P("M05",KaitPassive.PerfectSelf,"完美自我","",0,"进入气竭时，恢复1气。","息"),
        P("R28",KaitPassive.DeepReservoir,"气海扩张","",1,"气上限+3。","池"),
        P("O06",KaitPassive.QuiveringPalm,"震颤掌","",2,"拳击或踢击留下方向掌印；从其他方向命中时引爆，额外造成2点伤害。","印"),
        A("R30",KaitSkill.AirPalm,"空震","",1,1,"对指定方向首个敌人造成1点伤害，并推至尽头。","空","AirRay"),
        P("R31",KaitPassive.Passwall,"穿墙术","",0,"副盘数字可以穿过柱子。","穿"),
        P("R32",KaitPassive.BagHolding,"异次元袋","",1,"每次合成后，移除后方一个同原料值的数字。","藏"),
        P("R33",KaitPassive.ReverseGravity,"重力反转","",1,"副盘改为沿输入的反方向移动。","逆"),
        P("R34",KaitPassive.OldNewsArchive,"远古奥秘之书","",1,"同值数字达到5枚时，自动合成最旧的两枚。","积"),
        P("O03",KaitPassive.FollowThrough,"追身步","",0,"推开敌人后，自动跟进至其身前。","追"),
        P("R37",KaitPassive.KiAegis,"灵体护身","",2,"受击时，消耗3气抵挡。","御"),
        A("R38",KaitSkill.PhantomSlide,"空冥身","",1,3,"向指定方向滑行，穿过敌人且不攻击。","穿阵","Phantom"),
        A("R39",KaitSkill.UniqueDecoy,"次级幻影","",1,1,"在空格放置一个诱饵，吸引敌人锁定并抵挡一次攻击。","饵","Decoy"),
        P("N01",KaitPassive.SweepingPursuit,"横扫追击","",1,"近身拳击击杀后，对一个四邻敌人追加拳击。","追击"),
        A("N02",KaitSkill.ThunderWave,"雷鸣波","",0,1,"将四邻敌人向外推开1格。","震退","Thunder"),
        P("N03",KaitPassive.StoneBracers,"碎岩护腕","",1,"推动敌人撞上地形时，造成1点伤害。","撞1"),
        A("N04",KaitSkill.ShatterWave,"粉碎音波","",1,3,"对目标格及四邻造成1点伤害，并摧毁范围内的己方冰柱。","音波","Sonic"),
        A("N05",KaitSkill.MirrorImage,"镜影术","",0,1,"在空格生成一个残影。","镜","Mirror"),
        A("N06",KaitSkill.CommandAct,"命令术·出手","",0,1,"令指定敌人立即行动一次。","出手","CommandAct"),
        P("N08",KaitPassive.ManaPearl,"法力珍珠","",1,"每次合成8及以上数字时，恢复1气。","气+1"),
        P("N09",KaitPassive.WardingGlyph,"守卫刻文","",1,"每次合成时，对对应主盘格及四邻造成1点伤害。","刻"),
        P("N10",KaitPassive.MirrorResonance,"镜影共鸣","",2,"残影受击时，其他残影一同承受攻击；每个受击残影恢复1气。","共鸣"),
        P("N11",KaitPassive.ShadowBladeEcho,"影刃回响","",1,"残影受击时，对其四邻造成1点伤害。","影刃"),
        P("N12",KaitPassive.Misdirection,"误导术","",0,"敌人优先锁定四邻的残影。","误导"),
        P("N13",KaitPassive.LastingImage,"持久幻影","",2,"残影持续时间延长至两个敌方回合。","持久"),
        P("N14",KaitPassive.MagicMissile,"魔法飞弹","",0,"每次合成时，向对应格最近的敌人发射1点伤害的飞弹。","弹"),
        P("N15",KaitPassive.GravityPendulum,"重力摆锤","",1,"副盘移动产生合成时，沿反方向追加一次移动。","摆"),
        P("N16",KaitPassive.ResonanceCrystal,"共鸣水晶","",2,"每次合成后，将另外两个与结果同值的数字自动合成。","晶"),
        P("N17",KaitPassive.SpellEcho,"法术回响","",2,"每次合成，在原位触发两次合成效果。","回响"),
        P("N18",KaitPassive.BountyJar,"丰饶之壶","",1,"每次合成后，返还一个原料数字。","返还"),
        A("N19",KaitSkill.MageHand,"法师之手","",0,1,"删除副盘的一个数字。","手","MageHand")};
    public static KaitAbilityDef Get(string id)=>Cards.Find(d=>d.id==id||d.id=="yummn."+id)??LegacyCards.Find(d=>d.id==id||d.id=="yummn."+id);
    public static bool IsActive(KaitSkill s)=>Cards.Exists(d=>d.kind==KaitAbilityKind.Active&&d.skill==s);
    public static bool TargetsSelf(KaitSkill s)=>s==KaitSkill.MendWait||s==KaitSkill.ThunderWave;
    public static bool TargetsGround(KaitSkill s)=>s==KaitSkill.ShapeIce||s==KaitSkill.Darkness||s==KaitSkill.UniqueDecoy||s==KaitSkill.MirrorImage||s==KaitSkill.YummnShadowStep;
    public static bool TargetsSpecific(KaitSkill s)=>s==KaitSkill.CommandAct||s==KaitSkill.ShatterWave||s==KaitSkill.MageHand;
    public static bool TargetsDirection(KaitSkill s)=>IsActive(s)&&!TargetsSelf(s)&&!TargetsGround(s)&&!TargetsSpecific(s);
    public static string TargetHint(KaitSkill s)=>s==KaitSkill.Darkness?"点击主棋盘任意格释放":s==KaitSkill.YummnShadowStep?"点击任一空暗影格释放":s==KaitSkill.MageHand?"点击右侧副盘数字":s==KaitSkill.CommandAct?"点击一个敌人":s==KaitSkill.ShatterWave?"点击目标格及四邻范围":TargetsSelf(s)?"点击Yummn自身释放":TargetsGround(s)?"点击空目标格释放":"点选人物四周的方向释放";
    public static bool IsMonk(KaitAbilityDef d)=>d!=null&&d.id.StartsWith("yummn.");
    public static List<KaitAbilityDef> Pool()
    {
        return new List<KaitAbilityDef>(Cards);
    }
    private static KaitAbilityDef A(string id,KaitSkill s,string name,string tag,int rarity,int cost,string text,string sigil,string mode=null)=>new KaitAbilityDef{id="yummn."+id,kind=KaitAbilityKind.Active,skill=s,nameZh=name,nameEn=s.ToString(),rarity=(KaitRarity)rarity,kiExtraCost=cost,cardText=text,tags=new[]{"Yummn",tag},traditionTag=tag,allowedCharacters=new[]{"Yummn"},actionOverride=mode,sigil=sigil,copyable=false,origin=KaitAbilityOrigin.Dnd5e};
    private static KaitAbilityDef P(string id,KaitPassive p,string name,string tag,int rarity,string text,string sigil,params string[] prereqs)=>new KaitAbilityDef{id="yummn."+id,kind=KaitAbilityKind.Passive,passive=p,nameZh=name,nameEn=p.ToString(),rarity=(KaitRarity)rarity,cardText=text,tags=new[]{"Yummn",tag},traditionTag=tag,allowedCharacters=new[]{"Yummn"},prerequisiteIds=prereqs,sigil=sigil,copyable=false,origin=KaitAbilityOrigin.ProjectOriginal};
}
