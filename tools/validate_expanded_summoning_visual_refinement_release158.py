"""v0.0.158 Expanded Summoning visual refinement release on released v0.0.157.

Relative to released v0.0.157 master (after its release record, PR #53) the
release changes no identity: the released ledger is unchanged. It changes only
the pinned visual-refinement production files, the pinned Brown-Fur startup-
order and CotW controller save-loading repair files (PR #54) and the version
metadata; adds only the pinned compile items to the project; modifies only the
pinned assets, in place, to exactly their reviewed bytes (none is added or
removed); and adds only its own record to the static evidence. The released
Sprint 24 public boundary still holds. Its Tyrannosaurus and Deinonychus art
hashes are superseded by the refined bytes pinned here; every other Sprint 24
asset keeps its released bytes, and all of them stay present and packaged.
The historical sprint and release validators are left as they were.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import subprocess
import sys
from pathlib import Path

sys.dont_write_bytecode = True
import validate_development_candidate as identities
import validate_expanded_summoning_sprint21150 as common
import validate_expanded_summoning_sprint24154 as released155
import validate_alternate_summoning_release157 as released157

# Released v0.0.157 master, after its release record (PR #53).
RELEASED = "2eed3944f6eab657f9030d33acae6d81eb5f10ae"
VERSION = "0.0.158"
INFORMATIONAL = "0.0.158-expanded-summoning-visual-refinement"
RECORD_KEY = "expandedSummoningVisualRefinement158"

VISUAL_REFINEMENT_PRODUCTION = {
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningCreatureReview.cs",
    "src/KingmakerGunslinger/RuntimeTesting/VisualRefinementReviewPolicy.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningSummonGlowPatch.cs",
    "src/KingmakerGunslinger/Summoning/PrimateVisualPolicy.cs",
    "src/KingmakerGunslinger/Summoning/SalamanderHumanVisualAttachment.cs",
    "src/KingmakerGunslinger/Summoning/SalamanderTailGait.cs",
    "src/KingmakerGunslinger/Summoning/SummonGlowPolicy.cs",
}
STARTUP_REPAIR_PRODUCTION = {
    "src/KingmakerGunslinger/Blueprints/BrownFurBlueprints.cs",
    "src/KingmakerGunslinger/Bootstrap/BlueprintBootstrap.cs",
    "src/KingmakerGunslinger/BrownFur/BrownFurOptionalExtensionCoordinator.cs",
    "src/KingmakerGunslinger/BrownFur/BrownFurStartupGate.cs",
    "src/KingmakerGunslinger/Compatibility/ClassCatalogDiagnostics.cs",
    "src/KingmakerGunslinger/Compatibility/CotwSpontaneousSlotCompatibility.cs",
    "src/KingmakerGunslinger/EasternWeapons/EasternWeaponLatePublicationCoordinator.cs",
    "src/KingmakerGunslinger/FavoredClass/FavoredClassIntegrationCoordinator.cs",
    "src/KingmakerGunslinger/FeatureModules/BrownFurFeatureStatus.cs",
    "src/KingmakerGunslinger/Main.cs",
    "src/KingmakerGunslinger/RuntimeTesting/BrownFurNativeCastScenario.cs",
    "src/KingmakerGunslinger/Spells/ShieldOther/ShieldOtherFinalLiveReconciler.cs",
    "src/KingmakerGunslinger/Spells/Teleportation/TeleportationFinalLiveReconciler.cs",
}
VERSION_PRODUCTION = {"src/KingmakerGunslinger/Properties/AssemblyInfo.cs"}
PROJECT = "src/KingmakerGunslinger/KingmakerGunslinger.csproj"
PROJECT_LINES = [
    '    <Compile Include="Compatibility\\CotwSpontaneousSlotCompatibility.cs" />',
    '    <Compile Include="Summoning\\SummonGlowPolicy.cs" />',
    '    <Compile Include="Summoning\\ExpandedSummoningSummonGlowPatch.cs" />',
    '    <Compile Include="Summoning\\SalamanderTailGait.cs" />',
    '    <Compile Include="BrownFur\\BrownFurStartupGate.cs" />',
    '    <Compile Include="RuntimeTesting\\VisualRefinementReviewPolicy.cs" />',
]

# Every asset the release changes, with its reviewed bytes (the refined and
# repaired creature bodies, their albedos, and the Dire Rat icon and the
# runtime icon manifest that lists it).
ART = {
    "assets/ankylosaurus/ankylosaurus-albedo.png":
        "2cdd8bc7e9139e748a9a88ec52e8a360245b6ca5ecb7829eec78a57d5a598bac",
    "assets/ankylosaurus/ankylosaurus-mesh.json":
        "3eb7585e6f410711c7bf84bf4e23ff0df2c04fbe06ec226a0552a715c9fa9ca9",
    "assets/flying-animals/dire-bat-albedo.png":
        "ffb7ed2bcf2a7d2c5a48e7305b75da2d81d9ee6fb9a946ea90ae9caee76b9a4b",
    "assets/flying-animals/dire-bat-mesh.json":
        "b04e9dd146b1c9db87de7907f7b6affb4f7910ff8fcfe4cd65288217f17b7cce",
    "assets/flying-animals/eagle-mesh.json":
        "4b254a4b17bd1cab51b4856203dc07ff5ad70091d037e9e26eaa50aecaae93c4",
    "assets/flying-animals/giant-wasp-albedo.png":
        "2d26b8dad8436d26e625c0d16761c9ad35bff0f75fd984eed38198b8825a8943",
    "assets/flying-animals/giant-wasp-mesh.json":
        "a32d454cff58ec4827d91888a3d9803178ad74c731882a22e1bd2ebf83b205c5",
    "assets/flying-animals/stirge-mesh.json":
        "f9ea0f0e639faf6f642b41f59a6e3216e2838f5c3dbb608a26c0c7eb073b3b82",
    "assets/game/icons/expanded-summoning/dire-rat.png":
        "159700710b467fc906795db4b7d583ac5c9316f85c6dc395aaae5169982efb07",
    "assets/game/icons/expanded-summoning/icon-manifest.json":
        "c68ec0be7c9dbc3d9816f73c2a1082a1a4dd4e95c8ee7441697dee56171d6f13",
    "assets/pteranodon/pteranodon-albedo.png":
        "0d9e252094d1e7b02b6ee4286c3588d36d409224281f2269ee4d15bcfa337455",
    "assets/pteranodon/pteranodon-mesh.json":
        "42627bc93837ab0979bd4e1e338a9900bdb038cb16c71a4d20d2c50185a24345",
    "assets/sprint12-quadrupeds/dire-rat-albedo.png":
        "746cb3c4b0c417430fa22ffa2f568bd0571080fe9e7ce72b090577af7356f422",
    "assets/sprint12-quadrupeds/dire-rat-mesh.json":
        "cdc262ab43da10301e5f81d01fb94f7c0ee74d621e03fb4c444983341073fbf4",
    "assets/sprint12-quadrupeds/goblin-dog-albedo.png":
        "dc8a53f09f0ce21a601bfc8773945050ed4389dfebd51b5109827c38c255789c",
    "assets/sprint12-quadrupeds/goblin-dog-mesh.json":
        "fa07b94074272d45e2d014573e5d2feeb29f89c1bc58777d0421a9a31049710b",
    "assets/sprint12-quadrupeds/hyena-albedo.png":
        "0b92a99aeb02d3381c923da2a2eff3b093b41706a5f1570b39575face5a7d404",
    "assets/sprint12-quadrupeds/hyena-mesh.json":
        "bb02ff0e93fef0de73100d0dac0769cf36f461fdadf166eada09a40059bb07ef",
    "assets/sprint13-creatures/poisonous-frog-albedo.png":
        "ce040536c0a3de1815e7e4770083a0e42092045e9f754a207ed7cdf2ee56211e",
    "assets/sprint13-creatures/poisonous-frog-mesh.json":
        "17529a2781d680ad8e995eb52cfaef71fb8cccba848f6bd4039ed3f20279d79a",
    "assets/sprint13-creatures/shadow-mastiff-albedo.png":
        "f335df805a3e211a4effd4744823ca7d00c9b9e02e0e4339f5b316899c684246",
    "assets/sprint13-creatures/shadow-mastiff-mesh.json":
        "fdb05b264035c10dbba7c066957a11c9ebfbff49c069bb276b7a80c84a751862",
    "assets/sprint13-creatures/wolverine-albedo.png":
        "131a49db826403a5abdf0979825c3e737b568751a61aab4d8e595a1a99b22bd7",
    "assets/sprint13-creatures/wolverine-mesh.json":
        "f2c4385b53a3cead1b8dbd38299dd199bc4c7b5d1c09624ea1a1dbbd7dffb6d6",
    "assets/sprint14-insects/fire-beetle-mesh.json":
        "71acda0c408a0f925f50591744f9355ecc2f8cc8c126771e33ca4cd74eb8a661",
    "assets/sprint14-insects/giant-ant-drone-mesh.json":
        "2aa85130a354e498acb17bb3663d21618eb6f1248cf67cd16f02ce20496f86e8",
    "assets/sprint14-insects/giant-ant-soldier-mesh.json":
        "0eb9a2ea895477581ef18dc0af05f6b3d9b645e071418c4ab121e34f16048d1a",
    "assets/sprint14-insects/giant-ant-worker-mesh.json":
        "e6e3df7b477170b1f80bbd7ceefdfe573a6d31a3d70832fb4cb64e22841500fb",
    "assets/sprint14-insects/giant-stag-beetle-mesh.json":
        "a7276d63c5e14fb6f45adb055c437bf61980f43344ee329b4c6cd5d048cc3cd6",
    "assets/sprint16-crocodilians/crocodile-albedo.png":
        "108668f562d97b84ba8fff5caa6cda92a042100d37831f52fcfbd20d19537c85",
    "assets/sprint16-crocodilians/crocodile-mesh.json":
        "6615b76808762e29311fb390889dc8d94c63b942825573cb7266993780d96ef9",
    "assets/sprint16-crocodilians/dire-crocodile-albedo.png":
        "86488f581289d2dfe7f5f6bcb5f3ec9e0ea25403fb6aace460bcd0a82d562d3c",
    "assets/sprint16-crocodilians/dire-crocodile-mesh.json":
        "d4507796f688e79d9ddddacbcb54d0885ea409b06cd5c58690a3fb56464daf1e",
    "assets/sprint18-primates/ape-albedo.png":
        "85c7f480eeac7dddb125961061f0e62ba2460505fcee8198e88c208a95c6c7d9",
    "assets/sprint18-primates/ape-mesh.json":
        "2f0196eca297019770894b66d69cb474e9bba59c449a95590326f888086631ee",
    "assets/sprint18-primates/dire-ape-albedo.png":
        "37bdc23e705bfa0771105b768d1cd052157dfdc1d2c6bbd2a99236cf8268e219",
    "assets/sprint18-primates/dire-ape-mesh.json":
        "d0c1d0324cf7a12bbbc9b64874dfefb3ba010fb82020b87f72a47c9d9658700a",
    "assets/sprint19-fourarmed/girallon-mesh.json":
        "003512a54f6daeabd483f7cb85b1e84bf05b57b0d45a9ab1374cc09ca857d0bb",
    "assets/sprint19-fourarmed/xill-albedo.png":
        "eca0206533fad6c67cb2cbce79c5ff4eac7987b3311229c8d9f6b883522999fa",
    "assets/sprint19-fourarmed/xill-mesh.json":
        "d7a0cb0ade14f00e6df09bc8b5a57654083cbd1ebe23599fcdb37ec497f1a243",
    "assets/sprint20-arachnids/giant-scorpion-mesh.json":
        "1b48a74e4296f4fc0e4903b3fa9ec1b776aae1b85f512d91ebc833e3619bba7f",
    "assets/sprint21-arthropods/giant-crab-mesh.json":
        "dd7448eb9fc384350baf734fa8e7430994a5b4c08858f79b53eab1c30d2ed8eb",
    "assets/sprint23-armored-dinosaurs/stegosaurus-mesh.json":
        "6c5dcb055eafe011e76616c7a137a18d1c8ff1a63e603ddb4dbb8328b87c4117",
    "assets/sprint23-armored-dinosaurs/triceratops-albedo.png":
        "eb458f118a7f3eebbc44d068ea6b306443cba54a3288e30269d535575037f9d0",
    "assets/sprint23-armored-dinosaurs/triceratops-mesh.json":
        "f99f5d350a3a22e1a92f410687b2339c3a1107b8106c30a00744c834c7bf49fa",
    "assets/sprint24-theropods/deinonychus-albedo.png":
        "9e60ec305e08add31be809f4d82a82c727741102e3c3c4023494240f0f38780f",
    "assets/sprint24-theropods/deinonychus-mesh.json":
        "b085f640a33b7fa1232dc7c2b2677c0fe53fcb47f87c1bc873d0a358ab6ecdf6",
    "assets/sprint24-theropods/tyrannosaurus-albedo.png":
        "ff2b250fd753fd18a94fb7f00d94acaaf7467410a76a0b182de8eed36bd7c7af",
    "assets/sprint24-theropods/tyrannosaurus-mesh.json":
        "d9fe3f0ac21d5372bcb309f9b8d4790e379da9c2ee052737aeb1644a239fe320",
    "assets/ungulates/aurochs-albedo.png":
        "78d99e63a792aa2b40105167dd874c47ff52807931030e50f28bbf95c5793e01",
    "assets/ungulates/aurochs-mesh.json":
        "e1cd846250eb18d3a9bb5e852814cd38069c19581d32bd8bc60f135c562cc3e9",
    "assets/ungulates/bison-albedo.png":
        "1d7b9e28ba2836eb64bae3fb5c48c738d82fa3fd375edfb06a7397ba24130441",
    "assets/ungulates/bison-mesh.json":
        "7195063b5a13117adad7edf6d3cf5c2b38ee4008c14345b5c9f51609394f3641",
    "assets/ungulates/rhinoceros-albedo.png":
        "e8d172fdb09e68d5910dfc70623ed634696cd8347d2249b5744b49cc12770bbf",
    "assets/ungulates/rhinoceros-mesh.json":
        "3bcd3bcfa0a73ce05a7876bcac988850ab6a62795996f2a9235d53d4222b5224",
    "assets/ungulates/woolly-rhinoceros-albedo.png":
        "fcdfb0c4e0926c829a62da847ba1f14802b137dd3162f2fff2fa7ac8a104e2c6",
    "assets/ungulates/woolly-rhinoceros-mesh.json":
        "a53793a0283f263acf57fb4b56f20d8696ab9df6200bf547ac40f45d84da1265",
}

RECORD = {
    "releaseVersion": VERSION,
    "releaseInformationalVersion": INFORMATIONAL,
    "baseStableVersion": "0.0.157",
    "change": "Expanded Summoning visual refinement (rebuilt, repaired and "
              "outward-repaired creature bodies, the summoned glow removed from "
              "KMG summons, the Salamander's tail gait and surface, the Dire Rat "
              "icon) with the Brown-Fur startup-order and CotW controller "
              "save-loading repair",
    "appendedIdentities": 0,
    "releasedIdentityChanges": 0,
    "changedAssets": len(ART),
    "productionFiles": len(VISUAL_REFINEMENT_PRODUCTION) + len(STARTUP_REPAIR_PRODUCTION),
    "addedCompileItems": len(PROJECT_LINES),
    "visibleChoiceTotal": 1122,
    "guardedNativeReviewed": True,
    "ownerVisualReview": "provisional; the owner reviews in the game after release",
}


def check_identities(released_entries, current_entries):
    """The released ledger, exactly: this release appends nothing."""
    appended = identities.check_append_only(released_entries, current_entries)
    if appended != 0:
        raise AssertionError("v0.0.158 appends no identity, observed " + str(appended))


def check_project(released_text, current_text):
    current = current_text.splitlines()
    released = released_text.splitlines()
    for line in PROJECT_LINES:
        if current.count(line) != 1 or released.count(line) != 0:
            raise AssertionError("The project must add exactly one compile item: " + line)
        current.remove(line)
    if current != released:
        raise AssertionError("The project changed beyond the release's compile items")


def check_production_scope(changed):
    unexpected = sorted(set(changed) - VISUAL_REFINEMENT_PRODUCTION -
                        STARTUP_REPAIR_PRODUCTION - VERSION_PRODUCTION - {PROJECT})
    if unexpected:
        raise AssertionError("Production source outside the v0.0.158 release changed: " +
                             ", ".join(unexpected))


def check_art(changed, digest):
    """changed maps each asset path that differs from the release to its git
    status letter; digest returns a path's SHA-256."""
    unexpected = sorted(path for path in changed if path not in ART)
    if unexpected:
        raise AssertionError("Asset outside the reviewed refinement changed: " +
                             ", ".join(unexpected[:5]))
    moved = sorted(path for path, status in changed.items() if status != "M")
    if moved:
        raise AssertionError("Assets may only be modified in place: " + ", ".join(moved[:5]))
    unchanged = sorted(path for path in ART if path not in changed)
    if unchanged:
        raise AssertionError("Reviewed refined asset is still the released file: " +
                             ", ".join(unchanged[:5]))
    for path, expected in sorted(ART.items()):
        if digest(path) != expected:
            raise AssertionError("Refined asset differs from its reviewed bytes: " + path)


def check_sprint24_assets(root: Path):
    """Sprint 24's assets stay present and packaged; its released bytes hold
    except where this release pins refined ones."""
    for relative, released_hash in released155.ART.items():
        path = root / relative
        if not path.is_file():
            raise AssertionError("Missing Sprint 24 original asset: " + relative)
        expected = ART.get(relative, released_hash)
        if hashlib.sha256(path.read_bytes()).hexdigest() != expected:
            raise AssertionError("Sprint 24 asset differs from its released or reviewed bytes: " +
                                 relative)
    for token in ("sprint24-theropods", "deinonychus", "tyrannosaurus"):
        for script in ("scripts/Build-Local.ps1", "scripts/package.ps1",
                       "scripts/validate-build-output.ps1",
                       "scripts/validate-package.ps1"):
            if token not in (root / script).read_text(encoding="utf-8-sig"):
                raise AssertionError(script + " lacks package owner token: " + token)


def check_static(released, current):
    for key in set(released) | set(current):
        if key in ("version", "milestone", RECORD_KEY):
            continue
        if released.get(key) != current.get(key):
            raise AssertionError("Released static evidence changed: " + key)
    if current.get("version") != VERSION or current.get("milestone") != INFORMATIONAL:
        raise AssertionError("Static record version metadata mismatch")
    if current.get(RECORD_KEY) != RECORD:
        raise AssertionError("v0.0.158 release static record mismatch")


def check_version(info_version, props, assembly):
    if info_version != VERSION:
        raise AssertionError("Unsupported v0.0.158 release version: " + repr(info_version))
    if "<KmgVersion>" + VERSION + "</KmgVersion>" not in props or \
            "<KmgInformationalVersion>" + INFORMATIONAL + \
            "</KmgInformationalVersion>" not in props or \
            'AssemblyVersion("' + VERSION + '")' not in assembly or \
            'AssemblyFileVersion("' + VERSION + '")' not in assembly or \
            'AssemblyInformationalVersion("' + INFORMATIONAL + '")' not in assembly:
        raise AssertionError("Active v0.0.158 release version metadata mismatch")


def changed_production(root: Path):
    changed = subprocess.check_output(
        ["git", "diff", "--name-only", RELEASED, "--", "src/KingmakerGunslinger"],
        cwd=root, text=True).splitlines()
    changed += subprocess.check_output(
        ["git", "ls-files", "--others", "--exclude-standard", "--",
         "src/KingmakerGunslinger"], cwd=root, text=True).splitlines()
    return changed


def changed_assets(root: Path):
    changed = {}
    for line in subprocess.check_output(
            ["git", "diff", "--name-status", RELEASED, "--", "assets"],
            cwd=root, text=True).splitlines():
        fields = line.split("\t")
        changed[fields[-1]] = fields[0][:1]
    for path in subprocess.check_output(
            ["git", "ls-files", "--others", "--exclude-standard", "--", "assets"],
            cwd=root, text=True).splitlines():
        changed[path] = "A"
    return changed


def validate(root: Path):
    released_ledger = json.loads(common.blob(root, RELEASED, "blueprints/blueprints.json"))
    current_ledger = common.document(root, "blueprints/blueprints.json")
    for key in ("$schema", "schemaVersion", "namespace", "policy"):
        if current_ledger.get(key) != released_ledger.get(key):
            raise AssertionError("The identity ledger header changed: " + key)
    current = current_ledger["entries"]
    check_identities(released_ledger["entries"], current)

    # The released Sprint 24 public boundary, unchanged. Its 48 rows sit in
    # the released v0.0.156 ledger, which v0.0.157 extends exactly (its own
    # validator) and this release keeps unchanged (check_identities above).
    accepted = json.loads(common.blob(
        root, released155.BASELINE, "blueprints/blueprints.json"))["entries"]
    released156 = json.loads(common.blob(
        root, released157.RELEASED, "blueprints/blueprints.json"))["entries"]
    if released_ledger["entries"][:len(released156)] != released156:
        raise AssertionError("The released v0.0.156 ledger is not a prefix of v0.0.157")
    visibility_source = (root / "src/KingmakerGunslinger/Summoning/"
                         "SummonVisibilityCatalog.cs").read_text(encoding="utf-8-sig")
    stage = released155.validate_visibility(
        visibility_source, released155.validate_append(accepted, released156))
    if stage != "public":
        raise AssertionError("Released Sprint 24 routes are not public: " + stage)
    released155.validate_static(root, stage)
    check_sprint24_assets(root)

    check_production_scope(changed_production(root))
    check_project(common.blob(root, RELEASED, PROJECT).decode("utf-8-sig"),
                  (root / PROJECT).read_text(encoding="utf-8-sig"))
    check_art(changed_assets(root),
              lambda path: hashlib.sha256((root / path).read_bytes()).hexdigest())
    check_static(json.loads(common.blob(root, RELEASED, "validation/static-validation.json")),
                 common.document(root, "validation/static-validation.json"))
    check_version(common.document(root, "Info.json").get("Version"),
                  (root / "Directory.Build.props").read_text(encoding="utf-8-sig"),
                  (root / "src/KingmakerGunslinger/Properties/AssemblyInfo.cs")
                  .read_text(encoding="utf-8-sig"))

    manifest = subprocess.run(
        [sys.executable, str(root / "tools/expanded_summoning_manifest.py")],
        cwd=root, text=True, capture_output=True, check=False)
    if manifest.returncode:
        raise AssertionError("Expanded Summoning manifest validation failed: " +
                             (manifest.stdout + manifest.stderr)[-1000:])
    print("Visual refinement release PASS: identities=" + str(len(current)) +
          "; changedAssets=" + str(len(ART)) + "; version=" + VERSION +
          "; milestone=" + INFORMATIONAL + ".")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
                        default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    try:
        validate(args.root.resolve())
    except (AssertionError, ValueError, KeyError,
            subprocess.CalledProcessError) as error:
        print("Visual refinement release FAIL: " + str(error), file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
