"""Sprint35 exact reviewed product and append-only identities; native proof is separately attributed."""
from __future__ import annotations
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys
sys.dont_write_bytecode = True
import validate_expanded_summoning_sprint21150 as common
from expanded_summoning_manifest import parsed_creatures, suppressed_keys
import validate_development_candidate as identities
import expanded_summoning_manifest as manifest

PRODUCT = "dac431b8a0b06840cbfb388ca449ad51db7e0ed4"
RELEASED = "b7317f37a3f61e9f5b87c86cc1a9ec4e31071146"
VERSION = "0.0.173"
INFORMATIONAL = VERSION + "-expanded-summoning-barbed-bone-devils"
OLD_INFORMATIONAL = "0.0.172-expanded-summoning-bearded-devil-kyton"
ASSEMBLY = "src/KingmakerGunslinger/Properties/AssemblyInfo.cs"
OBSERVER = "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.InfernalEarly35.cs"
BARBED_OBSERVER = "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.InfernalBarbedCombat35.cs"
BONE_OBSERVER = "src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.InfernalBone35.cs"
OBSERVERS = {
    OBSERVER: "0840aca62ce12e33700276b2b6b1a89f5c8108626bbf6e9815bce7d0ba385171",
    BARBED_OBSERVER: "da3fb790e956a182afb0f18a80d1af1770cc9fcf4d4c4c501463f05fefb56b1c",
    BONE_OBSERVER: "c59c8160539506ead301486c90db3e2dfc313d5f79b7f1dde0065d2cdfdc2063",
}
ROOTS = ("src/KingmakerGunslinger", "assets", "assets-source", "blueprints")
RECORD_KEY = "expandedSummoningSprint35Release173"
RECORD = dict(releaseVersion=VERSION, releaseInformationalVersion=INFORMATIONAL,
              baseStableVersion="0.0.172", reviewedProductSource=PRODUCT,
              appendedIdentities=74, releasedIdentityChanges=0,
              change="Barbed Devil and Bone Devil, five untemplated Monster VII-IX routes, distinct contact, poison, fear, invisibility, Wall consumers and original bodies/icons",
              nativeEvidence="Final exact package requires fresh grouped public routes, persistence and affected integration; original failures and bounded reader re-evaluations remain attributed to their own artifacts",
              ownerPublicationAuthority="Sprint35 Barbed Devil and Bone Devil Fresh Codex Mission, with approved bounded movement-only Wall of Ice adaptation and explicit unsupported-SLA omissions")

def check_product(changed, untracked):
    if set(changed) - ({ASSEMBLY} | set(OBSERVERS)) or untracked:
        raise AssertionError("Product/project/assets/identities differ from reviewed source")

def check_observer(path, data):
    if path not in OBSERVERS or hashlib.sha256(data).hexdigest() != OBSERVERS[path]:
        raise AssertionError("Exact reviewed native observer correction differs")

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
    expected={"barbed-devil":8,"bone-devil":7}
    selected=[c for c in creatures if c["key"] in expected]
    if hidden or len(selected)!=len(expected) or {c["key"] for c in selected}!=set(expected) or any(
            c["monster"]!=expected[c["key"]] or c["ally"] is not None or c["templated"] for c in selected):
        raise AssertionError("Exactly Barbed SM VIII and Bone SM VII must be public and untemplated")
    if [c for c in creatures if c["key"] not in expected] != baseline:
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
    for path in OBSERVERS:
        check_observer(path,(root/path).read_bytes())
    check_metadata(json.loads(blob(PRODUCT,"Info.json")),common.document(root,"Info.json"),blob(PRODUCT,"Directory.Build.props"),(root/"Directory.Build.props").read_text(encoding="utf-8-sig"),blob(PRODUCT,ASSEMBLY),(root/ASSEMBLY).read_text(encoding="utf-8-sig"))
    path="blueprints/blueprints.json"
    check_ledger(json.loads(blob(RELEASED,path)),json.loads(blob(PRODUCT,path)),common.document(root,path))
    check_static(json.loads(blob(PRODUCT,"validation/static-validation.json")),common.document(root,"validation/static-validation.json"))
    check_census(parsed_creatures(),suppressed_keys(),released_creatures(root))
    result=subprocess.run([sys.executable,"-B",str(root/"tools/expanded_summoning_manifest.py")],cwd=root,capture_output=True,text=True)
    if result.returncode:raise AssertionError(result.stdout+result.stderr)
    print("Sprint35 release173 PASS: exact reviewed product/assets and observer correction, append-only released identities and exact two-creature public scope; version-only metadata. Native release qualification is separate.")

if __name__=="__main__":
    parser=argparse.ArgumentParser();parser.add_argument("--root",type=Path,default=Path(__file__).resolve().parents[1])
    try:validate(parser.parse_args().root.resolve())
    except (AssertionError,ValueError,KeyError,OSError,subprocess.CalledProcessError) as error:
        print("Sprint35 release173 FAIL: "+str(error),file=sys.stderr);raise SystemExit(1)
