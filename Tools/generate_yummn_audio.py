"""Original short synthesized Foley/ice cues; no sampled voices or external recordings."""
import math, random, wave, struct
from pathlib import Path
root=Path(__file__).resolve().parents[1]/'Assets/Resources/Audio/Yummn'
root.mkdir(parents=True,exist_ok=True)
for name,duration in [('Punch',.20),('Kill',.30),('Move',.16),('Frost',.60),('Block',.22),('Ready',.10),('Skill',.35)]:
    rng=random.Random(name);sr=44100;samples=[];low=0
    for i in range(int(duration*sr)):
        t=i/sr;u=t/duration;noise=rng.uniform(-1,1);low=.84*low+.16*noise
        env=min(t/.006,1)*(1-u)**2
        if name in ('Punch','Kill'):v=.65*math.sin(2*math.pi*(130*t-120*t*t))*(1-u)**3+.4*low+.12*noise*math.exp(-t*65)
        elif name=='Move':v=(noise-low)*.3*math.sin(math.pi*u)**2
        elif name=='Frost':v=.3*low+.12*(noise-low)+sum(math.sin(2*math.pi*f*t)*.07 for f in [785,1174,1568])*math.sin(math.pi*u)
        elif name=='Block':v=sum(math.sin(2*math.pi*f*t)*.14*math.exp(-t*f/130) for f in [720,1117,2073])+.15*low
        else:v=.22*math.sin(2*math.pi*(659 if name=='Ready' else 523)*t)+.10*math.sin(2*math.pi*1046*t)
        samples.append(max(-.8,min(.8,v*env)))
    peak=max(abs(x) for x in samples);gain=.65/peak
    with wave.open(str(root/(name+'.wav')),'wb') as f:
        f.setparams((1,2,sr,0,'NONE','not compressed'));f.writeframes(b''.join(struct.pack('<h',int(v*gain*32767)) for v in samples))
    print(name,len(samples))
