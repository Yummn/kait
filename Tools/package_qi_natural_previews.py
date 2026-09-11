"""Package approved-for-audition generations; never touches Unity assets."""
from pathlib import Path
import hashlib
import json
import shutil
import numpy as np
import soundfile as sf

root = Path(__file__).resolve().parents[1]
downloads = root.parent
out = root / 'AudioPreviews' / 'QiNatural-20260911'
out.mkdir(parents=True, exist_ok=True)
sources = {
    'A_spend': 'Single_short_natural_#1-1789086454508.wav',
    'A_gain': 'Single_gentle_inward_#1-1789086523362.wav',
    'B_spend': 'A_quick_soft_cotton__#1-1789086607634.wav',
    'B_gain': 'Soft_cotton_sleeves__#1-1789086667885.wav',
    'C_spend': 'One_compact_low_soft_#1-1789086727578.wav',
}
gains = sorted(downloads.glob('One_low_gentle_inwar_#1-*.wav'), key=lambda p: p.stat().st_mtime)
assert gains, 'Missing C gain download'
sources['C_gain'] = gains[-1].name
report = {'provider':'ElevenLabs Sound Effects', 'status':'preview only, not integrated',
          'order':'Each pair: spend, 0.8 seconds silence, gain', 'files':{}}
clips = {}
for key, name in sources.items():
    src = downloads / name
    data, sr = sf.read(src, always_2d=True)
    assert sr == 48000 and np.isfinite(data).all() and np.max(np.abs(data)) > 0
    raw = out / (key + '_raw.wav')
    shutil.copy2(src, raw)
    peak = float(np.max(np.abs(data)))
    # Audition volume only; retain untouched originals and avoid boosting low sounds.
    gain = min(1.0, 0.65 / peak)
    data = data * gain
    sf.write(out / (key + '.wav'), data, sr, subtype='PCM_16')
    clips[key] = data
    report['files'][key] = {'source':name, 'seconds':len(data)/sr, 'channels':data.shape[1],
        'raw_peak':peak, 'raw_full_scale_samples':int(np.sum(np.abs(data)>=.9999)),
        'preview_gain':gain, 'sha256':hashlib.sha256(src.read_bytes()).hexdigest()}
assert len({v['sha256'] for v in report['files'].values()}) == 6, 'Duplicate download'
for label in 'ABC':
    a,b=clips[label+'_spend'],clips[label+'_gain']
    silence=np.zeros((int(.8*48000),a.shape[1]))
    sf.write(out / (label+'_pair.wav'),np.concatenate([a,silence,b]),48000,subtype='PCM_16')
    check,sr=sf.read(out / (label+'_pair.wav'))
    assert len(check)==len(a)+len(silence)+len(b)
(out/'manifest.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(report,ensure_ascii=False,indent=2))
