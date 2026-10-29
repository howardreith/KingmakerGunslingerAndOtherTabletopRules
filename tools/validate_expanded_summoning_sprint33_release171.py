"""Sprint33 version-only promotion of the reviewed product; native proof remains separately attributed."""
from __future__ import annotations
import argparse
import json
from pathlib import Path
import subprocess
import sys
sys.dont_write_bytecode = True
import validate_expanded_summoning_sprint21150 as common
from expanded_summoning_manifest import parsed_creatures, suppressed_keys
from validate_expanded_summoning_sprint32_release169 import check_census

PRODUCT = "6b735841465e119dac125d56da997b7ad41a1a1b"
RELEASED = "43abeedfd223e95dec33a3556cc5606a029e4cd3"
VERSION = "0.0.171"
INFORMATIONAL = VERSION + "-expanded-summoning-shadow-outsiders"
OLD_INFORMATIONAL = INFORMATIONAL
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "expandedSummoningSprint33Release171"
RECORD = dict(releaseVersion=VERSION, releaseInformationalVersion=INFORMATIONAL,
              baseStableVersion="0.0.170", reviewedProductSource=PRODUCT,
              appendedIdentities=0, releasedIdentityChanges=0,
              change="Succubus body preserved, source cleanup/attack fallback and null duration dice repaired; original Invisible Stalker and Shadow Demon bodies; natural invisibility Purge exception",
              nativeEvidence="Original evidence retains its execution artifact; final package requires scoped fresh persistence/integration and explicit affected-dependency reuse",
              ownerPublicationAuthority="Sprint33 Dedicated Models Revised Mission, Revision2")

def check_product(changed, untracked):
    if set(changed) - {ASSEMBLY} or untracked:
        raise AssertionError("Product/project/assets/identities differ from reviewed source")

def check_metadata(old_info, info, old_props, props, old_assembly, assembly):
    if info != dict(old_info, Version=VERSION):
        raise AssertionError("Only Info version may change")
    expected_props=old_props.replace("<KmgVersion>"+old_info["Version"]+"</KmgVersion>","<KmgVersion>"+VERSION+"</KmgVersion>").replace(OLD_INFORMATIONAL,INFORMATIONAL)
    expected_assembly=old_assembly
    for attribute,before,after in (("AssemblyVersion",old_info["Version"],VERSION),("AssemblyFileVersion",old_info["Version"],VERSION),("AssemblyInformationalVersion",OLD_INFORMATIONAL,INFORMATIONAL)):
        token=attribute+'("'+before+'")'
        if expected_assembly.count(token)!=1: raise AssertionError("Ambiguous version metadata")
        expected_assembly=expected_assembly.replace(token,attribute+'("'+after+'")')
    if props.splitlines()!=expected_props.splitlines() or assembly.splitlines()!=expected_assembly.splitlines():
        raise AssertionError("Non-version build metadata changed")

def check_ledger(released,product,current):
    if current != product or current != released:
        raise AssertionError("Sprint33 preserves every released identity; no new routes or private IDs selected")

def check_static(product,current):
    expected=dict(product,version=VERSION,milestone=INFORMATIONAL)
    expected[RECORD_KEY]=RECORD
    if current != expected: raise AssertionError("Static evidence history or selected release record differs")

def validate(root):
    def blob(commit,path):return common.blob(root,commit,path).decode("utf-8-sig")
    changed=subprocess.check_output(["git","diff","--name-only","--no-renames",PRODUCT,"--",*ROOTS],cwd=root).decode().splitlines()
    untracked=subprocess.check_output(["git","ls-files","--others","--exclude-standard","--",*ROOTS],cwd=root).decode().splitlines()
    check_product(changed,untracked)
    check_metadata(json.loads(blob(PRODUCT,"Info.json")),common.document(root,"Info.json"),blob(PRODUCT,"Directory.Build.props"),(root/"Directory.Build.props").read_text(encoding="utf-8-sig"),blob(PRODUCT,ASSEMBLY),(root/ASSEMBLY).read_text(encoding="utf-8-sig"))
    path="blueprints/blueprints.json"
    check_ledger(json.loads(blob(RELEASED,path)),json.loads(blob(PRODUCT,path)),common.document(root,path))
    check_static(json.loads(blob(PRODUCT,"validation/static-validation.json")),common.document(root,"validation/static-validation.json"))
    check_census(parsed_creatures(),suppressed_keys())
    result=subprocess.run([sys.executable,"-B",str(root/"tools/expanded_summoning_manifest.py")],cwd=root,capture_output=True,text=True)
    if result.returncode:raise AssertionError(result.stdout+result.stderr)
    print("Sprint33 release171 PASS: exact reviewed product/assets, unchanged released identities and public census; version-only metadata. Native release qualification is separate.")

if __name__=="__main__":
    parser=argparse.ArgumentParser();parser.add_argument("--root",type=Path,default=Path(__file__).resolve().parents[1])
    try:validate(parser.parse_args().root.resolve())
    except (AssertionError,ValueError,KeyError,OSError,subprocess.CalledProcessError) as error:
        print("Sprint33 release171 FAIL: "+str(error),file=sys.stderr);raise SystemExit(1)