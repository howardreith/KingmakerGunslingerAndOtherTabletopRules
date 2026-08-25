#!/usr/bin/env python3
from __future__ import annotations

import argparse
import hashlib
import json
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_immediate_action95 as baseline

VERSION = "0.0.96"
INFORMATIONAL_VERSION = "0.0.96-firearm-audio-restoration"
PACKAGE = "KingmakerGunslinger-0.0.96-local-runtime.zip"
DETERMINISTIC_TEST_COUNT = 1224
STATIC_KEY = "firearmAudio96"
BANK_SHA256 = (
    "0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18")
MANIFEST_SHA256 = (
    "BF57981AD5EC2CBF3149ECAFC3EF737D87BC9035B14BCCC7D254DCA8F991C62E")
EVENTS = {
    "Pistol": "KMG_Firearm_Pistol_Shot",
    "Musket": "KMG_Firearm_Musket_Shot",
    "Blunderbuss": "KMG_Firearm_Blunderbuss_Shot",
    "Revolver": "KMG_Firearm_Revolver_Shot",
    "Rifle": "KMG_Firearm_Rifle_Shot",
}


def require_tokens(path: Path, *tokens: str) -> str:
    if not path.is_file():
        raise AssertionError(f"Firearm-audio release file missing: {path}")
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(
            f"{path.name} lacks firearm-audio release token(s): {missing}")
    return text


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def validate(root: Path) -> None:
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.STATIC_KEY = STATIC_KEY
    baseline.validate(root)

    loader = require_tokens(
        root / "src/KingmakerGunslinger/Audio/"
        "FirearmSoundBankManifestLoader.cs",
        "JsonTextReader", "manifest.json-parsing",
        "manifest.schema-extraction", "manifest.semantic-validation",
        "duplicate field", "unknown field")
    if "JsonConvert.DeserializeObject<FirearmSoundBankManifest>" in loader:
        raise AssertionError("Production manifest loader still uses global JsonConvert")

    tests = require_tokens(
        root / "tests/KingmakerGunslinger.DomainTests/FirearmAudioTests.cs",
        "ProductionManifestParsing", "CopiedManifestRepresentationParsing",
        "ProcessGlobalSerializerIsolation", "JsonConvert.DefaultSettings",
        "StrictManifestParsingFailures", "RetryParserAfterActualFault",
        "finally")
    if "original" not in tests or "JsonConvert.DefaultSettings = original" not in tests:
        raise AssertionError("Hostile serializer test does not restore global settings")

    require_tokens(
        root / "src/KingmakerGunslinger/RuntimeTesting/"
        "RuntimeTestScenarioCatalog.cs",
        "disposable-firearm-wwise-audio")
    require_tokens(
        root / "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs",
        "all-firearm-events-accepted", "ordinary-miss-event-accepted",
        "blunderbuss-misfire-no-normal-event",
        "native-crossbow-audio-isolation", "scatter-audio-once-per-volley",
        "deed-audio-commit-boundaries")
    require_tokens(
        root / "scripts/validate-package.ps1",
        "--validate-firearm-artifact", "KMG_Firearms.bnk", "Init.bnk")
    require_tokens(
        root / "docs/FIREARM-WWISE-MANUAL-AUDITORY-ACCEPTANCE.md",
        "Sound effect sounds working to me", "owner auditory release gate accepted")
    release_suffix = ("starter-bokken-combat-log-acadamae-toggle"
        if VERSION == "0.0.102" else "craft-magic-items-compatibility"
        if VERSION == "0.0.101" else "craft-magic-items-post-human-refinement"
        if VERSION == "0.0.100" else "craft-magic-items-ammunition-ui-repair"
        if VERSION == "0.0.99" else "craft-magic-items-compatibility"
        if VERSION == "0.0.98" else "compatibility-attribution-audit"
        if VERSION == "0.0.97" else "firearm-audio-restoration")
    require_tokens(
        root / f"docs/RELEASE-NOTES-{VERSION}.md",
        f"Kingmaker Gunslinger {VERSION}", f"{release_suffix}.zip",
        BANK_SHA256)
    require_tokens(
        root / "scripts/Publish-Release.ps1",
        f"docs\\RELEASE-NOTES-{VERSION}.md", "ConfirmReleaseReady")

    manifest_path = root / "assets/soundbanks/firearm-soundbank-manifest.json"
    bank_path = root / "assets/soundbanks/KMG_Firearms.bnk"
    if sha256(manifest_path) != MANIFEST_SHA256:
        raise AssertionError("Canonical firearm manifest bytes changed")
    if sha256(bank_path) != BANK_SHA256:
        raise AssertionError("Qualified firearm SoundBank bytes changed")
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    expected = {
        "schemaVersion": 1,
        "bankName": "KMG_Firearms",
        "bankFileName": "KMG_Firearms.bnk",
        "platform": "Windows",
        "wwiseVersion": "2016.2.6.6153",
        "sha256": BANK_SHA256,
        "mediaEmbedded": True,
        "events": EVENTS,
    }
    if manifest != expected:
        raise AssertionError("Canonical firearm manifest contract changed")

    static = json.loads((root / "validation/static-validation.json")
        .read_text(encoding="utf-8"))
    contract = static.get(STATIC_KEY, {})
    expected_static = {
        "productionManifestLoaderIsolated": True,
        "runtimeAudioRoutingQualified": True,
        "ownerAuditoryAcceptance": True,
        "soundBankBytesPreserved": True,
    }
    for key, value in expected_static.items():
        if contract.get(key) != value:
            raise AssertionError(
                f"Firearm-audio static validation mismatch: {key}")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exception:
        print(f"Firearm Audio Restoration {VERSION} validation failed: "
              f"{exception}", file=sys.stderr)
        return 1
    print(f"Firearm Audio Restoration {VERSION} source validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
