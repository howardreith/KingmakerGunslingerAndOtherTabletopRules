#!/usr/bin/env python3
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_eastern_favored93 as baseline

VERSION = "0.0.94"
INFORMATIONAL_VERSION = "0.0.94-in-harms-way-runtime-repair"
PACKAGE = "KingmakerGunslinger-0.0.94-local-runtime.zip"
DETERMINISTIC_TEST_COUNT = 1212
STATIC_KEY = "inHarmsWay94"
EXPECTED_LEDGER_ENTRIES = 1628
EXPECTED_ACTIVE_BLUEPRINTS = 1627
PROJECT_BLUEPRINT_COUNT = 12
ADDITIONAL_IDENTITIES = {}


def require_tokens(path: Path, *tokens: str) -> None:
    if not path.is_file():
        raise AssertionError(f"In Harm's Way gate file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks In Harm's Way contract token(s): {missing}")


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.STATIC_KEY = STATIC_KEY
    baseline.RUNTIME_QUALIFICATION_PENDING = VERSION not in {
        "0.0.98", "0.0.99", "0.0.100", "0.0.101", "0.0.102",
        "0.0.103", "0.0.104", "0.0.105", "0.0.106", "0.0.107",
        "0.0.108", "0.0.109", "0.0.110"}
    baseline.EXPECTED_LEDGER_ENTRIES = EXPECTED_LEDGER_ENTRIES
    baseline.EXPECTED_ACTIVE_BLUEPRINTS = EXPECTED_ACTIVE_BLUEPRINTS
    baseline.PROJECT_BLUEPRINT_COUNT = PROJECT_BLUEPRINT_COUNT
    baseline.HEIRLOOM_IDENTITIES.update(ADDITIONAL_IDENTITIES)
    baseline.validate(root)

    required = (
        "src/KingmakerGunslinger/BodyguardFeats/InHarmsWayCandidateGate.cs",
        "src/KingmakerGunslinger/RuntimeTesting/InHarmsWayHumanReproScenario.cs",
        "scripts/Invoke-InHarmsWayHumanRepro.ps1",
        "tests/KingmakerGunslinger.DomainTests/BodyguardPolicyTests.cs",
        "tests/KingmakerGunslinger.DomainTests/BodyguardRuntimeContractTests.cs",
        "src/KingmakerGunslinger/BodyguardFeats/BodyguardRuntime.cs",
    )
    for relative in required:
        if not (root / relative).is_file():
            raise AssertionError(
                f"In Harm's Way gate file missing: {relative}")

    require_tokens(root / required[0],
        "missing-in-harms-way-feat", "activatable-marker-divergence",
        "swift-cooldown-active", "delivery-contract-unavailable",
        "return \"eligible\"")
    require_tokens(root / required[1],
        "available-normal-hit", "available-confirmed-critical",
        "immediate-unavailable", "Rulebook.Trigger(attack)",
        "VictimHpLoss", "ProtectorHpLoss")
    require_tokens(root / required[2],
        "3414D67CB2E5F8C4F18A952D23247DC6DD9D9F5579066EA64CA7FF29E61B8F01",
        "KMG_IHW_HUMAN_REPRO_COPY.zks",
        "The original human test save changed")
    require_tokens(root / required[5], "selected-and-intercepted",
        "interception.candidate", "ObserveNativeDelivery")

    project = (root / "src/KingmakerGunslinger/"
        "KingmakerGunslinger.csproj").read_text(encoding="utf-8")
    test_project = (root / "tests/KingmakerGunslinger.DomainTests/"
        "KingmakerGunslinger.DomainTests.csproj").read_text(encoding="utf-8")
    runner = (root / "tests/KingmakerGunslinger.DomainTests/"
        "Program.cs").read_text(encoding="utf-8")
    for file_name in ("InHarmsWayCandidateGate.cs",
                      "InHarmsWayHumanReproScenario.cs"):
        if file_name not in project:
            raise AssertionError(f"Production file is not compiled: {file_name}")
    if "InHarmsWayCandidateGate.cs" not in test_project:
        raise AssertionError("Candidate-gate policy is not compiled in tests")
    if "bodyguard-policy.interception-gate-diagnostics" not in runner:
        raise AssertionError("Candidate-gate tests are not registered")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"In Harm's Way Runtime Repair {VERSION} validation failed: "
              f"{exception}", file=sys.stderr)
        return 1
    print(f"In Harm's Way Runtime Repair {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
