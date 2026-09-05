"""Arrange runtime screenshot crops for review; never modifies game art."""
from pathlib import Path
from PIL import Image, ImageDraw

root = Path(__file__).resolve().parents[1] / 'VFXScreenshots'
sheet = Image.new('RGB', (980, 1000), '#303030')
draw = ImageDraw.Draw(sheet)
for index, kind in enumerate(('push', 'push-left', 'push-up', 'push-down')):
    image = Image.open(root / f'push-a-{kind}.png')
    x, y = index % 2*490, index//2*500
    sheet.paste(image.crop((210, 270, 700, 740)), (x, y+30))
    draw.text((x+8,y+8), kind, fill='white')
sheet.save(root/'push-a-directions.png')
Image.open(root/'push-a-push.png').crop((300,460,700,630)).save(root/'push-a-detail.png')
