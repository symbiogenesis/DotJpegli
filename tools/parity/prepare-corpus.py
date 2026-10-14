#!/usr/bin/env python3
"""Fetch hash-pinned Kodak RGB8 PNGs and losslessly write canonical P6 inputs.

Uses only Python's standard library. No color management, gamma conversion,
resizing or lossy re-encoding is applied. Corpus pixels are never committed.
"""

import argparse
import hashlib
import json
import struct
import urllib.request
import zlib
from pathlib import Path


def png_rgb(data):
    if data[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError("Not a PNG")
    pos, compressed, dimensions = 8, bytearray(), None
    while pos < len(data):
        size = struct.unpack_from(">I", data, pos)[0]
        kind = data[pos + 4:pos + 8]
        payload = data[pos + 8:pos + 8 + size]
        crc = struct.unpack_from(">I", data, pos + 8 + size)[0]
        if zlib.crc32(kind + payload) & 0xffffffff != crc:
            raise ValueError("PNG CRC mismatch")
        pos += size + 12
        if kind == b"IHDR":
            width, height, depth, color, compression, filtering, interlace = struct.unpack(">IIBBBBB", payload)
            if (depth, color, compression, filtering, interlace) != (8, 2, 0, 0, 0):
                raise ValueError("Expected noninterlaced RGB8 PNG")
            if width * height > 1024 * 1024:
                raise ValueError("Unexpected corpus dimensions")
            dimensions = width, height
        elif kind == b"IDAT":
            compressed.extend(payload)
        elif kind == b"IEND":
            break
        elif kind[:1].isupper():
            raise ValueError(f"Unsupported critical chunk {kind!r}")
    if dimensions is None:
        raise ValueError("Missing IHDR")
    width, height = dimensions
    stride = width * 3
    raw = zlib.decompress(compressed)
    if len(raw) != (stride + 1) * height:
        raise ValueError("Unexpected pixel extent")
    pixels = bytearray()
    previous = bytearray(stride)
    for row in range(height):
        start = row * (stride + 1)
        mode = raw[start]
        if mode > 4:
            raise ValueError("Unknown PNG filter")
        current = bytearray(raw[start + 1:start + 1 + stride])
        for x in range(stride):
            left = current[x - 3] if x >= 3 else 0
            above = previous[x]
            upper_left = previous[x - 3] if x >= 3 else 0
            if mode == 1:
                predictor = left
            elif mode == 2:
                predictor = above
            elif mode == 3:
                predictor = (left + above) // 2
            elif mode == 4:
                prediction = left + above - upper_left
                a, b, c = abs(prediction - left), abs(prediction - above), abs(prediction - upper_left)
                predictor = left if a <= b and a <= c else above if b <= c else upper_left
            else:
                predictor = 0
            current[x] = (current[x] + predictor) & 255
        pixels.extend(current)
        previous = current
    return width, height, pixels


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", required=True, type=Path)
    parser.add_argument("--source", type=Path, help="Use existing PNG files instead of downloading")
    args = parser.parse_args()
    if not args.output.is_absolute() or (args.source and not args.source.is_absolute()):
        parser.error("Output and source paths must be absolute")
    args.output.mkdir(parents=True, exist_ok=False)
    manifest = json.loads(Path(__file__).with_name("corpus.json").read_text(encoding="utf-8"))
    for entry in manifest["images"]:
        name = entry["name"]
        if args.source:
            data = (args.source / name).read_bytes()
        else:
            with urllib.request.urlopen(manifest["baseUrl"] + name, timeout=60) as response:
                data = response.read()
        if hashlib.sha256(data).hexdigest() != entry["pngSha256"]:
            raise ValueError(f"Source hash mismatch: {name}")
        width, height, rgb = png_rgb(data)
        ppm = f"P6\n{width} {height}\n255\n".encode("ascii") + rgb
        if hashlib.sha256(ppm).hexdigest() != entry["ppmSha256"]:
            raise ValueError(f"Decoded pixel hash mismatch: {name}")
        (args.output / Path(name).with_suffix(".ppm")).write_bytes(ppm)
        print(f"Verified {name}: {width}x{height}")
    (args.output / "corpus.json").write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
