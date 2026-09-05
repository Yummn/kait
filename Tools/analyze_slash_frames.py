from pathlib import Path
from PIL import Image, ImageDraw
import numpy as np

root = Path(__file__).resolve().parents[1]
for variant in ('Normal', 'Finisher'):
    image = Image.open(root / f'Assets/Resources/KaitVisuals/Effects/WhiteGoldSlash{variant}.png').convert('RGB')
    print(variant)
    for frame in range(8):
        x, y = frame % 4, frame // 4
        crop = image.crop((round(x*image.width/4), round(y*image.height/2), round((x+1)*image.width/4), round((y+1)*image.height/2)))
        weights = np.maximum(np.asarray(crop).min(axis=2).astype(float)-160, 0)
        yy, xx = np.indices(weights.shape)
        if weights.sum() == 0:
            print(frame, 'no bright core')
        else:
            print(frame, round(float((xx*weights).sum()/weights.sum()/crop.width-.5), 4), round(float(.5-(yy*weights).sum()/weights.sum()/crop.height), 4))

for kind in ('slash', 'slash-left', 'slash-kill', 'slash-kill-left'):
    sheet = Image.new('RGB', (1230, 750), '#303030')
    draw = ImageDraw.Draw(sheet)
    for frame in range(9):
        image = Image.open(root / f'VFXScreenshots/{kind}-{frame}.png')
        crop = image.crop((240, 410, 650, 640))
        x, y = frame % 3*410, frame//3*250
        sheet.paste(crop, (x, y+20))
        draw.text((x+5,y+3), f'{kind} / {frame}', fill='white')
    sheet.save(root / f'VFXScreenshots/{kind}-contact.png')

# Cropped evidence only; this does not alter any game art.
comparison = Image.new('RGB', (820, 260), '#303030')
draw = ImageDraw.Draw(comparison)
for column, (name, label) in enumerate((('slash-2', 'Normal attack'), ('slash-kill-0', 'Finisher'))):
    shot = Image.open(root / f'VFXScreenshots/{name}.png')
    comparison.paste(shot.crop((240, 410, 650, 640)), (column*410, 30))
    draw.text((column*410+10, 8), label, fill='white')
comparison.save(root / 'VFXScreenshots/slash-comparison.png')
