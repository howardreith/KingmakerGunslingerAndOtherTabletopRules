"""v0.0.192: the Gunslinger remediation release (remediation Sprints 3 to 5, PRs #169 to #191); exactly the reviewed product delta from v0.0.191, version-only metadata, all released identities kept with the one authorized INT-07 note correction, two appended Mastodon Trample AI identities, Stirge withheld."""
from __future__ import annotations
import argparse
import json
from pathlib import Path
import re
import subprocess
import sys
sys.dont_write_bytecode = True
import validate_expanded_summoning_sprint21150 as common
from expanded_summoning_manifest import NATIVE_EXPANDED_OPTIONS, parsed_creatures, suppressed_keys
import expanded_summoning_manifest as manifest
from validate_development_candidate import AUTHORIZED_RELEASED_ROW_CORRECTIONS

# Reviewed product: master after PR #191. RELEASED is the v0.0.191 tag target.
PRODUCT = "5d297cfe4a880101e41e4e2352543330215b1efb"
RELEASED = "3e13965d9dfda843a18e19758e4689e3d3d95e91"
VERSION = "0.0.192"
INFORMATIONAL = VERSION + "-gunslinger-remediation"
OLD_INFORMATIONAL = "0.0.191-grit-ui-recipient-repair"
MANAGER = "0.31.1"
PINS = "scripts/loader-reference-pins.json"
PINNED = {"UnityModManager/UnityModManager.dll": "2e038ff23ab01d538dfef792096e1f5c04802d1edac3660db20d0d54c167c2c4",
          "UnityModManager/0Harmony12.dll": "aa1cd48317254985d8b700cc74953477d1b40c3022ce9aa4c95ed2b8327e1292"}
# The reviewed build path: these must equal PRODUCT exactly.
BUILD_INPUTS = (PINS, "scripts/ReferenceProvenance.Common.ps1", "scripts/Build-Local.ps1",
                "scripts/validate-package.ps1", "scripts/DevelopmentCandidate.Common.ps1",
                "tools/build_mod_from_private_references.py")
# Every product file the merged PRs #169 to #191 change from v0.0.191 (105 files,
# including the in-product runtime test runner). The list is exact in both directions.
SOURCES = (
    "assets-source/original-icons/icon-catalog.json",
    "blueprints/blueprints.json",
    "src/KingmakerGunslinger/Archetypes/SteadyAimMechanics.cs",
    "src/KingmakerGunslinger/Archetypes/UpCloseAndDeadlyDeliveryRecords.cs",
    "src/KingmakerGunslinger/Archetypes/UpCloseAndDeadlyMechanics.cs",
    "src/KingmakerGunslinger/Blueprints/AlternateSummoningAdapters.cs",
    "src/KingmakerGunslinger/Blueprints/BleedingWoundBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/DeadeyeBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/EasternWeaponNamedBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningAbilityBuilder.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningPublication.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.cs",
    "src/KingmakerGunslinger/Blueprints/FirearmScopedProficiencyBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/FirearmWeaponPresentation.cs",
    "src/KingmakerGunslinger/Blueprints/GunTrainingBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/GunslingerDodgeBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/MusketMasterBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/MysteriousStrangerBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/PistoleroBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/ProductionFirearmBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/StartlingShotBlueprints.cs",
    "src/KingmakerGunslinger/Blueprints/TargetingHeadBlueprints.cs",
    "src/KingmakerGunslinger/BrownFur/BrownFurOptionalExtensionCoordinator.cs",
    "src/KingmakerGunslinger/BrownFur/BrownFurPublicationAudit.cs",
    "src/KingmakerGunslinger/BrownFur/BrownFurPublicationTransaction.cs",
    "src/KingmakerGunslinger/CraftMagicItemsCompatibility/CraftMagicItemsReflectionBridge.cs",
    "src/KingmakerGunslinger/Deeds/DeadeyeAbilityLogic.cs",
    "src/KingmakerGunslinger/Deeds/DeadeyeCostPolicy.cs",
    "src/KingmakerGunslinger/Deeds/DeadeyeDecision.cs",
    "src/KingmakerGunslinger/Deeds/DeadeyeGritResourceLogic.cs",
    "src/KingmakerGunslinger/Deeds/DeadeyeRequest.cs",
    "src/KingmakerGunslinger/Deeds/DeadeyeRuntime.cs",
    "src/KingmakerGunslinger/Deeds/DeadeyeService.cs",
    "src/KingmakerGunslinger/Deeds/DeadeyeStatus.cs",
    "src/KingmakerGunslinger/Deeds/DodgeGritCostCalculator.cs",
    "src/KingmakerGunslinger/Deeds/GunslingerDodgeProneAbilityLogic.cs",
    "src/KingmakerGunslinger/Deeds/GunslingerDodgeRuntime.cs",
    "src/KingmakerGunslinger/Deeds/GunslingerDodgeRuntimeDiagnostics.cs",
    "src/KingmakerGunslinger/Deeds/QuickClearRuntime.cs",
    "src/KingmakerGunslinger/Deeds/TrueGritCatalog.cs",
    "src/KingmakerGunslinger/Diagnostics/CombatTracePatches.cs",
    "src/KingmakerGunslinger/Firearms/FirearmCapacityContainment.cs",
    "src/KingmakerGunslinger/Firearms/FirearmProficiencyScopeRules.cs",
    "src/KingmakerGunslinger/Firearms/FirearmStateTokenCatalog.cs",
    "src/KingmakerGunslinger/Firearms/ProductionFirearmCatalog.cs",
    "src/KingmakerGunslinger/Firing/BrokenSequenceInterruptionPolicy.cs",
    "src/KingmakerGunslinger/Firing/BrokenSequenceSuppressionRuntime.cs",
    "src/KingmakerGunslinger/Firing/FirearmAttackOrderLedger.cs",
    "src/KingmakerGunslinger/Gunsmithing/CompletedRestMaintenancePatch.cs",
    "src/KingmakerGunslinger/KingmakerGunslinger.csproj",
    "src/KingmakerGunslinger/Presentation/AuthoredMarkerRigContract.cs",
    "src/KingmakerGunslinger/Presentation/PresentationAudits.cs",
    "src/KingmakerGunslinger/Presentation/ProjectItemRosters.cs",
    "src/KingmakerGunslinger/Presentation/SourceCollection.cs",
    "src/KingmakerGunslinger/Recovery/RepairTestMusketRuntime.cs",
    "src/KingmakerGunslinger/Rules/SteadyAimPolicy.cs",
    "src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningNativeProfilePolicy.cs",
    "src/KingmakerGunslinger/RuntimeTesting/GunslingerDodgeScenarioVerdict.cs",
    "src/KingmakerGunslinger/RuntimeTesting/MotionSessionLifecycle.cs",
    "src/KingmakerGunslinger/RuntimeTesting/NativeIconScreenEvidence.cs",
    "src/KingmakerGunslinger/RuntimeTesting/Pol06ConversionObservationVerdict.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.AlternateSummoningComposition.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.BrownFurBoundary.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningBoundary.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningNativeProfile.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSatyr32.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.FirearmAdditionalInput.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.FirearmMaintenance.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.FirearmManualInput.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.FirearmMisfireRecovery.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.FirearmRestCompatibility.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.FirearmReviewKillChain.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.GunTrainingLevelUps.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.GunslingerDodge.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.InheritedTemplateCasts.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.PistoleroDeedObservation.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Pol06CotwConversions.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.Risk06TrampleRoute.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SpiderInterface.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SpiderSummonerPersistence.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2ConsoleConsolidation.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2ElementalConsole.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonCleanup2ElementalSources.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.SummonClericAlignment.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.TrueGritLevelUp.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs",
    "src/KingmakerGunslinger/RuntimeTesting/UpCloseAndDeadlyCaseVerdict.cs",
    "src/KingmakerGunslinger/RuntimeTesting/WeaponPresentationEvidenceScenario.cs",
    "src/KingmakerGunslinger/Summoning/ContextActionUngulateTrample.cs",
    "src/KingmakerGunslinger/Summoning/DruidSummonConversionPolicy.cs",
    "src/KingmakerGunslinger/Summoning/DruidSummonConversionVariantPatch.cs",
    "src/KingmakerGunslinger/Summoning/ElementalSourceChooserConsolePatch.cs",
    "src/KingmakerGunslinger/Summoning/ElementalSourceChooserPolicy.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningPublicationAudit.cs",
    "src/KingmakerGunslinger/Summoning/InheritedCastWindowContracts.cs",
    "src/KingmakerGunslinger/Summoning/InheritedTemplateCastContracts.cs",
    "src/KingmakerGunslinger/Summoning/InheritedTemplateCensus.cs",
    "src/KingmakerGunslinger/Summoning/InheritedTemplateDonorContract.cs",
    "src/KingmakerGunslinger/Summoning/InheritedTemplatePolicy.cs",
    "src/KingmakerGunslinger/Summoning/MastodonTrampleAiPolicy.cs",
    "src/KingmakerGunslinger/Summoning/UngulateTrampleAi.cs",
)
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "gunslingerRemediationRelease192"
RELEASED_IDENTITIES = 7933
APPENDED = (("KMG.Summoning.Special.Mastodon.TrampleAi", "4d88e6d656064c36a8df3f0e8df1f39f", "UngulateTrampleAi"),
            ("KMG.Summoning.Special.Mastodon.Brain", "7e9e643401714494adf441ffd267ed5a", "BlueprintBrain"))
WITHHELD = {"stirge"}
PUBLIC_CHOICES = 1208


def check_product(changed, untracked):
    if set(changed) - {ASSEMBLY} or untracked:
        raise AssertionError("Product/project/assets/identities differ from reviewed source: " +
                             ", ".join(sorted(set(changed) - {ASSEMBLY}) + sorted(untracked)))


def check_release_delta(root, released_info, product_info, released_pins, product_pins, current_pins):
    """The reviewed product differs from v0.0.191 exactly in SOURCES; Info,
    props, loader pins and build inputs are unchanged."""
    if product_info != released_info or released_info.get("ManagerVersion") != MANAGER:
        raise AssertionError("Info.json changed between v0.0.191 and the reviewed product")
    delta = subprocess.check_output(
        ["git", "diff", "--name-only", "--no-renames", RELEASED, PRODUCT, "--",
         *ROOTS, "Directory.Build.props"], cwd=root).decode().splitlines()
    if sorted(delta) != sorted(SOURCES):
        raise AssertionError("Product change since v0.0.191 is not exactly the reviewed remediation sources: +" +
                             ", ".join(sorted(set(delta) - set(SOURCES))) + " -" +
                             ", ".join(sorted(set(SOURCES) - set(delta))))
    pinned = {r.get("relativePath"): r.get("sha256") for r in product_pins.get("references", [])}
    if product_pins != released_pins or product_pins.get("minimumManagerVersion") != MANAGER or pinned != PINNED:
        raise AssertionError("The loader pins differ from the verified UMM 0.31.1 references of v0.0.191")
    if current_pins != product_pins:
        raise AssertionError("Loader pins differ from the reviewed product")
    drift = subprocess.check_output(
        ["git", "diff", "--name-only", "--no-renames", PRODUCT, "--", *BUILD_INPUTS],
        cwd=root).decode().splitlines()
    if drift:
        raise AssertionError("Reviewed build inputs changed: " + ", ".join(drift))


def check_metadata(old_info, info, old_props, props, old_assembly, assembly):
    if info != dict(old_info, Version=VERSION):
        raise AssertionError("Only Info version may change")
    if info.get("ManagerVersion") != MANAGER:
        raise AssertionError("Info ManagerVersion must stay " + MANAGER)
    expected_props = old_props.replace(
        "<KmgVersion>" + old_info["Version"] + "</KmgVersion>",
        "<KmgVersion>" + VERSION + "</KmgVersion>").replace(
            "<KmgInformationalVersion>" + OLD_INFORMATIONAL + "</KmgInformationalVersion>",
            "<KmgInformationalVersion>" + INFORMATIONAL + "</KmgInformationalVersion>")
    expected_assembly = old_assembly
    for attribute, before, after in (
            ("AssemblyVersion", old_info["Version"], VERSION),
            ("AssemblyFileVersion", old_info["Version"], VERSION),
            ("AssemblyInformationalVersion", OLD_INFORMATIONAL, INFORMATIONAL)):
        token = attribute + '("' + before + '")'
        if expected_assembly.count(token) != 1:
            raise AssertionError("Ambiguous version metadata")
        expected_assembly = expected_assembly.replace(
            token, attribute + '("' + after + '")')
    if (props.splitlines() != expected_props.splitlines() or
            assembly.splitlines() != expected_assembly.splitlines()):
        raise AssertionError("Non-version build metadata changed")


def check_ledger(released, product, current):
    """All v0.0.191 identities kept exactly and in order, except the one
    owner-authorized INT-07 note transition; exactly two appended."""
    if current != product:
        raise AssertionError("Identities differ from exact reviewed product")
    old_entries = released["entries"]
    new_entries = current["entries"]
    if len(old_entries) != RELEASED_IDENTITIES:
        raise AssertionError("Released identity baseline differs")
    if len(new_entries) != RELEASED_IDENTITIES + len(APPENDED):
        raise AssertionError("Exactly the two Mastodon Trample AI identities may be appended")
    authorized = {c["position"]: c for c in AUTHORIZED_RELEASED_ROW_CORRECTIONS}
    if sorted(authorized) != [206]:
        raise AssertionError("Only the INT-07 Gunsmith's Kit note correction is authorized")
    for index, (old, new) in enumerate(zip(old_entries, new_entries)):
        if old == new:
            continue
        correction = authorized.get(index)
        if correction is None or old != correction["before"] or new != correction["after"]:
            raise AssertionError("Released identity changed without authorization at position " + str(index))
    appended = [(e["symbol"], e["guid"], e["plannedType"]) for e in new_entries[RELEASED_IDENTITIES:]]
    if appended != list(APPENDED) or any(e["status"] != "active" for e in new_entries[RELEASED_IDENTITIES:]):
        raise AssertionError("Appended identities differ from the reviewed Mastodon Trample AI pair")
    if dict(released, entries=None) != dict(current, entries=None):
        raise AssertionError("Ledger metadata changed")


def released_creatures(root):
    text = common.blob(root, RELEASED,
        "src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs").decode("utf-8-sig")
    return [dict(key=k, name=n, monster=None if m == "null" else int(m),
        templated=t == "true", ally=None if a == "null" else int(a), visual=v or n)
        for k, n, m, t, a, v in manifest.ENTRY.findall(text)]


def check_census(creatures, hidden, baseline):
    if set(hidden) != WITHHELD:
        raise AssertionError("Exactly the owner-withdrawn Stirge may be withheld")
    if creatures != baseline:
        raise AssertionError("Released creature identities, ordering or routes changed")
    monster = [c for c in creatures if c["monster"] is not None]
    ally = [c for c in creatures if c["ally"] is not None]
    totals = (len(creatures), len(monster), sum(10 - c["monster"] for c in monster),
              len(ally), sum(10 - c["ally"] for c in ally))
    withheld = sum(10 - c["ally"] for c in ally if c["key"] in WITHHELD) + \
        sum(10 - c["monster"] for c in monster if c["key"] in WITHHELD)
    if totals != (134, 112, 600, 105, 588) or \
            totals[2] + totals[4] + len(NATIVE_EXPANDED_OPTIONS) - withheld != PUBLIC_CHOICES:
        raise AssertionError("Released roster/route census differs")


def check_inherited_template_policy(root):
    """DEF-05 obligation: v0.0.191 disclosed a twelve-name removal list; this
    release replaces it with the graph-derived per-placement policy, so the
    named list must be gone and the policy present."""
    path = "src/KingmakerGunslinger/Blueprints/ExpandedSummoningAbilityBuilder.cs"
    released = common.blob(root, RELEASED, path).decode("utf-8-sig")
    block = re.search(r"InheritedTemplateRemovalCreatures = \{(.*?)\};", released, re.S)
    if block is None or len(re.findall(r'"([a-z\-]+)"', block.group(1))) != 12:
        raise AssertionError("v0.0.191's disclosed twelve-name inherited-template list is not where it was")
    current = (root / path).read_text(encoding="utf-8-sig")
    if "InheritedTemplateRemovalCreatures" in current:
        raise AssertionError("The inherited-template name list must be replaced by the DEF-05 policy")
    if not (root / "src/KingmakerGunslinger/Summoning/InheritedTemplatePolicy.cs").is_file():
        raise AssertionError("The DEF-05 inherited-template policy is missing")


def check_static(product, current):
    record = current.get(RECORD_KEY)
    expected = dict(product, version=VERSION, milestone=INFORMATIONAL)
    expected[RECORD_KEY] = record
    if current != expected or not isinstance(record, dict) or \
            record.get("releaseVersion") != VERSION or \
            record.get("releaseInformationalVersion") != INFORMATIONAL or \
            record.get("reviewedProductSource") != PRODUCT or \
            record.get("appendedIdentities") != len(APPENDED) or \
            record.get("releasedIdentityChanges") != 1 or \
            record.get("visibleChoiceTotal") != PUBLIC_CHOICES:
        raise AssertionError("Static evidence history or selected release record differs")


def validate(root):
    def blob(commit, path):
        return common.blob(root, commit, path).decode("utf-8-sig")
    changed = subprocess.check_output(
        ["git", "diff", "--name-only", "--no-renames", PRODUCT, "--", *ROOTS],
        cwd=root).decode().splitlines()
    untracked = subprocess.check_output(
        ["git", "ls-files", "--others", "--exclude-standard", "--", *ROOTS],
        cwd=root).decode().splitlines()
    check_product(changed, untracked)
    check_release_delta(root, json.loads(blob(RELEASED, "Info.json")),
        json.loads(blob(PRODUCT, "Info.json")), json.loads(blob(RELEASED, PINS)),
        json.loads(blob(PRODUCT, PINS)), common.document(root, PINS))
    check_metadata(json.loads(blob(RELEASED, "Info.json")),
        common.document(root, "Info.json"), blob(RELEASED, "Directory.Build.props"),
        (root / "Directory.Build.props").read_text(encoding="utf-8-sig"),
        blob(RELEASED, ASSEMBLY), (root / ASSEMBLY).read_text(encoding="utf-8-sig"))
    path = "blueprints/blueprints.json"
    check_ledger(json.loads(blob(RELEASED, path)), json.loads(blob(PRODUCT, path)),
        common.document(root, path))
    check_static(json.loads(blob(PRODUCT, "validation/static-validation.json")),
        common.document(root, "validation/static-validation.json"))
    check_census(parsed_creatures(), suppressed_keys(), released_creatures(root))
    check_inherited_template_policy(root)
    result = subprocess.run([sys.executable, "-B",
        str(root / "tools/expanded_summoning_manifest.py")], cwd=root,
        capture_output=True, text=True)
    if result.returncode:
        raise AssertionError(result.stdout + result.stderr)
    print("Gunslinger remediation release192 PASS: exactly the reviewed 105-file product delta from v0.0.191, Info, loader pins and build inputs unchanged, exact reviewed master product and assets, all 7933 released identities kept in order with only the authorized INT-07 note correction, the two Mastodon Trample AI identities appended, 1208 public choices with Stirge withheld, the inherited-template name list replaced by the DEF-05 policy, version-only product metadata")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--root", type=Path,
        default=Path(__file__).resolve().parents[1])
    try:
        validate(parser.parse_args().root.resolve())
    except (AssertionError, ValueError, KeyError, OSError,
            subprocess.CalledProcessError) as error:
        print("Gunslinger remediation release192 FAIL: " + str(error), file=sys.stderr)
        raise SystemExit(1)
