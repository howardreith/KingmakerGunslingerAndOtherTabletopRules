#!/usr/bin/env python3
"""Validate the 0.0.139 Favored Class integration candidate.

Retains every inherited gate. Mechanical acceptance is the domain suite and
guarded runtime evidence, not these documentation/metadata checks. This is an
unpublished local candidate: the metadata must say so, must keep the exact
qualified host profile, and must never claim native qualification that was not
observed.
"""
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_better_vendors138 as baseline
import validate_sprint32

VERSION = "0.0.139"
INFORMATIONAL_VERSION = "0.0.139-favored-class-integration"
PACKAGE = "KingmakerGunslinger-0.0.139-local-runtime.zip"
PACKAGE_SUFFIX = "favored-class-integration"
DETERMINISTIC_TEST_COUNT = 1787
STATIC_KEY = "favoredClassIntegration139"

# Exact ordered (symbol, guid) pairs this candidate appends after the
# Better Vendors block.
APPENDED = (
    ("KMG.FavoredClass.Gunslinger.Grit.Partial", "718289fb8ab945e48880961722a344fd"),
    ("KMG.FavoredClass.Gunslinger.Grit.Full", "cd8674400bea40adbd8489a08b14eeff"),
    ("KMG.FavoredClass.Gunslinger.Misfire.Pistol.Partial", "115ab4b2b0174a708cb93adac7826079"),
    ("KMG.FavoredClass.Gunslinger.Misfire.Pistol.Full", "6d125884d3f94f83a59ba57112a31ec6"),
    ("KMG.FavoredClass.Gunslinger.Misfire.Musket.Partial", "103ecf217c6b4e93865a846a2c584f03"),
    ("KMG.FavoredClass.Gunslinger.Misfire.Musket.Full", "a603feebd92b45c683be87fe17b084c2"),
    ("KMG.FavoredClass.Gunslinger.Misfire.Blunderbuss.Partial", "7627279fc5ae4239b6ccaa51d3f55032"),
    ("KMG.FavoredClass.Gunslinger.Misfire.Blunderbuss.Full", "8bf4dee4bb164d93bf22a3bbff946dd7"),
    ("KMG.FavoredClass.Gunslinger.FirearmConfirmation.Partial", "b07889d80b5f48b285e364885a512815"),
    ("KMG.FavoredClass.Gunslinger.FirearmConfirmation.Full", "9d8e1f609b2b47d2b9c80a3525022e69"),
    ("KMG.FavoredClass.Gunslinger.PistolWhip.Partial", "e24c74437b88443298f1861c1eb2043e"),
    ("KMG.FavoredClass.Gunslinger.PistolWhip.Full", "84639afbb1d14aba83dcf6630eb626c0"),
    ("KMG.FavoredClass.Gunslinger.HalflingNimble.Partial", "40269699043e4fe3a512e5ee28dd93bd"),
    ("KMG.FavoredClass.Gunslinger.HalflingNimble.Full", "29a320ea1af0499d9185fe8321d60cae"),
    ("KMG.FavoredClass.Gunslinger.HalflingDodge.Partial", "b62476699d734b10bb1c124b549f634f"),
    ("KMG.FavoredClass.Gunslinger.HalflingDodge.Full", "5400219fafad4780b440dee91788eb68"),
    ("KMG.FavoredClass.Gunslinger.DrowNimble.Partial", "49359d21155e4741b6a1f438bf0db7ed"),
    ("KMG.FavoredClass.Gunslinger.DrowNimble.Full", "efb83ae0fde04abb96dd1e7cce0211e7"),
    ("KMG.FavoredClass.Gunslinger.Initiative.Partial", "385a62ab48214b32accaa2f5e7c86d66"),
    ("KMG.FavoredClass.Gunslinger.Initiative.Full", "403489a552e5470bb720a8148c2b09ed"),
    ("KMG.FavoredClass.Gunslinger.DirtyTrickTrip.Partial", "422ad9a4bd294b84b2f6230856ecdd10"),
    ("KMG.FavoredClass.Gunslinger.DirtyTrickTrip.Full", "ece977845f1c4b40a6df5183b6caa7e4"),
    ("KMG.FavoredClass.Alchemist.BombDamage.Partial", "6d101f4776294ec78ac07b2bf8b1ace7"),
    ("KMG.FavoredClass.Alchemist.BombDamage.Full", "c98af1a0630a452e8af968684e285183"),
    ("KMG.FavoredClass.Inquisitor.FireIntimidate.Partial", "96c9cb2bc89941b2a37f126aafccf3ca"),
    ("KMG.FavoredClass.Inquisitor.FireIntimidate.Full", "98847fc06c1c4f53a1da0ce0df78f266"),
    ("KMG.FavoredClass.Rogue.Demoralize.Partial", "a9f9782987db4b518ccc6726039f56fc"),
    ("KMG.FavoredClass.Rogue.Demoralize.Full", "bf08007130b24072b50fa165e640ffa1"),
    ("KMG.FavoredClass.Fighter.BullRushDefense.Full", "429de527d5dc46039e9d5a2311901374"),
    ("KMG.FavoredClass.Monk.UnarmedConfirmation.Partial", "8b3453ed61fe489a9f120c773215b68e"),
    ("KMG.FavoredClass.Monk.UnarmedConfirmation.Full", "c76759e1e881420a9248ce8fe74b5004"),
    ("KMG.FavoredClass.Cleric.AquaticPenetration.Full", "189538fd7b67433e8d0cea5e691dd8eb"),
    ("KMG.FavoredClass.Monk.GrappleStunning.Partial", "9c56579186ef4fdba0f07d956b1cb9a5"),
    ("KMG.FavoredClass.Monk.GrappleStunning.Full", "ca51bd6d19b241e48f05e6acf892a5f1"),
    ("KMG.MostlyHuman.Identity", "d71a4b4250914a7aa6d4bec53dfc768f"),
    ("KMG.MostlyHuman.Ifrit.Selection", "c0485fc76e514ad38c09961dbdc6efc7"),
    ("KMG.MostlyHuman.Ifrit.Standard", "7c508ee3415942bb91f584306d70bf70"),
    ("KMG.MostlyHuman.Ifrit.Trait", "8060afe7da3b40bbbc77b315ee23fdd3"),
    ("KMG.MostlyHuman.Oread.Selection", "e5bcb7c34a7b4b048c0d877019edac63"),
    ("KMG.MostlyHuman.Oread.Standard", "5c39e1237c4d4bbd9bfd0f1eaacb18c3"),
    ("KMG.MostlyHuman.Oread.Trait", "65aedd35c5a54273a9a33c2d58683a21"),
    ("KMG.MostlyHuman.Sylph.Selection", "2c93d16297764ce9a0f73622b9825c05"),
    ("KMG.MostlyHuman.Sylph.Standard", "0be790c9af23490e8f3ed46f5a59d00c"),
    ("KMG.MostlyHuman.Sylph.Trait", "21451d96af0843d088b04331ac7a5a99"),
    ("KMG.MostlyHuman.Undine.Selection", "bb0bae8a5720452eb16a1a6629e8fd4d"),
    ("KMG.MostlyHuman.Undine.Standard", "66868270c36a48c89947fbad8e6d979c"),
    ("KMG.MostlyHuman.Undine.Trait", "f3f6f6e074114b8da0a94a49b3cf4b47"),
)

HOST_SHA256 = "dcd3adf98d1a04c30d772381e7c56ce4beff35a98bcea165aff206a2f0aac26c"
HOST_MVID = "3efd38e7-8682-4b4d-8d53-e368a3664919"
COTW_SHA256 = "4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915"
COTW_MVID = "8caab254-aacf-4811-8093-44b9184e6e53"


def validate(root: Path) -> None:
    # Favored-class misfire reductions (G01/G18) made the scatter all-roll
    # aggregate use the effective threshold that decided each native roll.
    validate_sprint32.SCATTER_MISFIRE_AGGREGATE_TOKEN = "IsMisfire(misfireThreshold)"
    baseline.AUTHORIZED_APPENDED_AFTER = APPENDED
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.validate(root)

    # The host stays optional: no UMM requirement or compile-time reference.
    info = json.loads((root / "Info.json").read_text(encoding="utf-8"))
    if info.get("Requirements") != [] or info.get("Version") != VERSION:
        raise AssertionError("Info.json must not require Favored Class or Call of the Wild")
    project = root / "src/KingmakerGunslinger/KingmakerGunslinger.csproj"
    baseline.forbid_tokens(project, "ZFavoredClass.dll", "CallOfTheWild.dll",
        "<Reference Include=\"ZFavoredClass", "<Reference Include=\"CallOfTheWild")
    # The adapter reads the host by reflection and never executes host code:
    # no reflective invocation, no second Core.load(), no runtime identities.
    for source in (root / "src/KingmakerGunslinger/FavoredClass").rglob("*.cs"):
        baseline.forbid_tokens(source, "using ZFavoredClass", "using CallOfTheWild",
            "Core.load();", ".Invoke(", "Guid.NewGuid")
    baseline.require_tokens(
        root / "src/KingmakerGunslinger/FavoredClass/FavoredClassHostContract.cs",
        HOST_SHA256, HOST_MVID, COTW_SHA256, COTW_MVID,
        '"binary-sha256"', '"binary-mvid"', '"dependency-sha256"', '"dependency-mvid"',
        '"core-load-incomplete"', '"gunslinger-not-scanned"')
    baseline.require_tokens(
        root / "src/KingmakerGunslinger/Scatter/ScatterAttackVolleyService.cs",
        "return Evaluate(definition, plan, rolls, definition.MisfireValue);",
        "if (roll.IsMisfire(misfireThreshold)) misfires++;")
    baseline.require_tokens(
        root / "src/KingmakerGunslinger/Misfires/EffectiveFirearmMisfireValuePolicy.cs",
        "Math.Max(FavoredClassFloor, unclamped - favoredClassReduction)",
        "if (unclamped <= MinimumEffectiveValue)")
    baseline.require_tokens(
        root / "docs/FAVORED-CLASS-COMPATIBILITY.md",
        HOST_SHA256, HOST_MVID, COTW_SHA256, COTW_MVID,
        "N = fullRank + partialRank", "unrestricted", "outside")

    static = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))
    if static.get("version") != VERSION or static.get("milestone") != INFORMATIONAL_VERSION:
        raise AssertionError("Static validation does not identify the 0.0.139 candidate")
    state = static[STATIC_KEY]
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "publicReleaseAuthorized": False,
        "candidateOnly": True,
        "releaseVersion": VERSION,
        "releaseInformationalVersion": INFORMATIONAL_VERSION,
        "hostVerifiedVersion": "1.3.1",
        "hostVerifiedFileSha256": HOST_SHA256,
        "hostVerifiedMvid": HOST_MVID,
        "callOfTheWildVerifiedFileSha256": COTW_SHA256,
        "callOfTheWildVerifiedMvid": COTW_MVID,
        "hostRequired": False,
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"Favored Class candidate metadata mismatch: {key}")
    if not isinstance(state.get("compatibilityRuntimeQualificationPending"), bool):
        raise AssertionError("The compatibility qualification status must be recorded")
    if not isinstance(state.get("nativeRuntimeQualified"), bool):
        raise AssertionError("Native runtime qualification must be recorded explicitly")
    if state.get("nativeRuntimeQualified") and not state.get("nativeRuntimeEvidence"):
        raise AssertionError("A native qualification claim needs recorded evidence")

    baseline.require_tokens(root / "docs/RELEASE-NOTES-0.0.139.md",
        INFORMATIONAL_VERSION, "Favored Class", "optional", "candidate",
        "not published", "uninstall")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Favored Class integration {VERSION} validation failed: {exc}",
              file=sys.stderr)
        return 1
    print(f"Favored Class integration {VERSION} validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
