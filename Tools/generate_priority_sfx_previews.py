"""Deterministic original Foley-style previews for the 2026-09-13 priority VFX set."""
import math
import random
import struct
import wave
from pathlib import Path

SR = 48000
OUT = Path(__file__).resolve().parents[1] / "AudioPreviews" / "PriorityFeedback-20260913"
OUT.mkdir(parents=True, exist_ok=True)


def noise_source(rng, length):
    return [rng.uniform(-1.0, 1.0) for _ in range(length)]


def lowpass(values, amount):
    state = 0.0
    result = []
    for value in values:
        state += amount * (value - state)
        result.append(state)
    return result


def highpass(values, amount=0.04):
    low = lowpass(values, amount)
    return [value - base for value, base in zip(values, low)]


def burst(t, at, width):
    distance = abs(t - at)
    return 0.0 if distance >= width else (1.0 - distance / width) ** 2


def write(name, seconds, synth):
    count = round(seconds * SR)
    rng = random.Random("kait-priority-" + name)
    raw = noise_source(rng, count)
    low = lowpass(raw, 0.018)
    air = highpass(raw, 0.055)
    samples = [synth(i / SR, i / count, raw[i], low[i], air[i], rng) for i in range(count)]
    peak = max(max(abs(value) for value in samples), 1e-6)
    gain = 0.78 / peak
    # A tiny fixed stereo offset gives space without changing timing.
    delay = 17
    left = [max(-0.95, min(0.95, value * gain)) for value in samples]
    right = [left[max(0, i - delay)] * 0.92 + left[i] * 0.08 for i in range(count)]
    path = OUT / f"{name}.wav"
    with wave.open(str(path), "wb") as output:
        output.setparams((2, 2, SR, 0, "NONE", "not compressed"))
        frames = []
        for a, b in zip(left, right):
            frames.append(struct.pack("<hh", round(a * 32767), round(b * 32767)))
        output.writeframes(b"".join(frames))
    print(path)


def stone(t, u, raw, low, air, rng):
    thud = math.sin(2 * math.pi * (86 - 34 * u) * t) * math.exp(-t * 18)
    crack = air * (1.2 * burst(t, 0.028, 0.023) + 0.65 * burst(t, 0.09, 0.035))
    chips = sum(math.sin(2 * math.pi * f * t) * math.exp(-max(0, t - a) * 42) * (1 if t >= a else 0)
                for f, a in ((710, .035), (1040, .055), (1490, .083)))
    dust = low * math.sin(math.pi * min(1, t / .24)) * math.exp(-t * 4)
    return 0.55 * thud + 0.42 * crack + 0.08 * chips + 0.22 * dust


def sonic(t, u, raw, low, air, rng):
    pressure = math.sin(2 * math.pi * (118 - 62 * u) * t) * math.exp(-t * 8)
    slap = air * burst(t, .018, .018)
    ring = (math.sin(2 * math.pi * 430 * t) + .45 * math.sin(2 * math.pi * 690 * t)) * math.exp(-t * 12)
    wind = low * math.sin(math.pi * u) * (1 - u)
    return .5 * pressure + .48 * slap + .12 * ring + .28 * wind


def rune(t, u, raw, low, air, rng):
    sweep = air * math.sin(math.pi * min(1, u * 1.6)) * (1 - u) ** .6
    tone = sum(math.sin(2 * math.pi * f * max(0, t - a)) * math.exp(-max(0, t - a) * 7) * (1 if t >= a else 0)
               for f, a in ((392, .06), (587, .11), (784, .17)))
    pulse = math.sin(2 * math.pi * 132 * t) * burst(t, .24, .10)
    return .23 * sweep + .13 * tone + .25 * pulse + .08 * low


def missile(t, u, raw, low, air, rng):
    flight_end = .31
    flight = (air * .22 + math.sin(2 * math.pi * (470 + 120 * math.sin(t * 19)) * t) * .09) * min(1, t / .025) * (1 if t < flight_end else 0)
    whoosh = low * .36 * math.sin(math.pi * min(1, t / flight_end)) * (1 if t < flight_end else 0)
    hit = (air * .5 + math.sin(2 * math.pi * 170 * max(0, t - flight_end)) * .38) * burst(t, flight_end, .055)
    tail = math.sin(2 * math.pi * 690 * max(0, t - flight_end)) * math.exp(-max(0, t - flight_end) * 18) * (1 if t >= flight_end else 0)
    return flight + whoosh + hit + .08 * tail


def mirror(t, u, raw, low, air, rng):
    swell = low * math.sin(math.pi * min(1, u * 1.35)) * (1 - .45 * u)
    flutter = air * (.18 + .18 * math.sin(2 * math.pi * 13 * t)) * math.sin(math.pi * u)
    shimmer = sum(math.sin(2 * math.pi * f * t + i) for i, f in enumerate((523, 659, 880))) * .045 * math.sin(math.pi * u)
    settle = math.sin(2 * math.pi * 196 * max(0, t - .34)) * math.exp(-max(0, t - .34) * 12) * (1 if t >= .34 else 0)
    return .46 * swell + flutter + shimmer + .11 * settle


def mage_hand(t, u, raw, low, air, rng):
    reach = air * math.sin(math.pi * min(1, t / .20)) * (1 if t < .23 else 0)
    grip = (air * .35 + math.sin(2 * math.pi * 145 * max(0, t - .23)) * .35) * burst(t, .23, .045)
    pull = low * math.sin(math.pi * max(0, min(1, (t - .20) / .27))) * (1 if t >= .20 else 0)
    bead = math.sin(2 * math.pi * (680 - 260 * max(0, u - .45)) * t) * math.exp(-max(0, t - .23) * 8) * (1 if t >= .23 else 0)
    return .3 * reach + grip + .42 * pull + .08 * bead


write("StoneCollision", .48, stone)
write("SonicBurst", .52, sonic)
write("WardingGlyph", .58, rune)
write("MagicMissile", .52, missile)
write("MirrorCreate", .62, mirror)
write("MageHand", .55, mage_hand)
