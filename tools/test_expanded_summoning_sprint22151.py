"""Behavioral corruption fixtures for the new Sprint 22 source boundary."""
import copy
import sys
import unittest
sys.dont_write_bytecode = True
import validate_expanded_summoning_sprint22151 as gate

class Sprint22GateTests(unittest.TestCase):
    def setUp(self):
        self.old=[dict(symbol="KMG.Summoning.Unit.Ankylosaurus",guid="stable")]
        self.current=self.old+[dict(symbol=key,guid=value) for key,value in gate.ADDED.items()]
    def test_append_accepts_only_private_additions(self):
        self.assertEqual(1,gate.validate_append(self.old,self.current))
    def test_reallocation_fails(self):
        changed=copy.deepcopy(self.current);changed[0]["guid"]="moved"
        with self.assertRaises(AssertionError):gate.validate_append(self.old,changed)
    def test_extra_creature_fails(self):
        with self.assertRaises(AssertionError):gate.validate_append(self.old,self.current+[dict(symbol="Triceratops",guid="extra")])
    def test_duplicate_identity_fails(self):
        changed=copy.deepcopy(self.current);changed[-1]["guid"]="stable"
        with self.assertRaises(AssertionError):gate.validate_append(self.old,changed)
    def test_unrelated_profile_cannot_change(self):
        old='before\n                PS("ankylosaurus", "old"),\n                P("dire-bear", "stable")'
        new=old.replace('"old"','"new"')
        gate.validate_profile_isolation(old,new)
        with self.assertRaises(AssertionError):gate.validate_profile_isolation(old,new.replace('"stable"','"changed"'))
if __name__=="__main__":unittest.main()
