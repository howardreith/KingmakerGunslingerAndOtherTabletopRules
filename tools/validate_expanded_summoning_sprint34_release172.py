"""Sprint34 exact reviewed product and append-only identities; native proof is separately attributed."""
from __future__ import annotations
import argparse
import json
from pathlib import Path
import subprocess
import sys
sys.dont_write_bytecode = True
import validate_expanded_summoning_sprint21150 as common
from expanded_summoning_manifest import parsed_creatures, suppressed_keys
import validate_development_candidate as identities
import expanded_summoning_manifest as manifest

PRODUCT = "f95adb25fb7cb390d81b6a5039b6157afdd2ed86"
RELEASED = "6cfd8a92d5354880dc87f9683816106799c53add"
VERSION = "0.0.172"
INFORMATIONAL = VERSION + "-expanded-summoning-bearded-devil-kyton"
OLD_INFORMATIONAL = "0.0.172-expanded-summoning-bearded-devil-kyton"
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "expandedSummoningSprint34Release172"
RECORD = dict(releaseVersion=VERSION, releaseInformationalVersion=INFORMATIONAL,
              baseStableVersion="0.0.171", reviewedProductSource=PRODUCT,
              appendedIdentities=74, releasedIdentityChanges=0,
              change="Bearded Devil and Kyton, ten untemplated SM V-IX routes, source-owned mechanics and distinct original bodies/icons",
              nativeEvidence="Original evidence retains its execution artifact; final package requires scoped fresh persistence/integration and explicit affected-dependency reuse",
              ownerPublicationAuthority="Sprint34 Bearded Devil and Kyton Fresh Codex Mission, with approved owned-chain, targeted-gaze and omitted-Heal decisions")

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
    if current != product:
        raise AssertionError("Identities differ from exact reviewed product")
    for key in ("$schema", "schemaVersion", "namespace", "policy"):
        if current.get(key) != released.get(key):
            raise AssertionError("Released identity header changed: " + key)
    identities.check_append_only(released["entries"],current["entries"])

def released_creatures(root):
    text=common.blob(root,RELEASED,"src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs").decode("utf-8-sig")
    return [dict(key=k,name=n,monster=None if m=="null" else int(m),
                 templated=t=="true",ally=None if a=="null" else int(a),visual=v or n)
            for k,n,m,t,a,v in manifest.ENTRY.findall(text)]

def check_census(creatures,hidden,baseline):
    keys={"bearded-devil","kyton-chain-devil"}
    selected=[c for c in creatures if c["key"] in keys]
    if hidden or len(selected)!=len(keys) or {c["key"] for c in selected}!=keys or any(
            c["monster"]!=5 or c["ally"] is not None or c["templated"] for c in selected):
        raise AssertionError("Exactly two untemplated SM V outsiders must be public")
    if [c for c in creatures if c["key"] not in keys] != baseline:
        raise AssertionError("Released creature identities, ordering or routes changed")

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
    check_census(parsed_creatures(),suppressed_keys(),released_creatures(root))
    result=subprocess.run([sys.executable,"-B",str(root/"tools/expanded_summoning_manifest.py")],cwd=root,capture_output=True,text=True)
    if result.returncode:raise AssertionError(result.stdout+result.stderr)
    print("Sprint34 release172 PASS: exact reviewed product/assets, append-only released identities and exact two-creature public scope; version-only metadata. Native release qualification is separate.")

if __name__=="__main__":
    parser=argparse.ArgumentParser();parser.add_argument("--root",type=Path,default=Path(__file__).resolve().parents[1])
    try:validate(parser.parse_args().root.resolve())
    except (AssertionError,ValueError,KeyError,OSError,subprocess.CalledProcessError) as error:
        print("Sprint34 release172 FAIL: "+str(error),file=sys.stderr);raise SystemExit(1)