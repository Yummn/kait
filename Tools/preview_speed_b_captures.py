from pathlib import Path
from PIL import Image,ImageDraw
root=Path(__file__).resolve().parents[1]/'VFXScreenshots'
sheet=Image.new('RGB',(960,290),(36,32,42));draw=ImageDraw.Draw(sheet)
for index,kind in enumerate(['speed-low','speed-mid','speed-high']):
    shot=Image.open(root/f'speed-b-{kind}.png')
    sheet.paste(shot.crop((260,430,580,690)),(index*320,30))
    draw.text((index*320+10,10),kind.upper(),fill=(240,235,227))
sheet.save(root/'speed-b-contact.png')
