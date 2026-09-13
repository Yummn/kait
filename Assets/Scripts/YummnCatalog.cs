using System.Collections.Generic;
public static class YummnCatalog
{
    public const string Version="0.9.0-root-action";
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
        P("R01",KaitPassive.TwinPunch,"疾风连击","",1,"拳击耗2气打两拳，反击也生效；逐拳触发特效。气不足打普通拳。","双"),
        A("R02",KaitSkill.PreciseStep,"疾步如风·收势","",0,1,"只移动一格，总计1气；不附带拳击。","步","Precise"),
        P("R03",KaitPassive.FrugalStride,"风行之靴","",2,"高速移动固定消耗1气；有效自主移动补两个2。","渡"),
        P("O02",KaitPassive.OpenHand,"散打技巧·推掌","",0,"每拳命中未击杀时免费推开1格。","推"),
        P("R05",KaitPassive.StunStrike,"震慑拳","",1,"命中存活目标耗1气震慑，持续整个敌方回合；期间不重复耗气。","震"),
        P("R06",KaitPassive.WaitingGuard,"无甲防御","",0,"等待时抵挡本敌方回合第一次伤害；治疗等待也生效。","候"),
        P("M04",KaitPassive.DeflectMissiles,"拨挡飞弹","",1,"每次箭矢命中消耗1气抵挡；不足1气不触发。","拨"),
        P("R08",KaitPassive.DistantPull,"水鞭","",1,"方向上有非近身敌人时，原地造成1伤并拉至身前。","牵"),
        P("R09",KaitPassive.EndlessPush,"不坏气拳","",1,"每拳命中存活敌人，推至阻挡处；优先于散打技巧·推掌。","远推"),
        A("E03",KaitSkill.ShapeIce,"塑造流水","",1,2,"在空格生成永久冰柱；场上只保留最新一根。","柱","Ice"),
        A("E04",KaitSkill.FrostBreath,"冬之吐息","",0,2,"前方两格各1伤，冻结存活敌人。","霜","Winter"),
        P("E05",KaitPassive.FireSnake,"火蛇之牙","",1,"每次拳击使主目标身后一格的敌人受到1伤。","贯"),
        P("R13",KaitPassive.FreezePush,"寒霜之触","",1,"成功强制移动敌人后将其冻结。","封"),
        P("E06",KaitPassive.ShatteringPalm,"碎冰掌","",2,"任何攻击破冰时，被破冰者免伤，四邻敌人各受1伤；可连锁破冰。","碎"),
        P("R15",KaitPassive.FreezePunch,"冰封拳","",2,"拳击命中未击杀时冻结目标。","凝"),
        A("S01",KaitSkill.YummnShadowStep,"暗影步","",0,1,"传送至该方向最近的空阴影格。","影","Shadow"),
        A("R17",KaitSkill.EchoStep,"影遁术","",1,2,"传送至该方向的空残影格并消耗残影；不补2。","逐","Echo"),
        P("R19",KaitPassive.AllEchoWard,"暗影斗篷","",2,"所有残影可承受一个敌方回合内无限次攻击。","万"),
        A("S02",KaitSkill.Darkness,"黑暗术","",1,2,"空格布置永久暗幕；阻箭，格内敌人不能攻击。只保留一处。","幕","Darkness"),
        P("S06",KaitPassive.Opportunist,"伺机而动","",1,"高速行动中，敌人进入四邻后拳击一次，可触发疾风连击；每敌人每次操作限一次。","迎"),
        P("R40",KaitPassive.OpportunityAttack,"借机攻击","",1,"敌人离开四邻后踢击一次，不触发拳击技能；气竭也生效，每敌人每次操作限一次。","离"),
        P("R22",KaitPassive.EchoReprisal,"暗影反击","",1,"每个残影或诱饵受到攻击，都对攻击者反伤1。","反"),
        P("R23",KaitPassive.KillSupply,"斗战冥想","",2,"自主移动不补2；每次击杀总计补两个2。","杀2"),
        P("R24",KaitPassive.WaitSupply,"静谧心境","",1,"自主移动不补2；每次等待补一个2。","候2"),
        P("O05",KaitPassive.Wholeness,"混元体","",2,"从气竭恢复高速时回复1生命。","生"),
        A("R26",KaitSkill.MendWait,"疗伤冥想","",1,3,"回复1生命，然后推进一个敌方回合。","疗","Heal"),
        P("M05",KaitPassive.PerfectSelf,"完美自我","",0,"进入气竭时恢复1气；仍需回满才退出气竭。","息"),
        P("R28",KaitPassive.DeepReservoir,"气海扩张","",1,"气上限+3；击杀回气-1。","池"),
        P("O06",KaitPassive.QuiveringPalm,"震颤掌","",2,"拳击留方向掌印；下一次异向命中额外2伤并消印。","印"),
        A("R30",KaitSkill.AirPalm,"空震","",1,2,"原地对方向上第一个敌人造成1伤，并推至阻挡处。","空","AirRay"),
        P("R31",KaitPassive.Passwall,"穿墙术","",1,"副盘数字可穿过柱子，但不能停在柱子上。","穿"),
        P("R32",KaitPassive.BagHolding,"异次元袋","",1,"合成后移除一个同源小数字，优先移动方向后方。","藏"),
        P("R33",KaitPassive.ReverseGravity,"重力反转","",1,"副盘沿输入的反方向移动；人物方向不变。","逆"),
        P("R34",KaitPassive.OldNewsArchive,"远古奥秘之书","",1,"任意数字达到5枚，最旧两枚自动合成；持续检查，可连锁。","积"),
        P("O03",KaitPassive.FollowThrough,"追身步","",0,"成功推开敌人后，免费跟进至其最终位置前一格；路被挡时停在可达处，不补2。","追"),
        P("R37",KaitPassive.KiAegis,"灵体护身","",2,"高速受击时消耗3气抵挡；击杀回气-1。","御"),
        A("R38",KaitSkill.PhantomSlide,"空冥身","",1,3,"沿方向穿过敌人，不攻击；停在最后可停空格。","穿阵","Phantom"),
        A("R39",KaitSkill.UniqueDecoy,"次级幻影","",1,1,"空格放置唯一诱饵，吸引后续锁定；承受一次攻击即消失。","饵","Decoy"),
        P("N01",KaitPassive.SweepingPursuit,"横扫追击","",1,"近身拳击击杀后，追击一个四邻敌人；每敌人每次操作限一次。","追击"),
        A("N02",KaitSkill.ThunderWave,"雷鸣波","",0,2,"将四邻敌人各向外推1格。","震退","Thunder"),
        P("N03",KaitPassive.StoneBracers,"碎岩护腕","",1,"你推动的敌人撞到地形时受到1伤，每次推动限一次。","撞1"),
        A("N04",KaitSkill.ShatterWave,"粉碎音波","",1,3,"目标格及四邻敌人各受1伤，摧毁范围内自己的冰柱。","音波","Sonic"),
        A("N05",KaitSkill.MirrorImage,"镜影术","",1,2,"在空格生成一个残影。","镜","Mirror"),
        A("N06",KaitSkill.CommandAct,"命令术·出手","",1,2,"指定敌人立即执行一次正常行动，其他敌人不行动。","出手","CommandAct"),
        P("N08",KaitPassive.ManaPearl,"法力珍珠","",1,"每次合成8及以上数字，恢复1气。","气+1"),
        P("N09",KaitPassive.WardingGlyph,"守卫刻文","",1,"每次合成使对应主盘格及四邻敌人各受1伤。","刻"),
        P("N10",KaitPassive.MirrorResonance,"镜影共鸣","",2,"一个残影受击，其他残影也承受本次攻击；每次攻击每影限一次。","共鸣"),
        P("N11",KaitPassive.ShadowBladeEcho,"影刃回响","",1,"每个残影受击时，其四邻敌人各受1伤。","影刃"),
        P("N12",KaitPassive.Misdirection,"误导术","",0,"未锁定的敌人优先以四邻残影为目标，仍按兵种移动或瞄准；不改变已有预警。","误导"),
        P("N13",KaitPassive.LastingImage,"持久幻影","",2,"残影自然持续两个敌方回合，受击仍正常消耗。","持久"),
        P("N14",KaitPassive.MagicMissile,"魔法飞弹","",0,"每次合成发射一枚1伤飞弹，锁定对应格最近敌人。","弹"),
        P("N15",KaitPassive.GravityPendulum,"重力摆锤","",1,"基础副盘移动有合成时，反向再滑一次；每次操作限一次。","摆"),
        P("N16",KaitPassive.ResonanceCrystal,"共鸣水晶","",2,"本次操作新合成且仍在盘上的同值数字可跨格继续合成。","晶"),
        P("N17",KaitPassive.SpellEcho,"法术回响","",2,"后续合成重复本次操作之前的直接合成攻击；复制不再产生回响。","回响"),
        P("N18",KaitPassive.BountyJar,"丰饶之壶","",1,"每次合成记录一个原料数字，整次操作结束后返还；无空位丢弃。","返还"),
        A("N19",KaitSkill.MageHand,"法师之手","",0,1,"删除副盘一个数字；不移动、不补2、不触发合成。","手","MageHand")};
    public static KaitAbilityDef Get(string id)=>Cards.Find(d=>d.id==id||d.id=="yummn."+id)??LegacyCards.Find(d=>d.id==id||d.id=="yummn."+id);
    public static bool IsActive(KaitSkill s)=>Cards.Exists(d=>d.kind==KaitAbilityKind.Active&&d.skill==s);
    public static bool TargetsSelf(KaitSkill s)=>s==KaitSkill.MendWait||s==KaitSkill.ThunderWave;
    public static bool TargetsGround(KaitSkill s)=>s==KaitSkill.ShapeIce||s==KaitSkill.Darkness||s==KaitSkill.UniqueDecoy||s==KaitSkill.MirrorImage;
    public static bool TargetsSpecific(KaitSkill s)=>s==KaitSkill.CommandAct||s==KaitSkill.ShatterWave||s==KaitSkill.MageHand;
    public static bool TargetsDirection(KaitSkill s)=>IsActive(s)&&!TargetsSelf(s)&&!TargetsGround(s)&&!TargetsSpecific(s);
    public static string TargetHint(KaitSkill s)=>s==KaitSkill.MageHand?"点击右侧副盘数字":s==KaitSkill.CommandAct?"点击一个敌人":s==KaitSkill.ShatterWave?"点击目标格及四邻范围":TargetsSelf(s)?"点击Yummn自身释放":TargetsGround(s)?"点击空目标格释放":"点选人物四周的方向释放";
    public static bool IsMonk(KaitAbilityDef d)=>d!=null&&d.id.StartsWith("yummn.");
    public static List<KaitAbilityDef> Pool()
    {
        return new List<KaitAbilityDef>(Cards);
    }
    private static KaitAbilityDef A(string id,KaitSkill s,string name,string tag,int rarity,int cost,string text,string sigil,string mode=null)=>new KaitAbilityDef{id="yummn."+id,kind=KaitAbilityKind.Active,skill=s,nameZh=name,nameEn=s.ToString(),rarity=(KaitRarity)rarity,kiExtraCost=cost,cardText=text,tags=new[]{"Yummn",tag},traditionTag=tag,allowedCharacters=new[]{"Yummn"},actionOverride=mode,sigil=sigil,copyable=false,origin=KaitAbilityOrigin.Dnd5e};
    private static KaitAbilityDef P(string id,KaitPassive p,string name,string tag,int rarity,string text,string sigil,params string[] prereqs)=>new KaitAbilityDef{id="yummn."+id,kind=KaitAbilityKind.Passive,passive=p,nameZh=name,nameEn=p.ToString(),rarity=(KaitRarity)rarity,cardText=text,tags=new[]{"Yummn",tag},traditionTag=tag,allowedCharacters=new[]{"Yummn"},prerequisiteIds=prereqs,sigil=sigil,copyable=false,origin=KaitAbilityOrigin.ProjectOriginal};
}
