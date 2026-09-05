"""Review the landing dust at the feet of all six enemy types."""
from pathlib import Path
from PIL import Image, ImageDraw
root = Path(__file__).resolve().parents[1] / 'VFXScreenshots'
sheet = Image.new('RGB', (1080, 640), '#24212a')
draw = ImageDraw.Draw(sheet)
kinds = ['landing', 'landing-sword', 'landing-archer', 'landing-guard', 'landing-mage', 'landing-boss']
for i, kind in enumerate(kinds):
    crop = Image.open(root / f'landing-a-{kind}.png').convert('RGB').crop((390, 430, 750, 720))
    x, y = i % 3 * 360, i // 3 * 320
    sheet.paste(crop, (x,y+30));draw.text((x+12,y+10),kind,fill='white')
sheet.save(root/'landing-a-contact.png')
