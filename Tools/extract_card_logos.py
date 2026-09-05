"""Remove the uniform indigo matte without repainting approved card drawings.

All original PNGs remain untouched. Output alpha art and a blank-badge flat
card face are separate resources, so this treatment is reversible.
"""
from pathlib import Path
import json
import cv2
import numpy as np
from PIL import Image, ImageDraw
from extract_garden_cutouts import nearest_colours

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT/'Assets/Resources/KaitVisuals/CardLogos'
OUT = SRC/'Transparent'
QA = ROOT/'VFXScreenshots/card-polish'


def cutout(rgb, name):
    # Background has slight painted grain: take the median border colour,
    # then reject only close matches. Purple/black outlines stay foreground.
    border = np.concatenate((rgb[:8].reshape(-1,3),rgb[-8:].reshape(-1,3),
                              rgb[:,:8].reshape(-1,3),rgb[:,-8:].reshape(-1,3)))
    bg = np.median(border,axis=0)
    distance = np.linalg.norm(rgb-bg,axis=2)
    mask = (distance > 18).astype(np.uint8)
    n, labels, stats, _ = cv2.connectedComponentsWithStats(mask,8)
    keep = np.zeros(n,np.uint8)
    keep[1:] = stats[1:,cv2.CC_STAT_AREA] >= 12
    mask = keep[labels]
    if name in ('BirdEye','SweepTail','Follower'):
        # These dark illustrations reuse the matte's indigo inside feathers
        # and clothing. Protect those enclosed painted areas, not just colour.
        contours, _ = cv2.findContours(mask,cv2.RETR_EXTERNAL,cv2.CHAIN_APPROX_SIMPLE)
        cv2.drawContours(mask,contours,-1,1,cv2.FILLED)
    k = np.ones((3,3),np.uint8)
    inner = cv2.erode(mask,k)
    outer = cv2.dilate(mask,k,iterations=2)
    fg = nearest_colours(rgb,inner)
    delta = fg-bg
    alpha = np.clip(np.sum((rgb-bg)*delta,axis=2)/np.maximum(np.sum(delta*delta,axis=2),1),0,1)
    alpha[inner>0] = 1
    alpha[outer==0] = 0
    alpha[alpha<.025] = 0
    alpha[alpha>.975] = 1
    colour = (rgb-bg*(1-alpha[:,:,None]))/np.maximum(alpha[:,:,None],.025)
    colour[alpha==0] = fg[alpha==0]
    return np.dstack((np.clip(colour,0,255),alpha*255)).round().astype(np.uint8),bg


def clean_flat_face():
    source = ROOT/'Assets/Resources/KaitVisuals/EmeraldCourtyard/PassiveCardBlankFlat.png'
    im = np.asarray(Image.open(source).convert('RGBA')).copy()
    rgb = im[:,:,:3].copy()
    # Remove the baked inner crest only. Restrict to its source-pixel bounds;
    # the thin outer border and corner flourishes are intentionally preserved.
    selected = np.zeros(rgb.shape[:2],np.uint8)
    selected[40:658,55:970] = 1
    warm_line = (rgb[:,:,0].astype(int)-rgb[:,:,2].astype(int)>35) & (rgb[:,:,0]>135)
    mask = cv2.dilate((warm_line & (selected>0)).astype(np.uint8),np.ones((9,9),np.uint8))*255
    mask[selected==0] = 0
    im[:,:,:3] = cv2.inpaint(rgb,mask,10,cv2.INPAINT_TELEA)
    destination = source.with_name('PassiveCardFlatCompact.png')
    Image.fromarray(im).save(destination)
    Image.fromarray(im).resize((384,576)).save(QA/'flat-face.png')
    return str(destination)


def main():
    OUT.mkdir(exist_ok=True)
    QA.mkdir(parents=True,exist_ok=True)
    sheet = Image.new('RGB',(1440,790),'#eee0c8')
    draw = ImageDraw.Draw(sheet)
    report = {}
    for i,path in enumerate(sorted(SRC.glob('*.png'))):
        rgb = np.asarray(Image.open(path).convert('RGB'),dtype=np.float32)
        rgba,bg = cutout(rgb,path.stem)
        Image.fromarray(rgba).save(OUT/path.name)
        alpha = rgba[:,:,3]
        assert np.count_nonzero(alpha==0)>alpha.size*.2,path.name
        assert np.count_nonzero(alpha==255)>alpha.size*.1,path.name
        report[path.stem] = dict(background=bg.tolist(),transparent=int(np.count_nonzero(alpha==0)),
            opaque=int(np.count_nonzero(alpha==255)),edge=int(np.count_nonzero((alpha>0)&(alpha<255))))
        preview = Image.new('RGBA',(rgba.shape[1],rgba.shape[0]),'#eee0c8')
        preview.alpha_composite(Image.fromarray(rgba))
        preview = preview.convert('RGB').resize((225,225),Image.Resampling.LANCZOS)
        sheet.paste(preview,((i%6)*240,(i//6)*260+25))
        draw.text(((i%6)*240+5,(i//6)*260+5),path.stem,fill='#423331')
    sheet.save(QA/'transparent-logos.jpg',quality=96)
    report['flat_face'] = clean_flat_face()
    (QA/'alpha-report.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    print(json.dumps(report,indent=2))


if __name__=='__main__':
    main()
