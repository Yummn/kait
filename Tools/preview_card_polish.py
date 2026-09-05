"""Present unscaled crops of the actual player captures for visual review."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

root = Path(__file__).resolve().parents[1]/'VFXScreenshots'
font = ImageFont.truetype('C:/Windows/Fonts/msyh.ttc',18)
sheet = Image.new('RGB',(750,848),'#29262f')
draw = ImageDraw.Draw(sheet)
rows = [
    ('主动技能：透明插画 / 简约圆角框','card-polish-active.png',(636,322,1360,597),12),
    ('被动技能：去掉大装饰框，改为紧凑圆角框','card-polish-passive.png',(636,322,1360,597),330),
    ('吸附位置上移 24px：冷却状态完整露出','card-polish-cooldown.png',(641,920,1283,1080),648),
]
for label,filename,bounds,y in rows:
    draw.text((14,y),label,font=font,fill='#f3e4d5')
    crop = Image.open(root/filename).convert('RGB').crop(bounds)
    sheet.paste(crop,((750-crop.width)//2,y+28))
sheet.save(root/'card-polish-preview.png')
