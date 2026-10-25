"""Focused corruption checks; no candidate, game or publication."""
import copy,json,unittest
from pathlib import Path
import validate_expanded_summoning_sprints3031 as v
class Boundary(unittest.TestCase):
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
