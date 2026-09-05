"""Offline functional tests. Synthetic fixtures only; no paid API and no game assets changed."""
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import traceback
from PIL import Image, ImageDraw
import bridge

ROOT = Path(__file__).resolve().parent
os.environ['PATH'] = str(Path(sys.executable).parent) + os.pathsep + os.environ.get('PATH', '')
EVIDENCE = Path(os.environ.get('KAIT_VIBEGAME_EVIDENCE', ROOT / 'evidence'))
EVIDENCE.mkdir(parents=True, exist_ok=True)
WORK = Path(tempfile.mkdtemp(prefix='art-', dir=EVIDENCE))
RESULTS = []


def cli(*args):
    result = subprocess.run([sys.executable, '-X', 'utf8', str(ROOT / 'compat_main.py'), *map(str, args)],
                            cwd=WORK, capture_output=True, text=True, encoding='utf-8', timeout=60)
    with (WORK / 'commands.log').open('a', encoding='utf-8') as log:
        log.write(f'COMMAND: {args}\nEXIT: {result.returncode}\n{result.stdout}\n{result.stderr}\n')
    if result.returncode:
        raise AssertionError((result.stdout + result.stderr)[-3000:])
    return result.stdout


def test(name, operation):
    try:
        detail = operation()
        RESULTS.append({'name': name, 'status': 'PASS', 'detail': str(detail or '')})
    except Exception as error:
        RESULTS.append({'name': name, 'status': 'FAIL', 'detail': str(error)})
    print(f'{RESULTS[-1]["status"]}: {name}', flush=True)


# This is a test chart, not generated game art.
sheet = Image.new('RGBA', (192, 64), (255, 0, 255, 255))
draw = ImageDraw.Draw(sheet)
for index, color in enumerate([(250, 199, 183, 255), (93, 79, 87, 255), (240, 230, 200, 255)]):
    x = index * 64 + 16
    draw.rectangle((x, 12, x + 31, 51), fill=color)
sheet.save(WORK / 'sheet.png')
frames = WORK / 'frames'
frames.mkdir()
for index in range(3):
    frame = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    ImageDraw.Draw(frame).rectangle((10 + index * 6, 14, 35 + index * 6, 49), fill=(250, 199, 183, 255))
    frame.save(frames / f'frame_{index:02}.png')


test('CLI help', lambda: cli('--help'))
test('Image color and alpha analysis', lambda: cli('art', 'analyze', 'sheet.png'))
test('Asset directory inventory', lambda: cli('art', 'tree', 'frames'))


def remove_background():
    cli('art', 'rmbg', 'sheet.png', '-c', '255,0,255', '--seed', 'match', '-o', 'transparent.png')
    image = Image.open(WORK / 'transparent.png').convert('RGBA')
    assert image.getpixel((0, 0))[3] == 0
    assert image.getpixel((20, 20)) == (250, 199, 183, 255)
    return 'Background alpha=0; foreground RGBA preserved'
test('Background removal (local flood-fill)', remove_background)


def cut():
    cli('art', 'cut', 'transparent.png', '-m', '100', '-o', 'cut')
    files = list((WORK / 'cut').rglob('*.png'))
    assert len(files) == 3, f'Expected 3 images, found {len(files)}'
    return 'Three independent sprites extracted'
test('Sprite cutting', cut)


def atlas():
    cli('art', 'concat', 'frames', '-o', 'atlas.png', '--layout', 'row', '--spacing', '2')
    assert Image.open(WORK / 'atlas.png').size == (196, 64)
    manifest = json.loads((WORK / 'manifest.json').read_text())
    assert manifest
    return '196x64 image and nonempty manifest.json'
test('Atlas packing and manifest', atlas)


def flip():
    cli('art', 'edit', 'frames/frame_00.png', '-m', 'hflip', '-o', 'flipped.png')
    image = Image.open(WORK / 'flipped.png').convert('RGBA')
    assert list(image.getdata()) == list(Image.open(frames / 'frame_00.png').transpose(Image.Transpose.FLIP_LEFT_RIGHT).getdata())
test('Image flip', flip)
test('Image resize', lambda: cli('art', 'edit', 'frames/frame_00.png', '-m', 'resize', '-s', '2', '-o', 'resized.png'))
test('Edge cleanup', lambda: cli('art', 'perfectify', 'frames/frame_00.png', '-m', 'sprite', '-o', 'clean.png'))
test('Pixel grid cleanup', lambda: cli('art', 'pixel', 'process', 'frames/frame_00.png', '--grid-size', '2x2', '-o', 'pixel.png'))


def roundtrip():
    cli('art', 'pixel', 'encode', 'frames/frame_00.png', '--no-process', '-o', 'matrix.txt')
    cli('art', 'pixel', 'decode', 'matrix.txt', '-o', 'decoded.png')
    assert list(Image.open(WORK / 'decoded.png').convert('RGBA').getdata()) == list(Image.open(frames / 'frame_00.png').getdata())
test('Pixel matrix encode/decode round trip', roundtrip)


def video():
    cli('art', 'f2v', 'frames', '--fps', '6', '-o', 'animation.mp4')
    import cv2
    cap = cv2.VideoCapture(str(WORK / 'animation.mp4'))
    try:
        assert cap.isOpened()
        assert int(cap.get(cv2.CAP_PROP_FRAME_COUNT)) == 3
    finally:
        cap.release()
    return 'MP4 decoded successfully; 3 frames'
test('Frame animation export', video)
test('Video frame extraction', lambda: cli('art', 'v2f', 'animation.mp4', '-o', 'extracted'))
test('Generation provider registration (no API call)', lambda: cli('art', 'gen', 'list'))
test('Unity sidecar status', bridge.status)


def import_safety():
    try:
        bridge.import_asset(WORK / 'sheet.png', dry_run=True)
        raise AssertionError('Out-of-staging file was accepted')
    except ValueError:
        pass
    staging = bridge.STAGING
    staging.mkdir(parents=True, exist_ok=True)
    sample = staging / 'bridge-import-check.png'
    # Do not replace existing content even in the staging area.
    if sample.exists():
        return 'Existing staging sample preserved; outside-path rejection passed'
    sheet.save(sample)
    try:
        destination = bridge.import_asset(sample, dry_run=True)
        assert not destination.exists()
    finally:
        sample.unlink()
    return 'Out-of-staging path rejected; dry run does not write Unity assets'
test('Unity import path validation / dry run', import_safety)

(EVIDENCE / 'art-results.json').write_text(json.dumps({'work': str(WORK), 'results': RESULTS}, ensure_ascii=False, indent=2), encoding='utf-8')
print(f'Evidence: {WORK}')
raise SystemExit(1 if any(row['status'] == 'FAIL' for row in RESULTS) else 0)
