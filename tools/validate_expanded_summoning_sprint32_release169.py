"""Publish the exact native-qualified Sprint32 product with v169 metadata only."""
from __future__ import annotations
import argparse
import json
from pathlib import Path
import subprocess
import sys
sys.dont_write_bytecode = True
import validate_development_candidate as identities
import validate_expanded_summoning_sprint21150 as common
from expanded_summoning_manifest import NATIVE_EXPANDED_OPTIONS, parsed_creatures, suppressed_keys

QUALIFIED = "efd237d1cf12338d8d5eea50403af203785c3dc0"
RELEASED = "b306f85b5543fcaf9592e78f223f3e98455173b8"
VERSION = "0.0.169"
INFORMATIONAL = "0.0.169-expanded-summoning-satyr-lillend"
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
PRODUCT_ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "expandedSummoningSprint32Release169"
RECORD = {
    "releaseVersion": VERSION,
    "releaseInformationalVersion": INFORMATIONAL,
    "baseStableVersion": "0.0.168",
    "qualifiedDevelopmentSource": QUALIFIED,
    "appendedIdentities": 139,
    "releasedIdentityChanges": 0,
    "visibleChoiceTotal": 1189,
    "change": "Satyr and Lillend Azata; exact qualified product, project, assets and ledger",
    "nativeEvidence": "Original qualified development artifacts with reviewed reuse; release DLL changes only version metadata and build stamps",
    "ownerPublicationAuthority": "Howie Reith, 2026-10-27: Merge and release it thank you.",
}

def check_qualified_changes(changed, untracked):
    unexpected = sorted(set(changed) - {ASSEMBLY})
    if unexpected or untracked:
        raise AssertionError("Product differs from native-qualified87: " + ", ".join(unexpected + sorted(untracked)))

def check_metadata(old_info, info, old_props, props, old_assembly, assembly):
    expected_info = dict(old_info, Version=VERSION)
    if info != expected_info:
        raise AssertionError("Only the Info version may change")
    old_version = old_info["Version"]
    old_informational = "0.0.168-expanded-summoning-sprints30-31-followup"
    expected_props = old_props.replace("<KmgVersion>" + old_version + "</KmgVersion>",
                                      "<KmgVersion>" + VERSION + "</KmgVersion>")
    expected_props = expected_props.replace("<KmgInformationalVersion>" + old_informational + "</KmgInformationalVersion>",
                                            "<KmgInformationalVersion>" + INFORMATIONAL + "</KmgInformationalVersion>")
    expected_assembly = old_assembly
    for attribute, before, after in (("AssemblyVersion", old_version, VERSION),
                                     ("AssemblyFileVersion", old_version, VERSION),
                                     ("AssemblyInformationalVersion", old_informational, INFORMATIONAL)):
        token = attribute + '("' + before + '")'
        if expected_assembly.count(token) != 1:
            raise AssertionError("Qualified assembly version attribute is ambiguous")
        expected_assembly = expected_assembly.replace(token, attribute + '("' + after + '")')
    if props.splitlines() != expected_props.splitlines() or assembly.splitlines() != expected_assembly.splitlines():
        raise AssertionError("Non-version build property or assembly metadata changed")

def check_ledger(released, qualified, current):
    if current != qualified:
        raise AssertionError("Ledger differs from exact native-qualified87")
    for key in ("$schema", "schemaVersion", "namespace", "policy"):
        if current.get(key) != released.get(key):
            raise AssertionError("Released identity header changed: " + key)
    appended = identities.check_append_only(released["entries"], current["entries"])
    if len(released["entries"]) != 7372 or len(current["entries"]) != 7511 or appended != 139:
        raise AssertionError("Sprint32 must append exactly139 identities to released168")

def check_static(qualified, current):
    expected = dict(qualified, version=VERSION, milestone=INFORMATIONAL)
    expected[RECORD_KEY] = RECORD
    if current != expected:
        raise AssertionError("Release static record or historical evidence differs")

def check_census(creatures, hidden):
    monster = [c for c in creatures if c["monster"] is not None]
    ally = [c for c in creatures if c["ally"] is not None]
    totals = (len(creatures), len(monster), sum(10-c["monster"] for c in monster),
              len(ally), sum(10-c["ally"] for c in ally))
    if hidden or totals != (124, 102, 572, 105, 588) or totals[2]+totals[4]+len(NATIVE_EXPANDED_OPTIONS) != 1189:
        raise AssertionError("Qualified Sprint32 public roster/route census differs")

def validate(root: Path):
    root = root.resolve()
    def blob(commit, path):
        return common.blob(root, commit, path).decode("utf-8-sig")
    changed = subprocess.check_output(["git", "diff", "--name-only", "--no-renames", QUALIFIED, "--", *PRODUCT_ROOTS], cwd=root).decode("utf-8").splitlines()
    untracked = subprocess.check_output(["git", "ls-files", "--others", "--exclude-standard", "--", *PRODUCT_ROOTS], cwd=root).decode("utf-8").splitlines()
    check_qualified_changes(changed, untracked)
    check_metadata(json.loads(blob(QUALIFIED,"Info.json")), common.document(root,"Info.json"),
                   blob(QUALIFIED,"Directory.Build.props"), (root/"Directory.Build.props").read_text(encoding="utf-8-sig"),
                   blob(QUALIFIED,ASSEMBLY), (root/ASSEMBLY).read_text(encoding="utf-8-sig"))
    ledger = "blueprints/blueprints.json"
    check_ledger(json.loads(blob(RELEASED,ledger)), json.loads(blob(QUALIFIED,ledger)), common.document(root,ledger))
    check_static(json.loads(blob(QUALIFIED,"validation/static-validation.json")), common.document(root,"validation/static-validation.json"))
    check_census(parsed_creatures(), suppressed_keys())
    manifest = subprocess.run([sys.executable,"-B",str(root/"tools/expanded_summoning_manifest.py")], cwd=root, capture_output=True, text=True)
    if manifest.returncode:
        raise AssertionError("Summoning manifest failed: "+(manifest.stdout+manifest.stderr)[-1000:])
    print("Sprint32 release169 PASS: exact qualified87 product/project/assets; identities7372+139=7511; public choices1189; version-only build metadata.")

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument("--root",type=Path,default=Path(__file__).resolve().parents[1])
    try:
        validate(parser.parse_args().root)
    except (AssertionError,ValueError,KeyError,OSError,subprocess.CalledProcessError) as error:
        print("Sprint32 release169 FAIL: "+str(error),file=sys.stderr)
        return 1
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
