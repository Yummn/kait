"""User-authorized local cutout of approved generation sheets; originals stay intact."""
from pathlib import Path
from collections import deque
import json
import numpy as np
from PIL import Image, ImageFilter, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / 'Assets/Art/Build061'
OUT = ROOT / 'Assets/Resources/KaitVisuals/Build061'
QA = ROOT / 'VFXScreenshots/Build061'

def key_magenta(image):
    rgb = np.asarray(image.convert('RGB')).astype(float)
    r,g,b = rgb[:,:,0],rgb[:,:,1],rgb[:,:,2]
    background = (r>210)&(b>210)&(g<75)
    edge = np.asarray(Image.fromarray((background*255).astype('uint8')).filter(ImageFilter.MaxFilter(3)))>0
    distance = np.max(np.abs(rgb - np.array([255,0,255])),axis=2)
    alpha = np.ones(r.shape)
    alpha[background]=0
    fringe=edge & ~background & (r>g+65) & (b>g+65) & (np.abs(r-b)<85)
    alpha[fringe]=np.clip((distance[fringe]-40)/65,0,1)
    partial=(alpha>0)&(alpha<1)
    rgb[partial]=(rgb[partial]-np.array([255,0,255])*(1-alpha[partial,None]))/alpha[partial,None]
    rgb[alpha==0]=0
    return Image.fromarray(np.dstack([np.clip(rgb,0,255),alpha*255]).astype('uint8'),'RGBA')

def key_checker(image):
    rgb=np.asarray(image.convert('RGB'))
    eligible=(rgb.max(axis=2).astype(int)-rgb.min(axis=2)<35)&(rgb.min(axis=2)>145)
    # Only edge-connected checkerboard is removed. The enclosed cream face stays opaque.
    binary=Image.fromarray((eligible*255).astype('uint8'))
    padded=Image.new('L',(image.width+2,image.height+2),255);padded.paste(binary,(1,1))
    ImageDraw.floodfill(padded,(0,0),128)
    outside=np.asarray(padded)[1:-1,1:-1]==128
    rgba=np.dstack([rgb,np.where(outside,0,255).astype('uint8')]);rgba[outside]=0
    return Image.fromarray(rgba,'RGBA')

def process(name,cols,rows,count,cell_size,frame=False):
    source=Image.open(SRC/(name+'-source.png')).convert('RGB')
    keyed=key_checker(source) if frame else key_magenta(source)
    def separators(parts,length,axis):
        occupancy=(np.asarray(keyed)[:,:,3]>128).sum(axis=axis)
        cuts=[0]
        for index in range(1,parts):
            target=round(index*length/parts);lo=max(cuts[-1]+1,target-40);hi=min(length,target+41)
            candidates=np.arange(lo,hi);cost=occupancy[lo:hi]
            best=candidates[cost==cost.min()]
            cuts.append(int(best[np.argmin(np.abs(best-target))]))
        return cuts+[length]
    xs=separators(cols,source.width,0);ys=separators(rows,source.height,1)
    atlas=Image.new('RGBA',(cols*cell_size[0],rows*cell_size[1]))
    stats=[]
    for i in range(count):
        x,y=i%cols,i//cols
        cut=keyed.crop((xs[x],ys[y],xs[x+1],ys[y+1]))
        bbox=cut.getbbox()
        if bbox is None:raise RuntimeError(f'{name} {i}: empty art')
        cut=cut.crop(bbox)
        if frame:
            cut=cut.resize((cell_size[0]-4,cell_size[1]-4),Image.Resampling.LANCZOS)
        else:
            cut.thumbnail((cell_size[0]-24,cell_size[1]-24),Image.Resampling.LANCZOS)
        tile=Image.new('RGBA',cell_size);tile.alpha_composite(cut,((cell_size[0]-cut.width)//2,(cell_size[1]-cut.height)//2))
        atlas.alpha_composite(tile,(x*cell_size[0],y*cell_size[1]))
        stats.append({'cell':i,'source_bounds':bbox,'transparent_pixels':int((np.asarray(tile)[:,:,3]==0).sum())})
    atlas.save(OUT/(name+'.png'))
    background=Image.new('RGBA',atlas.size,(48,43,54,255));background.alpha_composite(atlas)
    background.convert('RGB').save(QA/(name+'-cutout-preview.png'))
    return stats

if __name__=='__main__':
    OUT.mkdir(parents=True,exist_ok=True);QA.mkdir(parents=True,exist_ok=True)
    stats={'CardFrames':process('CardFrames',3,2,6,(368,528),True)}
    for name,rows,count in [('Icons0',4,16),('Icons1',4,16),('Icons2',2,7),('Effects',4,16)]:
        stats[name]=process(name,4,rows,count,(256,256))
    (QA/'alpha-validation.json').write_text(json.dumps(stats,ensure_ascii=False,indent=2),encoding='utf-8')
    print('Prepared six faces, 39 icons and 16 VFX motifs. Originals preserved.')
