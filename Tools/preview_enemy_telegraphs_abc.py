"""Editable native UI glyphs and telegraph animations; no game imports or writes."""
from pathlib import Path
import math,json
from PIL import Image,ImageDraw,ImageFont
OUT=Path(__file__).resolve().parents[1]/'VFXPreviews/EnemyTelegraphs-ABC-20260910'
OUT.mkdir(parents=True,exist_ok=True)
S=2;PW=352;H=422;C=58;OX=31;OY=90
FONTS={n:ImageFont.truetype('C:/Windows/Fonts/msyh.ttc',n*S) for n in (11,12,14,16,19,22)}
def ln(d,points,col,w=2):d.line([(round(x*S),round(y*S)) for x,y in points],fill=col,width=w*S,joint='curve')
def poly(d,p,col):d.polygon([(round(x*S),round(y*S)) for x,y in p],fill=col)
def rr(d,b,col,fill=None,w=1,r=5):d.rounded_rectangle(tuple(round(v*S) for v in b),r*S,outline=col,fill=fill,width=w*S)
def txt(d,x,y,s,n=14,col='#ece4d6'):d.text((x*S,y*S),s,font=FONTS[n],fill=col,anchor='mm')
def cen(c):return OX+(c[0]+.5)*C,OY+(c[1]+.5)*C
def bounds(c):x,y=cen(c);return x-C/2+1,y-C/2+1,x+C/2-1,y+C/2-1
def icon(d,kind,x,y,col,scale=1,angle=0):
    # Distinct compact silhouettes; all coordinates local to the attack source.
    ca,sa=math.cos(angle),math.sin(angle)
    def p(pts):return [(x+(a*ca-b*sa)*scale,y+(a*sa+b*ca)*scale) for a,b in pts]
    def l(pts,w=2):ln(d,p(pts),col,w)
    def q(pts):poly(d,p(pts),col)
    if kind=='grunt':
      q([(-4,6),(-5,-5),(0,-16),(5,-5),(4,6)])
      l([(-9,7),(9,7)],3);l([(0,8),(0,17)],4)
    elif kind=='sword':
      q([(-3,5),(-4,-15),(0,-24),(4,-15),(3,5)])
      l([(-10,6),(0,9),(10,6)],3);l([(0,9),(0,20)],3);l([(-3,21),(3,21)],3)
    elif kind=='archer':
      l([(-9,-19),(-1,-16),(5,-8),(7,0),(5,8),(-1,16),(-9,19)],3)
      l([(-9,-19),(-4,0),(-9,19)],1);l([(-17,0),(18,0)],2);l([(10,-6),(18,0),(10,6)],2)
    elif kind=='armor':
      q([(-17,-13),(-8,-19),(-5,-11),(5,-11),(8,-19),(17,-13),(12,-4),(10,16),(-10,16),(-12,-4)])
      ln(d,p([(-7,-2),(7,-2)]),'#42505d',2);ln(d,p([(-6,6),(6,6)]),'#42505d',2)
    elif kind=='mage':
      l([(0,-4),(0,20)],3);q([(0,-22),(9,-12),(0,-2),(-9,-12)])
      l([(-15,-9),(-19,-13),(-15,-17)],1);l([(15,-9),(19,-13),(15,-17)],1)
    elif kind=='boss':
      q([(-16,-16),(0,-21),(16,-16),(14,5),(0,21),(-14,5)])
      ln(d,p([(0,-13),(0,10)]),'#42505d',3);ln(d,p([(-8,-5),(8,-5)]),'#42505d',3)
def chevron(d,x,y,col,dx=1,dy=0,size=6):
    ln(d,[(x-dx*size-dy*size,y-dy*size+dx*size),(x,y),(x-dx*size+dy*size,y-dy*size-dx*size)],col,2)
NAMES={'grunt':'4 · 杂兵','sword':'8 · 剑士','archer':'16 · 弓手','armor':'32 · 重甲兵','mage':'64 · 术士','boss':'128 · 盾骑士'}
LABELS={
 'grunt':['短刃蓄刺','刀尖点名','短刃警徽'],
 'sword':['长剑蓄斩','剑弧收束','交剑警徽'],
 'archer':['弓弦与流矢','单轨瞄准','箭羽定位'],
 'armor':['胸甲蓄势','甲片压迫','铆钉警徽'],
 'mage':['法杖与十字','四向锁心','菱晶锁定'],
 'boss':['盾面与战线','盾沿推进','盾剑指令']}
def draw_base(d,variant,kind):
    txt(d,PW/2,22,f'{"ABC"[variant]} · {LABELS[kind][variant]}',19)
    txt(d,PW/2,52,['来源武器 + 统一底框','来源动作 + 简化范围','兵种警徽 + 边缘方向'][variant],12,'#b6c8d4')
    for yy in range(5):
      for xx in range(5):
        b=bounds((xx,yy));rr(d,b,'#78909e','#b7c9d0' if (xx+yy)%2 else '#afc3cc')
        ln(d,[(b[0]+6,b[1]+3),(b[2]-6,b[1]+3)],'#d3e0e3',1)
def threat(d,c,col,v,fill=True):
    b=bounds(c);x,y=cen(c)
    if fill:rr(d,b,col,'#bdaca7',1)
    if v==0:rr(d,b,col,None,2)
    elif v==1:
      for dx,dy in [(-1,-1),(-1,1),(1,-1),(1,1)]:
        xx=x+dx*26;yy=y+dy*26;ln(d,[(xx-dx*9,yy),(xx,yy),(xx,yy-dy*9)],col,2)
    else:
      rr(d,b,col,None,1);ln(d,[(b[0]+9,b[3]-3),(b[2]-9,b[3]-3)],col,3)
def frame(kind,v,i):
    im=Image.new('RGB',(PW*S,H*S),'#252e39');d=ImageDraw.Draw(im);draw_base(d,v,kind)
    src=(1,2);target=(2,2);cells=[target]
    if kind in ('archer','boss'):src=(0,2);target=(4,2);cells=[(x,2) for x in range(1,5)]
    if kind=='mage':src=(0,4);cells=[(2,2),(1,2),(3,2),(2,1),(2,3)]
    pulse=(math.sin(i*math.pi/12)+1)/2;firing=30<=i<35;clear=i>=35
    col='#ffe39a' if firing else '#b95840' if pulse>.5 else '#9a4f42'
    sx,sy=cen(src);tx,ty=cen(target)
    if not clear:
      for c in cells:threat(d,c,col,v)
      if kind=='archer':
        for c in cells:
          x,y=cen(c)
          if v==0:
            chevron(d,x-5+(i%10),y-14,col);chevron(d,x+7+(i%10),y+14,col)
          elif v==1:ln(d,[(x-25,y),(x+25,y)],col,1);chevron(d,x-2+i%12,y,col)
          else:
            for yy in (-22,22):chevron(d,x,y+yy,col,size=4)
      if kind=='mage':
        for c in cells:
          x,y=cen(c)
          if c==(2,2):
            if v==2:
              for dx,dy in [(0,-1),(0,1),(-1,0),(1,0)]:
                a=x+dx*22;b=y+dy*22;poly(d,[(a,b-4),(a+4,b),(a,b+4),(a-4,b)],col)
          else:
            dx=(tx-x)/C;dy=(ty-y)/C
            if v==0:chevron(d,x+dx*8,y+dy*8,col,dx,dy)
            elif v==1:chevron(d,x+dx*(6+pulse*6),y+dy*(6+pulse*6),col,dx,dy);ln(d,[(x-dx*15,y-dy*15),(x+dx*5,y+dy*5)],col,1)
            else:ln(d,[(x-dx*15,y-dy*15),(x+dx*15,y+dy*15)],col,1)
      if kind=='boss':
        for c in cells:
          x,y=cen(c)
          if v==0:
            for yy in (-20,20):ln(d,[(x-24,y+yy),(x+24,y+yy)],col,2)
          elif v==1:ln(d,[(x-18+i%12,y-20),(x-18+i%12,y+20)],col,2)
          else:chevron(d,x,y-20,col);chevron(d,x,y+20,col)
      if kind in ('grunt','sword','armor'):
        # Only entry edge carries a cue; no glyph stack beneath the target.
        chevron(d,tx-20,ty,col,size=4)
    rr(d,(sx-23,sy-24,sx+23,sy+24),'#eed8b4','#43515f',2,7)
    iconcol='#ffe39a' if firing else '#f4e7cb'
    shift=0;angle=0;scale=.80
    if not clear:
      if v==0:shift=2*pulse
      elif v==1:
        if kind in ('grunt','sword'):angle=-.4+.5*pulse;shift=3*pulse
        elif kind=='armor':scale=.72+.12*pulse
        elif kind=='boss':shift=4*pulse
        else:shift=2*pulse
      else:
        rr(d,(sx-26,sy-27,sx+26,sy+27),col,None,2,8)
    icon(d,kind,sx+shift,sy,iconcol,scale,angle)
    if v==2 and kind=='sword':icon(d,'sword',sx,sy,iconcol,.65,-.55)
    if v==2 and kind=='boss':icon(d,'sword',sx+14,sy,iconcol,.55,.3)
    if v==1 and kind=='sword' and not clear:
      pts=[(sx+math.cos(a)*29,sy+math.sin(a)*29) for a in [(-.9+j*.09) for j in range(21)]];ln(d,pts,col,2)
    if v==2 and kind=='armor':
      for dx in (-18,18):
        for dy in (-17,17):rr(d,(sx+dx-1,sy+dy-1,sx+dx+1,sy+dy+1),iconcol,iconcol,1,1)
    if kind=='boss':ln(d,[(sx+26,sy-21),(sx+28,sy-17),(sx+28,sy+17),(sx+26,sy+21)],'#e2c47e',3)
    rr(d,(tx-12,ty-12,tx+12,ty+12),'#e7ecdd','#4f8d8d',1,6);txt(d,tx,ty,'你',14)
    txt(d,PW/2,399,'出手 · 边线转黄' if firing else '回到等待' if clear else '蓄势 · 范围保持可见',14,'#f0c3a7')
    return im.resize((PW,H),Image.Resampling.LANCZOS)
def overlap(i):
    im=Image.new('RGB',(PW*S,H*S),'#252e39');d=ImageDraw.Draw(im);draw_base(d,0,'sword')
    # Replace the header with this composite's explanation.
    d.rectangle((0,0,PW*S,78*S),fill='#252e39');txt(d,PW/2,23,'三名敌人 · 同一危险格',19);txt(d,PW/2,54,'底框只画一次，兵种留在来源格',12,'#b6c8d4')
    col='#ffe39a' if 30<=i<35 else '#b95840';t=(2,2);tx,ty=cen(t)
    if i<35:threat(d,t,col,0)
    for kind,c,dx,dy in [('grunt',(1,2),1,0),('sword',(2,1),0,1),('armor',(3,2),-1,0)]:
      x,y=cen(c);rr(d,(x-23,y-24,x+23,y+24),'#eed8b4','#43515f',2,7);icon(d,kind,x,y,'#f4e7cb',.8)
      if i<35:chevron(d,tx-dx*21,ty-dy*21,col,dx,dy,4)
    rr(d,(tx-12,ty-12,tx+12,ty+12),'#e7ecdd','#4f8d8d',1,6);txt(d,tx,ty,'你',14)
    if i<35:rr(d,(tx+13,ty-27,tx+26,ty-14),col,'#733f35',1,3);txt(d,tx+19,ty-20,'3',11)
    txt(d,PW/2,399,'不叠加底色亮度 / 不堆叠中心纹样',12,'#efc3ab')
    return im.resize((PW,H),Image.Resampling.LANCZOS)
def save(frames,name):
    path=OUT/(name+'.gif');frames[0].save(path,save_all=True,append_images=frames[1:],duration=80,loop=0,disposal=2,optimize=False)
    frames[9].save(OUT/(name+'.png'))
    with Image.open(path) as g:
      duration=0
      for n in range(g.n_frames):g.seek(n);duration+=g.info['duration']
      assert duration==3200 and g.info['loop']==0 and g.n_frames>=3
    return name
manifest=[]
for kind in NAMES:
    frames=[]
    for i in range(40):
      canvas=Image.new('RGB',(PW*3,H),'#252e39')
      for v in range(3):canvas.paste(frame(kind,v,i),(PW*v,0))
      frames.append(canvas)
    manifest.append(save(frames,kind+'-ABC'))
manifest.append(save([overlap(i) for i in range(40)],'overlap'))
(OUT/'manifest.json').write_text(json.dumps({'previews':manifest,'variants':LABELS,'loop_ms':3200,'status':'concept only; not integrated'},ensure_ascii=False,indent=2),encoding='utf-8')
print('Created and validated',len(manifest),'looping GIFs')
