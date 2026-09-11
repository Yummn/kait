"""Extract six generated effect tiles; remove black matte without recoloring light."""
from pathlib import Path
import sys
import numpy as np
from PIL import Image
im=Image.open(sys.argv[1]).convert('RGB'); out=Path(sys.argv[2]); out.mkdir(parents=True,exist_ok=True)
names=['Punch','Flurry','Palm','Stun','Ward','Frost']
for i,name in enumerate(names):
    x=i%3; y=i//3
    # Exclude only the generated sheet's separator lines, not the artwork.
    a=np.array(im.crop((x*im.width//3+6,y*im.height//2+6,(x+1)*im.width//3-6,(y+1)*im.height//2-6))).astype(float)/255
    alpha=np.max(a,axis=2); alpha[alpha<.04]=0
    rgb=np.divide(a,alpha[:,:,None],out=np.zeros_like(a),where=alpha[:,:,None]>0)
    Image.fromarray(np.uint8(np.clip(np.dstack((rgb,alpha)),0,1)*255)).save(out/(name+'.png'))
