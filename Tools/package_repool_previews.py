"""Package generated frame sheets for selection, never alter Unity resources."""
from pathlib import Path
import json, shutil
from PIL import Image, ImageDraw, ImageFont

root = Path(__file__).resolve().parents[1]
out = root / 'VFXPreviews' / 'Repool-20260912'
out.mkdir(parents=True, exist_ok=True)
manifest = json.loads((out / 'manifest.json').read_text(encoding='utf-8'))
font = ImageFont.truetype('C:/Windows/Fonts/msyh.ttc', 22)
names = {'Stun':'震慑', 'IceGuard':'破冰免伤', 'Heal':'治疗', 'Decoy':'唯一诱饵', 'Reflect':'残影反伤', 'Deflect':'耗气拨箭'}
stats=[]
for item in manifest:
    src=Path(item['path'])
    dst=out/(item['name']+'.png')
    if src.resolve()!=dst.resolve(): shutil.copy2(src,dst)
    im=Image.open(dst).convert('RGBA')
    stats.append({'name':item['name'],'size':im.size,'alpha':im.getchannel('A').getextrema()})
for kind,title in names.items():
    paths=[out/f'{kind}_{v}.png' for v in 'ABC']
    if not all(p.exists() for p in paths): continue
    sheets=[Image.open(p).convert('RGBA') for p in paths]
    frames=[]
    for i in range(8):
        canvas=Image.new('RGB',(840,310),(48,44,55))
        d=ImageDraw.Draw(canvas)
        for j,im in enumerate(sheets):
            w,h=im.size
            cell=im.crop((round((i%4)*w/4),round((i//4)*h/2),round((i%4+1)*w/4),round((i//4+1)*h/2)))
            cell.thumbnail((248,248),Image.Resampling.LANCZOS)
            canvas.paste(cell,(j*280+(280-cell.width)//2,48+(248-cell.height)//2),cell)
            d.text((j*280+18,10),f'{"ABC"[j]}  {title}',font=font,fill='#f5e7cd')
        frames.append(canvas)
    frames[3].save(out/f'{kind}-ABC.png')
    if kind!='Stun': frames += [Image.new('RGB',(840,310),(48,44,55))]*4
    frames[0].save(out/f'{kind}-ABC.gif',save_all=True,append_images=frames[1:],duration=120,loop=0,disposal=2)
cards=Image.new('RGB',(840,620),(48,44,55))
draw=ImageDraw.Draw(cards)
for j,(kind,title) in enumerate(names.items()):
    p=out/f'Card_{kind}.png'
    if not p.exists(): continue
    icon=Image.open(p).convert('RGBA')
    icon.thumbnail((244,244),Image.Resampling.LANCZOS)
    x,y=(j%3)*280,(j//3)*310
    cards.paste(icon,(x+(280-icon.width)//2,y+48+(244-icon.height)//2),icon)
    draw.text((x+20,y+10),title,font=font,fill='#f5e7cd')
cards.save(out/'Cards.png')
(out/'validation.json').write_text(json.dumps(stats,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(stats,ensure_ascii=False))
