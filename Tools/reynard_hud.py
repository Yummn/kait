from pathlib import Path
P=Path('Assets/Scripts')
def edit(f,a,b):
 p=P/f;s=p.read_text(encoding='utf-8-sig');assert a in s,(f,a);p.write_text(s.replace(a,b),encoding='utf-8')
edit('KaitGame.cs','        RefreshYummnSkillTargets();\n        if(buildDirectionText','        RefreshYummnSkillTargets();\n        if(buildDirectionText')
edit('KaitGame.cs','        SetHealthBar(runHealthBar, run.kateHp);','        RefreshReynardHud();\n        SetHealthBar(runHealthBar, run.kateHp);')
edit('KaitGame.cs','if(run.IsYummn&&targetingSkill!=KaitSkill.None)','if((run.IsYummn||run.IsReynard)&&targetingSkill!=KaitSkill.None)')
edit('KaitGame.cs','if(!run.IsYummn)InterruptKaitAnimationForMovement();','if(run.IsReynard&&busy)return;\n        if(!run.IsYummn)InterruptKaitAnimationForMovement();')
edit('KaitGame.YummnTargeting.cs','bool visible=run.IsYummn&&!run.ended&&YummnCatalog.TargetsDirection(targetingSkill);','bool visible=!run.ended&&(run.IsReynard?targetingSkill==KaitSkill.ReynardLightningBolt:run.IsYummn&&YummnCatalog.TargetsDirection(targetingSkill));')
edit('KaitSkillCard.cs','    public void SetAvailability(bool ready,','    private string reynardState;\n    public void SetReynardState(string text){reynardState=text;RefreshText();}\n    public void SetAvailability(bool ready,')
edit('KaitSkillCard.cs','        if(readable&&YummnCatalog.IsMonk(def))','        if(def.spellLevel>0)state.text=reynardState??ReynardCatalog.Roman(def.spellLevel)+"环";\n        if(readable&&YummnCatalog.IsMonk(def))')
edit('KaitSkillCard.cs','        if(!string.IsNullOrEmpty(missingRequirement)&&readable)','        if(def.spellLevel>0)footer.text=targeting?ReynardCatalog.TargetHint(Skill):reynardState??ReynardCatalog.Roman(def.spellLevel)+"环";\n        if(!string.IsNullOrEmpty(missingRequirement)&&readable)')
edit('KaitSkillDeck.cs','            card.SetPending(run.IsAbilityPending','            card.SetReynardState(run.IsReynard?(run.Reynard.focusedSkill==card.Skill?"维持中":ReynardCatalog.Roman(ReynardCatalog.Get(card.Skill)?.spellLevel??0)+"环 · "+(run.ReynardCastFailure(card.Skill)??"可用")):null);\n            card.SetPending(run.IsAbilityPending')
# Cache portrait sprite; never allocate sprites during every RefreshAll.
edit('KaitGame.Reynard.cs','    private readonly','    private readonly') if False else None
edit('KaitGame.Reynard.cs',' private readonly List<GameObject>',' private Sprite reynardPortrait;\n private readonly List<GameObject>')
edit('KaitGame.Reynard.cs','storybookPortrait.sprite=Sprite.Create(tex,new Rect(0,0,tex.width,tex.height),Vector2.one*.5f);','if(reynardPortrait==null)reynardPortrait=Sprite.Create(tex,new Rect(0,0,tex.width,tex.height),Vector2.one*.5f);storybookPortrait.sprite=reynardPortrait;')
edit('KaitTutorialBook.cs','    public bool AppendixOpen =>','    public bool ReynardMode;\n    public bool AppendixOpen =>')
edit('KaitTutorialBook.cs','        heading.text=YummnMode','        if(ReynardMode){ShowReynardTutorial();return;}\n        heading.text=YummnMode')
idx=(P/'KaitTutorialBook.cs').read_text(encoding='utf-8').index('    public void Next()')
p=P/'KaitTutorialBook.cs';s=p.read_text(encoding='utf-8');s=s[:idx]+'''    private void ShowReynardTutorial()
    {
        SetComicLayout(false);heading.text="Reynard · 方向编咒";title.text="方向施咒，等待召狐";
        comic.gameObject.SetActive(false);leftCaption.text=rightCaption.text=thirdCaption.text="";
        body.gameObject.SetActive(true);body.rectTransform.anchoredPosition=new Vector2(-235,0);body.rectTransform.sizeDelta=new Vector2(820,510);body.fontSize=28;
        SetReadableCopy(body,"每次方向操作尝试移动一格，同时滑动右盘。\\n\\n连续同向：贯火，直线贯穿。\\n转向90°：折火，向前后两个方向攻击。\\n反向：回火，攻击四邻。首次方向只起势。\\n\\n等待：把镜狐召到脚下对应的右盘数字。没有数字则保留原位置。\\n\\n镜狐数字不随滑动移动。同值数字可以撞入合并。");
        SetReadableCopy(lead,"2＝I环 · 4＝II环 · 8＝III环\\n高环可施放低环法术。");
        SetReadableCopy(tip,"点卡牌，再点目标施放。法术耗竭节点；合并使节点恢复。六槽自由混装，同时只维持一个持续法术。方向、等待、施法均补一个2并推进敌人行动。");
        counter.text="核心规则";previous.interactable=false;nextLabel.text="关闭";PageIndex=PageCount-1;
        appendix.gameObject.SetActive(false);if(yummnDiagram!=null)yummnDiagram.gameObject.SetActive(false);bossNumberLabel.gameObject.SetActive(false);
    }
''' +s[idx:];p.write_text(s,encoding='utf-8')
for f in ['KaitGame.cs','KaitGame.Yummn.cs']:
 p=P/f;s=p.read_text(encoding='utf-8-sig');s=s.replace('book.YummnMode=mainMenu.Selected==KaitCharacter.Yummn;','book.ReynardMode=mainMenu.Selected==KaitCharacter.Reynard;book.YummnMode=mainMenu.Selected==KaitCharacter.Yummn;').replace('book.YummnMode=run.IsYummn;','book.ReynardMode=run.IsReynard;book.YummnMode=run.IsYummn;');p.write_text(s,encoding='utf-8')
