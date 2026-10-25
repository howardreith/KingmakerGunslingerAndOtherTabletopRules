"""Focused corruption checks; no candidate, game or publication."""
import copy,json,unittest,tempfile,subprocess
from pathlib import Path
import validate_expanded_summoning_sprints3031 as v
class Boundary(unittest.TestCase):
    def test_released_asset_git_text_and_binary_rules(self):
        with tempfile.TemporaryDirectory(prefix='kmg-s3031-asset-contract-') as folder:
            root=Path(folder)
            def git(*args):return subprocess.run(['git','-C',str(root),*args],check=True,capture_output=True,text=True).stdout.strip()
            git('init','--quiet')
            (root/'.gitattributes').write_text('*.txt text\n*.bin binary\n',encoding='utf-8')
            (root/'record.txt').write_bytes(b'original\nsecond\n')
            (root/'asset.bin').write_bytes(b'\x00\x0d\x0a\xff')
            git('add','.');git('-c','user.name=Fixture','-c','user.email=fixture@invalid','commit','--quiet','-m','owned fixture')
            baseline=git('rev-parse','HEAD')
            (root/'record.txt').write_bytes(b'original\r\nsecond\r\n')
            self.assertTrue(v.released_asset_unchanged(root,'record.txt',baseline))
            self.assertTrue(v.released_asset_unchanged(root,'asset.bin',baseline))
            (root/'record.txt').write_bytes(b'changed\r\nsecond\r\n')
            self.assertFalse(v.released_asset_unchanged(root,'record.txt',baseline))
            (root/'asset.bin').write_bytes(b'\x00\x0a\xff')
            self.assertFalse(v.released_asset_unchanged(root,'asset.bin',baseline))
    def test_scope(self):
        prior=[('lantern-archon',3,False,None)]; current=prior+sorted(v.ADDITIONS)
        v.check_scope(prior,current,set())
        for bad,hidden in [(current[1:],set()),(current,{'lemure'}),(current+[('extra',9,False,None)],set()),([('lantern-archon',4,False,None)]+current[1:],set())]:
            with self.assertRaises(AssertionError):v.check_scope(prior,bad,hidden)
    def test_identities(self):
        root=Path(__file__).resolve().parents[1]
        prior=json.loads(v.common.blob(root,v.BASELINE,'blueprints/blueprints.json'))['entries']
        current=json.loads((root/'blueprints/blueprints.json').read_text(encoding='utf-8-sig'))['entries']
        v.check_tail(prior,current)
        for index in (0,len(prior)):
            bad=copy.deepcopy(current);bad[index]['guid']='0'*32
            with self.assertRaises(AssertionError):v.check_tail(prior,bad)
if __name__=='__main__':unittest.main()
