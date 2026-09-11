"""Read-only source-bank audition. Tiny ASR is a screening aid, not verified translation."""
from pathlib import Path
import json
import math
import numpy as np
import soundfile as sf
import whisper

def resample(data,sr,target):
    positions=np.arange(round(len(data)*target/sr))*sr/target
    if data.ndim==1:return np.interp(positions,np.arange(len(data)),data)
    return np.stack([np.interp(positions,np.arange(len(data)),data[:,i]) for i in range(data.shape[1])],axis=1)

ROOT=Path('C:/Users/yummn/Desktop/应用/Spine/素材/语音')
OUT=Path('C:/Users/yummn/Downloads/kait/AudioPreviews/YummnVoice-20260910')
OUT.mkdir(parents=True,exist_ok=True)
names=['Xiao','Coonya','Tsubaki','Momiji','Mashiro','Shy']
suffixes=[*(f'Battle_N_{i}' for i in range(1,7)),'Battle_H_1','Battle_H_2','Go_1','Battle_Hit_1','Win_1','Battle_Die_1']
paths={p.name:p for p in ROOT.rglob('*.wav')}
model=whisper.load_model('tiny',device='cuda')
allrows=[]
for name in names:
    rows=[]
    for suffix in suffixes:
        path=paths.get(f'{name}_{suffix}.wav')
        if path is None: continue
        data,sr=sf.read(path,dtype='float32',always_2d=True)
        mono=data.mean(axis=1)
        g=math.gcd(sr,16000)
        samples=resample(mono,sr,16000).astype(np.float32)
        result=model.transcribe(samples,language='ja',temperature=0,condition_on_previous_text=False,verbose=False)
        row={'name':name,'event':suffix,'path':path.as_posix(),'seconds':round(len(data)/sr,2),'asr_unverified':result['text'].strip()}
        rows.append(row);allrows.append(row)
    # Four representative clips in unchanged order, with silence separators.
    # No gain, pitch, speed, or source-file changes; PCM sample-rate conversion only.
    picks=[]
    for key in ['Battle_N_2','Battle_N_3','Battle_H_1','Go_1']:
        row=next((r for r in rows if r['event']==key),None)
        if row:
            data,sr=sf.read(row['path'],dtype='float32',always_2d=True)
            if data.shape[1]==1: data=np.repeat(data,2,axis=1)
            if sr!=48000:
                data=resample(data,sr,48000)
            picks.extend([data,np.zeros((int(.7*48000),2),dtype=np.float32)])
    if picks: sf.write(OUT/f'{name}-audition.wav',np.concatenate(picks),48000,subtype='PCM_16')
    print(json.dumps({'name':name,'clips':rows},ensure_ascii=False),flush=True)
(OUT/'screening.json').write_text(json.dumps(allrows,ensure_ascii=False,indent=2),encoding='utf-8')
