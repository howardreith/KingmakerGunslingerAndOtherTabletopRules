#!/usr/bin/env python3
from __future__ import annotations

import hashlib
import json
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

NS = {"m": "http://schemas.microsoft.com/developer/msbuild/2003"}


def fail(message: str) -> None:
    raise SystemExit("ERROR: " + message)


def main() -> int:
    root = Path(__file__).resolve().parents[1]
    info = json.loads((root / "Info.json").read_text(encoding="utf-8-sig"))
    if info.get("Version") != "0.0.18" or info.get("ManagerVersion") != "0.32.4":
        fail("Info.json does not match the Sprint 18 runtime candidate.")

    props = (root / "Directory.Build.props").read_text(encoding="utf-8")
    if "0.0.18-s18-runtime-smoke-candidate" not in props:
        fail("Directory.Build.props version is stale.")

    project = ET.parse(root / "src/KingmakerGunslinger/KingmakerGunslinger.csproj")
    compile_items = [n.attrib["Include"] for n in project.findall(".//m:Compile", NS)]
    if len(compile_items) != 104 or len(set(compile_items)) != 104:
        fail(f"Expected 104 unique main compile items; found {len(compile_items)}.")
    if "Development\\ImmediateModeGui.cs" not in compile_items:
        fail("ImmediateModeGui.cs is not compiled.")

    ui = (root / "src/KingmakerGunslinger/Development/DevelopmentUi.cs").read_text(encoding="utf-8")
    if "GUILayout." in ui or "ImmediateModeGui." not in ui:
        fail("Development UI has an unexpected direct GUILayout dependency.")

    program = (root / "tests/KingmakerGunslinger.DomainTests/Program.cs").read_text(encoding="utf-8")
    tests = re.findall(r'Case\("[^"]+",\s*[A-Za-z0-9_]+\)', program)
    if len(tests) != 373:
        fail(f"Expected 373 declared tests; found {len(tests)}.")

    manifest = json.loads((root / "blueprints/blueprints.json").read_text(encoding="utf-8"))
    entries = manifest.get("entries", [])
    if len(entries) != 12 or sum(1 for e in entries if e.get("status") == "active") != 8:
        fail("Blueprint ledger changed unexpectedly.")

    forbidden = []
    for path in root.rglob("*"):
        if path.is_file() and path.suffix.lower() in {".dll", ".exe", ".pdb", ".mdb"}:
            forbidden.append(path.relative_to(root).as_posix())
    if forbidden:
        fail("Source tree contains compiled binaries: " + ", ".join(forbidden))

    print("Sprint 18 source validation passed.")
    print("Blueprint manifest SHA-256:", hashlib.sha256((root / "blueprints/blueprints.json").read_bytes()).hexdigest())
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
