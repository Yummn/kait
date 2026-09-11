"""Code-native weapon UI variants, not character art or game screenshots."""
import ast, math
from pathlib import Path
from PIL import Image,ImageDraw

# Reuse the established editable weapon glyphs without running the old exporter.
source=Path(__file__).with_name('preview_enemy_telegraphs_abc.py')
tree=ast.parse(source.read_text(encoding='utf-8-sig'))
nodes=[]
for node in tree.body:
    if isinstance(node,ast.Assign) and any(isinstance(t,ast.Name) and t.id=='manifest' for t in node.targets):break
    nodes.append(node)
ns={'__file__':str(source)}
exec(compile(ast.Module(body=nodes,type_ignores=[]),str(source),'exec'),ns)
out=source.parents[1]/'VFXPreviews/GroundWeapons-20260911'
out.mkdir(parents=True,exist_ok=True)
names=list(ns['NAMES']);drawtext=ns['txt'];icon=ns['icon'];rr=ns['rr'];ln=ns['ln']
for variant in range(3):
    frames=[]
    for frame in range(24):
        im=Image.new('RGB',(1440,840),'#24303a');d=ImageDraw.Draw(im)
        drawtext(d,360,25,['A · 裸武器印记','B · 细圆环','C · 四角定位'][variant],22)
        drawtext(d,360,54,'脚下所在格 · 示意人物轮廓，不是游戏截图',12)
        color='#ad563c' if frame<18 else '#dfb54d'
        for i,kind in enumerate(names):
            x=130+(i%3)*230;y=153+(i//3)*166
            rr(d,(x-69,y-59,x+69,y+72),'#758d99','#c0d0d4',1,3)
            gx,gy=x,y+43
            if variant==1:d.ellipse(((gx-29)*2,(gy-22)*2,(gx+29)*2,(gy+22)*2),outline=color,width=2)
            if variant==2:
                for sx in (-1,1):
                    for sy in (-1,1):
                        xx=gx+sx*31;yy=gy+sy*23
                        ln(d,[(xx-sx*9,yy),(xx,yy),(xx,yy-sy*6)],color,1)
            icon(d,kind,gx,gy,color,.8)
            # Deliberately abstract pawn so no generated character is mistaken for Spine.
            d.ellipse(((x-18)*2,(y-34)*2,(x+18)*2,(y+2)*2),fill='#596573',outline='#e0e5df',width=3)
            rr(d,(x-22,y+3,x+22,y+28),'#d9e1dd','#596573',1,8)
            drawtext(d,x,y-76,ns['NAMES'][kind],14)
        frames.append(im.resize((1080,630),Image.Resampling.LANCZOS))
    frames[4].save(out/f'{"ABC"[variant]}.png')
    frames[0].save(out/f'{"ABC"[variant]}.gif',save_all=True,append_images=frames[1:],duration=100,loop=0,disposal=2)
print(out)
