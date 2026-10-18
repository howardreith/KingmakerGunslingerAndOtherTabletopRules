"""Sprint 26 exact scope, identity/art preservation and hidden/public stage boundary.
No native PASS is inferred here. Publication evidence belongs to the qualified
publisher ledger; this validates source inputs before either native candidate.
"""
from __future__ import annotations
import argparse, hashlib, json, re, subprocess, sys
from pathlib import Path
sys.dont_write_bytecode=True
import validate_development_candidate as identities
import validate_expanded_summoning_sprint21150 as common
from expanded_summoning_manifest import ENTRY
BASELINE="555b16e04613c0800ab0e884beac827184bf46ee"
ADDED={'KMG.Summoning.Unit.Gorgon': ('ce2fc58942b54e558208ea67d595cb7b', 'BlueprintUnit'),
 'KMG.Summoning.Unit.Bulette': ('f84a0babcc864630bae225b2019d6bba', 'BlueprintUnit'),
 'KMG.Summoning.Ability.SM.Tier8.Gorgon.One': ('40d3686b2f334a3090595cab7cb6314b',
                                               'BlueprintAbility'),
 'KMG.Summoning.Ability.SM.Tier9.Gorgon.OneD3': ('b5e9e129433e48af9a5e4fdd3652dc4f',
                                                 'BlueprintAbility'),
 'KMG.Summoning.Ability.SNA.Tier6.Bulette.One': ('66dc2f30f97f4d00b04aeee18224fb0d',
                                                 'BlueprintAbility'),
 'KMG.Summoning.Ability.SNA.Tier7.Bulette.OneD3': ('2c975d0b39bc449b802dfba98490b830',
                                                   'BlueprintAbility'),
 'KMG.Summoning.Ability.SNA.Tier8.Bulette.OneD4PlusOne': ('0687356eddcd4ca9970d87f79e8192e3',
                                                          'BlueprintAbility'),
 'KMG.Summoning.Ability.SNA.Tier9.Bulette.OneD4PlusOne': ('3b0d295dbdbc4931b1e6aff7b89b74c8',
                                                          'BlueprintAbility'),
 'KMG.Summoning.Natural.Gorgon.Gore2d8': ('6fa1a7d22d31434b85708d7ba7735649',
                                          'BlueprintItemWeapon'),
 'KMG.Summoning.Natural.Gorgon.Hoof1d6': ('fab6e52557c5426e8e09fbd714443c2c',
                                          'BlueprintItemWeapon'),
 'KMG.Summoning.Natural.Bulette.Bite2d8': ('f7801f4601554a478920e839ec40d9a4',
                                           'BlueprintItemWeapon'),
 'KMG.Summoning.Natural.Bulette.Claw2d6': ('d70dfa263eff44f2a547c9f47be0f87e',
                                           'BlueprintItemWeapon'),
 'KMG.Summoning.Natural.Bulette.BiteType': ('8b42eea07bc7444ca874e76a55571f6d',
                                            'BlueprintWeaponType'),
 'KMG.Summoning.Natural.Gorgon.UnitType': ('2359801423014dccbcd0b0bd0d92c788', 'BlueprintUnitType'),
 'KMG.Summoning.Natural.Gorgon.Chassis': ('3d4461a55b224472ad2853699c596b4f', 'BlueprintFeature'),
 'KMG.Summoning.Natural.Bulette.UnitType': ('b1e583d64a0349f9bfdc35de88e9d543',
                                            'BlueprintUnitType'),
 'KMG.Summoning.Natural.Bulette.Chassis': ('dee2bf278a5c4f54bb274d616584e5fb', 'BlueprintFeature'),
 'KMG.Summoning.Special.Gorgon.Breath': ('6e0309f69ef04b13b31147bd402dccd1', 'BlueprintAbility'),
 'KMG.Summoning.Special.Gorgon.Petrification': ('139703f90a8d4929992b783ad0e30bd7',
                                                'BlueprintBuff'),
 'KMG.Summoning.Special.Gorgon.Cooldown': ('59f6d7b4e2d44e73bf9c78508dbce36c', 'BlueprintBuff'),
 'KMG.Summoning.Special.Gorgon.Trample': ('c55343b590f54f0ea18fd258c92dfcd5', 'BlueprintAbility'),
 'KMG.Summoning.Special.Gorgon.Brain': ('06b7e98fd6f84c28a228b75382b7c306', 'BlueprintBrain'),
 'KMG.Summoning.Special.Gorgon.BreathAi': ('86298771c01e4f549a8be4b377ee75ae',
                                           'BlueprintAiCastSpell'),
 'KMG.Summoning.Special.Bulette.Leap': ('3aff604814c3419184037ef39511dbf4', 'BlueprintAbility'),
 'KMG.Summoning.Special.Bulette.Brain': ('48781e4f3e3a41bc945ff88de4bfef14', 'BlueprintBrain'),
 'KMG.Summoning.Special.Bulette.LeapAi': ('7cb623b639df482196a5a29a18baed27',
                                          'BlueprintAiCastSpell'),
 'KMG.Summoning.Adapter.MonsterTactician.Tier8.Gorgon.One': ('889cc8b2ef555bcb8088f0d43b388803',
                                                             'BlueprintAbility'),
 'KMG.Summoning.Adapter.MonsterTactician.Tier9.Gorgon.OneD3': ('ebe575783dd452cb9bd0f1254805ffd1',
                                                               'BlueprintAbility'),
 'KMG.Summoning.Adapter.Summoner.Tier8.Gorgon.One': ('004c25b7f897530fb15b7b2d8f49d19c',
                                                     'BlueprintAbility'),
 'KMG.Summoning.Adapter.Summoner.Tier9.Gorgon.OneD3': ('4eeea627d8385e8d8e5ff0050dcb8967',
                                                       'BlueprintAbility'),
 'KMG.Summoning.Adapter.MasterSummoner.Tier8.Gorgon.One': ('c737e65356b55fbcad441e8e393c946a',
                                                           'BlueprintAbility'),
 'KMG.Summoning.Adapter.MasterSummoner.Tier9.Gorgon.OneD3': ('f718c328a7e9520eaf57b4bedd918f6e',
                                                             'BlueprintAbility'),
 'KMG.Summoning.Adapter.Occultist.Tier8.Gorgon.One': ('1bf9dcf73e175ee4bfebed32365fb4cc',
                                                      'BlueprintAbility'),
 'KMG.Summoning.Adapter.Occultist.Tier9.Gorgon.OneD3': ('0f054425f03c58209974ca9c2846686e',
                                                        'BlueprintAbility'),
 'KMG.Summoning.Adapter.Naturalist.Tier6.Bulette.One': ('b5378e6d415d5d1fa65f1444e7f9191a',
                                                        'BlueprintAbility'),
 'KMG.Summoning.Adapter.Naturalist.Tier7.Bulette.OneD3': ('819c22768c445b958a0318383adc4bce',
                                                          'BlueprintAbility'),
 'KMG.Summoning.Adapter.Naturalist.Tier8.Bulette.OneD4PlusOne': ('90271cb31f235aeeaebc691b4e9def53',
                                                                 'BlueprintAbility'),
 'KMG.Summoning.Adapter.Naturalist.Tier9.Bulette.OneD4PlusOne': ('dca2640fed3d53bbbd93f88a4f2d4de9',
                                                                 'BlueprintAbility'),
 'KMG.Summoning.Adapter.FeralHunter.Tier6.Bulette.One': ('7ac17edfb3ab583d9ba22e8a9422975d',
                                                         'BlueprintAbility'),
 'KMG.Summoning.Adapter.FeralHunter.Tier7.Bulette.OneD3': ('f854f35b334b57f59e4ab17b74d1cde3',
                                                           'BlueprintAbility'),
 'KMG.Summoning.Adapter.FeralHunter.Tier8.Bulette.OneD4PlusOne': ('a348b5c386cc544c9b857e1680e3bfdf',
                                                                  'BlueprintAbility'),
 'KMG.Summoning.Adapter.FeralHunter.Tier9.Bulette.OneD4PlusOne': ('ad16a133ce675c8d80bca55d209adf2b',
                                                                  'BlueprintAbility'),
 'KMG.Summoning.Adapter.Preservationist.Tier7.Bulette.OneD3': ('f2edc9c81e025cf59ded1e15f31fd9c8',
                                                               'BlueprintAbility'),
 'KMG.Summoning.Adapter.Preservationist.Tier9.Bulette.OneD4PlusOne': ('ad5bde31b3e95c5fadae9e6f20bda6ff',
                                                                      'BlueprintAbility'),
 'KMG.Summoning.Special.Gorgon.BreathProjectile': ('fa7ec7798c9e475595e79ae419fe6241',
                                                   'BlueprintProjectile')}
ALLOWED_PRODUCTION=['src/KingmakerGunslinger/Assets/PteranodonAssetRuntime.cs',
 'src/KingmakerGunslinger/Blueprints/ExpandedSummoningBlueprints.cs',
 'src/KingmakerGunslinger/Blueprints/ExpandedSummoningNaturalBuilder.MagicalBeasts.cs',
 'src/KingmakerGunslinger/Blueprints/ExpandedSummoningNaturalBuilder.cs',
 'src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.MagicalBeasts.cs',
 'src/KingmakerGunslinger/Blueprints/ExpandedSummoningSpecialBuilder.cs',
 'src/KingmakerGunslinger/Blueprints/OwnedIconAssignments.cs',
 'src/KingmakerGunslinger/KingmakerGunslinger.csproj',
 'src/KingmakerGunslinger/Properties/AssemblyInfo.cs',
 'src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningNativeProfilePolicy.cs',
 'src/KingmakerGunslinger/RuntimeTesting/ExpandedSummoningRosterPersistencePolicy.cs',
 'src/KingmakerGunslinger/RuntimeTesting/IconConsumerCensus.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningNativeProfile.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningRosterPersistence.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint17LoadReset.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint22Review.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint23Review.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint25Review.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint26GorgonBoundaries.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint26Inputs.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint26LeapBoundaries.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint26Lifecycle.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint26Persistence.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint26Review.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs',
 'src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs',
 'src/KingmakerGunslinger/Summoning/BuletteLeapCombat.cs',
 'src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs',
 'src/KingmakerGunslinger/Summoning/ExpandedSummoningDonorCatalog.cs',
 'src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs',
 'src/KingmakerGunslinger/Summoning/ExpandedSummoningNaturalProfiles.cs',
 'src/KingmakerGunslinger/Summoning/ExpandedSummoningPteranodonViewPatch.cs',
 'src/KingmakerGunslinger/Summoning/GorgonPetrification.cs',
 'src/KingmakerGunslinger/Summoning/GorgonPetrificationLife.cs',
 'src/KingmakerGunslinger/Summoning/Sprint26MagicalBeastRules.cs',
 'src/KingmakerGunslinger/Summoning/SummonIconCatalog.cs',
 'src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs',
 'src/KingmakerGunslinger/Summoning/UngulateRulesPolicy.cs']
ART={'assets/ungulates/gorgon-mesh.json': '0f3c8ce10c4c0d50650ecb5a042985d1b242977716c5b7a0286754d370fdc508', 'assets/ungulates/gorgon-albedo.png': '4e374e18cf44af6e90f7bda29fb0124d03a124aa5443bc4334c2d0f8caf13b7c', 'assets/game/icons/expanded-summoning/gorgon.png': '73e297daa18abec90aec316eb09d97b658a698b884b94a1d9e428a1564536efd', 'assets/game/icons/expanded-summoning/bulette.png': '83c3959b973f7f99bb561ca4a2e1dc03e3f22fc90906dd800b215dba94078b82', 'assets/game/icons/bulette-leap.png': 'b3deefb33e0ad604a42f3e049b5d5ef6a91077ad3963af7db7aae40081a24b5c', 'assets-source/original-icons/expanded-summoning/sources/gorgon.png': '3ad08001ecf64e3c644a6a113b056fe069f171e7062a36d5d0df1f84ae0e32f3', 'assets-source/original-icons/expanded-summoning/sources/bulette.png': '66b537744e14bcc0a470b47f4e2cf536532e33950476aba091da619c5f0afac9'}

def rows(text):
    return [(key,int(monster) if monster != "null" else None,templated=="true",int(ally) if ally != "null" else None)
        for key,name,monster,templated,ally,visual in ENTRY.findall(text)]
def check_scope(released,current,hidden):
    selected=[x for x in current if x[0] in ("gorgon","bulette")]
    if selected!=[("gorgon",8,False,None),("bulette",None,False,6)]:
        raise AssertionError("Exact untemplated charter family/tier rows differ")
    if [x for x in current if x[0] not in ("gorgon","bulette")]!=released:
        raise AssertionError("Released creature order/placements changed")
    if hidden not in ({"gorgon","bulette"},set()):
        raise AssertionError("Only the two Sprint26 keys may be withheld or published together")
    return "hidden" if hidden else "public"
def check_tail(released,current):
    identities.check_append_only(released,current)
    tail=current[len(released):]
    actual={x["symbol"]:(x["guid"],x["plannedType"]) for x in tail}
    if actual!=ADDED or len(tail)!=len(ADDED) or any(x["status"]!="active" for x in tail):
        raise AssertionError("Sprint26 must append exactly its allocated identities and no template child")
def validate(root):
    read=lambda p:(root/p).read_text(encoding="utf-8-sig")
    blob=lambda p:common.blob(root,BASELINE,p)
    info=json.loads(read("Info.json"))
    if info["Version"]!="0.0.160":raise AssertionError("Wrong active Sprint26 patch version")
    for name,path,pattern in identities.VERSION_FIELDS:
        values=re.findall(pattern,read(path))
        expected="0.0.160-expanded-summoning-sprint26-gorgon-bulette" if "Informational" in name else "0.0.160"
        if values!=[expected]:raise AssertionError("Version metadata differs: "+name)
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
    # Every released raster/model and static evidence row is protected. New
    # original art has separate provenance/export validation and exact pins.
    for p in identities.git(root,"ls-tree","-r","--name-only",BASELINE,"--","assets","assets-source").stdout.splitlines():
        if p.lower().endswith((".png",".jpg",".jpeg",".mesh","-mesh.json")) and (root/p).read_bytes()!=blob(p):
            raise AssertionError("Released art changed: "+p)
    static=json.loads(read("validation/static-validation.json"));prior=json.loads(blob("validation/static-validation.json"))
    for key,value in prior.items():
        if key not in ("version","milestone") and static.get(key)!=value:raise AssertionError("Historical static evidence changed: "+key)
    record=static.get("expandedSummoningSprint26160",{})
    if record.get("stage")!=stage or record.get("ownerAestheticApproval")!="NOT_PERFORMED_NONBLOCKING" or record.get("candidateOnly") is not True:
        raise AssertionError("Truthful Sprint26 stage/visual-review metadata absent")
    for p,digest in ART.items():
        if hashlib.sha256((root/p).read_bytes()).hexdigest()!=digest:raise AssertionError("Sprint26 art pin differs: "+p)
    result=subprocess.run([sys.executable,"-B",str(root/"tools/expanded_summoning_manifest.py")],cwd=root)
    if result.returncode:raise AssertionError("Controlled manifest/catalog reconciliation failed")
    print("Sprint26 source boundary PASS: "+stage+"; exact two creatures, six roots, "+str(len(ADDED))+" append-only identities; no native qualification inferred.")
def main():
    parser=argparse.ArgumentParser();parser.add_argument("--root",type=Path,default=Path(__file__).resolve().parents[1]);args=parser.parse_args()
    try:validate(args.root.resolve());return 0
    except (AssertionError,ValueError,OSError,KeyError) as exc:print(str(exc),file=sys.stderr);return 1
if __name__=="__main__":raise SystemExit(main())
