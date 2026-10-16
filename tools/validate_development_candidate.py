"""Source validation for a development candidate built on released v0.0.156.

A development candidate is unreleased work packaged for one named purpose,
such as a guarded native observation of a review branch. It is never a
release and this validator never qualifies one: the release validator for
the active version still runs, unchanged, whenever Build-Local builds a
release candidate.

What it keeps is every check that does not describe one particular release's
content: the released version metadata unchanged (no bump), the released
ledger as an exact prefix (append-only), schema, format and uniqueness of
every identity (collisions), the released Sprint 24 public boundary and
original assets, the released static record unchanged, and the Expanded
Summoning manifest validation, which also re-derives every alternate
summoning adapter identity. What it deliberately does not apply are the
release's own production-scope and identity-freeze rules; those are what make
a release a release, and a development candidate makes no such claim.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import re
import subprocess
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_expanded_summoning_sprint21150 as common
import validate_expanded_summoning_sprint24154 as released155
import validate_firearm_continuation_release156 as released156

# The released v0.0.156 commit: the accepted baseline every development
# candidate on this version must extend without changing.
BASELINE = "607da683b1f6fd545dab90fd6f4121d155573142"
BASELINE_VERSION = released156.VERSION
PURPOSE = re.compile(r"^[a-z0-9][a-z0-9-]{1,46}[a-z0-9]$")
SYMBOL = re.compile(r"^KMG\.[A-Za-z0-9_.]+$")
GUID = re.compile(r"^[0-9a-f]{32}$")
FIELDS = ("symbol", "guid", "plannedType", "status", "milestone", "notes")
STATUSES = ("reserved", "active", "retired")


def check_purpose(purpose):
    if not isinstance(purpose, str) or PURPOSE.match(purpose) is None or \
            "--" in purpose:
        raise AssertionError("Development purpose must be a lower-case "
                             "hyphenated name of 3-48 characters: " +
                             repr(purpose))
    return purpose


def check_entries(entries):
    """Schema, format and uniqueness of every identity row."""
    if not isinstance(entries, list) or not entries:
        raise AssertionError("The identity ledger has no entries")
    symbols = set()
    guids = set()
    for index, row in enumerate(entries):
        if not isinstance(row, dict) or tuple(sorted(row)) != tuple(sorted(FIELDS)):
            raise AssertionError("Identity row %d has an invalid field set" % index)
        if not isinstance(row["symbol"], str) or SYMBOL.match(row["symbol"]) is None:
            raise AssertionError("Identity row %d has a malformed symbol" % index)
        if not isinstance(row["guid"], str) or GUID.match(row["guid"]) is None:
            raise AssertionError("Identity row %d has a malformed GUID: %s" %
                                 (index, row["symbol"]))
        if not isinstance(row["plannedType"], str) or not row["plannedType"]:
            raise AssertionError("Identity row %d lacks a planned type" % index)
        if row["status"] not in STATUSES:
            raise AssertionError("Identity row %d has an unknown status" % index)
        if not isinstance(row["milestone"], str) or not row["milestone"] or \
                not isinstance(row["notes"], str):
            raise AssertionError("Identity row %d lacks milestone or notes" % index)
        if row["symbol"] in symbols:
            raise AssertionError("Duplicate identity symbol: " + row["symbol"])
        if row["guid"] in guids:
            raise AssertionError("Duplicate identity GUID: " + row["guid"])
        symbols.add(row["symbol"])
        guids.add(row["guid"])
    return len(entries)


def check_append_only(accepted, current):
    """The released ledger is an exact, unchanged prefix of the current one."""
    check_entries(accepted)
    check_entries(current)
    if len(current) < len(accepted) or current[:len(accepted)] != accepted:
        raise AssertionError("A released identity row changed, moved or was removed")
    return len(current) - len(accepted)


def check_version(info_version, props, assembly):
    """The released version metadata, exactly: a development candidate is
    never a new version."""
    try:
        released156.check_version(info_version, props, assembly)
    except AssertionError:
        raise AssertionError("Development candidates keep the released " +
                             BASELINE_VERSION + " version metadata unchanged")


def check_static(released, current):
    if released != current:
        changed = sorted(key for key in set(released) | set(current)
                         if released.get(key) != current.get(key))
        raise AssertionError("A development candidate cannot change the released "
                             "static record: " + ", ".join(changed))


def validate(root: Path, purpose: str):
    check_purpose(purpose)
    baseline_info = json.loads(common.blob(root, BASELINE, "Info.json"))
    if baseline_info.get("Version") != BASELINE_VERSION:
        raise AssertionError("The accepted baseline is not released " + BASELINE_VERSION)
    check_version(common.document(root, "Info.json").get("Version"),
                  (root / "Directory.Build.props").read_text(encoding="utf-8-sig"),
                  (root / "src/KingmakerGunslinger/Properties/AssemblyInfo.cs")
                  .read_text(encoding="utf-8-sig"))

    accepted = json.loads(common.blob(root, BASELINE, "blueprints/blueprints.json"))
    current = common.document(root, "blueprints/blueprints.json")
    for key in ("$schema", "schemaVersion", "namespace", "policy"):
        if current.get(key) != accepted.get(key):
            raise AssertionError("The identity ledger header changed: " + key)
    appended = check_append_only(accepted["entries"], current["entries"])

    # The released Sprint 24 public boundary and original assets, unchanged.
    visibility_source = (root / "src/KingmakerGunslinger/Summoning/"
                         "SummonVisibilityCatalog.cs").read_text(encoding="utf-8-sig")
    stage = released155.validate_visibility(visibility_source, "registered")
    if stage != "public":
        raise AssertionError("Released Sprint 24 routes are not public: " + stage)
    released155.validate_static(root, stage)
    released155.validate_art_and_package_owners(root, stage)
    check_static(json.loads(common.blob(root, BASELINE, "validation/static-validation.json")),
                 common.document(root, "validation/static-validation.json"))

    manifest = subprocess.run(
        [sys.executable, str(root / "tools/expanded_summoning_manifest.py")],
        cwd=root, text=True, capture_output=True, check=False)
    if manifest.returncode:
        raise AssertionError("Expanded Summoning manifest validation failed: " +
                             (manifest.stdout + manifest.stderr)[-1000:])
    ledger_bytes = (root / "blueprints/blueprints.json").read_bytes()
    return {
        "schemaVersion": 1,
        "validator": "tools/validate_development_candidate.py",
        "purpose": purpose,
        "releaseQualification": False,
        "baselineCommit": BASELINE,
        "baselineVersion": BASELINE_VERSION,
        "releasedIdentities": len(accepted["entries"]),
        "identities": len(current["entries"]),
        "appendedIdentities": appended,
        "ledgerSha256": hashlib.sha256(ledger_bytes).hexdigest(),
        "manifestValidation": manifest.stdout.strip().splitlines(),
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    parser.add_argument("--purpose", required=True)
    parser.add_argument("--summary", type=Path)
    args = parser.parse_args()
    try:
        summary = validate(args.root.resolve(), args.purpose)
    except (AssertionError, ValueError, KeyError, TypeError,
            subprocess.CalledProcessError) as error:
        print("Development candidate validation FAIL: " + str(error), file=sys.stderr)
        return 1
    if args.summary is not None:
        args.summary.write_text(json.dumps(summary, indent=2) + "\n", encoding="utf-8")
    print("Development candidate validation PASS (not a release qualification): "
          "purpose=" + summary["purpose"] + "; baseline=" + BASELINE_VERSION +
          "; identities=" + str(summary["identities"]) + "; appended=" +
          str(summary["appendedIdentities"]) + ".")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
