#!/usr/bin/env python3
"""Portable source validator for Sprint 30 with inherited Sprint 29 checks."""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_sprint29

VERSION = "0.0.30"
INFORMATIONAL_VERSION = "0.0.30-s30-generic-firearm-actions"
TEST_COUNT = 611


def fail(message: str) -> None:
    raise RuntimeError(message)


def read(root: Path, relative: str) -> str:
    path = root / relative
    if not path.is_file():
        fail(f"Required Sprint 30 file is missing: {relative}")
    return path.read_text(encoding="utf-8")


def require_tokens(text: str, tokens: list[str], label: str) -> None:
    missing = [token for token in tokens if token not in text]
    if missing:
        fail(f"{label} is missing required token(s): {missing}")


def validate(
    root: Path,
    version: str = VERSION,
    informational_version: str = INFORMATIONAL_VERSION,
    test_count: int = TEST_COUNT,
) -> None:
    root = root.resolve()
    validate_sprint29.validate(
        False,
        root=root,
        version=version,
        informational_version=informational_version,
        test_count=test_count,
        require_current_guide_match=False,
    )

    qualification = json.loads(read(root, "BUILD-QUALIFICATION-S30.json"))
    expected_qualification = {
        "modVersion": VERSION,
        "informationalVersion": INFORMATIONAL_VERSION,
        "declaredTests": TEST_COUNT,
        "testRuns": 3,
        "testFailures": 0,
        "repeatedTestOutputByteIdentical": True,
        "exactReferenceCompileRuns": 2,
        "sameOutputPathDllByteIdentical": True,
        "sameOutputPathPdbByteIdentical": True,
        "privateReferencesRedistributed": False,
        "readyForKingmakerSmokeTest": True,
    }
    for key, expected in expected_qualification.items():
        if qualification.get(key) != expected:
            fail(f"BUILD-QUALIFICATION-S30.json has incorrect {key!r}.")
    if "accepted" not in str(qualification.get("runtimeAcceptance", "")).lower():
        fail("Sprint 30 must be classified as runtime-accepted.")
    if qualification.get("runtimePassRunIds") != [
        "20260801T0446479175229Z-feac50caa3fd439a80b9a09c7a383cc0",
        "20260801T0448285054152Z-4e5925080ce1422fbcb44c2ee07adcac",
    ]:
        fail("Sprint 30 runtime PASS run IDs are missing or out of order.")
    if qualification.get("runtimeSaveWriteObserved") is not False:
        fail("Sprint 30 must record that no save-writing API was observed.")

    static = json.loads(read(root, "validation/static-validation.json"))
    if static.get("version") != version or static.get("milestone") != informational_version:
        fail("validation/static-validation.json does not identify the active milestone.")
    sprint = static.get("sprint29", {})
    if sprint.get("testCount") != test_count or sprint.get("sprint30EntryApproved") is not True:
        fail("Static validation does not record the active test count and Sprint 30 entry decision.")
    if sprint.get("runtimeAcceptancePending") is not False or \
            sprint.get("runtimeAcceptancePassed") is not True:
        fail("Static validation does not record Sprint 30 runtime acceptance.")

    project = read(root, "src/KingmakerGunslinger/KingmakerGunslinger.csproj")
    require_tokens(
        project,
        [
            r'Actions\ExactEquippedFirearmContext.cs',
            r'Actions\ExactEquippedFirearmResolver.cs',
            r'Actions\FirearmActionDecision.cs',
            r'Actions\FirearmActionKind.cs',
            r'Actions\FirearmActionPolicy.cs',
        ],
        "Main project Sprint 30 compile list",
    )
    test_project = read(
        root, "tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj"
    )
    require_tokens(
        test_project,
        [
            "Sprint30Tests.cs",
            r'Actions\FirearmActionDecision.cs',
            r'Actions\FirearmActionKind.cs',
            r'Actions\FirearmActionPolicy.cs',
        ],
        "Domain test Sprint 30 compile list",
    )

    resolver = read(root, "src/KingmakerGunslinger/Actions/ExactEquippedFirearmResolver.cs")
    require_tokens(
        resolver,
        [
            "AddDistinct",
            "markers.Length != 1",
            "More than one distinct marked firearm is equipped",
            "FirearmRuntimeState.Service.TryGetOrCreate",
        ],
        "Exact equipped firearm resolver",
    )
    policy = read(root, "src/KingmakerGunslinger/Actions/FirearmActionPolicy.cs")
    require_tokens(
        policy,
        [
            "FirearmActionKind.Reload",
            "FirearmActionKind.Overhaul",
            "FirearmActionKind.Repair",
            "Multi-round and partial reload are deferred until Sprint 33",
        ],
        "Generic action policy",
    )
    profile = read(root, "src/KingmakerGunslinger/Firearms/ReloadProfile.cs")
    require_tokens(profile, ["AmmunitionId _ammunition", "AmmunitionId Ammunition"], "Reload profile")
    component = read(root, "src/KingmakerGunslinger/Firearms/FirearmDefinitionComponent.cs")
    require_tokens(
        component,
        ["m_AmmunitionId", "definition.Reload.Ammunition.Value"],
        "Definition component ammunition round trip",
    )
    for relative in [
        "src/KingmakerGunslinger/Reloading/ReloadTestMusketRuntime.cs",
        "src/KingmakerGunslinger/Recovery/OverhaulTestMusketRuntime.cs",
        "src/KingmakerGunslinger/Recovery/RepairTestMusketRuntime.cs",
    ]:
        adapter = read(root, relative)
        require_tokens(
            adapter,
            ["ExactEquippedFirearmResolver.TryResolve", "FirearmActionPolicy.Evaluate"],
            relative,
        )

    tests = read(root, "tests/KingmakerGunslinger.DomainTests/Sprint30Tests.cs")
    require_tokens(
        tests,
        [
            "GenericReloadWreckedRejected",
            "GenericOverhaulWrecked",
            "GenericRepairLoadedRejected",
            "ReloadProfileAmmunitionIdentity",
        ],
        "Sprint 30 tests",
    )
    guide = read(root, "SMOKE-TEST-GUIDE-0.0.30.md")
    require_tokens(
        guide,
        [
            "native Heavy Crossbow",
            "stage=MaintenanceLoopPassed",
            "interrupt Repair",
            "Sprint 31 content may begin",
        ],
        "Sprint 30 smoke guide",
    )
    require_tokens(
        read(root, "SPRINT-30-REPORT.md"),
        [
            "marker-first context",
            "611 tests",
            "Runtime acceptance",
            "20260801T0448285054152Z-4e5925080ce1422fbcb44c2ee07adcac",
            "de9f8507e5180adeb5df8dab4559e901da68022be556ef4fe1ffb874034e3d3f",
        ],
        "Sprint 30 report",
    )
    print("Sprint 30 source invariant validation passed with inherited Sprint 29 checks.")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root)
    except Exception as exception:
        print(f"Sprint 30 validation failed: {exception}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
