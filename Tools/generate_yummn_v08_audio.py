"""Original deterministic synthesis for v0.8; no extracted voices/samples."""
import math, random, struct, wave
from pathlib import Path
root=Path(__file__).resolve().parents[1]/'Assets/Resources/Audio/Yummn/V08'
root.mkdir(parents=True,exist_ok=True)
for name,seconds in [('Water',.38),('Air',.23),('Ice',.35),('Frost',.45),('Fire',.32),('Shatter',.28),('Shadow',.35),('PalmSeal',.28),('Ki',.20),('Exhaust',.30),('Recover',.42)]:
    sr=44100; rng=random.Random(name); values=[]; low=0
    for i in range(round(seconds*sr)):
        t=i/sr; u=t/seconds; noise=rng.uniform(-1,1); low=.94*low+.06*noise
        env=min(t/.012,1)*(1-u)**2
        if name in ('Ice','Frost','Shatter'):
            value=sum(math.sin(2*math.pi*f*t)*math.exp(-t*(10+j*4))*.11 for j,f in enumerate((1130,1687,2493,3181)))+.13*(noise-low)
        elif name=='Water': value=.5*low+.18*math.sin(2*math.pi*(650*t+60*math.sin(t*18)*t))
        elif name in ('Air','Fire'): value=(noise-low)*(.13 if name=='Air' else .2)+.4*low
        elif name in ('Shadow','Exhaust'):value=.19*math.sin(2*math.pi*(190*t-160*t*t))+.3*low
        elif name=='PalmSeal':value=.18*math.sin(2*math.pi*220*t)+.1*math.sin(2*math.pi*660*t)+.2*low
        else:value=sum(math.sin(2*math.pi*f*t)*.12 for f in ((784,1175) if name=='Ki' else (523,659,1046)))
        values.append(value*env)
    gain=.36/max(abs(v) for v in values)
    with wave.open(str(root/(name+'.wav')),'wb') as output:
        output.setparams((1,2,sr,0,'NONE','not compressed'))
        output.writeframes(b''.join(struct.pack('<h',round(v*gain*32767)) for v in values))
    print(name,seconds)
