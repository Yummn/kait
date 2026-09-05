"""QA crops only; leaves the generated game asset unchanged."""
from pathlib import Path
from PIL import Image, ImageDraw

root = Path(__file__).resolve().parents[1] / 'VFXScreenshots'
kinds = ['arrow', 'arrow-left', 'arrow-up', 'arrow-down', 'arrow-kait', 'hit']
sheet = Image.new('RGB', (1200, 620), '#24212a')
draw = ImageDraw.Draw(sheet)
for i, kind in enumerate(kinds):
    crop = Image.open(root / f'arrow-a-{kind}.png').convert('RGB').crop((300, 420, 700, 700))
    x, y = i % 3 * 400, i // 3 * 310
    sheet.paste(crop, (x, y + 30))
    draw.text((x + 12, y + 10), kind, fill='white')
sheet.save(root / 'arrow-a-contact.png')
