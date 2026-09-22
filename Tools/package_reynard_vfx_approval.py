"""Build animated approval previews from Reynard 4x2 transparent VFX sheets."""
import argparse
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

parser = argparse.ArgumentParser()
parser.add_argument("--folder", default="01-FoxfireImpact")
parser.add_argument("--a", default="A-MoonFoxfire.png")
parser.add_argument("--b", default="B-FoxClawBurst.png")
parser.add_argument("--c", default="C-FoxRuneImpact.png")
args = parser.parse_args()

ROOT = Path(__file__).resolve().parents[1] / "ArtPreviews" / "ReynardVFXApproval" / args.folder
SHEETS = {"A": args.a, "B": args.b, "C": args.c}


def contain(image: Image.Image, size=(280, 280)) -> Image.Image:
    box = image.getbbox()
    if box is None:
        return Image.new("RGBA", size, (0, 0, 0, 0))
    cropped = image.crop(box)
    scale = min((size[0] - 24) / cropped.width, (size[1] - 24) / cropped.height)
    cropped = cropped.resize(
        (max(1, round(cropped.width * scale)), max(1, round(cropped.height * scale))),
        Image.Resampling.LANCZOS,
    )
    result = Image.new("RGBA", size, (0, 0, 0, 0))
    result.alpha_composite(cropped, ((size[0] - cropped.width) // 2, (size[1] - cropped.height) // 2))
    return result


def panel(effect: Image.Image, label: str) -> Image.Image:
    image = Image.new("RGBA", (300, 320), (48, 50, 79, 255))
    draw = ImageDraw.Draw(image)
    draw.rounded_rectangle((8, 8, 292, 292), 22, fill=(104, 105, 151, 255), outline=(246, 229, 204, 255), width=4)
    draw.line((150, 10, 150, 290), fill=(205, 205, 230, 45), width=2)
    draw.line((10, 150, 290, 150), fill=(205, 205, 230, 45), width=2)
    image.alpha_composite(effect, (10, 10))
    font = ImageFont.load_default(size=22)
    draw.text((150, 305), label, anchor="mm", fill=(255, 244, 222, 255), font=font)
    return image


all_frames = {}
for label, filename in SHEETS.items():
    sheet = Image.open(ROOT / filename).convert("RGBA")
    if sheet.getchannel("A").getextrema()[0] == 255:
        raise RuntimeError(f"{filename} has no transparent pixels")
    width, height = sheet.size
    frames = []
    for index in range(8):
        x0 = round((index % 4) * width / 4)
        x1 = round((index % 4 + 1) * width / 4)
        y0 = round((index // 4) * height / 2)
        y1 = round((index // 4 + 1) * height / 2)
        frames.append(panel(contain(sheet.crop((x0, y0, x1, y1))), label).convert("RGB"))
    all_frames[label] = frames
    frames[0].save(ROOT / f"{label}.gif", save_all=True, append_images=frames[1:], duration=95, loop=0, disposal=2)

comparison = []
for index in range(8):
    row = Image.new("RGB", (900, 320), (48, 50, 79))
    for column, label in enumerate(SHEETS):
        row.paste(all_frames[label][index], (column * 300, 0))
    comparison.append(row)
comparison[0].save(ROOT / "ABC.gif", save_all=True, append_images=comparison[1:], duration=95, loop=0, disposal=2)

for path in [ROOT / "A.gif", ROOT / "B.gif", ROOT / "C.gif", ROOT / "ABC.gif"]:
    with Image.open(path) as gif:
        if gif.n_frames != 8:
            raise RuntimeError(f"{path.name}: expected 8 frames, got {gif.n_frames}")
    print(path)
