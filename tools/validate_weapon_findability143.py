#!/usr/bin/env python3
"""Retain inherited gates and qualify the narrowly specified 0.0.143 layout.

Runtime scene, route, pickup and persistence evidence are separate gates.
"""
import argparse
import json
import re
import subprocess
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_data_content_traits142 as baseline
from weapon_findability_native_reference import production_targets

VERSION='0.0.143'
BASE='4ba8d4aca087391144abf401f526189f59b26535'
MOVES={
    'TheLastWordSymbol':'b54aad6aa2844fa4c87f46088cde018b',
    'WatchAtWorldsEndSymbol':'e113fb75d9461924ab64df78c019991a',
    'UnfixedForm':'3172f82c9f21b8a439a6552850b39b4c',
    'MoonlitCrossing':'732080f3aa72fd14cb520902e4b4db89',
    'WinterReed':'4dc53495b10c62f4a90d4ae094d232c3',
    'DrawnHorizon':'d03682992a98a614c9d149fca2bab853',
    'ThunderAtTheGate':'dd50c5c9d07eaaf49869308ce8720aec',
    'PaperLantern':'5cce787544ae5964c8ac43ab7cf768ca',
    'RiverKingsMeasureSymbol':'77ad78d755a49af45abee46d86191b16',
}
def validate(root):
    baseline.VERSION=VERSION
    baseline.INFORMATIONAL_VERSION=VERSION+'-weapon-findability-fixes'
    baseline.PACKAGE_SUFFIX='weapon-findability-fixes'
    baseline.validate(root)
    items=production_targets(root)
    if {r['key']:r['targetGuid'] for r in items if r['key'] in MOVES} != MOVES:
        raise AssertionError('Specified seven and two scene-evidenced assignments differ')
    old_items=production_targets(root, lambda path: subprocess.check_output(
        ['git','show',BASE+':'+path],cwd=root).decode('utf-8-sig'))
    if [r for r in items if r['key'] not in MOVES] != [r for r in old_items if r['key'] not in MOVES]:
        raise AssertionError('One of the other 20 named weapon placements changed without scene evidence')
    corrections=json.loads((root/'validation/weapon-findability-scene-corrections.json').read_text())['corrections']
    if {r['key']:r['targetGuid'] for r in corrections} != {k:MOVES[k] for k in ('PaperLantern','RiverKingsMeasureSymbol')} or any(
            r['publishedSceneReferenceCount'] != 0 or r['actualSceneReferenceCount'] != 1 or not r['entityId'] for r in corrections):
        raise AssertionError('Additional corrections lack exact measured scene-reference evidence')
    if len({r['areaName'] for r in items})!=27:
        raise AssertionError('29 weapons plus unchanged Cord must use 28 exact areas')
    references=json.loads((root/'validation/weapon-findability-native-reference.json').read_text())['weapons']
    if [dict((k,r[k]) for k in items[0]) for r in references] != items:
        raise AssertionError('Complete native comparison contracts differ from the production registry')
    # The other 20 placements and every item identity, economy, effect,
    # crafting and module contract remain exactly as in the audited baseline.
    for path in ['blueprints/blueprints.json',
                 'src/KingmakerGunslinger/Blueprints/MagicFirearmBlueprints.cs',
                 'src/KingmakerGunslinger/Blueprints/EasternWeaponNamedBlueprints.cs',
                 'src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearNamedBlueprints.cs',
                 'src/KingmakerGunslinger/Blueprints/ElvenBranchedSpearCampaignBlueprints.cs',
                 'src/KingmakerGunslinger/CraftMagicItemsCompatibility/CraftMagicItemsRegistrationCatalog.cs']:
        old=subprocess.check_output(['git','show',BASE+':'+path],cwd=root).decode('utf-8-sig').replace('\r\n','\n')
        if (root/path).read_text(encoding='utf-8-sig') != old:
            raise AssertionError('Protected weapon contract changed: '+path)
    state=json.loads((root/'validation/static-validation.json').read_text(encoding='utf-8-sig'))['weaponFindability143']
    count=len(re.findall(r'\bCase\("',(root/'tests/KingmakerGunslinger.DomainTests/Program.cs').read_text()))
    if state['deterministicTestCount']!=count or state['worldLootWeaponCount']!=29 or state['inventoryItemCount']!=30 or state['distinctTargetCount']!=30 or state['distinctExactAreaCount']!=28:
        raise AssertionError('Active candidate counts differ from the complete registry')
    bridge=(root/'src/KingmakerGunslinger/Development/KingmakerDevelopmentBridge.RareFirearms.cs').read_text()
    if 'originalContentsPreserved=observer-qualified' in bridge or 'knownReferences=0:unique-area-owned' in bridge:
        raise AssertionError('Hardcoded mechanical evidence remains')
    print('Weapon findability 0.0.143 repository validation PASS; physical qualification requires native runtime evidence')

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--root',type=Path,default=Path(__file__).resolve().parents[1])
    validate(parser.parse_args().root.resolve())
