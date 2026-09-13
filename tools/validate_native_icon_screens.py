"""Verify native capture provenance and completeness; never infer visual approval."""
import argparse
import hashlib
import json
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
