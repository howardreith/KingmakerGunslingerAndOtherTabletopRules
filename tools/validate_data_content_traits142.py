#!/usr/bin/env python3
"""Validate the selective DATA 0.0.142 candidate on the exact released checkpoint."""
from __future__ import annotations
import argparse, fnmatch, hashlib, json, re, subprocess, sys
from pathlib import Path
sys.dont_write_bytecode=True
import validate_favored_class140 as baseline
import inspect_elemental_character_trait_icons as trait_icons

VERSION="0.0.142"
INFORMATIONAL_VERSION="0.0.142-elemental-race-traits-and-content"
PACKAGE_SUFFIX="elemental-race-traits-and-content"
BASE="97f0a966b3219ce0122626a492529b509e1db880"
FORBIDDEN=(
    "EXPANDED-SUMMONING-*", "planning/EXPANDED-SUMMONING-*",
    "src/KingmakerGunslinger/Summoning/*",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoning*",
    "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoning*",
    "assets-source/original-icons/expanded-summoning/*",
    "assets-source/original-models/flying-animals/*",
    "assets-source/original-models/sprint12-*",
    "assets-source/original-models/sprint13-*",
    "assets-source/original-models/ungulates/*",
    "assets/game/icons/expanded-summoning/*",
    "assets/flying-animals/*", "assets/sprint12-*", "assets/sprint13-*",
    "assets/ungulates/*", "docs/RELEASE-NOTES-0.0.141.md",
    "tools/validate_expanded_summoning_phase2a141.py",
)
def released(root,path):
    return subprocess.check_output(["git","show",BASE+":"+path],cwd=root)
def assert_release_boundary(root):
    original=subprocess.check_output(["git","ls-tree","-r","--name-only",BASE],cwd=root,text=True).splitlines()
    protected={p for p in original if any(fnmatch.fnmatchcase(p,k) for k in FORBIDDEN)}
    changed=subprocess.check_output(["git","diff","--name-only",BASE,"--"],cwd=root,text=True).splitlines()
    conflicts=[p for p in changed if any(fnmatch.fnmatchcase(p,k) for k in FORBIDDEN)]
    untracked=subprocess.check_output(["git","ls-files","--others","--exclude-standard"],cwd=root,text=True).splitlines()
    conflicts += [p for p in untracked if any(fnmatch.fnmatchcase(p,k) for k in FORBIDDEN)]
    if conflicts:
        raise AssertionError("Released Summoning surface changed: "+repr(conflicts))
    for p in protected:
        if not (root/p).is_file():
            raise AssertionError("Released file absent: "+p)
    manifest=json.loads((root/"blueprints/blueprints.json").read_text(encoding="utf-8-sig"))
    old=json.loads(released(root,"blueprints/blueprints.json"))
    if manifest["entries"][:len(old["entries"])]!=old["entries"]:
        raise AssertionError("Released blueprint entries changed or reordered")
    if len(manifest["entries"])!=len(old["entries"])+11:
        raise AssertionError("Only the eleven frozen DATA identities may be appended")
    summoning_manifest="assets-source/original-icons/expanded-summoning/icon-manifest.json"
    if hashlib.sha256((root/summoning_manifest).read_bytes()).hexdigest()!="40754cb1ce93473befaf10be1c4d0fce1296ef73c218d83c6a493f014d0c734d":
        raise AssertionError("Released delegated icon authority drift")
    return len(protected)
def validate(root):
    boundary=assert_release_boundary(root)
    program=(root/"tests/KingmakerGunslinger.DomainTests/Program.cs").read_text(encoding="utf-8-sig")
    count=len(re.findall(r'\bCase\("',program))
    state=json.loads((root/"validation/static-validation.json").read_text(encoding="utf-8-sig"))
    record=state["dataContentTraits142"]
    if record["deterministicTestCount"]!=(count if VERSION=="0.0.142" else 2294) or record["stableTraitIdentityCount"]!=11 or record["originalTraitIconCount"]!=4:
        raise AssertionError("Active DATA counts differ from exact tree")
    old_state=json.loads(released(root,"validation/static-validation.json"))
    if state["expandedSummoningPhase2A141"]!=old_state["expandedSummoningPhase2A141"]:
        raise AssertionError("Historical 0.0.141 qualification changed")
    # Retain the complete inherited gate chain. Its active-version/count inputs
    # follow this candidate; historical release records remain immutable.
    baseline.VERSION=VERSION
    baseline.INFORMATIONAL_VERSION=INFORMATIONAL_VERSION
    baseline.PACKAGE="KingmakerGunslinger-"+VERSION+"-local-runtime.zip"
    baseline.PACKAGE_SUFFIX=PACKAGE_SUFFIX
    baseline.DETERMINISTIC_TEST_COUNT=count
    baseline.validate(root)
    intake=trait_icons.inspect(root)
    if intake["AssetReadiness"]!="READY" or not intake["existingIconContractPass"]:
        raise AssertionError("All four original icon assets must qualify")
    for path,tokens in {
        "src/KingmakerGunslinger/ElementalRaces/ElementalCharacterTraitPublicationCoordinator.cs":(
            "ResolveCanonicalGraphIfFullyRegistered","CreateDetached(","WithdrawAcquisition()","ElementalCharacterTraitsChanged"),
        "src/KingmakerGunslinger/ElementalRaces/ElementalCharacterTraitNativeGraph.cs":(
            "ElementalCharacterTraitNativeRegistration","Preflight()","Rollback()","CurrentPackFast","BlueprintsByAssetId.Add"),
        "src/KingmakerGunslinger/ElementalRaces/ElementalCharacterTraitOwnedGrant.cs":(
            "[JsonProperty] private Fact _ownedFact","Owner.RemoveFact(owned)","ElementalCharacterTraitsChanged"),
        "docs/RELEASE-NOTES-0.0.142.md":(
            "0.0.142-elemental-race-traits-and-content","Fiery Glare","Stoic Dignity","Wings of Air","Whiteout","serialized merchant"),
    }.items():
        text=(root/path).read_text(encoding="utf-8-sig")
        for token in tokens:
            if token not in text: raise AssertionError(path+" lacks "+token)
    print("DATA 0.0.142 repository validation PASS; current tests="+str(count)+"; immutable released files="+str(boundary))
def main():
    parser=argparse.ArgumentParser()
    parser.add_argument("--root",type=Path,default=Path(__file__).resolve().parents[1])
    args=parser.parse_args()
    try:validate(args.root.resolve())
    except Exception as e:
        print("DATA 0.0.142 validation failed: "+str(e),file=sys.stderr);return 1
    return 0
if __name__=="__main__":raise SystemExit(main())
