"""Approved local background-only matte; source RGB and source file preserved."""
from pathlib import Path
import sys
sys.path.insert(0,str(Path(__file__).parent/'.pydeps'))
import cv2
import numpy as np
from PIL import Image
src=Path(sys.argv[1]);out=Path(sys.argv[2]);out.parent.mkdir(parents=True,exist_ok=True)
a=np.array(Image.open(src).convert('RGB'));f=a.astype(float)
spread=np.ptp(f,axis=2);grey=f.mean(2)
mask=np.full(grey.shape,cv2.GC_PR_FGD,np.uint8)
mask[(spread<24)&(grey>75)&(grey<236)]=cv2.GC_PR_BGD
mask[(spread<8)&(grey>85)&(grey<225)]=cv2.GC_BGD
mask[(spread>40)|(grey>244)]=cv2.GC_FGD
cv2.grabCut(a,mask,None,np.zeros((1,65)),np.zeros((1,65)),6,cv2.GC_INIT_WITH_MASK)
alpha=np.uint8((mask==cv2.GC_FGD)|(mask==cv2.GC_PR_FGD))*255
n,labels,stats,_=cv2.connectedComponentsWithStats((alpha>0).astype(np.uint8),8)
for i in range(1,n):
    if stats[i,cv2.CC_STAT_AREA]<40:alpha[labels==i]=0
rgba=Image.fromarray(np.dstack((a,alpha)));rgba.save(out)
proof=Image.new('RGBA',rgba.size,'#7997ae');proof.alpha_composite(rgba)
proof.convert('RGB').save(Path(__file__).parent.parent/'Logs/storybook0914-matte-proof.png')
print('alpha coverage',float(np.mean(alpha>0)),'center alpha',alpha[a.shape[0]//2,a.shape[1]//2])
