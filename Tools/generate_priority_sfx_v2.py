"""Distinctive second-pass original cues for the priority Yummn feedback set."""
import math
import random
import struct
import wave
from pathlib import Path

SR = 48000
OUT = Path(__file__).resolve().parents[1] / "AudioPreviews" / "PriorityFeedback-20260913-v2"
OUT.mkdir(parents=True, exist_ok=True)


def triangle(t, center, radius):
    x = abs(t - center) / radius
    return 0.0 if x >= 1 else (1 - x) ** 2


def write(name, seconds, render):
    rng = random.Random("yummn-v2-" + name)
    count = round(seconds * SR)
    white = [rng.uniform(-1, 1) for _ in range(count)]
    low = []
    state = 0.0
    for sample in white:
        state += .012 * (sample - state)
        low.append(state)
    high = [white[i] - low[i] for i in range(count)]
    stereo = [render(i / SR, i / count, white[i], low[i], high[i], rng) for i in range(count)]
    peak = max(max(abs(left), abs(right)) for left, right in stereo)
    gain = .82 / max(peak, 1e-7)
    path = OUT / f"{name}.wav"
    with wave.open(str(path), "wb") as output:
        output.setparams((2, 2, SR, 0, "NONE", "not compressed"))
        frames = []
        for left, right in stereo:
            frames.append(struct.pack("<hh", round(max(-.96, min(.96, left * gain)) * 32767), round(max(-.96, min(.96, right * gain)) * 32767)))
        output.writeframes(b"".join(frames))
    print(path)


stone_chips = [(0.075, 910, -.75), (0.108, 1370, .55), (0.151, 1830, -.3),
               (0.205, 1120, .8), (0.273, 2460, -.65), (0.344, 1560, .4),
               (0.438, 790, -.2), (0.515, 1210, .7)]


def stone(t, u, white, low, high, rng):
    initial = high * (1.5 * triangle(t, .035, .027) + .85 * triangle(t, .105, .042))
    floor = math.sin(2 * math.pi * (92 - 46 * u) * t) * math.exp(-t * 13)
    split = math.sin(2 * math.pi * 236 * max(0, t - .045)) * math.exp(-max(0, t - .045) * 26) * (t >= .045)
    left_chips = right_chips = 0.0
    for at, frequency, pan in stone_chips:
        age = t - at
        if age < 0:
            continue
        click = (math.sin(2 * math.pi * frequency * age) + .35 * math.sin(2 * math.pi * frequency * 1.71 * age)) * math.exp(-age * 34)
        left_chips += click * (1 - pan) * .5
        right_chips += click * (1 + pan) * .5
    rubble = low * math.exp(-max(0, t - .12) * 4.2) * (t >= .12)
    common = .48 * initial + .55 * floor + .15 * split + .38 * rubble
    return common + .22 * left_chips, common + .22 * right_chips


def sonic(t, u, white, low, high, rng):
    hit_at = .245
    pre = min(1, t / hit_at)
    suction = (low * .62 + high * .045) * pre ** 2 * (t < hit_at)
    tightening = math.sin(2 * math.pi * (170 + 540 * pre * pre) * t) * .11 * pre ** 3 * (t < hit_at)
    age = max(0, t - hit_at)
    slap = high * 1.25 * triangle(t, hit_at, .018)
    boom = math.sin(2 * math.pi * (105 - 58 * age) * age) * math.exp(-age * 8.5) * (t >= hit_at)
    ring = math.sin(2 * math.pi * 346 * age) * math.exp(-age * 15) * (t >= hit_at)
    return_sweep = (low * .55 + high * .075) * math.sin(math.pi * min(1, age / .5)) * math.exp(-age * 2.8) * (t >= hit_at)
    left = suction - tightening + slap + .62 * boom + .11 * ring + return_sweep
    right = suction + tightening + slap + .62 * boom + .11 * ring + return_sweep
    return left, right


ward_notes = [(0.045, 587, -.85), (0.125, 740, .85), (0.205, 880, -.45), (0.285, 1175, .45)]


def ward(t, u, white, low, high, rng):
    left = right = 0.0
    for at, frequency, pan in ward_notes:
        age = t - at
        if age < 0:
            continue
        bell = (math.sin(2 * math.pi * frequency * age) + .38 * math.sin(2 * math.pi * frequency * 2.01 * age)) * math.exp(-age * 10.5)
        left += bell * (1 - pan) * .5
        right += bell * (1 + pan) * .5
    seal_age = t - .34
    seal = 0.0 if seal_age < 0 else (math.sin(2 * math.pi * 156 * seal_age) * math.exp(-seal_age * 15) + high * triangle(t, .34, .028))
    shimmer = 0.0 if seal_age < 0 else math.sin(2 * math.pi * 1397 * seal_age) * math.exp(-seal_age * 6) * .045
    return .18 * left + .42 * seal + shimmer, .18 * right + .42 * seal + shimmer


def command(t, u, white, low, high, rng):
    snap1 = (high * .82 + math.sin(2 * math.pi * 205 * t) * .35) * triangle(t, .035, .024)
    snap2 = (high * .52 + math.sin(2 * math.pi * 260 * max(0, t - .105)) * .28) * triangle(t, .105, .027)
    lock_age = t - .125
    lock = 0.0 if lock_age < 0 else math.sin(2 * math.pi * (520 - 280 * min(1, lock_age / .24)) * lock_age) * math.exp(-lock_age * 9)
    clamp = high * triangle(t, .205, .016)
    tail = low * math.exp(-max(0, t - .20) * 8) * (t >= .20)
    return snap1 + .8 * snap2 + .26 * lock + .36 * clamp + .28 * tail, .72 * snap1 + snap2 + .26 * lock + .36 * clamp + .28 * tail


write("StoneCollision_v2", .68, stone)
write("SonicBurst_v2", .82, sonic)
write("WardingGlyph_v2", .76, ward)
write("CommandAct_v2", .48, command)
