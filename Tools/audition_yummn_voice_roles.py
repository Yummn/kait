"""Create source-preserving single-character auditions; no game import or ASR."""
from pathlib import Path
import numpy as np
import soundfile as sf

root = Path('C:/Users/yummn/Desktop/应用/Spine/素材/语音')
out = Path('C:/Users/yummn/Downloads/kait/AudioPreviews/YummnVoice-20260910')
paths = {p.name: p for p in root.rglob('*.wav')}
for name in ['Aqua', 'Bridget']:
    pieces = []
    for event in ['Battle_N_2', 'Battle_N_3', 'Battle_H_1', 'Battle_Hit_1', 'Go_1', 'Win_1']:
        path = paths[f'{name}_{event}.wav']
        data, sr = sf.read(path, dtype='float32', always_2d=True)
        print(name, event, round(len(data)/sr, 2), path)
        if data.shape[1] == 1:
            data = np.repeat(data, 2, axis=1)
        if sr != 48000:
            positions = np.arange(round(len(data)*48000/sr))*sr/48000
            data = np.stack([np.interp(positions, np.arange(len(data)), data[:, i]) for i in range(2)], axis=1)
        pieces.extend([data, np.zeros((33600, 2))])
    target = out / f'{name}-role-audition.wav'
    sf.write(target, np.concatenate(pieces), 48000, subtype='PCM_16')
    info = sf.info(target)
    assert info.frames > 0 and info.samplerate == 48000
    print('PREVIEW', target, info.duration)
    for event in ['Battle_Die_1', 'Fail_1']:
        assert f'{name}_{event}.wav' in paths
