#!/usr/bin/env python3
"""Deterministically preserve and process the approved SSE Library WAV set."""
from __future__ import annotations

import argparse
import hashlib
import json
import shutil
import wave
from pathlib import Path

MAPPING = {
    "GUNAntq_Flintlock fire_CS_USC.wav": ["pistol-shot"],
    "GUNAntq_Flintlock rifle fire_CS_USC.wav": ["rifle-shot"],
    "GUNAntq_Musket shots_CS_USC.wav": ["musket-shot"],
    "GUNPis_Exterior pistol shot_CS_USC.wav": ["revolver-shot"],
    "GUNShotg_Classic western shotgun blast with reverb_CS_USC.wav": ["blunderbuss-shot"],
}


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def decode_24(data: bytes) -> list[int]:
    return [int.from_bytes(data[index:index + 3], "little", signed=True)
            for index in range(0, len(data), 3)]


def process(source: Path, output: Path) -> dict[str, object]:
    with wave.open(str(source), "rb") as reader:
        channels, width, rate, count = (reader.getnchannels(), reader.getsampwidth(),
                                        reader.getframerate(), reader.getnframes())
        data = reader.readframes(count)
    if channels != 1 or width != 3 or rate != 48000:
        raise RuntimeError(f"Unexpected SSE WAV contract: {source}")
    samples = decode_24(data)
    peak = max(abs(value) for value in samples)
    threshold = round(((1 << 23) - 1) * 0.0031622776601683794)  # -50 dBFS
    active = [index for index, value in enumerate(samples) if abs(value) >= threshold]
    if not active:
        raise RuntimeError(f"Silent SSE WAV: {source}")
    padding = round(rate * 0.010)
    first = max(0, active[0] - padding)
    last = min(len(samples), active[-1] + padding + 1)
    trimmed = samples[first:last]
    maximum = rate * 4
    tail_limited = len(trimmed) > maximum
    if tail_limited:
        trimmed = trimmed[:maximum]
        fade = rate // 4
        for index in range(fade):
            sample_index = len(trimmed) - fade + index
            trimmed[sample_index] = round(trimmed[sample_index] *
                                          (fade - index - 1) / fade)
    target = round(32767 * 0.70)
    rendered = [max(-32768, min(32767, round(value * target / peak)))
                for value in trimmed]
    output.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(output), "wb") as writer:
        writer.setnchannels(1)
        writer.setsampwidth(2)
        writer.setframerate(rate)
        writer.writeframes(b"".join(value.to_bytes(2, "little", signed=True)
                                    for value in rendered))
    return {
        "source": source.name,
        "sourceSha256": sha256(source),
        "sourceChannels": channels,
        "sourceSampleRate": rate,
        "sourceBitDepth": width * 8,
        "sourceDurationSeconds": round(count / rate, 6),
        "sourcePeak": round(peak / ((1 << 23) - 1), 6),
        "sourceClippedSamples": sum(abs(value) >= (1 << 23) - 1 for value in samples),
        "processed": output.name,
        "processedSha256": sha256(output),
        "processedBitDepth": 16,
        "processedDurationSeconds": round(len(rendered) / rate, 6),
        "processedPeak": 0.70,
        "mapping": MAPPING[source.name],
        "modifications": "trim below -50 dBFS with 10 ms padding; " +
            ("cap excessive tail at 4 s with 250 ms fade; " if tail_limited else "") +
            "peak-normalize to -3.10 dBFS; deterministic 16-bit PCM mono",
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    originals = args.output / "original"
    processed = args.output / "processed"
    records = []
    for name in sorted(MAPPING):
        source = args.source / name
        if not source.is_file():
            raise FileNotFoundError(source)
        originals.mkdir(parents=True, exist_ok=True)
        preserved = originals / name
        shutil.copyfile(source, preserved)
        if sha256(source) != sha256(preserved):
            raise RuntimeError(f"Preservation hash mismatch: {source}")
        records.append(process(preserved, processed / name))
    manifest = {"schemaVersion": 1, "license": "CC0-1.0",
                "source": "SSE Library: GUNS", "records": records}
    (args.output / "audio-manifest.json").write_text(
        json.dumps(manifest, indent=2) + "\n", encoding="utf-8", newline="\n")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
