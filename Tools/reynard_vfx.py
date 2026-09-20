from pathlib import Path
p=Path('Assets/Scripts/KaitGame.Reynard.cs');s=p.read_text(encoding='utf-8-sig');s=s.replace('kaitSpine?.PlayOnce(r.reynardEvents.Count>0?"attack":"idle","idle");','kaitSpine?.PlayOnce(r.reynardCast!=KaitSkill.None?"skill":r.reynardEvents.Count>0?"attack":"idle","idle");')
s=s.replace('if(r.reynardEvents.Count>0)GameAudio.PlaySkillUse(KaitSkill.HungerOfHadar);','if(r.reynardCast!=KaitSkill.None)GameAudio.PlayReynard("SpellCast");\n  else if(r.reynardEvents.Count>0)GameAudio.PlayReynard(r.reynardEvents[0].kind=="Backfire"?"Backfire":r.reynardEvents[0].kind=="Turn"?"Turnfire":"Foxfire");')
s=s.replace('image.sprite=ReynardArt.Load("Cards/"+icon);','image.sprite=ReynardArt.ImpactFrame(0);')
s=s.replace('image.rectTransform.sizeDelta=Vector2.one*size*(.5f+.5f*Mathf.Sin(Mathf.PI*Mathf.Min(1,t/.3f)));','image.sprite=ReynardArt.ImpactFrame(Mathf.Clamp((int)(t/.3f*8),0,7));image.rectTransform.sizeDelta=Vector2.one*size;')
s=s.replace('  var p=run.Reynard.mirrorFoxCell;','  RefreshReynardSustain();\n  var p=run.Reynard.mirrorFoxCell;')
# Persistent footprint overlays, under units and warning outlines.
i=s.rfind('}');s=s[:i]+''' private void RefreshReynardSustain()
 {
  var skill=run.Reynard.focusedSkill;if(skill==KaitSkill.None)return;
  string id=ReynardCatalog.Get(skill).id.Substring(8);
  for(int y=1;y<6;y++)for(int x=1;x<6;x++){
   var p=new Vector2Int(x,y);bool area=skill==KaitSkill.ReynardFog||skill==KaitSkill.ReynardWeb;
   if(area?Mathf.Abs(x-run.Reynard.focusedCell.x)+Mathf.Abs(y-run.Reynard.focusedCell.y)>1:p!=run.katePos)continue;
   var go=new GameObject("Reynard Sustain",typeof(RectTransform),typeof(Image));go.transform.SetParent(battleUnderEffectLayer,false);go.transform.position=battleCells[x+y*KaitRun.BattleSize].transform.position;
   var im=go.GetComponent<Image>();im.sprite=ReynardArt.Load("Cards/"+id);im.preserveAspect=true;im.raycastTarget=false;im.color=new Color(1,1,1,area?.44f:.23f);im.rectTransform.sizeDelta=Vector2.one*(area?100:118);reynardPersistent.Add(go);
  }
 }
''' +s[i:];p.write_text(s,encoding='utf-8')
p=Path('Assets/Scripts/ReynardArt.cs');s=p.read_text(encoding='utf-8-sig');i=s.index(' public static string EnemyId');s=s[:i]+''' static readonly Sprite[] impact=new Sprite[8];
 public static Sprite ImpactFrame(int index){if(impact[index]!=null)return impact[index];var t=Resources.Load<Texture2D>("Reynard/FoxfireImpact");if(t==null)return null;float sx=t.width/1280f,sy=t.height/1280f;float top=index<4?225:635;return impact[index]=Sprite.Create(t,new Rect(index%4*320*sx,t.height-(top+400)*sy,320*sx,400*sy),Vector2.one*.5f);}
''' +s[i:];p.write_text(s,encoding='utf-8')
