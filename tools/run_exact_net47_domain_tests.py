#!/usr/bin/env python3
"""Compile the dependency-free harness against .NET Framework 4.7 refs and execute it on CoreCLR.

This is an evidence tool, not the Kingmaker build. It proves that the checked-in
C# 7.3 sources compile against the exact net47 reference surface without warnings,
then executes the resulting IL on a supplied modern dotnet runtime.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

MSBUILD_NS = {"m": "http://schemas.microsoft.com/developer/msbuild/2003"}
SUMMARY_RE = re.compile(r"^Completed (\d+) tests; failures=(\d+)\.$")


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--dotnet", required=True, type=Path)
    parser.add_argument("--csc", required=True, type=Path)
    parser.add_argument("--net47-ref-dir", required=True, type=Path)
    parser.add_argument("--output-dir", required=True, type=Path)
    parser.add_argument("--runtime-version", default="8.0.0")
    parser.add_argument("--runs", type=int, default=3)
    return parser.parse_args()


def compile_files(project: Path) -> list[Path]:
    tree = ET.parse(project)
    result: list[Path] = []
    for node in tree.findall(".//m:Compile", MSBUILD_NS):
        include = node.attrib.get("Include")
        if not include:
            continue
        path = (project.parent / include.replace("\\", "/")).resolve()
        if not path.is_file():
            raise FileNotFoundError(f"Compile item does not exist: {include} -> {path}")
        result.append(path)
    if not result or len(result) != len(set(result)):
        raise RuntimeError("Compile-item list is empty or contains duplicates.")
    return result


def main() -> int:
    args = parse_args()
    if args.runs < 1:
        raise ValueError("--runs must be at least one.")
    for path in (args.dotnet, args.csc):
        if not path.is_file():
            raise FileNotFoundError(path)

    refs = [args.net47_ref_dir / name for name in ("mscorlib.dll", "System.dll", "System.Core.dll")]
    for path in refs:
        if not path.is_file():
            raise FileNotFoundError(path)

    root = Path(__file__).resolve().parents[1]
    project = root / "tests" / "KingmakerGunslinger.DomainTests" / "KingmakerGunslinger.DomainTests.csproj"
    sources = compile_files(project)
    case_count = len(re.findall(r'Case\("[^"]+",\s*[A-Za-z0-9_]+\)', (project.parent / "Program.cs").read_text(encoding="utf-8")))

    output = args.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=True)
    executable = output / "KingmakerGunslinger.DomainTests.exe"

    command = [
        str(args.dotnet),
        str(args.csc),
        "/noconfig",
        "/nostdlib+",
        "/langversion:7.3",
        "/target:exe",
        "/optimize+",
        "/debug:pdbonly",
        "/warnaserror+",
        "/define:TRACE",
        "/deterministic+",
        "/utf8output",
        f"/out:{executable}",
        *(f"/reference:{path}" for path in refs),
        *(str(path) for path in sources),
    ]
    env = os.environ.copy()
    env["DOTNET_ROOT"] = str(args.dotnet.parent)
    env["DOTNET_ROLL_FORWARD"] = "Major"
    compiled = subprocess.run(command, cwd=root, env=env, text=True, capture_output=True, check=False)
    (output / "compile.stdout.txt").write_text(compiled.stdout, encoding="utf-8", newline="\n")
    (output / "compile.stderr.txt").write_text(compiled.stderr, encoding="utf-8", newline="\n")
    if compiled.returncode != 0:
        print(compiled.stdout, end="")
        print(compiled.stderr, end="", file=sys.stderr)
        return compiled.returncode

    runtime_major = int(args.runtime_version.split(".", 1)[0])
    runtime_tfm = "netcoreapp3.1" if runtime_major == 3 else "net{0}.0".format(runtime_major)
    runtime_config = {
        "runtimeOptions": {
            "tfm": runtime_tfm,
            "framework": {"name": "Microsoft.NETCore.App", "version": args.runtime_version},
            "rollForward": "Major",
        }
    }
    runtime_config_path = output / "KingmakerGunslinger.DomainTests.runtimeconfig.json"
    runtime_config_path.write_text(json.dumps(runtime_config, separators=(",", ":")) + "\n", encoding="utf-8")

    run_hashes: list[str] = []
    for number in range(1, args.runs + 1):
        completed = subprocess.run(
            [str(args.dotnet), str(executable)],
            cwd=root,
            env=env,
            text=True,
            capture_output=True,
            check=False,
        )
        stdout = output / f"run{number}.stdout.txt"
        stderr = output / f"run{number}.stderr.txt"
        stdout.write_text(completed.stdout, encoding="utf-8", newline="\n")
        stderr.write_text(completed.stderr, encoding="utf-8", newline="\n")
        if completed.returncode != 0:
            print(completed.stdout, end="")
            print(completed.stderr, end="", file=sys.stderr)
            return completed.returncode
        lines = completed.stdout.splitlines()
        match = SUMMARY_RE.match(lines[-1] if lines else "")
        if not match or int(match.group(1)) != case_count or int(match.group(2)) != 0:
            raise RuntimeError("Unexpected test summary: " + (lines[-1] if lines else "<empty>"))
        run_hashes.append(sha256(stdout))

    if len(set(run_hashes)) != 1:
        raise RuntimeError("Repeated test output was not byte-identical.")

    source_digest = hashlib.sha256()
    for source in sorted(sources, key=lambda p: str(p)):
        source_digest.update(str(source.relative_to(root)).encode("utf-8"))
        source_digest.update(b"\0")
        source_digest.update(source.read_bytes())
        source_digest.update(b"\0")

    report = {
        "schemaVersion": 1,
        "targetFrameworkReferenceSurface": ".NET Framework 4.7",
        "languageVersion": "7.3",
        "warningsAsErrors": True,
        "compileExitCode": compiled.returncode,
        "declaredTests": case_count,
        "runs": args.runs,
        "failures": 0,
        "repeatedOutputIdentical": True,
        "testExecutableSha256": sha256(executable),
        "testOutputSha256": run_hashes[0],
        "sourceSetSha256": source_digest.hexdigest(),
        "compilerSha256": sha256(args.csc),
        "referenceAssemblies": {path.name: sha256(path) for path in refs},
    }
    (output / "executed-test-evidence.json").write_text(
        json.dumps(report, indent=2, sort_keys=True) + "\n", encoding="utf-8"
    )
    print(f"Compiled against net47 and completed {case_count} tests x {args.runs}; failures=0.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
