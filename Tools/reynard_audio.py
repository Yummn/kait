from pathlib import Path
import wave,array,math,json
root=Path('Assets/Resources/Audio');out=root/'Reynard';out.mkdir(exist_ok=True)
def read(p):
 with wave.open(str(root/p),'rb') as w:
  assert w.getsampwidth()==2
  a=array.array('h',w.readframes(w.getnframes()));n=w.getnchannels();return [sum(a[i:i+n])/n/32768 for i in range(0,len(a),n)],w.getframerate()
def mix(name,layers,duration):
 hz=48000;b=[0.0]*int(hz*duration)
 for path,gain,pitch,delay in layers:
  a,sr=read(path)
  for i in range(int(delay*hz),len(b)):
   pos=(i/hz-delay)*sr*pitch;j=int(pos)
   if j>=len(a)-1:break
   b[i]+=(a[j]*(1-pos+j)+a[j+1]*(pos-j))*gain
 peak=max(abs(x) for x in b) or 1
 data=array.array('h',(int(max(-1,min(1,x/peak*.63))*32767*min(1,i/240,(len(b)-1-i)/1440)) for i,x in enumerate(b)))
 with wave.open(str(out/(name+'.wav')),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(hz);w.writeframes(data.tobytes())
 return {'name':name,'layers':layers,'duration':duration,'method':'Offline layering and resampling of existing approved generated game sounds; not a new model generation.'}
fire='Yummn/SelectedModel/Fire.wav';water='Yummn/SelectedModel/Water.wav';cast='UI/SelectedModel/Cast_B.wav';impact='Ranged/SelectedModel/MagicImpact_B2.wav'
rows=[mix('Foxfire',[(fire,.65,1.12,0),(water,.18,1.25,.02)],.55),mix('Turnfire',[(fire,.5,1.18,0),(fire,.35,.95,.07)],.65),mix('Backfire',[(fire,.4,.86,0),(impact,.27,1.1,.04)],.6),mix('SlotReady',[(water,.6,1.3,0),(cast,.25,1.15,.06)],.8),mix('SpellCast',[(cast,.6,.95,0),(water,.24,.88,.05)],.8)]
Path('Docs/Reynard音效来源.json').write_text(json.dumps(rows,ensure_ascii=False,indent=2),encoding='utf-8')
