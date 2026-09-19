"""Reject unusable or misattributed screenshot evidence, including the first occluded run."""
import copy
import hashlib
from pathlib import Path
import struct
import sys
import tempfile
import unittest
import zlib
sys.dont_write_bytecode = True
from validate_native_icon_screens import validate


class NativeScreenEvidenceTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.directory = Path(self.temp.name)
        def chunk(kind, data):
            return struct.pack('>I', len(data)) + kind + data + struct.pack('>I', zlib.crc32(kind+data))
        png = b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', 640, 480, 8, 2, 0, 0, 0))
        png += chunk(b'IDAT', zlib.compress(b'\0' * (480*(640*3+1)))) + chunk(b'IEND', b'')
        (self.directory/'native-ui-000.png').write_bytes(png)
        self.build = {'dllMvid':'mvid', 'dllSha256':'dll'}
        self.result = {'runId':'run', 'scenario':'native-scenario', 'status':'PASS', 'runtimeIdentity':'assembly'}
        self.identity = {'runtimeIdentity':'assembly', 'moduleVersionId':'mvid', 'loadedModuleSha256':'dll', 'processId':123}
        self.evidence = {'runId':'run', 'scenario':'native-scenario', 'dllMvid':'mvid',
            'captureApi':'UnityEngine.ScreenCapture.CaptureScreenshot(string)',
            'modManagerOverlay':{'restored':True, 'status':'original-window-state-restored', 'originallyOpened':True,
                'settingsIndex':-1, 'previousSettingsIndex':-1, 'gameScriptCount':0},
            'records':[{'file':'native-ui-000.png', 'stage':'real-stage', 'nativeState':{'owner':'fixture'},
                'status':'captured-native-screen-awaiting-visual-inspection', 'requestedFrame':1, 'completedFrame':5,
                'width':640, 'height':480, 'sha256':hashlib.sha256(png).hexdigest()}]}

    def errors(self):
        return validate(self.evidence, self.result, self.build, self.identity, self.directory)

    def test_complete_evidence_is_only_provenance(self):
        self.assertEqual([], self.errors())
        self.result['runtimeIdentity'] = 'assembly;mvid=mvid;pid=123'
        self.assertEqual([], self.errors())
        self.result['runtimeIdentity'] = 'assembly;mvid=mvid;pid=124'
        self.assertTrue(self.errors())

    def test_first_occluded_capture_without_overlay_observation_is_rejected(self):
        del self.evidence['modManagerOverlay']
        self.assertTrue(self.errors())

    def test_wrong_run_or_artifact_is_rejected(self):
        for key in ['runId', 'scenario', 'dllMvid']:
            with self.subTest(key=key):
                original = self.evidence[key]
                self.evidence[key] = 'different'
                self.assertTrue(self.errors())
                self.evidence[key] = original

    def test_unrestored_or_callback_dispatching_overlay_is_rejected(self):
        original = copy.deepcopy(self.evidence['modManagerOverlay'])
        for key, value in [('restored',False), ('settingsIndex',0), ('previousSettingsIndex',0), ('gameScriptCount',1)]:
            self.evidence['modManagerOverlay'] = dict(original, **{key:value})
            self.assertTrue(self.errors())

    def test_modified_or_truncated_png_is_rejected(self):
        path = self.directory/'native-ui-000.png'
        path.write_bytes(path.read_bytes()[:-12])
        self.assertTrue(self.errors())

    def test_duplicate_and_parent_paths_are_rejected_without_reading_them(self):
        self.evidence['records'].append(copy.deepcopy(self.evidence['records'][0]))
        self.assertTrue(self.errors())
        self.evidence['records'][0]['file'] = '../native-ui-000.png'
        self.assertTrue(self.errors())

    def test_no_frames_or_incomplete_capture_is_rejected(self):
        for key, value in [('completedFrame',1), ('width',1920), ('status','pending')]:
            original = self.evidence['records'][0][key]
            self.evidence['records'][0][key] = value
            self.assertTrue(self.errors())
            self.evidence['records'][0][key] = original
        self.evidence['records'] = []
        self.assertTrue(self.errors())

    def prepare_native_row(self):
        record = self.evidence['records'][0]
        record['stage'] = 'native-weapon-row:Pistol'
        record['nativeState'] = {
            'viewport': {'nativeApi':'UnityEngine.UI.ScrollRect.normalizedPosition', 'rowActive':True,
                         'rowVerticallyVisible':True, 'restored':True, 'rowMinY':20, 'rowMaxY':60,
                         'viewportMinY':0, 'viewportMaxY':200},
            'targetRow': {'name':'Pistol', 'featureGuid':'1e1f627d26ad36f43bbd26cc2bf8ac7e',
                          'parameterGuid':'pistol', 'acronym':'P', 'iconIsNull':True}}
        return record['nativeState']

    def test_active_row_outside_viewport_or_unrestored_scroll_is_rejected(self):
        state = self.prepare_native_row()
        self.assertEqual([], self.errors())
        for key,value in [('rowMinY',-50), ('rowMaxY',250), ('rowMaxY',float('nan')),
                          ('rowActive',False), ('rowVerticallyVisible',False), ('restored',False)]:
            with self.subTest(key=key,value=value):
                original = state['viewport'][key]
                state['viewport'][key] = value
                self.assertTrue(self.errors())
                state['viewport'][key] = original
        state['targetRow']['iconIsNull'] = False
        self.assertTrue(self.errors())

    def test_learning_capture_requires_exact_preview_and_spell_row(self):
        state = self.prepare_native_row()
        self.evidence['records'][0]['stage'] = 'native-learning-row:ba34257984f4c41408ce1dc2004e342e-5'
        state['targetRow'] = {'name':'Teleport', 'spellGuid':'82e3fb1dce1647b58d3b7169c8520af0',
                              'classGuid':'ba34257984f4c41408ce1dc2004e342e',
                              'spellLevel':5, 'previewOnly':True, 'enabled':True}
        self.assertEqual([], self.errors())
        for key,value in [('spellGuid',''), ('classGuid',''), ('spellLevel',4), ('previewOnly',False), ('enabled',False)]:
            original = state['targetRow'][key]
            state['targetRow'][key] = value
            self.assertTrue(self.errors())
            state['targetRow'][key] = original

    def test_oracle_recall_requires_exact_native_class_spell_level_and_preview(self):
        state = self.prepare_native_row()
        record = self.evidence['records'][0]
        record['stage'] = 'native-learning-row:32c02466b2364c8a906e6e4761175099-6'
        state['targetRow'] = {'name':'Word of Recall', 'spellGuid':'596d85a666204d6ea5c0188e53f4b4de',
                              'classGuid':'32c02466b2364c8a906e6e4761175099',
                              'spellLevel':6, 'previewOnly':True, 'enabled':True}
        self.assertEqual([], self.errors())
        for key, value in [('classGuid','ba34257984f4c41408ce1dc2004e342e'),
                           ('spellGuid','82e3fb1dce1647b58d3b7169c8520af0'),
                           ('spellLevel',5), ('previewOnly',False), ('enabled',False)]:
            original = state['targetRow'][key]
            state['targetRow'][key] = value
            self.assertTrue(self.errors())
            state['targetRow'][key] = original
        record['stage'] += '-mismatched-stage'
        self.assertTrue(self.errors())

    def prepare_scroll_row(self):
        state = self.prepare_native_row()
        self.evidence['records'][0]['stage'] = 'native-scroll-row:teleport'
        state['surface'] = 'native-scroll-inventory'
        state['targetRow'] = {'name':'Scroll of Teleport', 'itemGuid':'2c283c993b233df53b487fc7eeac2ba3',
            'spellGuid':'82e3fb1dce1647b58d3b7169c8520af0', 'iconName':'KMG_Icon_teleport',
            'renderedIconExact':True, 'spellIconMatchesItem':True, 'itemReferenceRetained':True,
            'itemCount':1, 'charges':1, 'identified':True, 'otherItemRows':3, 'otherItemIconsExact':True}
        return state

    def prepare_racial_feat_row(self):
        state = self.prepare_native_row()
        guid = 'e116e1e0a17a4aceb001000000000001'
        self.evidence['records'][0]['stage'] = 'native-racial-feat-row:' + guid
        state.update(surface='native-racial-feat-selector', race='Oread', showAll=True, expectedFeatGuids=[guid])
        state['targetRow'] = dict(name='Elemental Strike', featureGuid=guid, iconName='KMG_Icon_elemental-strike',
            renderedIconExact=True, titleExact=True, nativeEligibility='CanSelect', nativeInteractable=True,
            selectionAndEligibilityRetained=True, nativeMarkersRetained=True, otherIconRows=8, otherIconsExact=True,
            nativeMarkers=dict(m_DisableMark=False, m_ForbiddenMark=False, m_AllreadyUsedMark=False))
        return state

    def test_racial_feat_requires_its_real_art_title_and_preserved_eligibility(self):
        target = self.prepare_racial_feat_row()['targetRow']
        self.assertEqual([], self.errors())
        # A disabled native row is valid evidence; enabling it is not required.
        target.update(nativeEligibility='PrerequisitesNotMet', nativeInteractable=False)
        target['nativeMarkers']['m_DisableMark'] = True
        self.assertEqual([], self.errors())
        for key, value in [('featureGuid','unrelated'), ('iconName','borrowed'), ('renderedIconExact',False),
                           ('titleExact',False), ('nativeEligibility',''), ('nativeInteractable',None),
                           ('selectionAndEligibilityRetained',False), ('nativeMarkersRetained',False),
                           ('nativeMarkers',{}), ('otherIconRows',0), ('otherIconRows',True), ('otherIconsExact',False)]:
            with self.subTest(key=key):
                original = target[key]
                target[key] = value
                self.assertTrue(self.errors())
                target[key] = original

    def test_racial_feat_requires_exact_native_filter_race_and_target_set(self):
        state = self.prepare_racial_feat_row()
        for key, value in [('surface','mock-list'), ('showAll',False), ('race','Ifrit'), ('race','unknown'),
                           ('expectedFeatGuids',[])]:
            original = state[key]
            state[key] = value
            self.assertTrue(self.errors())
            state[key] = original
        self.evidence['records'][0]['stage'] = 'native-racial-feat-row:unrelated'
        self.assertTrue(self.errors())

    def test_scroll_requires_exact_item_spell_and_preserved_controls(self):
        state = self.prepare_scroll_row()
        self.assertEqual([], self.errors())
        original_target = copy.deepcopy(state['targetRow'])
        for item, spell, key in [('2a2b0185be1d2dba5aa1b5a24774e17d', '73d19adfe18743e0a2a3a21abf4af5f3', 'greater-teleport'),
                                  ('42d3daeb7d503687df8953b47727372b', '596d85a666204d6ea5c0188e53f4b4de', 'word-of-recall')]:
            state['targetRow'].update(itemGuid=item, spellGuid=spell, iconName='KMG_Icon_' + key)
            self.assertEqual([], self.errors())
        state['targetRow'] = original_target
        for key,value in [('itemGuid','donor'), ('spellGuid','wrong-spell'), ('iconName','borrowed'),
                          ('renderedIconExact',False), ('itemReferenceRetained',False), ('otherItemRows',0),
                          ('otherItemRows',True), ('otherItemIconsExact',False), ('itemCount',2), ('itemCount',True), ('charges',0)]:
            with self.subTest(key=key):
                original = state['targetRow'][key]
                state['targetRow'][key] = value
                self.assertTrue(self.errors())
                state['targetRow'][key] = original

    def prepare_native_buff(self):
        state = self.prepare_native_row()
        guid = 'e116e1e0a17a4aceb001000000000013'
        self.evidence['records'][0]['stage'] = 'native-racial-buff-row:' + guid
        state.update(surface='native-racial-buff-sheet', race='Ifrit')
        state['viewport'].update(nativeApi='native viewport observation', scrollMutationRequested=False,
            positionApi='UnityEngine.UI.ScrollRect.normalizedPosition', scrollType='UnityEngine.UI.ScrollRect',
            contentObject='Content', viewportObject='Viewport')
        target = state['targetRow'] = dict(name='Elemental Strike', buffGuid=guid, ownerId='owned-mercenary',
            expectedSprite='elemental-strike', renderedSprite='elemental-strike', spriteExact=True, labelExact=True,
            labelTruncated=False, labelOverflowing=False, buffActive=False, buffTurnedOn=False, ownerOutsideWorld=True,
            ownerTurnedOnRetained=True, buffCollectionInactive=True, nativeSectionShown=True,
            nativeAlpha=0.5, nativeStateRetained=True, nativeControlGuid='native-bless', nativeControlExact=True,
            fixtureBuffCount=5, worldAndOriginalFactsRetained=True)
        def rectangle(x1, x2, y1, y2):
            return dict(minX=x1, maxX=x2, minY=y1, maxY=y2)
        target['labelGeometry'] = dict(api='native TMP_TextInfo final mesh vertices; read-only',
            complete=True, generatedText=target['name'], parsedText=target['name'], characterCount=16, requiredGlyphCount=15,
            allGlyphsGenerated=True, finiteGeometry=True, glyphsWithinRow=True, glyphsWithinNameRect=True,
            glyphsWithinClippingMasks=True, clipMaskCount=1, glyphsClearOfIconAndTimer=True,
            glyphsClearOfOtherRows=True, otherRowCount=4,
            otherRowBounds=[rectangle(0,300,70,130), rectangle(0,300,140,200),
                            rectangle(305,605,0,60), rectangle(305,605,70,130)],
            canvasRendererCulled=False, overflowMode='Overflow', overflowReported=False,
            glyphBounds=rectangle(70,200,30,55), rowBounds=rectangle(0,300,0,60), nameBounds=rectangle(60,300,30,60),
            iconBounds=rectangle(0,50,5,55), timerBounds=rectangle(80,120,0,20), timerIconBounds=rectangle(60,75,0,20),
            clippingMaskBounds=[rectangle(0,300,0,500)])
        return state

    def test_racial_buff_sheet_requires_exact_native_dormant_facts_and_control(self):
        state = self.prepare_native_buff()
        target = state['targetRow']
        guid = target['buffGuid']
        self.assertEqual([], self.errors())
        for key,value in [('buffGuid','other-buff'), ('renderedSprite','wrong'), ('spriteExact',False), ('labelExact',False),
                          ('labelTruncated',True), ('labelOverflowing',True), ('buffActive',True), ('buffTurnedOn',True),
                          ('ownerOutsideWorld',False), ('ownerTurnedOnRetained',False), ('buffCollectionInactive',False), ('nativeSectionShown',False),
                          ('nativeAlpha',0), ('nativeAlpha',True), ('nativeStateRetained',False),
                          ('nativeControlGuid',guid), ('nativeControlExact',False), ('fixtureBuffCount',4),
                          ('worldAndOriginalFactsRetained',False)]:
            with self.subTest(key=key):
                original=target[key]
                target[key]=value
                self.assertTrue(self.errors())
                target[key]=original
        for key,value in [('scrollMutationRequested',True), ('nativeApi','UnityEngine.UI.ScrollRect.normalizedPosition'),
                          ('positionApi','fabricated'), ('viewportObject','')]:
            original=state['viewport'][key]
            state['viewport'][key]=value
            self.assertTrue(self.errors())
            state['viewport'][key]=original
        state['race']='Oread'
        self.assertTrue(self.errors())

    def test_buff_overflow_flag_can_coexist_with_complete_unclipped_native_mesh(self):
        target = self.prepare_native_buff()['targetRow']
        geometry = target['labelGeometry']
        target['labelOverflowing'] = geometry['overflowReported'] = True
        geometry['nameBounds']['minY'] = 40
        geometry['glyphsWithinNameRect'] = False
        self.assertEqual([], self.errors())

    def test_buff_mesh_rejects_missing_glyphs_clipping_overlap_and_false_bounds(self):
        target = self.prepare_native_buff()['targetRow']
        original = copy.deepcopy(target['labelGeometry'])
        for key,value in [('complete',False), ('generatedText','Elemental Strik'), ('parsedText','different'),
                          ('characterCount',15), ('requiredGlyphCount',14), ('allGlyphsGenerated',False),
                          ('finiteGeometry',False), ('glyphsWithinRow',False), ('glyphsWithinClippingMasks',False),
                          ('glyphsClearOfIconAndTimer',False), ('canvasRendererCulled',True), ('overflowMode','Truncate'),
                          ('clipMaskCount',0), ('glyphsWithinNameRect',False), ('clippingMaskBounds',None)]:
            with self.subTest(key=key):
                target['labelGeometry'] = copy.deepcopy(original)
                target['labelGeometry'][key] = value
                self.assertTrue(self.errors())
        for key,edge,value in [('glyphBounds','maxX',float('nan')), ('rowBounds','maxY',54),
                               ('iconBounds','maxX',80), ('timerBounds','maxY',40), ('timerIconBounds','maxY',40),
                               ('glyphBounds','minX',250)]:
            with self.subTest(key=key,edge=edge):
                target['labelGeometry'] = copy.deepcopy(original)
                target['labelGeometry'][key][edge] = value
                self.assertTrue(self.errors())
        target['labelGeometry'] = copy.deepcopy(original)
        target['labelGeometry']['clippingMaskBounds'][0]['minY'] = 40
        self.assertTrue(self.errors())
        del target['labelGeometry']
        self.assertTrue(self.errors())

    def test_buff_unmasked_row_is_diagnostic_but_real_clips_and_neighbor_rows_are_required(self):
        target = self.prepare_native_buff()['targetRow']
        geometry = target['labelGeometry']
        geometry['rowBounds']['maxX'] = 199.8
        geometry['glyphsWithinRow'] = False
        self.assertEqual([], self.errors())
        original = copy.deepcopy(geometry)
        for key, value in [('glyphsClearOfOtherRows', False), ('otherRowCount', 3), ('otherRowBounds', []),
                           ('clippingMaskBounds', []), ('glyphsWithinRow', True)]:
            with self.subTest(key=key):
                target['labelGeometry'] = copy.deepcopy(original)
                target['labelGeometry'][key] = value
                self.assertTrue(self.errors())
        target['labelGeometry'] = copy.deepcopy(original)
        target['labelGeometry']['otherRowBounds'][0]['minY'] = 40
        self.assertTrue(self.errors())
        target['labelGeometry'] = copy.deepcopy(original)
        target['labelGeometry']['otherRowBounds'][0]['maxY'] = float('nan')
        self.assertTrue(self.errors())
        target['labelGeometry'] = copy.deepcopy(original)
        target['labelGeometry']['clippingMaskBounds'][0]['maxX'] = 199.8
        self.assertTrue(self.errors())

    def test_strategic_text_control_requires_real_source_and_unmodified_native_presentation(self):
        state = self.prepare_native_row()
        self.evidence['records'][0]['stage'] = 'native-strategic-control:0'
        state['surface'] = 'native-strategic-text-control'
        target = state['targetRow'] = dict(name='Teleport\nCaster / Wizard / 2 uses', sourceKey='destination/caster/book/0',
            spellKind='Teleport', spellGuid='82e3fb1dce1647b58d3b7169c8520af0', sourceKind='Prepared',
            bookGuid='native-book', casterId='native-caster', uses=2, labelExact=True, labelTruncated=False,
            labelOverflowing=False, nativeFont='native-font', buttonActive=True, buttonInteractable=True,
            nativeImageCount=1, canonicalSpellImages=0, nativeImageNames=['native-background'],
            nativeImagesRetained=True, nativeControlCount=2, nativeControlsRetained=True, contextAndResourcesRetained=True)
        self.assertEqual([], self.errors())
        target.update(spellKind='GreaterTeleport', spellGuid='73d19adfe18743e0a2a3a21abf4af5f3', sourceKind='Spontaneous')
        self.assertEqual([], self.errors())
        for key,value in [('spellGuid','wrong-spell'), ('sourceKind','synthetic'), ('uses',0), ('uses',True),
                          ('name','one line'), ('labelExact',False), ('labelTruncated',True), ('labelOverflowing',True),
                          ('canonicalSpellImages',1), ('canonicalSpellImages',False), ('nativeImageCount',0),
                          ('nativeImageNames',[]), ('nativeImagesRetained',False), ('nativeControlCount',0),
                          ('nativeControlCount',True), ('nativeControlsRetained',False), ('contextAndResourcesRetained',False)]:
            with self.subTest(key=key):
                original=target[key]
                target[key]=value
                self.assertTrue(self.errors())
                target[key]=original

    def test_scroll_description_requires_actual_named_item_and_icon(self):
        state = self.prepare_scroll_row()
        self.evidence['records'][0]['stage'] = 'native-scroll-description:teleport'
        state['surface'] = 'native-scroll-description'
        state['description'] = {'shown':True, 'nameExact':True, 'tooltipItemExact':True, 'tooltipDataItemExact':True, 'matchingIcons':1}
        self.assertEqual([], self.errors())
        for key,value in [('shown',False), ('nameExact',False), ('tooltipItemExact',False), ('tooltipDataItemExact',False), ('matchingIcons',0)]:
            original = state['description'][key]
            state['description'][key] = value
            self.assertTrue(self.errors())
            state['description'][key] = original

    def test_scroll_merchant_requires_real_private_stock_and_empty_trade(self):
        state = self.prepare_scroll_row()
        self.evidence['records'][0]['stage'] = 'native-scroll-merchant-row:teleport'
        state['surface'] = 'native-scroll-merchant'
        state['merchant'] = dict.fromkeys(('nativeVendorBound', 'nativeStoreBound', 'privateStock',
            'transferCollectionsEmpty', 'playerInventoryUnchanged', 'unregisteredVendor', 'controlBlueprintRetained'), True)
        state['merchant']['stockCount'] = 4
        self.assertEqual([], self.errors())
        for key in state['merchant']:
            original = state['merchant'][key]
            state['merchant'][key] = 3 if key == 'stockCount' else False
            self.assertTrue(self.errors())
            state['merchant'][key] = original
        state['surface'] = 'native-scroll-inventory'
        self.assertTrue(self.errors())

    def test_native_extended_scroll_requires_exact_component_and_visible_row(self):
        viewport = self.prepare_native_row()['viewport']
        viewport.update(nativeApi='Kingmaker.UI.Common.ScrollRectExtended.ScrollToRectCenter',
                        scrollType='Kingmaker.UI.Common.ScrollRectExtended', contentObject='Content', viewportObject='Viewport')
        self.assertEqual([], self.errors())
        for key,value in [('nativeApi','unknown'), ('scrollType','UnityEngine.UI.ScrollRect'),
                          ('contentObject',''), ('viewportObject',''), ('rowVerticallyVisible',False)]:
            original = viewport[key]
            viewport[key] = value
            self.assertTrue(self.errors())
            viewport[key] = original

    def test_inactive_axis_normalization_cannot_hide_actual_content_displacement(self):
        viewport = self.prepare_native_row()['viewport']
        viewport.update(horizontalEnabled=False, originalX=0, captureX=1,
                        originalContentX=0, originalContentY=0, restoredContentX=0, restoredContentY=0)
        self.assertEqual([], self.errors())
        for key,value in [('restoredContentX',1), ('restoredContentY',1), ('restoredContentX',float('nan'))]:
            original = viewport[key]
            viewport[key] = value
            self.assertTrue(self.errors())
            viewport[key] = original

    def test_exotic_row_requires_its_actual_learned_proficiency(self):
        state = self.prepare_native_row()
        state['targetRow'].update(name='Wakizashi', parameterGuid=None, category=0x004b4d48,
                                  acronym='WK', learnedProficiencyGuid='b14f7d9b2b665801a9d5b916c6be4ea9',
                                  proficiencyPresent=True)
        self.assertEqual([], self.errors())
        for key, value in [('learnedProficiencyGuid','93ef81404f085e2a8b261bdab15d5a08'),
                           ('learnedProficiencyGuid',None), ('proficiencyPresent',False)]:
            original = state['targetRow'][key]
            state['targetRow'][key] = value
            self.assertTrue(self.errors())
            state['targetRow'][key] = original

    def prepare_selected_fact(self):
        state = self.prepare_native_row()
        self.evidence['records'][0]['stage'] = 'native-selected-fact:Weapon Focus (Pistol)'
        target = state['targetRow']
        target.update(parameterGuid='8b39bd79d27048dda58e0a513e529f2c', expectedLetter='P', nativeGlyph='P',
                      glyphActive=True, glyphTruncated=False, glyphOverflowing=False, font='native-font',
                      nativeBackground=True, factAndParameterRetained=True, fallbackIconPresent=True,
                      otherFactRows=3, otherFactIconsExact=True)
        return state

    def test_selected_fact_requires_real_glyph_parameter_and_preserved_controls(self):
        state = self.prepare_selected_fact()
        target = state['targetRow']
        self.assertEqual([], self.errors())
        for key,value in [('nativeGlyph','WF'), ('expectedLetter','M'), ('parameterGuid','unrelated'),
                          ('featureGuid','unrelated'), ('glyphActive',False), ('glyphTruncated',True),
                          ('glyphOverflowing',True), ('font',''), ('nativeBackground',False),
                          ('factAndParameterRetained',False), ('fallbackIconPresent',False),
                          ('otherFactRows',0), ('otherFactIconsExact',False)]:
            with self.subTest(key=key):
                original = target[key]
                target[key] = value
                self.assertTrue(self.errors())
                target[key] = original
        target.update(featureGuid='070f4f07b5164d8a82d647a93539746d', parameterGuid=None)
        self.assertEqual([], self.errors())

    def test_total_layout_correction_requires_native_mode_and_sufficient_extent(self):
        state = self.prepare_selected_fact()
        ancestor = {'firearmFitApplied':True, 'fitterEnabled':True, 'originalFitterEnabled':False,
                    'originalVerticalFit':'PreferredSize', 'verticalFit':'PreferredSize',
                    'horizontalFit':'Unconstrained', 'originalHorizontalFit':'PreferredSize',
                    'height':1461, 'preferredHeight':1461}
        state['nativeLayout'] = {'targetAncestors':[ancestor]}
        self.assertEqual([], self.errors())
        for key,value in [('fitterEnabled',False), ('originalFitterEnabled',True), ('originalVerticalFit','Clamp'),
                          ('verticalFit','MinSize'), ('horizontalFit','MinSize'), ('originalHorizontalFit',None),
                          ('height',862), ('height',float('nan')), ('preferredHeight',None)]:
            with self.subTest(key=key):
                original = ancestor[key]
                ancestor[key] = value
                self.assertTrue(self.errors())
                ancestor[key] = original


if __name__ == '__main__':
    unittest.main(testRunner=unittest.TextTestRunner(stream=sys.stdout, verbosity=2))
