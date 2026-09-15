"""Approved background-only cutouts. Preserve RGB; do not resample illustrations."""
from pathlib import Path
import sys
sys.path.insert(0,str(Path(__file__).parent/'.pydeps'))
import cv2
import numpy as np
from PIL import Image

root=Path(__file__).resolve().parent.parent
out=root/'Assets/Resources/KaitVisuals/Storybook0915'
out.mkdir(parents=True,exist_ok=True)
for season in ('Snow','Grass'):
    # Existing bottom-right bough only. Purple field is background, not foliage.
    im=Image.open(root/f'Assets/Resources/KaitVisuals/Storybook0910/{season}Backdrop.png').convert('RGB').crop((1260,550,1672,941))
    a=np.array(im);f=a.astype(float);r,g,b=f[:,:,0],f[:,:,1],f[:,:,2]
    mask=np.full(r.shape,cv2.GC_PR_BGD,np.uint8)
    mask[(g-r>3)|((b-r>40)&(g-r>-2))]=cv2.GC_FGD
    mask[(r-g>8)&(b-g<37)]=cv2.GC_BGD
    mask[(r>180)&(g>175)&(b>110)]=cv2.GC_FGD
    mask[:6,:]=cv2.GC_BGD;mask[:,:6]=cv2.GC_BGD
    cv2.grabCut(a,mask,None,np.zeros((1,65)),np.zeros((1,65)),6,cv2.GC_INIT_WITH_MASK)
    alpha=np.uint8((mask==cv2.GC_FGD)|(mask==cv2.GC_PR_FGD))*255
    count,labels,stats,_=cv2.connectedComponentsWithStats((alpha>0).astype(np.uint8),8)
    for i in range(1,count):
        if stats[i,cv2.CC_STAT_AREA]<50:alpha[labels==i]=0
    cut=Image.fromarray(np.dstack((a,alpha)))
    cut.save(out/f'{season}Bough.png')
    proof=Image.new('RGBA',im.size,'#ddc6b2');proof.alpha_composite(cut)
    proof.convert('RGB').save(root/f'Logs/0915-{season}-bough-proof.png')
    print(season,'coverage',float(np.mean(alpha>0)))

# Generated apron may contain a printed checkerboard. Same background-only method
# as 0914; never alter the selected grass/flower RGB.
src=Path(sys.argv[1]);a=np.array(Image.open(src).convert('RGB'));f=a.astype(float)
spread=np.ptp(f,axis=2);grey=f.mean(2)
mask=np.full(grey.shape,cv2.GC_PR_FGD,np.uint8)
mask[(spread<24)&(grey>75)&(grey<236)]=cv2.GC_PR_BGD
mask[(spread<8)&(grey>85)&(grey<225)]=cv2.GC_BGD
mask[(spread>40)|(grey>244)]=cv2.GC_FGD
cv2.grabCut(a,mask,None,np.zeros((1,65)),np.zeros((1,65)),6,cv2.GC_INIT_WITH_MASK)
alpha=np.uint8((mask==cv2.GC_FGD)|(mask==cv2.GC_PR_FGD))*255
alpha[(spread<24)&(grey>75)&(grey<236)]=0
n,labels,stats,_=cv2.connectedComponentsWithStats((alpha>0).astype(np.uint8),8)
for i in range(1,n):
    if stats[i,cv2.CC_STAT_AREA]<40:alpha[labels==i]=0
rgba=Image.fromarray(np.dstack((a,alpha)));rgba.save(out/'GrassApron.png')
proof=Image.new('RGBA',rgba.size,'#ede5cf');proof.alpha_composite(rgba)
proof.convert('RGB').save(root/'Logs/0915-grass-apron-proof.png')
print('apron coverage',float(np.mean(alpha>0)),'center',int(alpha[a.shape[0]//2,a.shape[1]//2]))
