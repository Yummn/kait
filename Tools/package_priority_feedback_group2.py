"""Remove baked checkerboards and build GIF auditions for priority VFX group 2."""

from pathlib import Path
import sys

import cv2
import numpy as np
from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1] / "VFXPreviews" / "PriorityFeedback-20260913-Group2"
NAMES = [
    "SweepPursuit",
    "ThunderWave",
    "ManaPearl",
    "MirrorResonance",
    "ShadowBladeEcho",
    "Misdirection",
    "LastingIllusion",
]
if len(sys.argv) > 1:
    NAMES = sys.argv[1:]


def key_checker(image: Image.Image) -> Image.Image:
    """Remove only neutral, bright checker cells connected to the outer edge."""
    rgb = np.asarray(image.convert("RGB"))
    # The generator rendered the checker with uneven illumination, so its dark
    # squares extend well below mid-grey.  Saturation is the reliable separator:
    # the effects have coloured outlines enclosing any intentional white cores.
    eligible = (
        (rgb.max(axis=2).astype(int) - rgb.min(axis=2) < 55)
        & (rgb.mean(axis=2) > 70)
    )
    _, labels = cv2.connectedComponents(eligible.astype("uint8"), connectivity=8)
    edge_labels = np.unique(
        np.concatenate((labels[0], labels[-1], labels[:, 0], labels[:, -1]))
    )
    edge_labels = edge_labels[edge_labels != 0]
    outside = np.isin(labels, edge_labels)
    alpha = np.where(outside, 0, 255).astype("uint8")
    rgba = np.dstack((rgb, alpha))
    rgba[outside] = 0
    return Image.fromarray(rgba, "RGBA")


def contain(image: Image.Image, size: tuple[int, int]) -> Image.Image:
    box = image.getbbox()
    result = Image.new("RGBA", size)
    if box is None:
        return result
    cropped = image.crop(box)
    scale = min((size[0] - 24) / cropped.width, (size[1] - 24) / cropped.height, 1.0)
    cropped = cropped.resize(
        (max(1, round(cropped.width * scale)), max(1, round(cropped.height * scale))),
        Image.Resampling.LANCZOS,
    )
    result.alpha_composite(
        cropped,
        ((size[0] - cropped.width) // 2, (size[1] - cropped.height) // 2),
    )
    return result


for name in NAMES:
    source_path = ROOT / f"{name}.png"
    source = key_checker(Image.open(source_path))
    source.save(source_path)
    width, height = source.size
    frames = []
    for index in range(8):
        x0 = round((index % 4) * width / 4)
        x1 = round((index % 4 + 1) * width / 4)
        y0 = round((index // 4) * height / 2)
        y1 = round((index // 4 + 1) * height / 2)
        effect = contain(source.crop((x0, y0, x1, y1)), (300, 300))
        preview = Image.new("RGBA", (320, 320), (190, 216, 229, 255))
        draw = ImageDraw.Draw(preview)
        draw.rounded_rectangle(
            (9, 9, 311, 311),
            radius=26,
            fill=(205, 225, 235, 255),
            outline=(86, 102, 127, 255),
            width=3,
        )
        draw.line((160, 10, 160, 310), fill=(126, 151, 169, 80), width=2)
        draw.line((10, 160, 310, 160), fill=(126, 151, 169, 80), width=2)
        preview.alpha_composite(effect, (10, 10))
        frames.append(preview.convert("RGB").quantize(colors=255))
    frames[0].save(
        ROOT / f"{name}.gif",
        save_all=True,
        append_images=frames[1:],
        duration=85,
        loop=0,
        disposal=2,
    )
    print(ROOT / f"{name}.gif")
