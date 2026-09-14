"""Build small animated GIF auditions from 4x2 transparent VFX sheets."""
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1] / "VFXPreviews" / "PriorityFeedback-20260913"
NAMES = ["StoneCollision", "SonicBurst", "WardingGlyph", "MagicMissile", "MageHand", "MirrorCreate", "CommandAct"]


def contain(image, size):
    box = image.getbbox()
    if box is None:
        return Image.new("RGBA", size, (0, 0, 0, 0))
    cropped = image.crop(box)
    scale = min((size[0] - 28) / cropped.width, (size[1] - 28) / cropped.height, 1.0)
    cropped = cropped.resize((max(1, round(cropped.width * scale)), max(1, round(cropped.height * scale))), Image.Resampling.LANCZOS)
    result = Image.new("RGBA", size, (0, 0, 0, 0))
    result.alpha_composite(cropped, ((size[0] - cropped.width) // 2, (size[1] - cropped.height) // 2))
    return result


for name in NAMES:
    source = Image.open(ROOT / f"{name}.png").convert("RGBA")
    if source.getchannel("A").getextrema() == (255, 255):
        raise RuntimeError(f"{name} has no transparent pixels")
    width, height = source.size
    frames = []
    for index in range(8):
        x0 = round((index % 4) * width / 4)
        x1 = round((index % 4 + 1) * width / 4)
        y0 = round((index // 4) * height / 2)
        y1 = round((index // 4 + 1) * height / 2)
        effect = contain(source.crop((x0, y0, x1, y1)), (300, 300))
        preview = Image.new("RGBA", (320, 320), (186, 211, 226, 255))
        draw = ImageDraw.Draw(preview)
        draw.rounded_rectangle((9, 9, 311, 311), radius=26, fill=(200, 220, 232, 255), outline=(86, 102, 127, 255), width=3)
        draw.line((160, 10, 160, 310), fill=(126, 151, 169, 80), width=2)
        draw.line((10, 160, 310, 160), fill=(126, 151, 169, 80), width=2)
        preview.alpha_composite(effect, (10, 10))
        frames.append(preview.convert("RGB").quantize(colors=255, method=Image.Quantize.MEDIANCUT))
    frames[0].save(ROOT / f"{name}.gif", save_all=True, append_images=frames[1:], duration=75, loop=0, disposal=2)
    print(ROOT / f"{name}.gif")
