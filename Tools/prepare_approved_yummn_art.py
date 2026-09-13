"""Approved A illustrations: crop card furniture, remove background, preserve painted RGB."""
from pathlib import Path
import sys,json
sys.path.insert(0,str(Path(__file__).parent/'.pydeps'))
import numpy as np
import cv2
from PIL import Image,ImageDraw
root=Path(__file__).resolve().parents[1]
manifest=json.loads((root/'Tools/approved_yummn_a_manifest.json').read_text(encoding='utf-8'))
proof=root/'VFXScreenshots/ApprovedA';proof.mkdir(parents=True,exist_ok=True)
mode=sys.argv[1] if len(sys.argv)>1 else 'contact'
cards=manifest['cards']
if mode=='matte':
    out=root/'Assets/Resources/KaitVisuals/Yummn/ApprovedA';out.mkdir(parents=True,exist_ok=True)
    cv2.setNumThreads(4)
    for card in cards:
        if len(sys.argv)>2 and card['id']!=sys.argv[2]:continue
        im=Image.open(Path(manifest['sourceRoot'])/card['file']).convert('RGBA')
        if card['number']<=10:
            w,h=im.size;top=.264 if card['id']=='R02' else .255 if card['id']=='E03' else .215
            im=im.crop((int(w*.105),int(h*top),int(w*.902),int(h*.715)))
        im.thumbnail((900,900));rgba=np.array(im);a=rgba[:,:,:3].copy()
        if rgba[:,:,3].min()<64 and card['id']!='R28':
            alpha=rgba[:,:,3]
        else:
            rgb=a.astype(float);grey=rgb.mean(2);spread=np.ptp(rgb,axis=2)
            cream=card['number']<=10
            if cream:
                bg=(rgb[:,:,0]>240)&(rgb[:,:,1]>223)&(rgb[:,:,2]>188)&((rgb[:,:,0]-rgb[:,:,2])>8)&(spread<68)
            elif card['id']=='R28':
                bg=((spread<30)&(grey>80))|(rgba[:,:,3]<64)
            elif np.mean(a[:8])>240:
                bg=(rgb.min(2)>242)&(spread<12)
            else:
                bg=(spread<23)&(grey>110)
            count,labels,stats,_=cv2.connectedComponentsWithStats(bg.astype(np.uint8),8)
            border=np.unique(np.concatenate((labels[0],labels[-1],labels[:,0],labels[:,-1])))
            connected=np.isin(labels,border[border!=0])
            # Closed pale cloth/skin regions are not background merely because
            # their colours resemble the paper/checker. Protect these interiors.
            mask=np.full(grey.shape,cv2.GC_FGD,np.uint8)
            mask[connected]=cv2.GC_BGD
            local_mean=cv2.blur(grey,(3,3));local_std=np.sqrt(np.maximum(0,cv2.blur(grey*grey,(3,3))-local_mean*local_mean))
            for component in range(1,count):
                region=labels==component
                interior=cv2.erode(region.astype(np.uint8),np.ones((3,3),np.uint8))>0
                if not cream and interior.any() and local_std[interior].mean()>6 and np.mean(a[:8])<240:
                    mask[region]=cv2.GC_BGD
            mask[:2]=mask[-2:]=cv2.GC_BGD;mask[:,:2]=mask[:,-2:]=cv2.GC_BGD
            edge=cv2.dilate((mask==cv2.GC_BGD).astype(np.uint8),np.ones((3,3),np.uint8))>0
            mask[edge&(mask!=cv2.GC_BGD)]=cv2.GC_PR_FGD
            cv2.grabCut(a,mask,None,np.zeros((1,65)),np.zeros((1,65)),3,cv2.GC_INIT_WITH_MASK)
            alpha=np.uint8((mask==cv2.GC_FGD)|(mask==cv2.GC_PR_FGD))*255
            count,labels,stats,_=cv2.connectedComponentsWithStats((alpha>0).astype(np.uint8),8)
            for i in range(1,count):
                if stats[i,cv2.CC_STAT_AREA]<(500 if cream else 50):alpha[labels==i]=0
        result=Image.fromarray(np.dstack((a,alpha)))
        bounds=result.getbbox()
        if not bounds:raise RuntimeError(card['id']+' has empty alpha')
        result=result.crop(bounds);result.thumbnail((472,472),Image.Resampling.LANCZOS)
        icon=Image.new('RGBA',(512,512));icon.alpha_composite(result,((512-result.width)//2,(512-result.height)//2))
        icon.save(out/f"{card['id']}.png")
        print(card['id'],flush=True)
    mode='processed'
for group in range(6):
    sheet=Image.new('RGB',(1000,600),'#39303e');draw=ImageDraw.Draw(sheet)
    for index,card in enumerate(cards[group*10:group*10+10]):
        path=Path(manifest['sourceRoot'])/card['file']
        if mode=='processed':path=root/'Assets/Resources/KaitVisuals/Yummn/ApprovedA'/f"{card['id']}.png"
        im=Image.open(path).convert('RGBA');im.thumbnail((185,260))
        x=(index%5)*200+(200-im.width)//2;y=(index//5)*300+25
        sheet.paste(im,(x,y),im);draw.text(((index%5)*200+12,(index//5)*300+5),f"{card['number']:02} {card['id']}",fill='white')
    sheet.save(proof/f'{mode}_{group+1}.jpg')
