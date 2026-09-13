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


if __name__ == '__main__':
    unittest.main(testRunner=unittest.TextTestRunner(stream=sys.stdout, verbosity=2))
