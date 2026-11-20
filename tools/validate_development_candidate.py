"""Source validation for a development candidate built on a released version.

A development candidate is unreleased work packaged for one named purpose,
such as a guarded native observation of a review branch. It is never a
release and this validator never qualifies one: the release validator for
the active version still runs, unchanged, whenever Build-Local builds a
release candidate.

Its baseline is the release of the active version, found from the tag the
release publisher pushes (`v<version>` on the released commit), so it needs
no edit when a new version is released. A first version pinned the v0.0.156
commit and refused every candidate once v0.0.157 shipped.

What it keeps is every check that does not describe one particular release's
content: the released version metadata unchanged (no bump), the released
ledger as an exact prefix (append-only), schema, format and uniqueness of
every identity (collisions), the released static record unchanged, the
released Sprint 24 routes public with their original assets present and
packaged, and the Expanded Summoning manifest validation, which also
re-derives every alternate summoning adapter identity. What it deliberately
does not apply are a release's own content contracts: production scope,
identity freezes and the exact bytes of released art. Those are what make a
release a release, and a development candidate makes no such claim; the
v0.0.155 art pin, inherited here, refused a branch refining the
Tyrannosaurus before it could be observed natively.

The released ledger is compared by `check_development_ledger`, which is
`check_append_only` with one more permitted difference: the exact released
rows named in AUTHORIZED_RELEASED_ROW_CORRECTIONS may change from their exact
released form to their exact authorized form, at their exact position, and in
that direction only. `check_append_only` itself is unchanged, because the
historical release validators import it and must keep describing their own
releases exactly.
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
from expanded_summoning_manifest import ENTRY

PURPOSE = re.compile(r"^[a-z0-9][a-z0-9-]{1,46}[a-z0-9]$")
SYMBOL = re.compile(r"^KMG\.[A-Za-z0-9_.]+$")
GUID = re.compile(r"^[0-9a-f]{32}$")
RELEASE_VERSION = re.compile(r"^[0-9]+\.[0-9]+\.[0-9]+$")
FIELDS = ("symbol", "guid", "plannedType", "status", "milestone", "notes")
VERSION_FIELDS = (
    ("KmgVersion", "Directory.Build.props", r"<KmgVersion>([^<]*)</KmgVersion>"),
    ("KmgInformationalVersion", "Directory.Build.props",
     r"<KmgInformationalVersion>([^<]*)</KmgInformationalVersion>"),
    ("AssemblyVersion", "src/KingmakerGunslinger/Properties/AssemblyInfo.cs",
     r'AssemblyVersion\("([^"]*)"\)'),
    ("AssemblyFileVersion", "src/KingmakerGunslinger/Properties/AssemblyInfo.cs",
     r'AssemblyFileVersion\("([^"]*)"\)'),
    ("AssemblyInformationalVersion",
     "src/KingmakerGunslinger/Properties/AssemblyInfo.cs",
     r'AssemblyInformationalVersion\("([^"]*)"\)'),
)
STATUSES = ("reserved", "active", "retired")
# Released creatures the owner has explicitly withdrawn from public routes.
# Each is a settled product cut with its own record, not a new withheld
# cohort: the guard below still refuses every other released creature.
OWNER_WITHDRAWN_RELEASED = {
    # Manual-playtest repair R1 CUT-01 (2026-11-07): Stirge omitted from this
    # release; identities, assets and saves retained.
    "stirge": "planning/summon-playtest-repair/R1-STATUS.md",
}
# Released ledger rows the owner has authorized to change. Each entry is one
# exact transition of one whole six-field row at one position: the row as
# released, and the row as it must now read. Nothing else about any released
# row may differ, and the reverse transition is not permitted.
AUTHORIZED_RELEASED_ROW_CORRECTIONS = (
    {
        "position": 206,
        "authorization": ("Owner, 2026-11-16, remediation story S3 T6 / INT-07 "
                          "(GS-REP-0001): correct the Gunsmith's Kit note, "
                          "which described Wrecked field repair. See "
                          "docs/BLUEPRINT-MANIFEST.md."),
        "before": {
            "symbol": "KMG.Gunsmithing.GunsmithKit",
            "guid": "d52aacb753434691b1ed85a16cc87104",
            "plannedType": "BlueprintItem",
            "status": "active",
            "milestone": "Sixth Playtest 0.0.66",
            "notes": ("Stable non-consumable Gunsmith's Kit required for basic "
                      "ammunition crafting and for unified "
                      "Broken-or-Wrecked-to-Normal firearm repair."),
        },
        "after": {
            "symbol": "KMG.Gunsmithing.GunsmithKit",
            "guid": "d52aacb753434691b1ed85a16cc87104",
            "plannedType": "BlueprintItem",
            "status": "active",
            "milestone": "Sixth Playtest 0.0.66",
            "notes": ("Stable non-consumable Gunsmith's Kit required for basic "
                      "ammunition crafting and for Broken-to-Normal field "
                      "repair. Wrecked firearms are restored only by a "
                      "completed full rest."),
        },
    },
)
PACKAGE_SCRIPTS = ("scripts/Build-Local.ps1", "scripts/package.ps1",
                   "scripts/validate-build-output.ps1",
                   "scripts/validate-package.ps1")


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


def authorized_correction(index, released, current):
    """The authorized correction this exact transition is, or None."""
    for correction in AUTHORIZED_RELEASED_ROW_CORRECTIONS:
        if index == correction["position"] and released == correction["before"] \
                and current == correction["after"]:
            return correction
    return None


def check_development_ledger(accepted, current):
    """The released ledger is an unchanged prefix of the current one, except
    for authorized corrections of released rows. Returns the number of
    appended identities and a record of each correction applied. A corrected
    row is not a new identity."""
    check_entries(accepted)
    check_entries(current)
    if len(current) < len(accepted):
        raise AssertionError("A released identity row changed, moved or was removed")
    corrections = []
    for index, released in enumerate(accepted):
        row = current[index]
        if row == released:
            continue
        correction = authorized_correction(index, released, row)
        if correction is None:
            raise AssertionError("A released identity row changed, moved or was "
                                 "removed: row %d (%s)" % (index, released["symbol"]))
        changed = sorted(field for field in FIELDS if released[field] != row[field])
        corrections.append({
            "position": index,
            "symbol": row["symbol"],
            "guid": row["guid"],
            "fields": changed,
            "before": {field: released[field] for field in changed},
            "after": {field: row[field] for field in changed},
            "authorization": correction["authorization"],
        })
    return len(current) - len(accepted), corrections


def git(root: Path, *arguments):
    return subprocess.run(["git", "-c", "core.longpaths=true", *arguments],
                          cwd=root, text=True, capture_output=True, check=False)


def resolve_baseline(root: Path, version):
    """The released commit of the active version: the publisher's tag
    `v<version>`, which the candidate must extend."""
    if not isinstance(version, str) or RELEASE_VERSION.match(version) is None:
        raise AssertionError("Info.json does not name a release version: " +
                             repr(version))
    tag = "v" + version
    found = git(root, "rev-parse", "--verify", "--quiet",
                "refs/tags/" + tag + "^{commit}")
    commit = found.stdout.strip()
    if found.returncode or not commit:
        raise AssertionError("No release tag " + tag + " for the active version. "
                             "Development candidates extend a released version; "
                             "fetch its tag with: git fetch --tags origin")
    if git(root, "merge-base", "--is-ancestor", commit, "HEAD").returncode:
        raise AssertionError("The candidate does not extend release " + tag +
                             " (" + commit[:10] + ")")
    released = json.loads(common.blob(root, commit, "Info.json")
                          .decode("utf-8-sig")).get("Version")
    if released != version:
        raise AssertionError("Release tag " + tag + " points at a commit whose "
                             "Info.json says " + repr(released))
    return tag, commit


def version_metadata(info_version, read):
    """Every version string the build stamps, read through `read(path)`."""
    found = {"Info.json Version": info_version}
    for name, path, pattern in VERSION_FIELDS:
        values = re.findall(pattern, read(path))
        if len(values) != 1:
            raise AssertionError("Expected exactly one " + name + " in " + path)
        found[name] = values[0]
    return found


def check_version(released, current):
    """The released version metadata, exactly: a development candidate is
    never a new version."""
    changed = sorted(name for name in set(released) | set(current)
                     if released.get(name) != current.get(name))
    if changed:
        raise AssertionError("Development candidates keep the released " +
                             str(released.get("Info.json Version")) +
                             " version metadata unchanged: " + ", ".join(changed))


def check_released_publication(released_keys, current_keys,
                               released_suppressed, current_suppressed):
    """A new withheld cohort must never hide any released public creature."""
    if not released_keys or not current_keys or not released_keys <= current_keys:
        raise AssertionError("A released creature disappeared from the catalog")
    if not released_suppressed <= released_keys or not current_suppressed <= current_keys:
        raise AssertionError("A suppression key has no registered creature")
    hidden_released = ((released_keys - released_suppressed) & current_suppressed) - \
        set(OWNER_WITHDRAWN_RELEASED)
    if hidden_released:
        raise AssertionError("Released public creatures became suppressed: " +
                             ", ".join(sorted(hidden_released)))


def check_released_art_owned(root: Path):
    """The released Sprint 24 original assets are still present and still
    packaged. Their bytes may change: refining released art is exactly what
    a development candidate is built to observe."""
    for relative in sorted(released155.ART):
        if not (root / relative).is_file():
            raise AssertionError("Missing released Sprint 24 original asset: " +
                                 relative)
    for token in ("sprint24-theropods", "deinonychus", "tyrannosaurus"):
        for script in PACKAGE_SCRIPTS:
            if token not in (root / script).read_text(encoding="utf-8-sig"):
                raise AssertionError(script + " lacks package owner token: " + token)


def check_static(released, current):
    if released != current:
        changed = sorted(key for key in set(released) | set(current)
                         if released.get(key) != current.get(key))
        raise AssertionError("A development candidate cannot change the released "
                             "static record: " + ", ".join(changed))


def validate(root: Path, purpose: str):
    check_purpose(purpose)
    version = common.document(root, "Info.json").get("Version")
    tag, baseline = resolve_baseline(root, version)
    check_version(
        version_metadata(version, lambda path: common.blob(root, baseline, path)
                         .decode("utf-8-sig")),
        version_metadata(version, lambda path: (root / path)
                         .read_text(encoding="utf-8-sig")))

    accepted = json.loads(common.blob(root, baseline, "blueprints/blueprints.json"))
    current = common.document(root, "blueprints/blueprints.json")
    for key in ("$schema", "schemaVersion", "namespace", "policy"):
        if current.get(key) != accepted.get(key):
            raise AssertionError("The identity ledger header changed: " + key)
    appended, corrections = check_development_ledger(accepted["entries"],
                                                     current["entries"])

    # Preserve every released public creature; new withheld cohorts are independent.
    # The original Sprint24 asset/package ownership guard remains in force.
    visibility_source = (root / "src/KingmakerGunslinger/Summoning/"
                         "SummonVisibilityCatalog.cs").read_text(encoding="utf-8-sig")
    catalog_path = "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs"
    released_catalog = common.blob(root, baseline, catalog_path).decode("utf-8-sig")
    current_catalog = (root / catalog_path).read_text(encoding="utf-8-sig")
    released_visibility = common.blob(root, baseline,
        "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs").decode("utf-8-sig")
    check_released_publication(
        {row[0] for row in ENTRY.findall(released_catalog)},
        {row[0] for row in ENTRY.findall(current_catalog)},
        released155.suppressed_keys(released_visibility),
        released155.suppressed_keys(visibility_source))
    check_released_art_owned(root)
    check_static(json.loads(common.blob(root, baseline,
                                        "validation/static-validation.json")),
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
        "baselineTag": tag,
        "baselineCommit": baseline,
        "baselineVersion": version,
        "releasedIdentities": len(accepted["entries"]),
        "identities": len(current["entries"]),
        "appendedIdentities": appended,
        "authorizedReleasedRowCorrections": corrections,
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
          "purpose=" + summary["purpose"] + "; baseline=" + summary["baselineTag"] +
          "; identities=" + str(summary["identities"]) + "; appended=" +
          str(summary["appendedIdentities"]) + "; authorized released-row corrections=" +
          (", ".join("row %d %s %s" % (value["position"], value["symbol"],
                                        "/".join(value["fields"]))
                     for value in summary["authorizedReleasedRowCorrections"]) or "none") +
          ".")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
