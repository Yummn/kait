from pathlib import Path
from PIL import Image, ImageDraw

root=Path(__file__).resolve().parents[1]/'VFXScreenshots'
cases=[('boundary','RIGHT',(708,584)),('boundary-left','LEFT',(132,584)),
       ('boundary-up','UP',(420,296)),('boundary-down','DOWN',(420,872))]
sheet=Image.new('RGB',(640,580),(36,32,42))
draw=ImageDraw.Draw(sheet)
for i,(kind,label,(x,y)) in enumerate(cases):
    shot=Image.open(root/f'boundary-a-{kind}.png')
    # Screenshot crops only; the approved source atlas is never edited.
    crop=shot.crop((max(0,x-160),y-170,max(0,x-160)+320,y+90))
    column,row=i%2,i//2
    sheet.paste(crop,(column*320,row*290+30))
    draw.text((column*320+10,row*290+10),label,fill=(240,235,227))
sheet.save(root/'boundary-a-contact.png')
print(root/'boundary-a-contact.png')
