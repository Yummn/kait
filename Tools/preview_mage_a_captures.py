"""Contact sheet from player QA screenshots; no changes to game art."""
from pathlib import Path
from PIL import Image, ImageDraw
root = Path(__file__).resolve().parents[1] / 'VFXScreenshots'
sheet = Image.new('RGB', (1000, 1060), '#24212a')
draw = ImageDraw.Draw(sheet)
for i, kind in enumerate(['mage-aim', 'mage', 'mage-wall', 'mage-edge']):
    src = Image.open(root / f'mage-a-{kind}.png').convert('RGB')
    crop = src.crop((60, 180, 780, 900)).resize((500,500), Image.Resampling.LANCZOS)
    x, y = i % 2 * 500, i // 2 * 530
    sheet.paste(crop, (x,y+30));draw.text((x+12,y+10),kind,fill='white')
sheet.save(root/'mage-a-contact.png')
