"""Local, reproducible matte extraction of the approved generated garden art.

Original RGB sources are never modified. Flower petals are protected by the
enclosing outline; canopy/trunk holes are deliberately retained.
"""
from pathlib import Path
import json
import cv2
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
SOURCE = Path('C:/Users/yummn/.codex/generated_images/01a04dd3-06e5-7563-be9d-9815a9c77a36')
DEST = ROOT / 'Assets/Resources/KaitVisuals/EmeraldCourtyard'
QA = ROOT / 'VFXScreenshots/garden-cutouts'
ART = {
    'TreeTrunk': 'exec-ea6f1a33-b995-4468-9aeb-609a50255ef4.png',
    'TreeCanopy': 'exec-aa27874b-aafe-4f4d-b3af-56357e6e8f76.png',
    'FlowerClump': 'exec-319d979e-02b5-4a18-b5d4-c955ec183147.png',
}


def nearest_colours(rgb, seeds):
    _, labels = cv2.distanceTransformWithLabels(
        (1-seeds).astype(np.uint8), cv2.DIST_L2, 5, labelType=cv2.DIST_LABEL_PIXEL)
    table = np.zeros((int(labels.max())+1, 3), np.float32)
    table[labels[seeds > 0]] = rgb[seeds > 0]
    return table[labels]


def extract(rgb, name):
    # The painted checks are bright and neutral, unlike the dark outline and
    # coloured paint. Keep only substantial illustration components.
    spread = np.ptp(rgb, axis=2)
    mask = ((spread > 22) | (rgb.min(axis=2) < 170)).astype(np.uint8)
    count, labels, stats, _ = cv2.connectedComponentsWithStats(mask, 8)
    keep = np.zeros(count, np.uint8)
    keep[1:] = (stats[1:, cv2.CC_STAT_AREA] >= 35)
    mask = keep[labels]
    if name == 'FlowerClump':
        # Ivory petals can resemble the checks, but are enclosed by a continuous
        # green outline. This specific clump has no interior background holes.
        contours, _ = cv2.findContours(mask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
        cv2.drawContours(mask, contours, -1, 1, cv2.FILLED)
    kernel = np.ones((3,3), np.uint8)
    inner = cv2.erode(mask, kernel, iterations=1)
    outer = cv2.dilate(mask, kernel, iterations=2)
    foreground = nearest_colours(rgb, inner)
    background = nearest_colours(rgb, 1-outer)
    delta = foreground-background
    alpha = np.clip(np.sum((rgb-background)*delta, axis=2) /
                    np.maximum(np.sum(delta*delta, axis=2), 1), 0, 1)
    alpha[inner > 0] = 1
    alpha[outer == 0] = 0
    alpha[alpha < .025] = 0
    alpha[alpha > .975] = 1
    # Unmatte the painted white/grey edge instead of retaining a pale halo.
    colour = (rgb-background*(1-alpha[:,:,None])) / np.maximum(alpha[:,:,None], .025)
    colour[alpha == 0] = foreground[alpha == 0]
    return np.dstack((np.clip(colour,0,255), alpha*255)).round().astype(np.uint8)


def main():
    QA.mkdir(parents=True, exist_ok=True)
    DEST.mkdir(parents=True, exist_ok=True)
    report = {}
    montage = Image.new('RGB', (1440, 940), '#252932')
    draw = ImageDraw.Draw(montage)
    for i, (name, filename) in enumerate(ART.items()):
        rgb = np.asarray(Image.open(SOURCE/filename).convert('RGB'), dtype=np.float32)
        rgba = extract(rgb, name)
        output = Image.fromarray(rgba, 'RGBA')
        output.save(DEST/(name+'.png'))
        alpha = rgba[:,:,3]
        assert np.all(alpha[:5] == 0) and np.all(alpha[-5:] == 0), name
        assert np.count_nonzero(alpha == 0) > alpha.size * .20
        assert np.count_nonzero(alpha == 255) > alpha.size * .10
        if name == 'FlowerClump':
            for x,y in [(645,290),(548,343),(357,532),(861,555)]:
                assert alpha[y,x] == 255, 'Petal lost'
        report[name] = dict(source=str(SOURCE/filename), output=str(DEST/(name+'.png')),
            size=[output.width,output.height], transparent=int(np.count_nonzero(alpha==0)),
            opaque=int(np.count_nonzero(alpha==255)), antialiased=int(np.count_nonzero((alpha>0)&(alpha<255))))
        for row, bg in enumerate(['#253037','#78A566']):
            preview = Image.new('RGBA', output.size, bg)
            preview.alpha_composite(output)
            preview.convert('RGB').save(QA/(name+('-dark.png' if row==0 else '-grass.png')))
            preview.thumbnail((460,425), Image.Resampling.LANCZOS)
            montage.paste(preview.convert('RGB'), (i*480+(480-preview.width)//2, row*470+30))
            draw.text((i*480+15,row*470+8),name,fill='white')
    montage.save(QA/'overview.jpg',quality=95)
    (QA/'alpha-report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps(report,ensure_ascii=False,indent=2))


if __name__ == '__main__':
    main()
