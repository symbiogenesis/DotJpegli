"""Create the untimed README photograph showcase; requires Pillow 13.0.0.

All external executables are explicit arguments. No latency or allocation
measurements are made here. Use the normal CI harness for benchmarks.
"""

import argparse
import hashlib
import json
import platform
import subprocess
from pathlib import Path

from PIL import Image, ImageCms, ImageDraw, ImageFont, __version__, features


SOURCE_URL = "https://esawebb.org/media/archives/images/publicationtiff/weic2205a.tif"
SOURCE_SHA256 = "145becf8e9ca2c35473d102da19a34786185bc5da947367db75dc78a0c323e71"
PIN = "031a0077f5799a6041004267fc12b956c1f52a20"


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ("source-tiff", "output", "scratch", "dotnet", "dotjpegli", "jpegli", "mozjpeg", "cjxl", "djxl", "avifenc", "avifdec", "ssimulacra2"):
        parser.add_argument("--" + name, required=True, type=Path)
    parser.add_argument("--revision", required=True)
    args = parser.parse_args()
    if __version__ != "13.0.0":
        raise ValueError("Pinned source preparation requires Pillow 13.0.0")
    if digest(args.source_tiff) != SOURCE_SHA256:
        raise ValueError("Official source TIFF SHA256 mismatch")
    args.output.mkdir(parents=True, exist_ok=True)
    args.scratch.mkdir(parents=True, exist_ok=True)
    commands = []

    def run(tool, *arguments, capture_all=False):
        command = [str(tool.resolve()), *[str(value) for value in arguments]]
        completed = subprocess.run(command, capture_output=True, text=True)
        if completed.returncode:
            raise RuntimeError(f"{Path(tool).name} exited {completed.returncode}: {completed.stdout}\n{completed.stderr}")
        commands.append({"executable": Path(tool).name, "arguments": [str(value) for value in arguments], "stdout": completed.stdout, "stderr": completed.stderr})
        return (completed.stdout + completed.stderr if capture_all else completed.stdout).strip()

    with Image.open(args.source_tiff) as image:
        # The original TIFF is already sRGB. Repackage the same RGB samples,
        # then resize once; every encoder receives this exact common source.
        original_dimensions = list(image.size)
        source_profile = image.info.get("icc_profile")
        import io
        profile_name = ImageCms.getProfileName(ImageCms.ImageCmsProfile(io.BytesIO(source_profile))).strip()
        if "sRGB" not in profile_name:
            raise ValueError("Unexpected TIFF color profile")
        source = Image.frombytes("RGB", image.size, image.convert("RGB").tobytes())
    source = source.resize((2400, 1390), Image.Resampling.LANCZOS)
    source.save(args.output / "source.png", compress_level=9)
    source.save(args.scratch / "source.ppm")
    (args.scratch / "source.rgb").write_bytes(source.tobytes())
    paths = {name: args.output / filename for name, filename in (
        ("DotJpegli", "dotjpegli.jpg"), ("Google jpegli", "google-jpegli.jpg"),
        ("mozjpeg", "mozjpeg.jpg"), ("libjpeg-turbo", "libjpeg-turbo.jpg"),
        ("WebP", "webp.webp"), ("AVIF", "avif.avif"), ("JPEG XL", "jpeg-xl.jxl"))}
    run(args.dotnet, "run", "--file", args.dotjpegli.resolve(), "-c", "Release", "--", (args.scratch / "source.rgb").resolve(), paths["DotJpegli"].resolve(), str(source.width), str(source.height))
    if run(args.jpegli, "version") != PIN:
        raise ValueError("Unexpected native jpegli version")
    run(args.jpegli, "encode", (args.scratch / "source.ppm").resolve(), paths["Google jpegli"].resolve(), "1", "420", "2", "1", "1")
    run(args.mozjpeg, "-quality", "90", "-sample", "2x2", "-progressive", "-optimize", "-outfile", paths["mozjpeg"].resolve(), (args.scratch / "source.ppm").resolve())
    source.save(paths["libjpeg-turbo"], format="JPEG", quality=90, subsampling=2, progressive=True, optimize=True)
    source.save(paths["WebP"], format="WEBP", quality=90, method=6)
    run(args.avifenc, "--codec", "aom", "--qcolor", "90", "--speed", "6", "--jobs", "1", "--depth", "8", "--yuv", "420", "--cicp", "1/13/6", "--range", "full", (args.output / "source.png").resolve(), paths["AVIF"].resolve())
    run(args.cjxl, (args.output / "source.png").resolve(), paths["JPEG XL"].resolve(), "--distance=1", "--effort=7", "--num_threads=1")
    decodes = {}
    for label, path in paths.items():
        decoded = args.scratch / (path.stem + ".png")
        if label == "AVIF":
            run(args.avifdec, "--depth", "8", path.resolve(), decoded.resolve())
        elif label == "JPEG XL":
            run(args.djxl, path.resolve(), decoded.resolve(), "--num_threads=1")
        else:
            with Image.open(path) as image:
                image.convert("RGB").save(decoded)
        with Image.open(decoded) as image:
            if image.size != source.size:
                raise ValueError("Decoded dimensions differ from the common source")
            decodes[label] = image.convert("RGB").copy()
    versions = {
        "DotJpegli": "2.0.0",
        "Google jpegli": PIN,
        "mozjpeg": "4.1.1",
        "libjpeg-turbo": features.version("libjpeg_turbo"),
        "WebP": features.version("webp"),
        "AVIF": "libavif 1.4.2 / libaom 3.14.1",
        "JPEG XL": "libjxl 0.12.0 (4128790)",
    }
    banners = {"mozjpeg": run(args.mozjpeg, "-version", capture_all=True), "libavif": run(args.avifenc, "--version", capture_all=True), "libjxl": run(args.cjxl, "--version", capture_all=True)}
    for name, expected in (("mozjpeg", "version 4.1.1"), ("libavif", "Version: 1.4.2"), ("libavif", "aom [enc]:3.14.1"), ("libjxl", "v0.12.0 4128790")):
        if expected not in banners[name]:
            raise ValueError(f"Unexpected {name} version: {banners[name]}")
    library = args.dotjpegli.resolve().parent.parent / "src/DotJpegli/bin/Release/net11.0/DotJpegli.dll"
    settings = {
        "DotJpegli": "distance 1, 4:2:0, progressive level 2, AQ and optimized Huffman, one worker",
        "Google jpegli": "distance 1, 4:2:0, progressive level 2, AQ and optimized Huffman",
        "mozjpeg": "quality 90, 4:2:0, progressive, optimized Huffman, default trellis",
        "libjpeg-turbo": "quality 90, 4:2:0, progressive, optimized Huffman (Pillow)",
        "WebP": "quality 90, method 6 (Pillow)",
        "AVIF": "quality 90, speed 6, libaom, 8-bit 4:2:0 full-range BT.601, sRGB primaries/transfer, one worker",
        "JPEG XL": "distance 1, effort 7, one worker",
    }
    results = [{"encoder": "Source PNG", "file": "source.png", "bytes": (args.output / "source.png").stat().st_size, "sha256": digest(args.output / "source.png"), "ssimulacra2": 100.0}]
    for label, path in paths.items():
        score = float(run(args.ssimulacra2, (args.output / "source.png").resolve(), (args.scratch / (path.stem + ".png")).resolve()))
        results.append({"encoder": label, "version": versions[label], "settings": settings[label], "file": path.name, "bytes": path.stat().st_size, "sha256": digest(path), "ssimulacra2": score})

    # Eight corresponding 480x300 crops in a compact 2-column contact sheet.
    # Crops keep their exact pixels; no resizing or JPEG recompression is used.
    crop = (850, 420, 1330, 720)
    width, height, gap, label_height = 480, 300, 20, 62
    sheet = Image.new("RGB", (1020, 4 * (height + label_height + gap) + 88), "#0b1020")
    draw = ImageDraw.Draw(sheet)
    font = ImageFont.load_default(size=19)
    title_font = ImageFont.load_default(size=25)
    draw.text((20, 18), "COSMIC CLIFFS / detail at one source pixel per pixel", font=title_font, fill="#f2f5fb")
    for index, result in enumerate(results):
        label = result["encoder"]
        x, y = 20 + (index % 2) * (width + gap), 66 + (index // 2) * (height + label_height + gap)
        image = source if index == 0 else decodes[label]
        sheet.paste(image.crop(crop), (x, y + label_height))
        draw.text((x, y + 3), label, font=title_font, fill="#83dfd1" if label == "DotJpegli" else "#f2f5fb")
        size = f"{result['bytes']:,} bytes / {result['bytes'] / 1024:.1f} KiB"
        draw.text((x, y + 33), size, font=font, fill="#b6c4d9")
    draw.text((20, sheet.height - 24), "NASA, ESA, CSA, and STScI / CC BY 4.0 / cropped and re-encoded", font=ImageFont.load_default(size=16), fill="#b6c4d9")
    sheet.save(args.output / "detail-comparison.png", compress_level=9)
    manifest = {
        "schemaVersion": 1, "purpose": "Untimed single-photograph illustration; selected settings are not matched perceptual quality and do not rank encoders universally.",
        "managedSourceRevision": args.revision,
        "managedLibrarySha256": digest(library),
        "source": {"title": "NIRCam Image of the Cosmic Cliffs in Carina", "page": "https://esawebb.org/images/weic2205a/", "download": SOURCE_URL, "sha256": SOURCE_SHA256, "bytes": args.source_tiff.stat().st_size, "dimensions": original_dimensions, "profile": profile_name, "credit": "NASA, ESA, CSA, and STScI", "license": "https://creativecommons.org/licenses/by/4.0/", "licensePolicy": "https://esawebb.org/copyright/"},
        "preparation": {"pillow": __version__, "dimensions": list(source.size), "method": "sRGB RGB8 TIFF samples; Pillow Lanczos resize to 2400x1390; strip metadata and ICC (sRGB assumed); lossless PNG level 9 and binary P6 PPM share identical RGB samples", "rgbSha256": hashlib.sha256(source.tobytes()).hexdigest()},
        "measurement": {"size": "Complete on-disk file length including all headers and metadata; KiB = bytes / 1024", "quality": "SSIMULACRA2 from official libjxl 0.12.0 (4128790), common source PNG vs full-size decoded RGB PNG; JPEGs decoded by Pillow/libjpeg-turbo; AVIF by avifdec/dav1d; JPEG XL by djxl", "crop": list(crop), "timingsCollected": False, "host": platform.platform()},
        "tools": {name: {"file": getattr(args, name).name, "sha256": digest(getattr(args, name))} for name in ("dotjpegli", "jpegli", "mozjpeg", "cjxl", "djxl", "avifenc", "avifdec", "ssimulacra2")},
        "versionBanners": banners,
        "results": results,
        "assets": [{"file": "detail-comparison.png", "bytes": (args.output / "detail-comparison.png").stat().st_size, "sha256": digest(args.output / "detail-comparison.png")}],
    }
    (args.output / "comparison.json").write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8", newline="\n")
    (args.scratch / "commands.json").write_text(json.dumps(commands, indent=2) + "\n", encoding="utf-8", newline="\n")
    for result in results:
        print(f"{result['encoder']}: {result['bytes']:,} bytes / {result['bytes'] / 1024:.1f} KiB / SSIMULACRA2 {result['ssimulacra2']:.2f}")


if __name__ == "__main__":
    main()
