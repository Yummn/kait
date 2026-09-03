"""Normalize packaged character voice WAV files to a shared active-speech level."""

from __future__ import annotations

import argparse
import math
import os
from pathlib import Path

import numpy as np
import soundfile as sf


def active_level_db(samples: np.ndarray, sample_rate: int) -> float | None:
    if samples.ndim == 1:
        samples = samples[:, None]

    frame_power = np.mean(np.square(samples.astype(np.float64)), axis=1)
    block_size = max(1, int(sample_rate * 0.05))
    block_count = len(frame_power) // block_size
    if block_count == 0:
        return None

    blocks = frame_power[: block_count * block_size].reshape(block_count, block_size)
    block_power = np.mean(blocks, axis=1)
    block_db = 10.0 * np.log10(np.maximum(block_power, 1e-12))
    relative_gate = float(np.max(block_db)) - 20.0
    gate_db = max(-50.0, relative_gate)
    active = block_power[block_db >= gate_db]
    if active.size == 0:
        return None
    return 10.0 * math.log10(max(float(np.mean(active)), 1e-12))


def normalize_file(path: Path, target_db: float, peak_db: float) -> tuple[float, float, float]:
    samples, sample_rate = sf.read(path, always_2d=True, dtype="float32")
    before_db = active_level_db(samples, sample_rate)
    if before_db is None:
        raise ValueError("no active speech detected")

    requested_gain_db = max(-20.0, min(20.0, target_db - before_db))
    gain = 10.0 ** (requested_gain_db / 20.0)
    peak_limit = 10.0 ** (peak_db / 20.0)
    source_peak = float(np.max(np.abs(samples)))
    if source_peak > 0.0:
        gain = min(gain, peak_limit / source_peak)

    normalized = np.clip(samples * gain, -1.0, 1.0)
    temp_path = path.with_name(path.stem + ".normalized.wav")
    sf.write(temp_path, normalized, sample_rate, subtype="PCM_16")
    os.replace(temp_path, path)

    after_db = active_level_db(normalized, sample_rate)
    if after_db is None:
        raise ValueError("normalized file has no active speech")
    applied_gain_db = 20.0 * math.log10(max(gain, 1e-12))
    return before_db, after_db, applied_gain_db


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("root", type=Path)
    parser.add_argument("--target-db", type=float, default=-20.0)
    parser.add_argument("--peak-db", type=float, default=-1.0)
    args = parser.parse_args()

    paths = sorted(args.root.rglob("*.wav"))
    if not paths:
        raise SystemExit(f"No WAV files found below {args.root}")

    results: list[tuple[Path, float, float, float]] = []
    for path in paths:
        before_db, after_db, gain_db = normalize_file(path, args.target_db, args.peak_db)
        results.append((path, before_db, after_db, gain_db))
        print(f"{path.name}: {before_db:6.2f} -> {after_db:6.2f} dBFS  gain {gain_db:+6.2f} dB")

    before_values = np.array([item[1] for item in results])
    after_values = np.array([item[2] for item in results])
    print(
        f"Normalized {len(results)} files. "
        f"Before range {before_values.min():.2f}..{before_values.max():.2f} dBFS; "
        f"after range {after_values.min():.2f}..{after_values.max():.2f} dBFS."
    )


if __name__ == "__main__":
    main()
