#!/usr/bin/env python3
"""Compare two retained CI performance diagnostics; never run benchmarks.

--output is a file prefix: reports are written to PREFIX.md and PREFIX.json.
--self-test validates the comparator with synthetic evidence only.
"""

import argparse
import copy
import json
import math
import re
import sys
from pathlib import Path
from statistics import mean


HOST_FIELDS = (
    "cpu", "affinity", "os", "osArchitecture", "processArchitecture",
    "runtimeProductVersion", "encoderTargetFramework", "explicitNet11Simd",
    "logicalProcessors", "frequency", "isHighResolution", "isHardwareAccelerated",
    "vectorFloatWidth", "vector128", "vector256", "vector512", "serverGc",
    "gcLatencyMode", "dotnetTieredCompilation", "complusTieredCompilation",
    "instrumentation", "githubRunnerOs", "githubRunnerArchitecture",
)


def require(condition, message):
    if not condition:
        raise ValueError(message)


def positive_number(value, label):
    require(type(value) in (int, float) and math.isfinite(value) and value > 0,
            f"{label} must be finite and positive")
    return value


def sha256(value, label):
    require(isinstance(value, str) and re.fullmatch(r"[0-9a-f]{64}", value),
            f"Invalid {label} SHA-256")
    return value


def case_key(case):
    require(isinstance(case["id"], str) and case["id"], "Missing case image ID")
    require(type(case["sampling"]) is int and type(case["progressiveLevel"]) is int,
            "Invalid case sampling/progression")
    positive_number(case["distance"], "Case distance")
    return case["id"], case["sampling"], case["progressiveLevel"], case["distance"]


def indexed(items, key, label):
    require(isinstance(items, list) and items, f"Missing {label}")
    result = {key(item): item for item in items}
    require(len(result) == len(items), f"Duplicate {label}")
    return result


def allocated_per_encode(case, block_count):
    iterations = case["iterations"]
    require(type(iterations) is int and iterations > 0, "Invalid encode iteration count")
    blocks = case["blocks"]
    require(isinstance(blocks, list) and len(blocks) == block_count,
            "Allocation blocks do not match the evidence policy")
    for block in blocks:
        require(type(block["allocatedManagedBytes"]) is int and block["allocatedManagedBytes"] >= 0,
                "Allocated bytes must be nonnegative integers")
    return mean(block["allocatedManagedBytes"] / iterations for block in blocks)


def compare(before, after, baseline_revision, current_revision):
    for evidence, revision in ((before, baseline_revision), (after, current_revision)):
        run, summary = evidence["run"], evidence["summary"]
        require(summary["valid"] is True, "Both comparisons require stable matched evidence")
        require(summary["mode"] == "diagnostic", "Expected selected-case diagnostic evidence")
        require(run["testedRevision"] == revision and summary["testedRevision"] == revision,
                "Tested revision mismatch")
        require(type(run["expectedCases"]) is int and run["expectedCases"] > 0,
                "Invalid expected case count")
        require(summary["selectedCoverageComplete"] is True
                and summary["cases"] == run["expectedCases"], "Incomplete selected-case evidence")
        require(run["environment"]["instrumentation"] is None,
                "Instrumented timings cannot be used for this comparison")
        for field in ("managedEncoderSha256", "managedRunnerSha256",
                      "nativeExecutableSha256", "corpusManifestSha256"):
            sha256(run[field], field)
    left, right = before["run"], after["run"]
    require(left["policy"] == right["policy"], "Evidence policy mismatch")
    block_count = left["policy"]["blocks"]
    require(type(block_count) is int and block_count > 0, "Invalid policy block count")
    require(left["githubRunId"] and left["githubRunId"] == right["githubRunId"],
            "Comparisons must belong to the same CI run")
    for field in ("upstreamPin", "nativeExecutableSha256", "corpusManifestSha256"):
        require(left[field] == right[field], f"Reference/input mismatch: {field}")
    for field in HOST_FIELDS:
        require(left["environment"][field] == right["environment"][field],
                f"Host/runtime mismatch: {field}")
    require(re.fullmatch(r"[0-9]+", str(left["environment"]["affinity"])),
            "Comparisons must be pinned to one CPU")

    old = indexed(before["cases"], case_key, "baseline cases")
    new = indexed(after["cases"], case_key, "current cases")
    require(old.keys() == new.keys() and len(old) == left["expectedCases"]
            and len(new) == right["expectedCases"], "Selected-case mismatch")
    old_sources = indexed(before["sources"], lambda source: source["id"], "baseline sources")
    new_sources = indexed(after["sources"], lambda source: source["id"], "current sources")
    require(old_sources.keys() == new_sources.keys() == {key[0] for key in old},
            "Selected source-image mismatch")
    for image_id in old_sources:
        for field in ("category", "width", "height", "ppmSha256", "pixelsSha256"):
            require(old_sources[image_id][field] == new_sources[image_id][field],
                    f"Source-image mismatch: {image_id}/{field}")
        for field in ("ppmSha256", "pixelsSha256"):
            sha256(old_sources[image_id][field], f"{image_id}/{field}")

    comparisons = []
    for label in sorted(new):
        b, a = old[label], new[label]
        for field in ("managedJpegSha256", "nativeJpegSha256"):
            sha256(b[field], field)
            require(b[field] == a[field], f"JPEG bytes changed: {label}/{field}")
        bm = positive_number(b["managed"]["median"], "Baseline managed median")
        am = positive_number(a["managed"]["median"], "Current managed median")
        bn = positive_number(b["native"]["median"], "Baseline native median")
        an = positive_number(a["native"]["median"], "Current native median")
        comparisons.append({
            "case": label, "baselineManagedNs": bm, "currentManagedNs": am,
            "managedRatio": am / bm, "nativeRatio": an / bn,
            "baselineAllocatedBytesPerEncode": allocated_per_encode(b, block_count),
            "currentAllocatedBytesPerEncode": allocated_per_encode(a, block_count),
        })
    return {"baselineRevision": baseline_revision, "currentRevision": current_revision,
            "githubRunId": left["githubRunId"], "environment": right["environment"],
            "policy": right["policy"], "comparisons": comparisons}


def markdown(result):
    environment = result["environment"]
    report = ("# Same-host encoder development comparison\n\n"
              f"Baseline `{result['baselineRevision']}`, current `{result['currentRevision']}`; "
              f"CPU: {environment['cpu']}; affinity: {environment['affinity']}; "
              f"Vector128/256/512: {environment['vector128']}/{environment['vector256']}/{environment['vector512']}.\n\n"
              "Both uninstrumented diagnostics use identical input and JPEG hashes, the same pinned native executable, "
              "CPU affinity, runtime, SIMD capabilities and evidence policy. Revisions are measured separately; "
              "their samples are not interleaved. Native-calibrated iteration counts and raw samples are retained. "
              "This diagnostic cannot replace the current revision's managed/native bounds or establish whole-corpus parity.\n\n"
              "Allocated bytes are the mean per-block managed allocation total divided by encodes per block; "
              "pool capacity and peak working set are separate quantities.\n\n"
              "| Case | Baseline managed (ms) | Current managed (ms) | Current/baseline managed | "
              "Current/baseline native | Baseline allocated B/encode | Current allocated B/encode |\n"
              "| --- | ---: | ---: | ---: | ---: | ---: | ---: |\n")
    for item in result["comparisons"]:
        image, sampling, progression, distance = item["case"]
        report += (f"| {image}/{sampling}/p{progression}/d{distance:g} | "
                   f"{item['baselineManagedNs'] / 1e6:.3f} | {item['currentManagedNs'] / 1e6:.3f} | "
                   f"{item['managedRatio']:.6f} | {item['nativeRatio']:.6f} | "
                   f"{item['baselineAllocatedBytesPerEncode']:.1f} | {item['currentAllocatedBytesPerEncode']:.1f} |\n")
    return report


def read_evidence(directory):
    return {name: json.loads((directory / f"{name}.json").read_text(encoding="utf-8-sig"))
            for name in ("run", "summary", "cases", "sources")}


def self_test():
    baseline_revision, current_revision = "1" * 40, "2" * 40
    environment = {field: None for field in HOST_FIELDS}
    environment.update(cpu="Synthetic CPU", affinity="0", vector128=True, vector256=True, vector512=False)
    before = {
        "run": {"testedRevision": baseline_revision, "expectedCases": 1, "policy": {"blocks": 3},
                "githubRunId": "synthetic", "upstreamPin": "3" * 40, "environment": environment,
                **{field: "a" * 64 for field in ("managedEncoderSha256", "managedRunnerSha256",
                                                 "nativeExecutableSha256", "corpusManifestSha256")}},
        "summary": {"valid": True, "mode": "diagnostic", "testedRevision": baseline_revision,
                    "selectedCoverageComplete": True, "cases": 1},
        "sources": [{"id": "synthetic", "category": "controlled", "width": 8, "height": 8,
                     "ppmSha256": "b" * 64, "pixelsSha256": "c" * 64}],
        "cases": [{"id": "synthetic", "sampling": 420, "progressiveLevel": 0, "distance": 1,
                   "iterations": 5, "blocks": [{"allocatedManagedBytes": n} for n in (2000, 2200, 2400)],
                   "managed": {"median": 100}, "native": {"median": 125},
                   "managedJpegSha256": "d" * 64, "nativeJpegSha256": "e" * 64}],
    }
    after = copy.deepcopy(before)
    after["run"]["testedRevision"] = after["summary"]["testedRevision"] = current_revision
    after["cases"][0].update(iterations=10, managed={"median": 80},
                             blocks=[{"allocatedManagedBytes": n} for n in (3000, 3300, 3600)])
    item = compare(before, after, baseline_revision, current_revision)["comparisons"][0]
    require(item["managedRatio"] == 0.8 and item["nativeRatio"] == 1
            and item["baselineAllocatedBytesPerEncode"] == 440
            and item["currentAllocatedBytesPerEncode"] == 330, "Allocation/ratio normalization self-test failed")
    mutations = [
        lambda e: e["summary"].update(valid=False),
        lambda e: e["summary"].update(selectedCoverageComplete=False),
        lambda e: e["run"]["policy"].update(blocks=4),
        lambda e: e["run"].update(testedRevision="0" * 40),
        lambda e: e["run"].update(nativeExecutableSha256="f" * 64),
        lambda e: e["run"].update(corpusManifestSha256="f" * 64),
        lambda e: e["run"].update(githubRunId="different"),
        lambda e: e["run"]["environment"].update(instrumentation="profiler"),
        lambda e: e["sources"][0].update(pixelsSha256="f" * 64),
        lambda e: e["cases"][0].update(managedJpegSha256="f" * 64),
        lambda e: e["cases"][0].update(nativeJpegSha256="f" * 64),
        lambda e: e["cases"].append(copy.deepcopy(e["cases"][0])),
        lambda e: e["cases"][0].update(iterations=0),
        lambda e: e["cases"][0]["blocks"][0].update(allocatedManagedBytes=-1),
    ]
    for field in ("vector128", "vector256", "vector512"):
        mutations.append(lambda e, field=field: e["run"]["environment"].update({field: not environment[field]}))
    for mutate in mutations:
        invalid = copy.deepcopy(after)
        mutate(invalid)
        try:
            compare(before, invalid, baseline_revision, current_revision)
        except ValueError:
            continue
        raise ValueError("Mismatch rejection self-test failed")
    print(f"Performance revision comparator self-test passed ({len(mutations)} rejected mismatches; synthetic data only).")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--baseline", type=Path)
    parser.add_argument("--current", type=Path)
    parser.add_argument("--baseline-revision")
    parser.add_argument("--current-revision")
    parser.add_argument("--output", type=Path, help="Output prefix for .md and .json reports")
    parser.add_argument("--summary", type=Path, help="Append Markdown to this job-summary file")
    parser.add_argument("--self-test", action="store_true")
    args = parser.parse_args()
    if args.self_test:
        self_test()
        return
    for name in ("baseline", "current", "baseline_revision", "current_revision", "output"):
        if getattr(args, name) is None:
            parser.error(f"--{name.replace('_', '-')} is required")
    for revision in (args.baseline_revision, args.current_revision):
        require(re.fullmatch(r"[0-9a-f]{40}", revision), "Revision arguments must be full lowercase Git SHA-1 IDs")
    result = compare(read_evidence(args.baseline), read_evidence(args.current),
                     args.baseline_revision, args.current_revision)
    report = markdown(result)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    Path(str(args.output) + ".md").write_text(report, encoding="utf-8")
    Path(str(args.output) + ".json").write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    if args.summary is not None:
        with args.summary.open("a", encoding="utf-8") as output:
            output.write(report)
    print(report)


if __name__ == "__main__":
    try:
        main()
    except (OSError, ValueError, KeyError, TypeError) as error:
        print(f"Performance revision comparison invalid: {error}", file=sys.stderr)
        sys.exit(1)
