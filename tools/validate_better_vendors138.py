#!/usr/bin/env python3
"""Validate the 0.0.138 Better Vendors progression release.

Retains every inherited gate. Mechanical acceptance is the domain suite and
guarded runtime evidence, not these documentation/metadata checks. The release
was owner authorized with the merchant and persistence acceptance areas NOT RUN
and waived: the metadata must record that waiver, keep the five open areas
listed, and must not claim native runtime qualification that was not observed.
"""
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_rapid_reload_combat_feat137 as baseline
import validate_icon_overhaul132
import validate_magic_circle

VERSION = "0.0.138"
INFORMATIONAL_VERSION = "0.0.138-better-vendors-progression"
PACKAGE = "KingmakerGunslinger-0.0.138-local-runtime.zip"
PACKAGE_SUFFIX = "better-vendors-progression"
DETERMINISTIC_TEST_COUNT = 1742
STATIC_KEY = "betterVendorsProgression138"
PRESERVED_MANIFEST_ENTRIES = 1913
CATALOG = "docs/better-vendors-progression-catalog.json"

# Exact ordered identities appended after the preserved 1913-entry ledger.
APPENDED = (
    ("KMG.Firearms.PistolPlus2Item", "b767c7389ba64eb19008fa9d66712985"),
    ("KMG.Firearms.PistolPlus3Item", "e52bc130e64a4000875eaa0f903b5ffe"),
    ("KMG.Firearms.PistolPlus4Item", "7f7d1dede53648eeb2c72b65d0a128b3"),
    ("KMG.Firearms.PistolPlus5Item", "f8e9c22e1f404215b5fbc795e3853531"),
    ("KMG.Firearms.ReliablePistolPlus1Item", "03e9ba98b2954416b93f73b7045daed9"),
    ("KMG.Firearms.ReliablePistolPlus2Item", "04b5b1bb6e3f47bd8b7b7427ba913a7b"),
    ("KMG.Firearms.ReliablePistolPlus3Item", "aff91d56deba4542b28ac1a57741a1af"),
    ("KMG.Firearms.ReliablePistolPlus4Item", "cf82e7d300eb4400a4980ddb9c4ed44e"),
    ("KMG.Firearms.ReliablePistolPlus5Item", "4a4e1316201a47aeb4b74b8d31bddb7e"),
    ("KMG.Firearms.MusketPlus2Item", "839dd0d49e7c4fbc92764475e31ee175"),
    ("KMG.Firearms.MusketPlus3Item", "6980d1bfc1d94cd19ea033ad31335c79"),
    ("KMG.Firearms.MusketPlus4Item", "3785d6ecc69542a2a45d3c3f8e80fd79"),
    ("KMG.Firearms.MusketPlus5Item", "640573a7a9f74535a1054793cf0fbfba"),
    ("KMG.Firearms.ReliableMusketPlus1Item", "ffd5c4578cce403ebb42efb161a30f43"),
    ("KMG.Firearms.ReliableMusketPlus2Item", "ae4b32641ad847bfb4dc4e257dbd6415"),
    ("KMG.Firearms.ReliableMusketPlus3Item", "e9fe300679ec44dfacf6be958ebf7b19"),
    ("KMG.Firearms.ReliableMusketPlus4Item", "14d649b4ab534ebeb02f3c9477aca192"),
    ("KMG.Firearms.ReliableMusketPlus5Item", "15393669d06e4749b0e7729ec1cbebdf"),
    ("KMG.Firearms.BlunderbussPlus2Item", "ec12481bf56a4fa4addeefafdcfb0be7"),
    ("KMG.Firearms.BlunderbussPlus3Item", "1324e5cb0c374dd6b998546e83509d20"),
    ("KMG.Firearms.BlunderbussPlus4Item", "779fede186964943bb202aeaa9a517c4"),
    ("KMG.Firearms.BlunderbussPlus5Item", "ea8803dcb9a146e5af5265e5a5816b32"),
    ("KMG.Firearms.ReliableBlunderbussPlus1Item", "8ca37dd6f1c741bdbe7f18570037699c"),
    ("KMG.Firearms.ReliableBlunderbussPlus2Item", "3d45b093ac204b4ea859c76be35ecf0d"),
    ("KMG.Firearms.ReliableBlunderbussPlus3Item", "237cdb2b9292418387674526e5aea35d"),
    ("KMG.Firearms.ReliableBlunderbussPlus4Item", "165a278175c745bd8a15bff452a0a00c"),
    ("KMG.Firearms.ReliableBlunderbussPlus5Item", "f09173f6eb12483f82c711d2532b5095"),
    ("KMG.ElvenBranchedSpear.Plus2Item", "59c1cc59146a42b0bcecd080bf418f2b"),
    ("KMG.ElvenBranchedSpear.Plus3Item", "5ee1cb27471943cea19b3e76b3e06e46"),
    ("KMG.ElvenBranchedSpear.Plus4Item", "a15696e9747d47598c0149e7f5bad740"),
    ("KMG.ElvenBranchedSpear.Plus5Item", "972a58902b9148bbbc176750a0e0b867"),
    ("KMG.EasternWeapons.Wakizashi.Plus2Item", "744ffd16fc1b4bd98bd06eb419c0ec7d"),
    ("KMG.EasternWeapons.Wakizashi.Plus3Item", "331b4668855841da933887e29adf69d9"),
    ("KMG.EasternWeapons.Wakizashi.Plus4Item", "f89d3f4ff5e14ef984be001735e3f284"),
    ("KMG.EasternWeapons.Wakizashi.Plus5Item", "31887bcf6c124135aa1e4ff828c1dc21"),
    ("KMG.EasternWeapons.Katana.Plus2Item", "8a6b48a90a4743c9a74cd65b6d063094"),
    ("KMG.EasternWeapons.Katana.Plus3Item", "cc429a9210234467acfeeb16d6859193"),
    ("KMG.EasternWeapons.Katana.Plus4Item", "9c5a6631e1ac4a25a446fd27d3e7effa"),
    ("KMG.EasternWeapons.Katana.Plus5Item", "a31923daa72d44db93ae8a30ed12f956"),
    ("KMG.EasternWeapons.Nodachi.Plus2Item", "be8ad97d3b5f479788ddb73988b4fda8"),
    ("KMG.EasternWeapons.Nodachi.Plus3Item", "f25d6a89ba364cb0874bb22e559e06bf"),
    ("KMG.EasternWeapons.Nodachi.Plus4Item", "973eb3e4d6954ba2943dae2791649b07"),
    ("KMG.EasternWeapons.Nodachi.Plus5Item", "6502156bda38474a871062a2cbefd8cb"),
)


def require_tokens(path: Path, *tokens: str) -> str:
    text = path.read_text(encoding="utf-8")
    missing = [token for token in tokens if token not in text]
    if missing:
        raise AssertionError(f"{path.name} lacks required contract(s): {missing}")
    return text


def forbid_tokens(path: Path, *tokens: str) -> None:
    text = path.read_text(encoding="utf-8")
    present = [token for token in tokens if token in text]
    if present:
        raise AssertionError(f"{path.name} contains forbidden token(s): {present}")


def validate(root: Path) -> None:
    if len(APPENDED) != 43 or len({guid for _, guid in APPENDED}) != 43:
        raise AssertionError("The appended progression identity list is malformed")
    # The Magic Circle block stays exact; only these identities may follow it.
    validate_magic_circle.AUTHORIZED_APPENDED = APPENDED
    validate_icon_overhaul132.MANIFEST_TOTAL = PRESERVED_MANIFEST_ENTRIES + len(APPENDED)
    validate_icon_overhaul132.MANIFEST_ACTIVE = 1911 + len(APPENDED)
    baseline.VERSION = VERSION
    baseline.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    baseline.PACKAGE = PACKAGE
    baseline.PACKAGE_SUFFIX = PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT = DETERMINISTIC_TEST_COUNT
    baseline.validate(root)

    entries = json.loads((root / "blueprints/blueprints.json").read_text(
        encoding="utf-8"))["entries"]
    tail = entries[PRESERVED_MANIFEST_ENTRIES:]
    if [(entry["symbol"], entry["guid"]) for entry in tail] != list(APPENDED):
        raise AssertionError("Better Vendors progression identities drifted")
    if any(entry["plannedType"] != "BlueprintItemWeapon" or
           entry["status"] != "active" or
           entry["milestone"] != "Better Vendors progression" for entry in tail):
        raise AssertionError("Progression identities must be active weapon items")

    catalog = json.loads((root / CATALOG).read_text(encoding="utf-8"))
    rows = catalog["entries"]
    if len(rows) != 50 or sum(row["status"] == "reused" for row in rows) != 7:
        raise AssertionError("Machine-readable progression catalog count changed")
    new_rows = [(row["symbol"], row["guid"]) for row in rows if row["status"] == "new"]
    if new_rows != list(APPENDED):
        raise AssertionError("Machine-readable catalog and manifest diverged")
    by_symbol = {entry["symbol"]: entry["guid"] for entry in entries}
    for row in rows:
        if by_symbol.get(row["symbol"]) != row["guid"]:
            raise AssertionError("Catalog GUID mismatch: " + row["symbol"])
        equivalent = row["actualEnhancement"] + (1 if row["reliable"] else 0)
        if row["equivalentBonus"] != equivalent or row["baseItemCost"] != (
                row["mundaneBaseCost"] + 300 + 2000 * equivalent * equivalent):
            raise AssertionError("Catalog price mismatch: " + row["symbol"])
        if row["reliable"] and row["family"] not in ("Pistol", "Musket", "Blunderbuss"):
            raise AssertionError("Reliable must stay firearm-only: " + row["symbol"])
    if [(tier["tier"], tier["militaryRank"], tier["quantityAddedPerStockEvent"])
            for tier in catalog["schedule"]] != [(1, 1, 5), (2, 3, 5), (3, 5, 5),
                                                 (4, 7, 2), (5, 9, 2)]:
        raise AssertionError("Verified Better Vendors schedule changed")
    if catalog["betterVendors"]["verifiedModVersion"] != "2.0.8" or \
            catalog["betterVendors"]["verifiedFileSha256"] != \
            "8843509852964d9016d2996a3050bfbe6f068360c4f2f6b8ff7f6440ca712009":
        raise AssertionError("Verified Better Vendors identity changed")

    # Better Vendors stays optional: no UMM requirement, reference or payload.
    info = json.loads((root / "Info.json").read_text(encoding="utf-8"))
    if info.get("Requirements") != [] or info.get("Version") != VERSION:
        raise AssertionError("Info.json must not require Better Vendors")
    forbid_tokens(root / "src/KingmakerGunslinger/KingmakerGunslinger.csproj",
        "BetterVendors.dll", "<Reference Include=\"BetterVendors")
    forbid_tokens(root / "src/KingmakerGunslinger/Acquisition/BetterVendors/BetterVendorsStockRuntime.cs",
        "stockUpToDate", "FreeformData")
    forbid_tokens(root / "src/KingmakerGunslinger/Acquisition/BetterVendors/BetterVendorsCompatibilityCoordinator.cs",
        "stockUpToDate", "FreeformData", "using BetterVendors")
    require_tokens(root / "src/KingmakerGunslinger/Acquisition/BetterVendors/BetterVendorsContract.cs",
        "04fc03cf-853f-46c8-b6d5-1404180451fb", "7de959347266092448d8a72089ef9778",
        "232246b356d95af5fc57e7a72c8e9cc43349414c2e1250ad37c91ad31bfc25d4",
        # The exact approved binary is the gate, not the fingerprints.
        '"binary-sha256"', '"binary-mvid"')
    require_tokens(root / "src/KingmakerGunslinger/Acquisition/BetterVendors/BetterVendorsGrantPlanner.cs",
        # Initial grants are claimed before stock changes.
        "claimed = record(grant.Spec.Guid);", "internal bool Withdraw(string guid)")
    require_tokens(root / "src/KingmakerGunslinger/Main.cs",
        "BetterVendorsCompatibilityCoordinator")

    state = json.loads((root / "validation/static-validation.json").read_text(
        encoding="utf-8"))[STATIC_KEY]
    expected = {
        "deterministicTestCount": DETERMINISTIC_TEST_COUNT,
        "publicReleaseAuthorized": True,
        "ownerAuthorizedRelease": True,
        "candidateOnly": False,
        "releaseVersion": VERSION,
        "releaseInformationalVersion": INFORMATIONAL_VERSION,
        "progressionEntries": 50,
        "reusedCanonicalEntries": 7,
        "newBlueprints": 43,
        "betterVendorsVerifiedVersion": "2.0.8",
        "betterVendorsVerifiedMvid": "04fc03cf-853f-46c8-b6d5-1404180451fb",
        "betterVendorsRequired": False,
        "saveLocalLedger": "UnitPartBetterVendorsProgressionGrants",
        "ordinaryVendorPublicationChanged": False,
        "betterVendorsVerifiedFileSha256":
            "8843509852964d9016d2996a3050bfbe6f068360c4f2f6b8ff7f6440ca712009",
    }
    for key, value in expected.items():
        if state.get(key) != value:
            raise AssertionError(f"Better Vendors release metadata mismatch: {key}")
    if not isinstance(state.get("nativeRuntimeQualified"), bool):
        raise AssertionError("Native runtime qualification must be recorded explicitly")
    if state.get("nativeRuntimeQualified") and not state.get("nativeRuntimeEvidence"):
        raise AssertionError("A native qualification claim needs recorded evidence")
    # Until merchant and persistence acceptance has real evidence, the five
    # acceptance areas stay listed, and the build is either merge-blocked or
    # released only under an explicit, recorded owner waiver.
    if not state.get("nativeRuntimeQualified"):
        waived = state.get("ownerAuthorizedRelease") is True and \
            state.get("acceptanceWaivedByOwner") is True and \
            bool(state.get("ownerReleaseInstruction"))
        if len(state.get("pendingAcceptance") or []) != 5 or \
                not (state.get("mergeBlocked") is True or waived):
            raise AssertionError(
                "Unqualified merchant acceptance must stay listed and either block merge or carry a recorded owner waiver")
    if not isinstance(state.get("craftMagicItemsInteractionTested"), bool):
        raise AssertionError("The Craft Magic Items interaction status must be recorded")

    require_tokens(root / "docs/RELEASE-NOTES-0.0.138.md",
        INFORMATIONAL_VERSION, "Better Vendors",
        "published under explicit owner authorization",
        "NOT RUN, waived by the owner", "2.0.8", "Military", "Reliable",
        "catch-up", "optional", "uninstall")
    require_tokens(root / "docs/BETTER-VENDORS-COMPATIBILITY.md",
        "2.0.8", "04fc03cf-853f-46c8-b6d5-1404180451fb",
        "8843509852964d9016d2996a3050bfbe6f068360c4f2f6b8ff7f6440ca712009",
        "7de959347266092448d8a72089ef9778", "stockUpToDate",
        "UnitPartBetterVendorsProgressionGrants", "Reliable", "catch-up",
        "replenish", "corrosive", "Military VII", "uninstall",
        "exact approved binary", "write-ahead",
        "## Merchant and persistence acceptance (NOT RUN, waived for 0.0.138)",
        "## Known limitations and intentional differences",
        "This integration itself never removes merchandise")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except Exception as exc:
        print(f"Better Vendors progression {VERSION} validation failed: {exc}",
              file=sys.stderr)
        return 1
    print(f"Better Vendors progression {VERSION} release validation passed.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
