#!/usr/bin/env python3
"""Run the dependency-free C# test harness with the .NET 8 SDK.

The checked-in test project deliberately remains a classic .NET Framework 4.7
project because that is the Kingmaker baseline. This tool reads its explicit
Compile items and materializes a temporary SDK-style project without changing
or duplicating the source-of-truth project file.
"""
from __future__ import annotations

import argparse
import shutil
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

MSBUILD_NS = {"m": "http://schemas.microsoft.com/developer/msbuild/2003"}


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("--configuration", choices=("Debug", "Release"), default="Release")
    parser.add_argument("--keep-generated-project", action="store_true")
    return parser.parse_args()


def compile_files(project: Path) -> list[Path]:
    tree = ET.parse(project)
    files: list[Path] = []
    for node in tree.findall(".//m:Compile", MSBUILD_NS):
        include = node.attrib.get("Include")
        if not include:
            continue
        candidate = (project.parent / include.replace("\\", "/")).resolve()
        if not candidate.is_file():
            raise FileNotFoundError(f"Compile item does not exist: {include} -> {candidate}")
        files.append(candidate)
    if not files:
        raise RuntimeError("The classic test project has no Compile items.")
    if len(set(files)) != len(files):
        raise RuntimeError("The classic test project contains duplicate Compile items.")
    return files


def xml_escape(value: str) -> str:
    return (
        value.replace("&", "&amp;")
        .replace("<", "&lt;")
        .replace(">", "&gt;")
        .replace('"', "&quot;")
    )


def write_sdk_project(path: Path, files: list[Path]) -> None:
    compile_items = "\n".join(
        f'    <Compile Include="{xml_escape(str(file))}" Link="Source/{index:03d}-{xml_escape(file.name)}" />'
        for index, file in enumerate(files, start=1)
    )
    content = f"""<Project Sdk=\"Microsoft.NET.Sdk\">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>7.3</LangVersion>
    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <Deterministic>true</Deterministic>
    <RestoreIgnoreFailedSources>true</RestoreIgnoreFailedSources>
  </PropertyGroup>
  <ItemGroup>
{compile_items}
  </ItemGroup>
</Project>
"""
    path.write_text(content, encoding="utf-8", newline="\n")


def main() -> int:
    args = parse_args()
    root = Path(__file__).resolve().parents[1]
    dotnet = shutil.which("dotnet")
    if not dotnet:
        print("ERROR: dotnet was not found. Install a .NET 8 SDK and retry.", file=sys.stderr)
        return 2

    classic_project = root / "tests" / "KingmakerGunslinger.DomainTests" / "KingmakerGunslinger.DomainTests.csproj"
    generated_dir = root / "artifacts" / "portable-domain-tests"
    generated_dir.mkdir(parents=True, exist_ok=True)
    generated_project = generated_dir / "KingmakerGunslinger.PortableDomainTests.csproj"
    write_sdk_project(generated_project, compile_files(classic_project))

    command = [
        dotnet,
        "run",
        "--project",
        str(generated_project),
        "--configuration",
        args.configuration,
        "--nologo",
    ]
    print("Running:", " ".join(command))
    completed = subprocess.run(command, cwd=root, check=False)

    if not args.keep_generated_project:
        for child in (generated_dir / "bin", generated_dir / "obj"):
            if child.exists():
                shutil.rmtree(child)
        generated_project.unlink(missing_ok=True)
        try:
            generated_dir.rmdir()
        except OSError:
            pass

    return completed.returncode


if __name__ == "__main__":
    raise SystemExit(main())
