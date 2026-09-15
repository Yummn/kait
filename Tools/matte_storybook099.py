"""User-approved background-only matte, with source RGB preserved."""
from pathlib import Path
import sys
sys.path.insert(0,str(Path(__file__).parent/'.pydeps'))
import cv2
import numpy as np
from PIL import Image
src=Path(sys.argv[1]); out=Path(sys.argv[2]); out.parent.mkdir(parents=True,exist_ok=True)
a=np.array(Image.open(src).convert('RGB')); f=a.astype(float)
grey=f.mean(2); spread=np.ptp(f,axis=2)
mask=np.full(grey.shape,cv2.GC_PR_FGD,np.uint8)
neutral=(spread<12)&(grey>110)&(grey<220)
mask[neutral]=cv2.GC_PR_BGD
mask[(spread<5)&(((grey>137)&(grey<145))|((grey>190)&(grey<201)))]=cv2.GC_BGD
mask[(spread>45)|(grey<65)|(grey>243)]=cv2.GC_FGD
mask[:5]=mask[-5:]=cv2.GC_BGD;mask[:,:5]=mask[:,-5:]=cv2.GC_BGD
cv2.grabCut(a,mask,None,np.zeros((1,65)),np.zeros((1,65)),8,cv2.GC_INIT_WITH_MASK)
alpha=np.uint8((mask==cv2.GC_FGD)|(mask==cv2.GC_PR_FGD))*255
n,labels,stats,_=cv2.connectedComponentsWithStats((alpha>0).astype(np.uint8),8)
for i in range(1,n):
    if stats[i,cv2.CC_STAT_AREA]<60: alpha[labels==i]=0
rgba=Image.fromarray(np.dstack((a,alpha)));rgba.save(out)
proof=Image.new('RGBA',rgba.size,'#484053');proof.alpha_composite(rgba)
proof.convert('RGB').save(Path(__file__).parent.parent/'Logs/storybook099-matte-proof.jpg')
print('Alpha coverage:',float(np.mean(alpha>0)))
