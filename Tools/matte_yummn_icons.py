"""Local background segmentation. Preserve original RGB; only write alpha."""
from pathlib import Path
import sys
sys.path.insert(0,str(Path(__file__).parent/'.pydeps'))
import cv2
import numpy as np
from PIL import Image

src=Image.open(sys.argv[1]).convert('RGB'); a=np.asarray(src).copy(); rgb=a.astype(float)
grey=rgb.mean(2);spread=np.ptp(rgb,axis=2)
mask=np.full(grey.shape,cv2.GC_PR_FGD,np.uint8)
neutral=(spread<22)&(grey>105)&(grey<240)
mask[neutral]=cv2.GC_PR_BGD
# Grey check interiors are background seeds; do not key darker grey cloth.
check=(spread<4)&(((grey>147)&(grey<154))|((grey>213)&(grey<220)))
mean=cv2.blur(grey,(9,9));std=np.sqrt(np.maximum(0,cv2.blur(grey*grey,(9,9))-mean*mean))
neutral_coverage=cv2.blur((spread<9).astype(float),(9,9))
core=check | ((spread<16)&(grey>125)&(grey<236)&(std>20)&(neutral_coverage>.75))
mask[core]=cv2.GC_BGD
mask[(spread>55)|(grey<62)|(grey>247)]=cv2.GC_FGD
mask[:4]=mask[-4:]=cv2.GC_BGD;mask[:,:4]=mask[:,-4:]=cv2.GC_BGD
cv2.grabCut(a,mask,None,np.zeros((1,65),np.float64),np.zeros((1,65),np.float64),8,cv2.GC_INIT_WITH_MASK)
alpha=np.uint8((mask==cv2.GC_FGD)|(mask==cv2.GC_PR_FGD))*255
# Drop only tiny isolated checker remnants, retain all substantial icon pieces.
count,labels,stats,_=cv2.connectedComponentsWithStats((alpha>0).astype(np.uint8),8)
for i in range(1,count):
    if stats[i,cv2.CC_STAT_AREA]<22:alpha[labels==i]=0
Image.fromarray(np.dstack((a,alpha))).save(sys.argv[2])
proof=Image.new('RGBA',src.size,'#37303e');proof.alpha_composite(Image.fromarray(np.dstack((a,alpha))));proof.convert('RGB').save('VFXScreenshots/yummn-icons-proof.jpg')
