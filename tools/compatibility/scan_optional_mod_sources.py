#!/usr/bin/env python3
"""Deterministic, standard-library static compatibility audit."""
from __future__ import annotations

import argparse
import hashlib
import json
import re
from pathlib import Path

GUID = re.compile(r"(?<![0-9a-fA-F])([0-9a-fA-F]{32})(?![0-9a-fA-F])")
HARMONY = re.compile(r"HarmonyPatch\s*\(\s*typeof\s*\(\s*([^\)]+)\s*\)\s*(?:,\s*(?:\"([^\"]+)\"|MethodType\.([A-Za-z]+)))?")
ROLE = re.compile(r"\[(HarmonyPrefix|HarmonyPostfix|HarmonyTranspiler|HarmonyFinalizer)\]")
BOOTSTRAP = re.compile(r"\b(Main\.Load|EntryMethod|PatchAll|\.Patch\s*\(|BlueprintsCache|Library\.AddAsset|AddAsset|AddBlueprint|ResourcesLibrary|OnGUI|OnToggle|OnUpdate|Assembly\.Load|GetType\s*\()")
TEXT_SUFFIXES = {".cs", ".json", ".ps1", ".py", ".xml", ".csproj", ".sln"}


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def text_files(root: Path):
    for path in sorted(root.rglob("*"), key=lambda p: str(p).lower()):
        relative_parts = path.relative_to(root).parts
        if path.is_file() and path.suffix.lower() in TEXT_SUFFIXES and ".git" not in relative_parts and "artifacts" not in relative_parts:
            yield path


def context(line: str) -> str:
    return " ".join(line.strip().split())[:320]


def scan_tree(owner: str, root: Path) -> dict:
    guids, patches, bootstrap = [], [], []
    identities = {"umm": [], "assemblies": [], "assetBundles": [], "soundBanks": [], "settings": []}
    for path in text_files(root):
        try:
            text = path.read_text(encoding="utf-8-sig", errors="replace")
        except OSError:
            continue
        rel = path.relative_to(root).as_posix()
        lines = text.splitlines()
        if path.name.lower() == "info.json":
            try:
                info = json.loads(text)
                identities["umm"].append({k: info.get(k) for k in ("Id", "DisplayName", "Version", "ManagerVersion", "AssemblyName", "EntryMethod", "Requirements")})
            except json.JSONDecodeError as exc:
                identities["umm"].append({"parseError": str(exc), "path": rel})
        for number, line in enumerate(lines, 1):
            for match in GUID.finditer(line):
                nearby = " ".join(lines[max(0, number - 3): min(len(lines), number + 2)])
                lowered = nearby.lower()
                if any(token in lowered for token in ("addasset", "addblueprint", "createblueprint", "assetguid")):
                    kind = "project-owned-definition" if owner == "gunslinger" else "third-party-project-owned-definition"
                    confidence = "high"
                elif any(token in lowered for token in ("getblueprint", "resourceslibrary", "blueprintreference")):
                    kind, confidence = "native-or-external-blueprint-reference", "medium"
                else:
                    kind, confidence = "unknown-heuristic", "low"
                guids.append({"guid": match.group(1).lower(), "owner": owner, "classification": kind, "confidence": confidence, "path": rel, "line": number, "context": context(line)})
            patch = HARMONY.search(line)
            if patch:
                role = "unknown"
                for lookahead in lines[number - 1:min(len(lines), number + 8)]:
                    role_match = ROLE.search(lookahead)
                    if role_match:
                        role = role_match.group(1).removeprefix("Harmony").lower()
                        break
                patches.append({"owner": owner, "targetType": patch.group(1).strip(), "targetMember": patch.group(2) or patch.group(3) or "unspecified", "role": role, "path": rel, "line": number, "confidence": "high"})
            if BOOTSTRAP.search(line):
                bootstrap.append({"owner": owner, "path": rel, "line": number, "context": context(line), "confidence": "medium"})
            for value in re.findall(r'"([^"\r\n]+\.(?:bnk|bundle|assets|json))"', line, re.IGNORECASE):
                lower = value.lower()
                if lower.endswith(".bnk"): identities["soundBanks"].append(value)
                elif lower.endswith((".bundle", ".assets")): identities["assetBundles"].append(value)
                elif "setting" in lower: identities["settings"].append(value)
    for values in identities.values():
        if isinstance(values, list) and values and isinstance(values[0], str):
            values[:] = sorted(set(values), key=str.lower)
    return {"owner": owner, "root": str(root), "guids": guids, "patches": patches, "bootstrap": bootstrap, "identities": identities}


def build_report(root: Path, reference_root: Path) -> dict:
    catalog = json.loads((root / "compatibility/reference-catalog.json").read_text(encoding="utf-8"))
    trees = [scan_tree("gunslinger", root / "src")]
    for entry in catalog["references"]:
        if entry["role"] != "source-reference":
            continue
        for alias in entry["folderAliases"]:
            candidate = (reference_root / alias).resolve()
            if candidate.is_dir():
                trees.append(scan_tree(entry["key"], candidate))
    definitions: dict[str, list[dict]] = {}
    patches: dict[str, list[dict]] = {}
    umm: dict[str, list[dict]] = {}
    for tree in trees:
        for item in tree["guids"]:
            if "project-owned-definition" in item["classification"]:
                definitions.setdefault(item["guid"], []).append(item)
        for item in tree["patches"]:
            key = f'{item["targetType"]}::{item["targetMember"]}'
            patches.setdefault(key, []).append(item)
        for item in tree["identities"]["umm"]:
            if item.get("Id"):
                umm.setdefault(item["Id"].lower(), []).append({"owner": tree["owner"], **item})
    collisions = [items for items in definitions.values() if len({i["owner"] for i in items}) > 1]
    overlaps = [{"target": key, "patches": items} for key, items in sorted(patches.items()) if len({i["owner"] for i in items}) > 1]
    duplicate_umm = [items for items in umm.values() if len({i["owner"] for i in items}) > 1]
    return {"schemaVersion": 1, "scanner": "tools/compatibility/scan_optional_mod_sources.py", "gunslingerCommitIndependent": True, "trees": trees, "hardGuidCollisions": collisions, "harmonyTargetOverlaps": overlaps, "duplicateUmmIdentities": duplicate_umm, "limitations": ["C# parsing is deterministic lexical analysis, not a compiler AST.", "Compiled-only Harmony targets require runtime reflection and are not inferred here.", "Native blueprint references are heuristic until runtime resolution."]}


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument("--reference-root", type=Path, default=Path(r"C:\Dev\KingmakerGunslingerLab\examples"))
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    root, reference = args.root.resolve(), args.reference_root.resolve()
    report = build_report(root, reference)
    output = args.output or root / "artifacts" / "compatibility" / "static-audit" / "optional-mod-static-audit.json"
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(report, indent=2, sort_keys=True) + "\n", encoding="utf-8")
    print(output)
    print(f'trees={len(report["trees"])} guidCollisions={len(report["hardGuidCollisions"])} harmonyOverlaps={len(report["harmonyTargetOverlaps"])} duplicateUmm={len(report["duplicateUmmIdentities"])}')
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
