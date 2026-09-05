"""Check the alpha field captured by the actual Windows player (not a mock)."""
from pathlib import Path
import json
import numpy as np
from PIL import Image

root = Path(__file__).resolve().parents[1]
shot = root / 'VFXScreenshots/garden-layered.png'
before = np.asarray(Image.open(str(shot)+'.shadow-before.png'))[:,:,3]
after = np.asarray(Image.open(str(shot)+'.shadow-after.png'))[:,:,3]
yy, xx = np.indices(after.shape)
weight = after.astype(np.float64)
report = {
    'source': str(shot),
    'max_alpha': int(after.max()),
    'top_half_coverage': int(after[:512].sum()),
    'bottom_half_coverage': int(after[512:].sum()),
    'centroid_from_top_left': [float((xx*weight).sum()/weight.sum()),
                              float((yy*weight).sum()/weight.sum())],
    'pixels_changed_in_1_5_seconds': int(np.count_nonzero(before != after)),
}
assert report['max_alpha'] >= 180, 'Empty or weak projection field'
assert report['top_half_coverage'] > report['bottom_half_coverage']*4, 'Tree shadow vertically inverted'
assert report['pixels_changed_in_1_5_seconds'] > 100, 'Shadow field is not updating'
assert all(value < 400 for value in report['centroid_from_top_left']), 'Shadow is misplaced'
Image.fromarray(255-after).save(root/'VFXScreenshots/garden-shadow-debug.png')
(root/'VFXScreenshots/garden-cutouts/runtime-report.json').write_text(
    json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(report,ensure_ascii=False,indent=2))
