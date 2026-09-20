from pathlib import Path
P=Path('Assets/Scripts')
p=P/'KaitSpineView.cs';s=p.read_text(encoding='utf-8-sig').replace('name==Stop','name==WallStop');p.write_text(s,encoding='utf-8')
p=P/'KaitMainMenu.cs';s=p.read_text(encoding='utf-8-sig');start=s.index('        menu.CharacterButtons =');end=s.index('        menu.Fit();',start)
s=s[:start]+'''        menu.CharacterButtons=new Button[3];
        string[] names={"KAIT","YUMMN","REYNARD"};
        string[] subtitles={"助跑蓄势 · 击杀转向","气与残影 · 三宗派构筑","方向编咒 · 镜狐法术位"};
        for(int i=0;i<3;i++){
            int index=i;float x=-728+i*458;
            var panel=MakeRect(names[i]+" Character",menu.Layout,new Vector2(x,0),new Vector2(436,980));
            var face=panel.gameObject.AddComponent<Image>();face.sprite=rounded;face.type=Image.Type.Sliced;face.color=new Color32(250,241,220,255);
            var portrait=MakeRect("Portrait",panel,new Vector2(0,75),new Vector2(420,720)).gameObject.AddComponent<RawImage>();
            portrait.texture=Resources.Load<Texture2D>("KaitVisuals/CharacterSelection/"+(KaitCharacter)i);portrait.raycastTarget=false;
            if(portrait.texture!=null){var fit=portrait.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=(float)portrait.texture.width/portrait.texture.height;}
            var anim=panel.gameObject.AddComponent<ReynardMenuHover>();anim.Portrait=portrait.rectTransform;anim.Index=i;anim.Menu=menu;
            var button=panel.gameObject.AddComponent<Button>();button.targetGraphic=face;button.onClick.AddListener(()=>{GameAudio.PlayClick();menu.Select((KaitCharacter)index);});menu.CharacterButtons[i]=button;
            menu.Label(font,names[i],new Vector2(x,-320),new Vector2(420,72),46,Plum);
            menu.Label(font,subtitles[i],new Vector2(x,-384),new Vector2(420,42),23,Plum);
        }
        float mx=686;
        menu.Label(font,"2048",new Vector2(mx,346),new Vector2(410,160),76,Peach);
        menu.Label(font,"三 境 之 间",new Vector2(mx,267),new Vector2(400,50),25,Peach);
        menu.selectedName=menu.Label(font,"",new Vector2(mx,176),new Vector2(410,50),29,Peach);
        menu.selectedTrait=menu.Label(font,"",new Vector2(mx,125),new Vector2(430,45),21,Color.white);
        menu.ContinueButton=menu.MakeButton(font,rounded,"继续游戏",new Vector2(mx,40),new Vector2(352,72),false,()=>{menu.RefreshSaves();if(menu.ContinueKey!=null)menu.ContinueCharacter?.Invoke(menu.ContinueKey);});
        menu.StartButton=menu.MakeButton(font,rounded,"开始游戏",new Vector2(mx,-55),new Vector2(352,86),true,start);
        menu.TutorialButton=menu.MakeButton(font,rounded,"玩法教程",new Vector2(mx,-162),new Vector2(352,72),false,tutorial);
        menu.SettingsButton=menu.MakeButton(font,rounded,"设置",new Vector2(mx,-258),new Vector2(352,72),false,settings);
        menu.LibraryButton=menu.MakeButton(font,rounded,"卡牌大全",new Vector2(mx,-354),new Vector2(352,72),false,()=>menu.OpenLibrary?.Invoke());
        menu.Select((KaitCharacter)Mathf.Clamp(PlayerPrefs.GetInt("Kait.Character",0),0,2));
''' +s[end:]
s=s.replace('for(int i=0;i<2;i++) portraits[i].Selected=i==(int)character;','for(int i=0;i<CharacterButtons.Length;i++){var c=CharacterButtons[i].colors;c.normalColor=i==(int)character?new Color(.83f,1f,.97f):Color.white;CharacterButtons[i].colors=c;}')
s=s.replace('selectedName.text=character==','selectedName.text=character==KaitCharacter.Reynard?"REYNARD · 方向编咒":character==').replace('selectedTrait.text=character==','selectedTrait.text=character==KaitCharacter.Reynard?"镜狐法术位 · 持续法术":character==')
p.write_text(s,encoding='utf-8')
p=P/'KaitSkillDeck.cs';s=p.read_text(encoding='utf-8-sig').replace('=> run.IsYummn ? run.CanUseYummnSkill(skill)', '=> run.IsReynard ? run.ReynardCastFailure(skill)==null : run.IsYummn ? run.CanUseYummnSkill(skill)');p.write_text(s,encoding='utf-8')
