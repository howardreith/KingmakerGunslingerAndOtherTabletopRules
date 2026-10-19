"""Sprint27 Griffon source boundary; no native or release qualification inferred."""
from __future__ import annotations
import argparse,hashlib,json,re,subprocess,sys
from pathlib import Path
sys.dont_write_bytecode=True
import validate_development_candidate as identities
import validate_expanded_summoning_sprint21150 as common
from expanded_summoning_manifest import ENTRY
BASELINE='511aabcc41a9ff780852a60a5db8fa074f22e05f'
ADDED={'KMG.Summoning.Unit.Griffon': ('4b55d1f8da5e4a3ea23f9a95191f8737',
                                'BlueprintUnit'),
 'KMG.Summoning.Ability.SNA.Tier4.Griffon.One': ('950ba5ed8e91463bb0918ccfd728c00b',
                                                 'BlueprintAbility'),
 'KMG.Summoning.Ability.SNA.Tier5.Griffon.OneD3': ('228cd53211814aa5bf79f75e3ad97eb3',
                                                   'BlueprintAbility'),
 'KMG.Summoning.Ability.SNA.Tier6.Griffon.OneD4PlusOne': ('e11a69dc774f4571ab471156a5a3a972',
                                                          'BlueprintAbility'),
 'KMG.Summoning.Ability.SNA.Tier7.Griffon.OneD4PlusOne': ('af5e7800577440b986ce34d23eb5466e',
                                                          'BlueprintAbility'),
 'KMG.Summoning.Ability.SNA.Tier8.Griffon.OneD4PlusOne': ('f6788d7de3c84c48a61dae7548ff59b8',
                                                          'BlueprintAbility'),
 'KMG.Summoning.Ability.SNA.Tier9.Griffon.OneD4PlusOne': ('087269592e8c405796786adeba55cd56',
                                                          'BlueprintAbility'),
 'KMG.Summoning.Natural.Griffon.Bite1d6': ('67549c5f756c495a8592ab27a019fec8',
                                           'BlueprintItemWeapon'),
 'KMG.Summoning.Natural.Griffon.Talon1d6': ('130a92b8f9d64a46908cf05258fbb2b0',
                                            'BlueprintItemWeapon'),
 'KMG.Summoning.Natural.Griffon.Rake1d4': ('4807a234c116416988cd388e5a923776',
                                           'BlueprintItemWeapon'),
 'KMG.Summoning.Natural.Griffon.UnitType': ('b31bad4d5053482d85d35d0ae27fac72',
                                            'BlueprintUnitType'),
 'KMG.Summoning.Natural.Griffon.Chassis': ('90b8b75d5117401e978a2f94a4f11546',
                                           'BlueprintFeature'),
 'KMG.Summoning.Special.Griffon.CombatTraits': ('689e319d9fcd4198a1edd44a9224c143',
                                                'BlueprintBuff'),
 'KMG.Summoning.Adapter.Naturalist.Tier4.Griffon.One': ('4654f98510825b75bbcf13319c0e2d64',
                                                        'BlueprintAbility'),
 'KMG.Summoning.Adapter.Naturalist.Tier5.Griffon.OneD3': ('682d531ff705509abc02524703c9418c',
                                                          'BlueprintAbility'),
 'KMG.Summoning.Adapter.Naturalist.Tier6.Griffon.OneD4PlusOne': ('fdf866fb48545a8397a09e510acf98ca',
                                                                 'BlueprintAbility'),
 'KMG.Summoning.Adapter.Naturalist.Tier7.Griffon.OneD4PlusOne': ('c0d91060eb165e83b7df192543d79fab',
                                                                 'BlueprintAbility'),
 'KMG.Summoning.Adapter.Naturalist.Tier8.Griffon.OneD4PlusOne': ('5b31b2433eb85c5d89a878abfa5064dd',
                                                                 'BlueprintAbility'),
 'KMG.Summoning.Adapter.Naturalist.Tier9.Griffon.OneD4PlusOne': ('d4ff859b051e57f89e13a2a4ff238753',
                                                                 'BlueprintAbility'),
 'KMG.Summoning.Adapter.FeralHunter.Tier4.Griffon.One': ('018f7653d25e5765979581788860b2de',
                                                         'BlueprintAbility'),
 'KMG.Summoning.Adapter.FeralHunter.Tier5.Griffon.OneD3': ('285409609a745ab68a6f2bacff66d1b8',
                                                           'BlueprintAbility'),
 'KMG.Summoning.Adapter.FeralHunter.Tier6.Griffon.OneD4PlusOne': ('cae6643e3f545a2b9b5af53b75166f73',
                                                                  'BlueprintAbility'),
 'KMG.Summoning.Adapter.FeralHunter.Tier7.Griffon.OneD4PlusOne': ('e00e25e6514c54c0aeb403ef22e5a3d7',
                                                                  'BlueprintAbility'),
 'KMG.Summoning.Adapter.FeralHunter.Tier8.Griffon.OneD4PlusOne': ('8581524487d259758ee4c6ea2f09dc49',
                                                                  'BlueprintAbility'),
 'KMG.Summoning.Adapter.FeralHunter.Tier9.Griffon.OneD4PlusOne': ('3294d2529e615387aa6072d36d7a09b5',
                                                                  'BlueprintAbility'),
 'KMG.Summoning.Adapter.Preservationist.Tier4.Griffon.One': ('4e8ef5b522b05564a00ef5f6def302dd',
                                                             'BlueprintAbility'),
 'KMG.Summoning.Adapter.Preservationist.Tier5.Griffon.OneD3': ('ad2199edb90b50dfb4bdd637a9ae3e64',
                                                               'BlueprintAbility'),
 'KMG.Summoning.Adapter.Preservationist.Tier7.Griffon.OneD4PlusOne': ('5e65c52e2e9f53c8aefb9046e3fb943b',
                                                                      'BlueprintAbility'),
 'KMG.Summoning.Adapter.Preservationist.Tier9.Griffon.OneD4PlusOne': ('376be12fe65753c58b30d8c376f1ffaa',
                                                                      'BlueprintAbility')}
ALLOWED_PRODUCTION=['src/KingmakerGunslinger/Assets/PteranodonAssetRuntime.Griffon.cs',
 'src/KingmakerGunslinger/Assets/PteranodonAssetRuntime.cs',
 'src/KingmakerGunslinger/Blueprints/ExpandedSummoningIconBuilder.cs',
 'src/KingmakerGunslinger/Blueprints/ExpandedSummoningNaturalBuilder.Griffon.cs',
 'src/KingmakerGunslinger/Blueprints/ExpandedSummoningNaturalBuilder.cs',
 'src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.Griffon.cs',
 'src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.cs',
 'src/KingmakerGunslinger/KingmakerGunslinger.csproj',
 'src/KingmakerGunslinger/Properties/AssemblyInfo.cs',
 'src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningNativeProfilePolicy.cs',
 'src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningRosterPersistencePolicy.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningNativeProfile.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningResourceReceipts.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningRosterPersistence.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint25Review.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint27Boundaries.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint27Extras.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint27Inputs.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint27Lifecycle.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint27Review.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs',
 'src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs',
 'src/KingmakerGunslinger/Summoning/ExpandedSummoningDonorCatalog.cs',
 'src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs',
 'src/KingmakerGunslinger/Summoning/ExpandedSummoningNaturalProfiles.cs',
 'src/KingmakerGunslinger/Summoning/ExpandedSummoningPteranodonViewPatch.cs',
 'src/KingmakerGunslinger/Summoning/ExpandedSummoningSpecialCombatComponents.cs',
 'src/KingmakerGunslinger/Summoning/GriffonReachPatch.cs',
 'src/KingmakerGunslinger/Summoning/GriffonLifetimeBoundary.cs',
 'src/KingmakerGunslinger/Summoning/GriffonRules.cs',
 'src/KingmakerGunslinger/Summoning/GriffonVisualPolicy.cs',
 'src/KingmakerGunslinger/Summoning/SummonIconCatalog.cs',
 'src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs']
ART={'assets-source/original-icons/expanded-summoning/sources/griffon.png': 'a6e77016a0d118a5d59a17cfe6cde92693ace0afa7d3c2115791caa7a0a7afca',
 'assets/game/icons/expanded-summoning/griffon.png': 'eef3de6182d7db3c6d25033e83c8eb5792c7c9aa094c54b997d2c86459ca16e3',
 'assets/sprint27-griffon/griffon-albedo.png': '997a14e3fc55a642136c2bf33612c650c36dd3f4153f5abfb7d8642f0912ec78',
 'assets/sprint27-griffon/griffon-mesh.json': '4694292d3cc275590fddade1e68169968884185573cc172455daf8b875e048ac'}

def rows(text):
    return [(key,int(monster) if monster!="null" else None,templated=="true",int(ally) if ally!="null" else None)
        for key,name,monster,templated,ally,visual in ENTRY.findall(text)]
def check_scope(released,current,hidden):
    if [v for v in current if v[0]=="griffon"] != [("griffon",None,False,4)]:
        raise AssertionError("Only the printed untemplated Griffon on SNA IV is selected")
    if [v for v in current if v[0]!="griffon"]!=released:
        raise AssertionError("Released creature placement/order changed")
    if hidden not in ({"griffon"},set()):raise AssertionError("Only Griffon may be suppressed or unsuppressed")
    return "hidden" if hidden else "public"
def check_tail(released,current):
    identities.check_append_only(released,current)
    tail=current[len(released):]
    actual={v["symbol"]:(v["guid"],v["plannedType"]) for v in tail}
    if actual!=ADDED or len(tail)!=len(ADDED) or any(v["status"]!="active" for v in tail):
        raise AssertionError("The exact29 Griffon identity allocations must remain unchanged")
def validate(root):
    read=lambda p:(root/p).read_text(encoding="utf-8-sig")
    blob=lambda p:common.blob(root,BASELINE,p)
    if json.loads(read("Info.json"))["Version"]!="0.0.161":raise AssertionError("Wrong Sprint27 stable patch")
    for name,path,pattern in identities.VERSION_FIELDS:
        expected="0.0.161-expanded-summoning-sprint27-griffon" if "Informational" in name else "0.0.161"
        if re.findall(pattern,read(path))!=[expected]:raise AssertionError("Version differs: "+name)
    current=json.loads(read("blueprints/blueprints.json"));released=json.loads(blob("blueprints/blueprints.json"))
    for key in ("$schema","schemaVersion","namespace","policy"):
        if current[key]!=released[key]:raise AssertionError("Identity ledger header changed")
    check_tail(released["entries"],current["entries"])
    catalog="src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs"
    visibility=read("src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs")
    match=re.search(r"SuppressedCreatureKeys\s*=\s*\n?\s*new HashSet<string>\((.*?)\);",visibility,re.S)
    if not match:raise AssertionError("Missing exact suppression carrier")
    stage=check_scope(rows(blob(catalog).decode("utf-8-sig")),rows(read(catalog)),set(re.findall(r'"([a-z0-9-]+)"',match.group(1))))
    changed=identities.git(root,"diff","--name-only",BASELINE,"--","src/KingmakerGunslinger").stdout.splitlines()
    changed+=identities.git(root,"ls-files","--others","--exclude-standard","--","src/KingmakerGunslinger").stdout.splitlines()
    if set(changed)-set(ALLOWED_PRODUCTION):raise AssertionError("Unrelated production edit: "+str(set(changed)-set(ALLOWED_PRODUCTION)))
    for p in identities.git(root,"ls-tree","-r","--name-only",BASELINE,"--","assets","assets-source").stdout.splitlines():
        if p.lower().endswith((".png",".jpg",".jpeg",".mesh","-mesh.json")) and (root/p).read_bytes()!=blob(p):raise AssertionError("Released art changed: "+p)
    static=json.loads(read("validation/static-validation.json"));prior=json.loads(blob("validation/static-validation.json"))
    for key,value in prior.items():
        if key not in ("version","milestone") and static.get(key)!=value:raise AssertionError("Historical static evidence changed: "+key)
    record=static.get("expandedSummoningSprint27161",{})
    if record.get("stage")!=stage or record.get("ownerAestheticApproval")!="NOT_PERFORMED_NONBLOCKING" or record.get("candidateOnly") is not True:
        raise AssertionError("Truthful source-only stage/visual metadata absent")
    for p,digest in ART.items():
        if hashlib.sha256((root/p).read_bytes()).hexdigest()!=digest:raise AssertionError("Griffon art pin differs: "+p)
    result=subprocess.run([sys.executable,"-B",str(root/"tools/expanded_summoning_manifest.py")],cwd=root)
    if result.returncode:raise AssertionError("Controlled manifest/catalog reconciliation failed")
    print("Sprint27 source boundary PASS: "+stage+"; one Griffon, six SNA roots,29 append-only identities; native evidence is separate.")
def main():
    parser=argparse.ArgumentParser();parser.add_argument("--root",type=Path,default=Path(__file__).resolve().parents[1]);args=parser.parse_args()
    try:validate(args.root.resolve());return 0
    except (AssertionError,ValueError,OSError,KeyError) as exc:print(str(exc),file=sys.stderr);return 1
if __name__=="__main__":raise SystemExit(main())
