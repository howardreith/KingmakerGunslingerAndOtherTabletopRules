"""Verify native capture provenance and completeness; never infer visual approval."""
import argparse
import hashlib
import json
import math
from pathlib import Path
import re
import struct


def validate(evidence, result, build, identity, directory):
    errors = []
    def require(condition, message):
        if not condition:
            errors.append(message)
    require(result.get('status') == 'PASS', 'Runtime scenario did not pass')
    require(evidence.get('runId') == result.get('runId'), 'Capture run differs from runtime result')
    require(evidence.get('scenario') == result.get('scenario'), 'Capture scenario differs from runtime result')
    require(evidence.get('dllMvid') == build.get('dllMvid'), 'Capture DLL differs from build')
    bare_identity = identity.get('runtimeIdentity')
    decorated_identity = f"{bare_identity};mvid={identity.get('moduleVersionId')};pid={identity.get('processId')}"
    require(bool(bare_identity) and result.get('runtimeIdentity') in (bare_identity, decorated_identity),
            'Runtime assembly identity differs from loaded-build record')
    require(identity.get('moduleVersionId') == build.get('dllMvid'), 'Runtime MVID differs from build')
    require(identity.get('loadedModuleSha256') == build.get('dllSha256'), 'Runtime DLL hash differs from build')
    require(evidence.get('captureApi') == 'UnityEngine.ScreenCapture.CaptureScreenshot(string)', 'Unexpected capture API')
    overlay = evidence.get('modManagerOverlay', {})
    require(overlay.get('restored') is True and overlay.get('status') == 'original-window-state-restored',
            'Native overlay state was not verifiably restored')
    require(isinstance(overlay.get('originallyOpened'), bool), 'Original overlay state is missing')
    if overlay.get('originallyOpened'):
        require(overlay.get('settingsIndex') == -1 and overlay.get('previousSettingsIndex') == -1
                and overlay.get('gameScriptCount') == 0, 'Closing overlay was not limited to the plain startup list')
    records = evidence.get('records', [])
    require(bool(records), 'No native screenshots')
    for index, record in enumerate(records):
        name = record.get('file', '')
        if name != f'native-ui-{index:03d}.png' or not re.fullmatch(r'native-ui-\d{3,}\.png', name):
            errors.append('Invalid or duplicate screenshot filename: ' + name)
            continue
        require(record.get('status') == 'captured-native-screen-awaiting-visual-inspection', 'Incomplete capture: ' + name)
        require(bool(record.get('stage')) and isinstance(record.get('nativeState'), dict), 'Missing native UI identity: ' + name)
        if record.get('stage', '').startswith(('native-weapon-row:', 'native-learning-row:', 'native-selected-fact:', 'native-scroll-row:', 'native-scroll-merchant-row:')):
            state = record.get('nativeState', {})
            state = state if isinstance(state, dict) else {}
            viewport = state.get('viewport', {})
            viewport = viewport if isinstance(viewport, dict) else {}
            api = viewport.get('nativeApi')
            known_scroll = api == 'UnityEngine.UI.ScrollRect.normalizedPosition' or (
                api == 'Kingmaker.UI.Common.ScrollRectExtended.ScrollToRectCenter' and
                viewport.get('scrollType') == 'Kingmaker.UI.Common.ScrollRectExtended' and
                bool(viewport.get('contentObject')) and bool(viewport.get('viewportObject')))
            require(known_scroll and
                    viewport.get('rowActive') is True and viewport.get('rowVerticallyVisible') is True and
                    viewport.get('restored') is True, 'Native row viewport/restoration is incomplete: ' + name)
            values = [viewport.get(key) for key in ('rowMinY', 'rowMaxY', 'viewportMinY', 'viewportMaxY')]
            finite = all(isinstance(value, (int, float)) and not isinstance(value, bool) and math.isfinite(value) for value in values)
            require(finite and values[2] - 1 <= values[0] < values[1] <= values[3] + 1,
                    'Native row is outside its recorded vertical viewport: ' + name)
            if 'originalContentX' in viewport:
                positions = [viewport.get(key) for key in ('originalContentX', 'originalContentY', 'restoredContentX', 'restoredContentY')]
                finite_positions = all(isinstance(value, (int, float)) and not isinstance(value, bool) and math.isfinite(value)
                                       for value in positions)
                require(finite_positions and math.hypot(positions[2]-positions[0], positions[3]-positions[1]) < 0.01,
                        'Actual native content position was not restored: ' + name)
            target = state.get('targetRow', {})
            target = target if isinstance(target, dict) else {}
            require(bool(target.get('name')), 'Native row target identity is missing: ' + name)
            if record['stage'].startswith('native-weapon-row:'):
                require(target.get('featureGuid') == '1e1f627d26ad36f43bbd26cc2bf8ac7e' and
                        bool(target.get('acronym')) and target.get('iconIsNull') is True and
                        (bool(target.get('parameterGuid')) or isinstance(target.get('category'), int)),
                        'Native weapon row identity/presentation differs: ' + name)
                exotic_proficiencies = {
                    0x004b4d47: '017d586ec4546feabf6eaaa67ce74a3f',
                    0x004b4d48: 'b14f7d9b2b665801a9d5b916c6be4ea9',
                    0x004b4d49: '93ef81404f085e2a8b261bdab15d5a08'}
                proficiency = exotic_proficiencies.get(target.get('category'))
                if proficiency:
                    require(target.get('learnedProficiencyGuid') == proficiency and
                            target.get('proficiencyPresent') is True,
                            'Native exotic weapon row lacks its learned proficiency: ' + name)
            elif record['stage'].startswith('native-learning-row:'):
                require(bool(target.get('spellGuid')) and bool(target.get('classGuid')) and
                        target.get('previewOnly') is True and target.get('enabled') is True and
                        target.get('spellLevel') in (5, 7), 'Native learning row identity/preview differs: ' + name)
            elif record['stage'].startswith('native-selected-fact:'):
                roots = {'1e1f627d26ad36f43bbd26cc2bf8ac7e', '09c9e82965fb4334b984a1e9df3bd088',
                         '31470b17e8446ae4ea0dacd6c5817d86', '7cf5edc65e785a24f9cf93af987d66b3',
                         'f4201c85a991369408740c6888362e20'}
                parameters = {'8b39bd79d27048dda58e0a513e529f2c':'P',
                              '939f52fce6ec4c5d8df2bb1cf793416a':'M',
                              '79579fdb7d494f4fb0e618f6e4c33990':'B'}
                rapid = {'070f4f07b5164d8a82d647a93539746d':'P',
                         '128963c18e0d48268d8696f9f14513d2':'M',
                         '40c69ab4ea844bd8a087cb8e84235bd9':'B'}
                letter = parameters.get(target.get('parameterGuid')) if target.get('featureGuid') in roots else (
                    rapid.get(target.get('featureGuid')) if target.get('parameterGuid') is None else None)
                require(bool(letter) and target.get('expectedLetter') == letter and target.get('nativeGlyph') == letter and
                        target.get('glyphActive') is True and target.get('glyphTruncated') is False and
                        target.get('glyphOverflowing') is False and bool(target.get('font')) and
                        target.get('nativeBackground') is True and target.get('factAndParameterRetained') is True and
                        target.get('fallbackIconPresent') is True and target.get('otherFactIconsExact') is True and
                        isinstance(target.get('otherFactRows'), int) and target['otherFactRows'] > 0,
                        'Actual native selected-fact identity/glyph/controls differ: ' + name)
                for ancestor in state.get('nativeLayout', {}).get('targetAncestors', []):
                    if ancestor.get('firearmFitApplied') is True:
                        height, preferred = ancestor.get('height'), ancestor.get('preferredHeight')
                        finite_heights = all(isinstance(value, (int, float)) and not isinstance(value, bool)
                                             and math.isfinite(value) for value in (height, preferred))
                        require(ancestor.get('fitterEnabled') is True and ancestor.get('originalFitterEnabled') is False
                                and ancestor.get('originalVerticalFit') == 'PreferredSize'
                                and ancestor.get('verticalFit') == 'PreferredSize' and ancestor.get('horizontalFit') == 'Unconstrained'
                                and ancestor.get('originalHorizontalFit') in ('Unconstrained', 'MinSize', 'PreferredSize', 'Clamp')
                                and finite_heights
                                and height >= preferred - 1,
                                'Native Total preferred layout did not cover its own content: ' + name)
        if record.get('stage', '').startswith(('native-scroll-row:', 'native-scroll-description:', 'native-scroll-merchant-row:')):
            state = record.get('nativeState', {})
            target = state.get('targetRow', {})
            strategic_items = {
                '2c283c993b233df53b487fc7eeac2ba3': ('82e3fb1dce1647b58d3b7169c8520af0', 'teleport'),
                '2a2b0185be1d2dba5aa1b5a24774e17d': ('73d19adfe18743e0a2a3a21abf4af5f3', 'greater-teleport'),
                '42d3daeb7d503687df8953b47727372b': ('596d85a666204d6ea5c0188e53f4b4de', 'word-of-recall')}
            expected = strategic_items.get(target.get('itemGuid'))
            require(expected is not None and target.get('spellGuid') == expected[0] and
                    target.get('iconName') == 'KMG_Icon_' + expected[1],
                    'Native scroll item/spell/art identity differs: ' + name)
            require(target.get('renderedIconExact') is True and target.get('spellIconMatchesItem') is True and
                    target.get('itemReferenceRetained') is True and target.get('identified') is True and
                    type(target.get('itemCount')) is int and target['itemCount'] == 1 and
                    type(target.get('charges')) is int and target['charges'] == 1 and
                    type(target.get('otherItemRows')) is int and target['otherItemRows'] > 0 and
                    target.get('otherItemIconsExact') is True,
                    'Native scroll slot/reference/control evidence differs: ' + name)
            if record['stage'].startswith('native-scroll-merchant-row:'):
                merchant = state.get('merchant', {})
                require(state.get('surface') == 'native-scroll-merchant' and
                        all(merchant.get(key) is True for key in ('nativeVendorBound', 'nativeStoreBound',
                            'privateStock', 'transferCollectionsEmpty', 'playerInventoryUnchanged',
                            'unregisteredVendor', 'controlBlueprintRetained')) and
                        type(merchant.get('stockCount')) is int and merchant['stockCount'] == 4,
                        'Native scroll merchant ownership/stock/control evidence differs: ' + name)
            if record['stage'].startswith('native-scroll-description:'):
                description = state.get('description', {})
                require(state.get('surface') == 'native-scroll-description' and description.get('shown') is True and
                        description.get('nameExact') is True and description.get('tooltipItemExact') is True and
                        description.get('tooltipDataItemExact') is True and
                        type(description.get('matchingIcons')) is int and description['matchingIcons'] > 0,
                        'Native scroll description identity/icon is incomplete: ' + name)
        require(record.get('completedFrame', -1) > record.get('requestedFrame', 0), 'No completed render frame: ' + name)
        path = directory / name
        if not path.is_file():
            errors.append('Missing screenshot: ' + name)
            continue
        data = path.read_bytes()
        require(hashlib.sha256(data).hexdigest() == record.get('sha256'), 'Screenshot hash differs: ' + name)
        if len(data) < 45 or data[:8] != b'\x89PNG\r\n\x1a\n' or data[-12:] != b'\x00\x00\x00\x00IEND\xaeB`\x82':
            errors.append('Incomplete PNG: ' + name)
            continue
        width, height = struct.unpack('>II', data[16:24])
        require((width, height) == (record.get('width'), record.get('height')) and width >= 640 and height >= 480,
                'PNG differs from recorded framebuffer: ' + name)
    return errors


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--evidence', type=Path, required=True)
    parser.add_argument('--build-manifest', type=Path, required=True)
    args = parser.parse_args()
    read = lambda p: json.loads(p.read_text(encoding='utf-8-sig'))
    errors = validate(read(args.evidence), read(args.evidence.parent/'runtime-result.json'),
                      read(args.build_manifest), read(args.evidence.parent/'runtime-loaded-build-identity.json'), args.evidence.parent)
    for error in errors:
        print('FAIL:', error)
    if not errors:
        print('PASS native capture provenance, completed frames, PNG hashes and overlay restoration. Visual inspection and owner approval remain separate.')
    return int(bool(errors))


if __name__ == '__main__':
    raise SystemExit(main())
