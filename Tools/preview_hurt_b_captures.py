"""Make a QA contact sheet from unmodified Windows-player captures."""
from pathlib import Path
from PIL import Image, ImageDraw

root = Path(__file__).resolve().parents[1] / 'VFXScreenshots'
kinds = ['hurt', 'hit', 'hit-overlap', 'kill', 'chain', 'block']
sheet = Image.new('RGB', (1200, 780), '#24212a')
draw = ImageDraw.Draw(sheet)
for i, kind in enumerate(kinds):
    source = Image.open(root / f'hurt-b-{kind}.png').convert('RGB')
    crop = source.crop((300, 340, 700, 690))
    x, y = (i % 3) * 400, (i // 3) * 390
    sheet.paste(crop, (x, y + 30))
    draw.text((x + 12, y + 10), kind, fill='white')
sheet.save(root / 'hurt-b-contact.png')
