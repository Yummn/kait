"""Background-only local matte approved by the user. Preserve source RGB."""
from pathlib import Path
import sys
sys.path.insert(0,str(Path(__file__).parent/'.pydeps'))
import cv2
import numpy as np
from PIL import Image

root=Path(__file__).parent.parent
source=Path('C:/Users/yummn/.codex/generated_images/01a09eba-3eb2-7a91-b165-d9e5436c0808')
out=root/'Assets/Resources/KaitVisuals/Storybook0910'
out.mkdir(parents=True,exist_ok=True)
jobs=[('exec-3e62fb51-488e-4fa9-958d-4f0313c8d0c4.png',['YummnPortrait','KaitPortrait','Fist','Heart','Qi','Gear'],.557),
      ('exec-d87d758b-20b7-4516-89a9-0bcbf47007d4.png',['IceA','IceB','IceWall','GrassA','GrassB','GrassWall'],.5)]
proof=Image.new('RGB',(1200,800),'#484053')
for job,(filename,names,split) in enumerate(jobs):
    image=Image.open(source/filename).convert('RGB');w,h=image.size
    for i,name in enumerate(names):
        col=i%3; row=i//3
        a=np.array(image.crop((col*w//3,0 if row==0 else int(h*split),(col+1)*w//3,int(h*split) if row==0 else h)))
        f=a.astype(float); neutral=(np.ptp(f,axis=2)<23)&(f.mean(2)>108)
        # Remove only neutral checker pixels connected to the crop boundary.
        n,labels,stats,_=cv2.connectedComponentsWithStats(neutral.astype(np.uint8),8)
        exterior=np.unique(np.concatenate((labels[0],labels[-1],labels[:,0],labels[:,-1])))
        exterior=exterior[exterior!=0]
        alpha=np.uint8(~np.isin(labels,exterior))*255
        n,parts,stats,_=cv2.connectedComponentsWithStats((alpha>0).astype(np.uint8),8)
        for p in range(1,n):
            if stats[p,cv2.CC_STAT_AREA]<120: alpha[parts==p]=0
        rgba=Image.fromarray(np.dstack((a,alpha)))
        box=rgba.getbbox()
        if not box: raise ValueError(name)
        rgba=rgba.crop((max(0,box[0]-3),max(0,box[1]-3),min(a.shape[1],box[2]+3),min(a.shape[0],box[3]+3)))
        rgba.save(out/(name+'.png'))
        thumb=rgba.copy();thumb.thumbnail((185,370))
        proof.paste(thumb,(i*200+(200-thumb.width)//2,job*400+(400-thumb.height)//2),thumb)
        print(name,rgba.size,'alpha coverage',float(np.mean(alpha>0)))
proof.save(root/'Logs/storybook0910-matte-proof.png')
