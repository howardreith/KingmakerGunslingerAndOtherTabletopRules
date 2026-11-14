#!/usr/bin/env python3
"""Compile the Kingmaker mod against an extracted private reference bundle.

The resulting ZIP is a compile candidate only. It is not runtime-qualified until
Kingmaker loads it and the in-game preflight/evidence workflow passes.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
import shutil
import subprocess
import sys
import xml.etree.ElementTree as ET
import zipfile
from pathlib import Path

MSBUILD_NS = {"m": "http://schemas.microsoft.com/developer/msbuild/2003"}
FIXED_ZIP_TIME = (2026, 1, 1, 0, 0, 0)


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--reference-bundle-dir", required=True, type=Path)
    parser.add_argument("--dotnet", required=True, type=Path)
    parser.add_argument("--csc", required=True, type=Path)
    parser.add_argument("--net47-ref-dir", required=True, type=Path)
    parser.add_argument("--output-dir", required=True, type=Path)
    parser.add_argument("--configuration", choices=("Debug", "Release"), default="Release")
    parser.add_argument("--git-commit", required=True)
    # Loader assemblies come only from the pinned floor, never from the bundle
    # or the installed loader: see scripts/loader-reference-pins.json.
    parser.add_argument("--loader-reference-dir", required=True, type=Path)
    parser.add_argument("--loader-pins", type=Path,
                        default=Path(__file__).resolve().parents[1] / "scripts" / "loader-reference-pins.json")
    return parser.parse_args()


def pinned_loader_references(loader_dir: Path, pins_path: Path) -> tuple[list[Path], dict]:
    """Every pinned loader reference, verified against its pin. A missing or
    differing file is an error; there is no substitute."""
    pins = json.loads(pins_path.read_text(encoding="utf-8-sig"))
    expected = ["UnityModManager/UnityModManager.dll", "UnityModManager/0Harmony12.dll"]
    if not isinstance(pins, dict):
        raise ValueError("Loader reference pins must be a JSON object.")
    rows = pins.get("references")
    if (pins.get("schemaVersion") != 1 or not isinstance(rows, list) or
            [row.get("relativePath") for row in rows] != expected):
        raise ValueError("Loader reference pins must name exactly: " + ", ".join(expected))
    paths: list[Path] = []
    identity = {"pinsSha256": sha256(pins_path),
                "minimumManagerVersion": pins.get("minimumManagerVersion"), "files": {}}
    for row in rows:
        path = loader_dir / row["relativePath"]
        if not path.is_file():
            raise FileNotFoundError(f"Pinned loader reference is missing: {path}")
        actual = sha256(path)
        if actual != row.get("sha256"):
            raise ValueError(f"Pinned loader reference differs from its pin: {row['relativePath']}; "
                             f"pinnedSha256={row.get('sha256')}; actualSha256={actual}")
        paths.append(path)
        identity["files"][row["relativePath"]] = {
            "sha256": actual, "assemblyVersion": row.get("assemblyVersion"), "source": row.get("url")}
    return paths, identity


def compile_files(project: Path) -> list[Path]:
    tree = ET.parse(project)
    files: list[Path] = []
    for node in tree.findall(".//m:Compile", MSBUILD_NS):
        include = node.attrib.get("Include")
        if not include:
            continue
        path = (project.parent / include.replace("\\", "/")).resolve()
        if not path.is_file():
            raise FileNotFoundError(f"Compile item does not exist: {include} -> {path}")
        files.append(path)
    if not files or len(files) != len(set(files)):
        raise RuntimeError("Main project compile items are empty or duplicated.")
    return files


def locate_bundle_root(path: Path) -> Path:
    path = path.resolve()
    candidates = [path, path / "KingmakerGunslinger-private-build-references"]
    for candidate in candidates:
        if (candidate / "Managed" / "Assembly-CSharp.dll").is_file():
            return candidate
    raise FileNotFoundError("Could not find Managed/Assembly-CSharp.dll in the private bundle directory.")


def write_deterministic_archive(zip_path: Path, package_root: Path, output: Path) -> None:
    """Write the compile candidate without inheriting compiler file mtimes."""
    files = sorted((path for path in package_root.rglob("*") if path.is_file()), key=lambda path: path.as_posix())
    with zipfile.ZipFile(
        zip_path,
        "w",
        compression=zipfile.ZIP_DEFLATED,
        compresslevel=9,
        strict_timestamps=True,
    ) as archive:
        for file in files:
            relative = file.relative_to(output).as_posix()
            info = zipfile.ZipInfo(relative, FIXED_ZIP_TIME)
            info.compress_type = zipfile.ZIP_DEFLATED
            info.create_system = 0
            info.external_attr = 0
            archive.writestr(
                info,
                file.read_bytes(),
                compress_type=zipfile.ZIP_DEFLATED,
                compresslevel=9,
            )


def main() -> int:
    args = parse_args()
    root = Path(__file__).resolve().parents[1]
    project = root / "src" / "KingmakerGunslinger" / "KingmakerGunslinger.csproj"
    info = json.loads((root / "Info.json").read_text(encoding="utf-8"))
    bundle = locate_bundle_root(args.reference_bundle_dir)
    managed = bundle / "Managed"

    private_refs = [
        managed / "Assembly-CSharp.dll",
        managed / "Assembly-CSharp-firstpass.dll",
        managed / "UniRx.dll",
        managed / "Newtonsoft.Json.dll",
        managed / "UnityEngine.dll",
        managed / "UnityEngine.AnimationModule.dll",
        managed / "UnityEngine.AudioModule.dll",
        managed / "UnityEngine.AssetBundleModule.dll",
        managed / "UnityEngine.CoreModule.dll",
        managed / "UnityEngine.ClothModule.dll",
        managed / "UnityEngine.PhysicsModule.dll",
        managed / "UnityEngine.UI.dll",
        managed / "UnityEngine.UIModule.dll",
        managed / "UnityEngine.TextRenderingModule.dll",
    ]
    loader_refs, loader_identity = pinned_loader_references(args.loader_reference_dir.resolve(),
                                                            args.loader_pins.resolve())
    framework_refs = [args.net47_ref_dir / name for name in ("mscorlib.dll", "System.dll", "System.Core.dll", "System.Xml.dll")]
    for path in [args.dotnet, args.csc, *framework_refs, *private_refs, *loader_refs]:
        if not path.is_file():
            raise FileNotFoundError(path)

    output = args.output_dir.resolve()
    bin_dir = output / "bin"
    package_root = output / info["Id"]
    shutil.rmtree(output, ignore_errors=True)
    bin_dir.mkdir(parents=True)
    package_root.mkdir(parents=True)
    if not all(character in "0123456789abcdef" for character in args.git_commit.lower()) or \
            len(args.git_commit) != 40:
        raise ValueError("--git-commit must be one full hexadecimal commit.")
    generated_identity = output / "GeneratedBuildIdentity.cs"
    generated_identity.write_text(
        "using System.Reflection;\n"
        f'[assembly: AssemblyMetadata("GitCommit", "{args.git_commit.lower()}")]\n',
        encoding="utf-8",
        newline="\n",
    )

    dll = bin_dir / info["AssemblyName"]
    optimize = args.configuration == "Release"
    compiler_arguments = [
        "/noconfig", "/nostdlib+", "/langversion:7.3", "/target:library",
        "/warnaserror+", "/deterministic+", "/utf8output", "/platform:anycpu",
        "/debug:pdbonly" if optimize else "/debug:portable",
        "/optimize+" if optimize else "/optimize-",
        "/define:TRACE" if optimize else "/define:DEBUG;TRACE",
        f"/out:{dll}",
        *(f"/reference:{path}" for path in [*framework_refs, *private_refs, *loader_refs]),
        *(str(path) for path in compile_files(project)),
        str(generated_identity),
    ]
    response_file = output / "compile.rsp"
    response_file.write_text(
        "\n".join('"' + value.replace('"', '\\"') + '"'
                  for value in compiler_arguments) + "\n",
        encoding="utf-8",
        newline="\n",
    )
    command = [str(args.dotnet), str(args.csc), f"@{response_file}"]
    env = os.environ.copy()
    env["DOTNET_ROOT"] = str(args.dotnet.parent)
    env["DOTNET_ROLL_FORWARD"] = "Major"
    completed = subprocess.run(command, cwd=root, env=env, text=True, capture_output=True, check=False)
    (output / "compile.stdout.txt").write_text(completed.stdout, encoding="utf-8", newline="\n")
    (output / "compile.stderr.txt").write_text(completed.stderr, encoding="utf-8", newline="\n")
    if completed.returncode != 0:
        print(completed.stdout, end="")
        print(completed.stderr, end="", file=sys.stderr)
        return completed.returncode

    shutil.copy2(dll, package_root / dll.name)
    shutil.copy2(root / "Info.json", package_root / "Info.json")
    blueprint_dir = package_root / "blueprints"
    blueprint_dir.mkdir(exist_ok=True)
    shutil.copy2(root / "blueprints" / "blueprints.json", blueprint_dir / "blueprints.json")
    shutil.copy2(root / "blueprints" / "blueprints.schema.json", blueprint_dir / "blueprints.schema.json")
    icon_dir = package_root / "assets" / "icons"
    icon_dir.mkdir(parents=True, exist_ok=True)
    for icon in sorted((root / "assets" / "game" / "icons").glob("*.png")):
        shutil.copy2(icon, icon_dir / icon.name)

    zip_path = output / f'{info["Id"]}-{info["Version"]}-compile-candidate.zip'
    write_deterministic_archive(zip_path, package_root, output)

    report = {
        "schemaVersion": 1,
        "classification": "ready-for-expanded-summoning-release-after-exact-reference-compile",
        "configuration": args.configuration,
        "modVersion": info["Version"],
        "compileExitCode": completed.returncode,
        "modDllSha256": sha256(dll),
        "gitCommit": args.git_commit.lower(),
        "packageSha256": sha256(zip_path),
        "blueprintManifestSha256": sha256(root / "blueprints" / "blueprints.json"),
        "privateReferenceHashes": {path.relative_to(bundle).as_posix(): sha256(path) for path in private_refs},
        "pinnedLoaderReferences": loader_identity,
        "readyForKingmakerSmokeTest": True,
        "nextSprintGateDecision": "Sprint30Blocked_PendingCompleteMaintenanceLoopRuntimeGate",
    }
    (output / "compile-candidate.json").write_text(json.dumps(report, indent=2, sort_keys=True) + "\n", encoding="utf-8")
    (output / (zip_path.name + ".sha256")).write_text(f"{report['packageSha256']}  {zip_path.name}\n", encoding="ascii")
    print(zip_path)
    print("Compile candidate produced; Kingmaker runtime qualification is still required.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
