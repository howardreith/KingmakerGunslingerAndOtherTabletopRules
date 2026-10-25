"""S30-31 public source boundary. Native/release qualification remains separate."""
from pathlib import Path
import argparse, hashlib, json, re, subprocess, sys
sys.dont_write_bytecode = True
import validate_development_candidate as identities
import validate_expanded_summoning_sprint21150 as common
from validate_expanded_summoning_sprint29163 import rows
from expanded_summoning_manifest import suppressed_keys

BASELINE = "60175837909f75e7507200bc1e22a6add04ddd51"
VERSION = "0.0.167"
INFORMATIONAL = VERSION + "-expanded-summoning-sprints30-31-outsider-celestial"
ADDITIONS = {("lemure",2,False,None),("dretch",3,False,None),("hound-archon",4,False,None),("astral-deva",9,False,None),("trumpet-archon",9,False,None)}
TAIL_COUNT = 232
TAIL_SHA256 = "66a9518cc70dfad4aac088d8277945f1f3902973863fc3bd9e8f1d648e78473a"
ART = {'assets-source/original-icons/expanded-summoning/icon-manifest.json': '08e26335a0256ed34af2bbbaae70d3e7e354f63a7bb47033d9d368485f332284', 'assets-source/original-icons/expanded-summoning/prompts/icon-prompts.json': '7b216671ef1b4286234be00e22d7207e41bbcf7a0765764e32414f9ab26eeb21', 'assets-source/original-icons/expanded-summoning/sources/astral-deva.png': 'edd91d0f9a5a586a43bfe878d996f9e6d1c2340302c98c143450f42af2971fb2', 'assets-source/original-icons/expanded-summoning/sources/dretch.png': '1e5b158b197a6a5a9099adf85c0c148d1c5bfc59bd37e0202864b4e33fe87527', 'assets-source/original-icons/expanded-summoning/sources/hound-archon.png': '99d63f3aeef8b6c80b9cd871d17eca5158a2d28b7fd07abcfdc3d6498df2c2bd', 'assets-source/original-icons/expanded-summoning/sources/lemure.png': 'b742a9462547674ecde44fac7e52a3b0cbcc6b278fe84c76a678d48cc30faf18', 'assets-source/original-icons/expanded-summoning/sources/trumpet-archon.png': 'd4a3353129b8dc640317f69dac208e30f375df310445513f1babe53c8601476e', 'assets-source/original-icons/expanded-summoning/sources/trumpet-blast.png': '51ccc53ac0c77f295b1c40d6a346634452dd518a1986cd6a455e861df23ce41d', 'assets-source/original-icons/expanded-summoning/sprint30-31-generation.json': '23a23294641dcedfcf75c1977761704859937e3c442db3ba8d6eddb5fc40d877', 'assets-source/original-icons/expanded-summoning/trumpet-blast-generation.json': '97eed661d2fabb0eddff1ab28865049df9ec7e2ba08281f736d5d46ad60d04cc', 'assets-source/original-icons/icon-catalog.json': 'b441376f7887ac4277cd16df560e940dfb3b7b62e6fbe2abdf03f1c327aa7534', 'assets-source/original-models/outsider-celestial/SOURCE.md': 'b3709bd39012b60c5c73f18cd869d81df97fcb94ff33156370f0ec92d38fd7f0', 'assets-source/original-models/outsider-celestial/build_dretch.py': '73c7a93e958ea64d8d17782cf309b0ef6f7b894abe4c181b8fdfe3a8a5e594ee', 'assets-source/original-models/outsider-celestial/build_hound_archon.py': '21f21d1e567ce1fe9c42b7b97848fe6df068d0b0f88e99c43fac7b9553c836dd', 'assets-source/original-models/outsider-celestial/build_lemure.py': '38cdbc49f710579956bdcffa2c0338447d0a4418010e108d03a410aab08df438', 'assets-source/original-models/outsider-celestial/dretch-source.json': 'e25f4c3654e6fe5db6f0df52be4fcbeae8078302572c9bb8ac9dc320ea7d03de', 'assets-source/original-models/outsider-celestial/hound-archon-source.json': '35671eb47900c2364477b599afe939d0e99846091726429d6361c4c90515d2c8', 'assets-source/original-models/outsider-celestial/lemure-source.json': '83235d2cae46c15822ca2fd468b179f6b63b8e8973d6081cd41ff527350ac614', 'assets/game/icons/expanded-summoning/astral-deva.png': 'ef392c647741ec0257947bb723c2bca14c8161b103681c41d8b34f62999b93d2', 'assets/game/icons/expanded-summoning/dretch.png': 'bd591e82e9fa075d4b822dae5fcd2440c855953e1f66512adeae8a17ead9e6ee', 'assets/game/icons/expanded-summoning/hound-archon.png': 'bb98e26a8ed2d48e5d401eba5906b6af481388f25e6ad49accca851534a6aae3', 'assets/game/icons/expanded-summoning/icon-manifest.json': '0f43dad22cdb1d793143a0c697754397ef42f6618c0609b1e33a9f4907849149', 'assets/game/icons/expanded-summoning/lemure.png': 'c03bc1c797069cb6646d59519ca7ba8859a34ef9152d7f309cef98ffc05790b9', 'assets/game/icons/expanded-summoning/trumpet-archon.png': '3abe3aafe42b2adafd537adafcd54c3593e2081adb01fb1e050a2b4fecc6bed6', 'assets/game/icons/expanded-summoning/trumpet-blast.png': 'd730f950d15f715cbba5f61b4079fb223b618a882f2c1147ea00a0fc05ca7215', 'assets/outsider-celestial/dretch-albedo.png': 'c29c4919c245c3fd2cb0d6eae8e35784a941fcb4a5e6ff2ccba046fd9400fe2e', 'assets/outsider-celestial/dretch-mesh.json': 'a1e88c96d46b74068852f4d81a42f957b08ea0bd9cee241a2553e8c6758f8260', 'assets/outsider-celestial/hound-archon-albedo.png': '3cd8134f082ee44d7fec44b5d837177a44180c5582a09cc43b6d651bcab63cc3', 'assets/outsider-celestial/hound-archon-mesh.json': '2b3037797dad47e54cd486c9179435e32b003b239f8d07196e03a4b7080480a8', 'assets/outsider-celestial/lemure-albedo.png': '38d84196ebabaa34a52988651bde02c11ca99afeab0723530a830efab7662402', 'assets/outsider-celestial/lemure-mesh.json': '80724617887774edaf4dfbec254d6a629962d0a04e48021c82c53862d65bd11d'}

def check_scope(released, current, hidden):
    if current[:len(released)] != released:
        raise AssertionError("Released creature identities, placements or ordering changed")
    if len(current) != len(released)+len(ADDITIONS) or set(current[len(released):]) != ADDITIONS:
        raise AssertionError("Only the five selected species may be appended; Lantern remains in place")
    if hidden:
        raise AssertionError("Final public candidate withholds a creature")

def check_tail(released,current):
    identities.check_append_only(released,current)
    tail=current[len(released):]
    digest=hashlib.sha256(json.dumps(tail,sort_keys=True,separators=(",",":")).encode()).hexdigest()
    if len(tail)!=TAIL_COUNT or digest!=TAIL_SHA256 or any(x['status']!='active' for x in tail):
        raise AssertionError("Selected private identities changed")

def validate(root):
    read=lambda p:(root/p).read_text(encoding='utf-8-sig')
    blob=lambda p:common.blob(root,BASELINE,p)
    if json.loads(read('Info.json'))['Version']!=VERSION:raise AssertionError('Wrong batch version')
    for name,path,pattern in identities.VERSION_FIELDS:
        expected=INFORMATIONAL if 'Informational' in name else VERSION
        if re.findall(pattern,read(path)) != [expected]:raise AssertionError('Version differs: '+name)
    prior=json.loads(blob('blueprints/blueprints.json')); current=json.loads(read('blueprints/blueprints.json'))
    for key in ('$schema','schemaVersion','namespace','policy'):
        if current[key]!=prior[key]:raise AssertionError('Identity header changed: '+key)
    check_tail(prior['entries'],current['entries'])
    catalog='src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs'
    check_scope(rows(blob(catalog).decode('utf-8-sig')),rows(read(catalog)),suppressed_keys())
    # Preserve released art and freeze the already-authored selected additions.
    for p in identities.git(root,'ls-tree','-r','--name-only',BASELINE,'--','assets','assets-source').stdout.splitlines():
        if p not in ART and (root/p).read_bytes()!=blob(p):raise AssertionError('Unselected asset changed: '+p)
    changed=set(identities.git(root,'diff','--name-only',BASELINE,'--','assets','assets-source').stdout.splitlines())
    changed.update(identities.git(root,'ls-files','--others','--exclude-standard','--','assets','assets-source').stdout.splitlines())
    if changed!=set(ART):raise AssertionError('Selected asset set differs')
    for p,digest in ART.items():
        if hashlib.sha256((root/p).read_bytes()).hexdigest()!=digest:raise AssertionError('Selected asset bytes changed: '+p)
    static=json.loads(read('validation/static-validation.json')); previous=json.loads(blob('validation/static-validation.json'))
    for key,value in previous.items():
        if key not in ('version','milestone') and static.get(key)!=value:raise AssertionError('Historical record changed: '+key)
    record=static.get('expandedSummoningSprints3031',{})
    if static.get('version')!=VERSION or static.get('milestone')!=INFORMATIONAL or record.get('stage')!='public' or record.get('candidateOnly') is not True or record.get('ownerAestheticApproval')!='NOT_PERFORMED_NONBLOCKING':raise AssertionError('Truthful public source-only record missing')
    identities.check_released_art_owned(root)
    subprocess.run([sys.executable,'-B',str(root/'tools/expanded_summoning_manifest.py')],cwd=root,check=True)
    print('S30-31 public source boundary PASS; native and final-artifact qualification are separate.')

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--root',type=Path,default=Path(__file__).resolve().parents[1]);args=parser.parse_args()
    try:validate(args.root.resolve())
    except (AssertionError,ValueError,KeyError,OSError,subprocess.CalledProcessError) as error:
        print('S30-31 source boundary FAIL: '+str(error),file=sys.stderr);sys.exit(1)
