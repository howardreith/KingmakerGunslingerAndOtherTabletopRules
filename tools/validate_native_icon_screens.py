"""Verify native capture provenance and completeness; never infer visual approval."""
import argparse
import hashlib
import json
import math
from pathlib import Path
import re
import struct


def valid_buff_label_geometry(target):
    """Validate final native glyph geometry, including Overflow-mode labels."""
    geometry = target.get('labelGeometry')
    if not isinstance(geometry, dict):
        return False
    generated = geometry.get('generatedText')
    if (geometry.get('api') != 'native TMP_TextInfo final mesh vertices; read-only' or
            not isinstance(generated, str) or generated != geometry.get('parsedText') or
            not isinstance(target.get('name'), str) or generated.casefold() != target['name'].casefold() or
            any(geometry.get(key) is not True for key in ('complete', 'allGlyphsGenerated', 'finiteGeometry',
                'glyphsWithinClippingMasks', 'glyphsClearOfIconAndTimer', 'glyphsClearOfOtherRows')) or
            geometry.get('canvasRendererCulled') is not False or geometry.get('overflowMode') != 'Overflow' or
            type(target.get('labelOverflowing')) is not bool or
            geometry.get('overflowReported') is not target['labelOverflowing'] or
            type(geometry.get('characterCount')) is not int or geometry['characterCount'] != len(generated) or
            type(geometry.get('requiredGlyphCount')) is not int or
            geometry['requiredGlyphCount'] != sum(not c.isspace() for c in generated) or geometry['requiredGlyphCount'] <= 0):
        return False

    def bounds(value):
        return (isinstance(value, dict) and all(type(value.get(key)) in (int, float) and math.isfinite(value[key])
                for key in ('minX', 'maxX', 'minY', 'maxY')) and
                value['minX'] <= value['maxX'] and value['minY'] <= value['maxY'])
    def contains(outer, inner):
        return (outer['minX'] - .01 <= inner['minX'] and inner['maxX'] <= outer['maxX'] + .01 and
                outer['minY'] - .01 <= inner['minY'] and inner['maxY'] <= outer['maxY'] + .01)
    def overlaps(left, right):
        return (left['minX'] < right['maxX'] - .01 and left['maxX'] > right['minX'] + .01 and
                left['minY'] < right['maxY'] - .01 and left['maxY'] > right['minY'] + .01)
    keys = ('glyphBounds', 'rowBounds', 'nameBounds', 'iconBounds', 'timerBounds', 'timerIconBounds')
    masks = geometry.get('clippingMaskBounds')
    neighbors = geometry.get('otherRowBounds')
    if (not all(bounds(geometry.get(key)) for key in keys) or not isinstance(masks, list) or
            type(geometry.get('clipMaskCount')) is not int or not masks or geometry['clipMaskCount'] != len(masks) or
            not all(bounds(mask) for mask in masks) or not isinstance(neighbors, list) or
            type(geometry.get('otherRowCount')) is not int or geometry['otherRowCount'] != len(neighbors) or
            len(neighbors) != target.get('fixtureBuffCount', 0) - 1 or not all(bounds(row) for row in neighbors)):
        return False
    glyph = geometry['glyphBounds']
    return (glyph['minX'] < glyph['maxX'] and glyph['minY'] < glyph['maxY'] and
            all(contains(mask, glyph) for mask in masks) and not any(overlaps(glyph, row) for row in neighbors) and
            geometry.get('glyphsWithinRow') is contains(geometry['rowBounds'], glyph) and
            geometry.get('glyphsWithinNameRect') is contains(geometry['nameBounds'], glyph) and
            not any(overlaps(glyph, geometry[key]) for key in ('iconBounds', 'timerBounds', 'timerIconBounds')))


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
        if record.get('stage', '').startswith(('native-weapon-row:', 'native-learning-row:', 'native-selected-fact:', 'native-scroll-row:', 'native-scroll-merchant-row:', 'native-racial-feat-row:', 'native-strategic-control:', 'native-racial-buff-row:')):
            state = record.get('nativeState', {})
            state = state if isinstance(state, dict) else {}
            viewport = state.get('viewport', {})
            viewport = viewport if isinstance(viewport, dict) else {}
            api = viewport.get('nativeApi')
            known_scroll = api == 'UnityEngine.UI.ScrollRect.normalizedPosition' or (
                api == 'Kingmaker.UI.Common.ScrollRectExtended.ScrollToRectCenter' and
                viewport.get('scrollType') == 'Kingmaker.UI.Common.ScrollRectExtended' and
                bool(viewport.get('contentObject')) and bool(viewport.get('viewportObject')))
            if record['stage'].startswith('native-racial-buff-row:'):
                known_scroll = api == 'native viewport observation' and viewport.get('scrollMutationRequested') is False and (
                    (viewport.get('positionApi'), viewport.get('scrollType')) in {
                        ('UnityEngine.UI.ScrollRect.normalizedPosition', 'UnityEngine.UI.ScrollRect'),
                        ('Kingmaker.UI.Common.ScrollRectExtended.ScrollToRectCenter', 'Kingmaker.UI.Common.ScrollRectExtended')}) and (
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
                arcane_classes = ('ba34257984f4c41408ce1dc2004e342e', 'b3a505fb61437dc4097f43c3f8f9a4cf')
                learning_pairs = {(class_guid, spell_guid, level)
                                  for class_guid in arcane_classes
                                  for spell_guid, level in (('82e3fb1dce1647b58d3b7169c8520af0', 5),
                                                           ('73d19adfe18743e0a2a3a21abf4af5f3', 7))}
                # v0.0.129 adds the existing optional Oracle's normal Recall
                # choice. Accept only its exact canonical class/spell/level.
                learning_pairs.add(('32c02466b2364c8a906e6e4761175099', '596d85a666204d6ea5c0188e53f4b4de', 6))
                require((target.get('classGuid'), target.get('spellGuid'), target.get('spellLevel')) in learning_pairs and
                        target.get('previewOnly') is True and target.get('enabled') is True and
                        record['stage'] == 'native-learning-row:' + str(target.get('classGuid')) + '-' + str(target.get('spellLevel')),
                        'Native learning row identity/preview differs: ' + name)
            elif record['stage'].startswith('native-racial-feat-row:'):
                keys = ['elemental-strike', 'scorching-weapons', 'inner-flame', 'blazing-aura', 'firesight',
                        'airy-step', 'wings-of-air', 'cloud-gazer', 'inner-breath', 'hydraulic-maneuver', 'triton-portal']
                feats = {f'e116e1e0a17a4aceb001{index:012d}': key for index, key in enumerate(keys, 1)}
                race_indices = {'Ifrit': [1, 2, 3, 4, 5], 'Oread': [1], 'Sylph': [1, 6, 7, 8, 9], 'Undine': [1, 10, 11]}
                expected = [f'e116e1e0a17a4aceb001{index:012d}' for index in race_indices.get(state.get('race'), [])]
                guid = target.get('featureGuid')
                require(state.get('surface') == 'native-racial-feat-selector' and state.get('showAll') is True and
                        bool(expected) and state.get('expectedFeatGuids') == expected and guid in expected and
                        record['stage'] == 'native-racial-feat-row:' + str(guid) and
                        target.get('iconName') == 'KMG_Icon_' + feats.get(guid, 'invalid'),
                        'Native racial feat identity/art/race/filter differs: ' + name)
                markers = target.get('nativeMarkers', {})
                require(target.get('renderedIconExact') is True and target.get('titleExact') is True and
                        isinstance(target.get('nativeEligibility'), str) and bool(target['nativeEligibility'].strip()) and
                        type(target.get('nativeInteractable')) is bool and
                        target.get('selectionAndEligibilityRetained') is True and target.get('nativeMarkersRetained') is True and
                        isinstance(markers, dict) and set(markers) == {'m_DisableMark', 'm_ForbiddenMark', 'm_AllreadyUsedMark'} and
                        all(type(value) is bool for value in markers.values()) and
                        type(target.get('otherIconRows')) is int and target['otherIconRows'] > 0 and target.get('otherIconsExact') is True,
                        'Native racial feat rendered icon/title/eligibility/controls differ: ' + name)
            elif record['stage'].startswith('native-racial-buff-row:'):
                expected = {
                    'Ifrit': {'e116e1e0a17a4aceb001000000000013','e116e1e0a17a4aceb001000000000015',
                              'e116e1e0a17a4aceb001000000000018','e117e1e0a17a4acec001000000000063'},
                    'Oread': {'e117e1e0a17a4acec001000000000064','e117e1e0a17a4acec001000000000071','e118e1e0a17a4acec001000000000010'},
                    'Sylph': {'e116e1e0a17a4aceb001000000000019','e117e1e0a17a4acec001000000000065','e117e1e0a17a4acec001000000000081'},
                    'Undine': {'e118e1e0a17a4acec001000000000003','e118e1e0a17a4acec001000000000005','e118e1e0a17a4acec001000000000006'}}
                targets = expected.get(state.get('race'), set())
                alpha = target.get('nativeAlpha')
                require(state.get('surface') == 'native-racial-buff-sheet' and target.get('buffGuid') in targets and
                        record['stage'] == 'native-racial-buff-row:' + str(target.get('buffGuid')) and bool(target.get('ownerId')) and
                        bool(target.get('name')) and bool(target.get('expectedSprite')) and
                        target.get('expectedSprite') == target.get('renderedSprite') and target.get('spriteExact') is True and
                        target.get('labelExact') is True and target.get('labelTruncated') is False,
                        'Native racial buff source/icon/title differs: ' + name)
                require(valid_buff_label_geometry(target), 'Native racial buff glyph mesh is missing, clipped or overlapping: ' + name)
                require(target.get('buffActive') is False and target.get('buffTurnedOn') is False and target.get('ownerOutsideWorld') is True and
                        target.get('ownerTurnedOnRetained') is True and target.get('buffCollectionInactive') is True and
                        target.get('nativeSectionShown') is True and
                        type(alpha) in (int,float) and math.isfinite(alpha) and 0 < alpha <= 1 and target.get('nativeStateRetained') is True and
                        bool(target.get('nativeControlGuid')) and target.get('nativeControlGuid') not in set().union(*expected.values()) and
                        target.get('nativeControlExact') is True and type(target.get('fixtureBuffCount')) is int and
                        target['fixtureBuffCount'] == len(targets) + 1 and target.get('worldAndOriginalFactsRetained') is True,
                        'Native racial buff lifecycle/control/original context differs: ' + name)
            elif record['stage'].startswith('native-strategic-control:'):
                spells = {'Teleport':'82e3fb1dce1647b58d3b7169c8520af0', 'GreaterTeleport':'73d19adfe18743e0a2a3a21abf4af5f3'}
                image_names = target.get('nativeImageNames', [])
                require(state.get('surface') == 'native-strategic-text-control' and
                        target.get('spellKind') in spells and target.get('spellGuid') == spells.get(target.get('spellKind')) and
                        target.get('sourceKind') in ('Prepared','Spontaneous') and bool(target.get('sourceKey')) and
                        bool(target.get('bookGuid')) and bool(target.get('casterId')) and
                        type(target.get('uses')) is int and target['uses'] > 0,
                        'Native strategic text source identity differs: ' + name)
                require(isinstance(target.get('name'), str) and len(target['name'].splitlines()) == 2 and
                        target.get('labelExact') is True and target.get('labelTruncated') is False and target.get('labelOverflowing') is False and
                        bool(target.get('nativeFont')) and target.get('buttonActive') is True and target.get('buttonInteractable') is True and
                        type(target.get('canonicalSpellImages')) is int and target['canonicalSpellImages'] == 0 and
                        type(target.get('nativeImageCount')) is int and target['nativeImageCount'] > 0 and
                        isinstance(image_names, list) and len(image_names) == target['nativeImageCount'] and
                        target.get('nativeImagesRetained') is True and target.get('nativeControlsRetained') is True and
                        type(target.get('nativeControlCount')) is int and target['nativeControlCount'] > 0 and
                        target.get('contextAndResourcesRetained') is True,
                        'Native strategic text label/background/controls/resources differ: ' + name)
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
