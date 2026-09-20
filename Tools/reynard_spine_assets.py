from pathlib import Path
import uuid,re,json,shutil
P=Path('Assets/Resources/Characters');ref=P/'Yummn'
def guid(p):
 m=Path(str(p)+'.meta')
 if m.exists():return re.search('guid: (\\w+)',m.read_text()).group(1)
 g=uuid.uuid4().hex;m.write_text('fileFormatVersion: 2\nguid: '+g+'\n',encoding='utf-8');return g
for d in (P/'Reynard').iterdir():
 if not d.is_dir():continue
 id=d.name;j=next(d.glob('*.json'));a=next(d.glob('*.atlas.txt'));png=next(d.glob('*.png'))
 # Source atlas texture names stay unchanged; one exact atlas per skeleton.
 for old,new,name in [('108231_Material.mat',id+'_Material.mat',id+'_Material'),('108231_Atlas.asset',id+'_Atlas.asset',id+'_Atlas'),('108231_SkeletonData.asset',id+'_SkeletonData.asset',id+'_SkeletonData')]:
  txt=(ref/old).read_text();txt=re.sub(r'm_Name: .*','m_Name: '+name,txt)
  if old.endswith('.mat'):txt=txt.replace('d8a4c34008093004eb09f178246dfc49',guid(png))
  elif 'Atlas' in old:txt=txt.replace('a7c0cabf906848e47b3a7ed93a2bf531',guid(a)).replace('63abf064048e95949a3ea46c00f3365b',guid(d/(id+'_Material.mat')))
  else:txt=txt.replace('2555fb7e06e4c3845b036d30e1294152',guid(d/(id+'_Atlas.asset'))).replace('0b1c212a9f39211438b7450e1175e6e4',guid(j)).replace('defaultMix: 0.2','defaultMix: 0.06')
  (d/new).write_text(txt);guid(d/new)
 data=json.loads(j.read_text(encoding="utf-8")); print(id,list(data.get('animations',{})), 'skins', [x['name'] for x in data['skins']])
shutil.copy2(r'C:\Users\yummn\Desktop\应用\Spine\素材\人物\S091伊甸的骄傲小人V3.79\目录\CardSpine_11202003.png','Assets/Resources/KaitVisuals/CharacterSelection/Reynard.png')

