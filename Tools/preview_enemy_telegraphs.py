"""Code-native grid telegraph concepts; previews only, no game assets changed."""
from pathlib import Path
import math, json
from PIL import Image, ImageDraw, ImageFont

OUT=Path(__file__).resolve().parents[1]/'VFXPreviews/EnemyTelegraphs-20260910'
OUT.mkdir(parents=True,exist_ok=True)
S=2; W,H=560,536; CELL=78; LEFT,TOP=85,76
FONT='C:/Windows/Fonts/msyh.ttc'
def font(n):return ImageFont.truetype(FONT,n*S)
def box(c):
    x,y=c;return (LEFT+x*CELL+2,TOP+y*CELL+2,LEFT+(x+1)*CELL-2,TOP+(y+1)*CELL-2)
def center(c):
    b=box(c);return ((b[0]+b[2])/2,(b[1]+b[3])/2)
def line(d,pts,color,width=2):d.line([(int(x*S),int(y*S)) for x,y in pts],fill=color,width=width*S,joint='curve')
def rect(d,b,color,width=2,radius=7,fill=None):d.rounded_rectangle(tuple(int(v*S) for v in b),radius*S,fill=fill,outline=color,width=width*S)
def text(d,xy,t,size=16,color='#f2e9db',anchor='mm'):d.text((xy[0]*S,xy[1]*S),t,font=font(size),fill=color,anchor=anchor)
KINDS=[('grunt','4 · 杂兵','短促齿纹 · 相邻单格'),('sword','8 · 剑士','收拢刃线 · 相邻单格'),('archer','16 · 弓手','流动半箭头 · 直线瞄准'),('armor','32 · 重甲兵','重型角扣 · 相邻单格'),('mage','64 · 术士','中心锁点 · 十字范围'),('boss','128 · 盾骑士','正面盾沿 · 前方整排（Yummn）')]
def render(kind,title,subtitle,i):
    im=Image.new('RGB',(W*S,H*S),'#252b35');d=ImageDraw.Draw(im)
    text(d,(W/2,25),title,23);text(d,(W/2,53),subtitle,14,'#b8c9d7')
    for y in range(5):
      for x in range(5):
        b=box((x,y));rect(d,b,'#8096a7',1,5,'#b9c8cf' if (x+y)%2 else '#b3c3cc')
        # Flat top-down tile details, no photographic textures or fake perspective.
        line(d,[(b[0]+7,b[1]+3),(b[2]-7,b[1]+3)],'#d5dfe1',1)
        line(d,[(b[0]+8,b[3]-4),(b[2]-8,b[3]-4)],'#9cadbb',1)
    source=(1,2);target=(2,2);cells=[target]
    if kind in ('archer','boss'):source=(0,2);target=(4,2);cells=[(x,2) for x in range(1,5)]
    if kind=='mage':source=(0,4);cells=[(2,2),(1,2),(3,2),(2,1),(2,3)]
    firing=36<=i<41;clear=i>=41
    pulse=(math.sin(i/36*math.pi*4)+1)/2
    color='#ffe29a' if firing else ('#cb654f' if pulse>.5 else '#a14f45')
    if not clear:
      overlay=Image.new('RGBA',im.size);o=ImageDraw.Draw(overlay)
      for c in cells:
        b=box(c);x,y=center(c)
        fill=(166,67,49,int(25+18*pulse)) if not firing else (237,169,60,24)
        rect(o,b,color,3 if firing else 2,6,fill)
        if kind=='grunt':
          for yy in (-17,0,17):line(o,[(x-15,y+yy-7),(x-7,y+yy),(x-15,y+yy+7)],color,2)
        elif kind=='sword':
          off=11-5*pulse
          line(o,[(x-20,y-24),(x-off,y),(x-20,y+24)],color,2)
          line(o,[(x+17,y-24),(x+off,y),(x+17,y+24)],color,2)
        elif kind=='armor':
          for dx,dy in [(-1,-1),(-1,1),(1,-1),(1,1)]:
            xx=x+dx*26;yy=y+dy*26
            line(o,[(xx-dx*11,yy),(xx,yy),(xx,yy-dy*11)],color,4)
          line(o,[(x-12,y-15),(x+1,y),(x-12,y+15)],color,3)
        elif kind=='archer':
          line(o,[(b[0]+6,y-15),(b[2]-6,y-15)],color,1)
          line(o,[(b[0]+6,y+15),(b[2]-6,y+15)],color,1)
          for shift in (-20,10):
            xx=x+shift+(i%12)/12*12
            line(o,[(xx-7,y-8),(xx,y),(xx-7,y+8)],color,2)
        elif kind=='mage':
          if c==(2,2):
            size=13+3*pulse
            line(o,[(x,y-size),(x+size,y),(x,y+size),(x-size,y),(x,y-size)],color,2)
            for dx,dy in [(1,0),(-1,0),(0,1),(0,-1)]:line(o,[(x+dx*21,y+dy*21),(x+dx*29,y+dy*29)],color,2)
          else:
            cx,cy=center((2,2));dx=0 if cx==x else (1 if cx>x else -1);dy=0 if cy==y else (1 if cy>y else -1)
            q=(x+dx*8,y+dy*8);line(o,[(q[0]-dx*9-dy*7,q[1]-dy*9+dx*7),q,(q[0]-dx*9+dy*7,q[1]-dy*9-dx*7)],color,2)
        else:
          for yy in (-22,22):line(o,[(b[0]+7,y+yy),(b[2]-7,y+yy)],color,3)
          line(o,[(x-10,y-12),(x+1,y),(x-10,y+12)],color,3)
      im=Image.alpha_composite(im.convert('RGBA'),overlay).convert('RGB');d=ImageDraw.Draw(im)
    # Neutral source/target tokens distinguish geometry without inventing character art.
    for c,label,fill in [(source,'敌','#4f5d72'),(target,'你','#5b8e91')]:
      x,y=center(c);rect(d,(x-16,y-16,x+16,y+16),'#f4ead7',2,9,fill);text(d,(x,y),label,17)
    if kind=='boss':
      x,y=center(source);line(d,[(x+25,y-26),(x+30,y-20),(x+30,y+20),(x+25,y+26)],'#efce82',4)
    label='出手：警示线转黄' if firing else '结束 / 循环预览' if clear else '蓄势：攻击范围保持可见'
    text(d,(W/2,487),label,16,'#ffe29a' if firing else '#efc3af')
    text(d,(W/2,516),'方案预览 · 动画时长不代表回合规则',12,'#a9b8c4')
    return im.resize((W,H),Image.Resampling.LANCZOS)

manifest=[]
for kind,title,subtitle in KINDS:
    frames=[render(kind,title,subtitle,i) for i in range(48)]
    dest=OUT/(kind+'.gif')
    frames[0].save(dest,save_all=True,append_images=frames[1:],duration=70,loop=0,optimize=False,disposal=2)
    frames[15].save(OUT/(kind+'-still.png'))
    with Image.open(dest) as g:
      count=g.n_frames; duration=0
      for frame in range(count):g.seek(frame);duration+=g.info['duration']
      assert count>=5 and g.info['loop']==0 and duration==48*70
      manifest.append({'file':dest.name,'title':title,'frames':count,'total_duration_ms':duration,'size':list(g.size)})
(OUT/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(manifest,ensure_ascii=False))
