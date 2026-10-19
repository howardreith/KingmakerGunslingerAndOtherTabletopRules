"""Offline source-boundary refusals; no native or release qualification."""
import copy,json,sys,tempfile,unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import patch
sys.dont_write_bytecode=True
import validate_expanded_summoning_sprint27161 as gate
import validate_repository as dispatch
class GriffonBoundaryTests(unittest.TestCase):
    def setUp(self):
        self.old=[dict(symbol="KMG.Released.A",guid="a"*32,plannedType="BlueprintUnit",status="active",milestone="released",notes="preserved")]
        self.tail=[dict(symbol=s,guid=v[0],plannedType=v[1],status="active",milestone="Sprint27",notes="allocated") for s,v in gate.ADDED.items()]
        self.released=[("released",1,True,1)]
        self.selected=[("griffon",None,False,4)]
    def test_exact_allocations_pass(self):gate.check_tail(self.old,self.old+self.tail)
    def test_prefix_is_immutable(self):
        for field,value in [("guid","b"*32),("status","retired"),("plannedType","BlueprintAbility"),("notes","changed")]:
            current=copy.deepcopy(self.old+self.tail);current[0][field]=value
            with self.assertRaises(AssertionError):gate.check_tail(self.old,current)
    def test_missing_extra_or_template_identity_refused(self):
        extra=dict(self.tail[0],symbol="KMG.Summoning.Ability.SM.Tier4.Griffon.Celestial",guid="f"*32)
        for current in [self.old+self.tail[:-1],self.old+self.tail+[extra]]:
            with self.assertRaises(AssertionError):gate.check_tail(self.old,current)
    def test_each_new_guid_type_and_status_are_pinned(self):
        for index in range(len(self.tail)):
            for field,value in [("guid","0"*32),("plannedType","WrongType"),("status","reserved")]:
                current=copy.deepcopy(self.old+self.tail);current[len(self.old)+index][field]=value
                with self.assertRaises(AssertionError):gate.check_tail(self.old,current)
    def test_hidden_and_public_stages_pass(self):
        for hidden,expected in [({"griffon"},"hidden"),(set(),"public")]:self.assertEqual(expected,gate.check_scope(self.released,self.released+self.selected,hidden))
    def test_only_griffon_suppression_is_permitted(self):
        for hidden in [{"tiger"},{"griffon","tiger"}]:
            with self.assertRaises(AssertionError):gate.check_scope(self.released,self.released+self.selected,hidden)
    def test_printed_family_tier_no_template_are_exact(self):
        for selected in [[("griffon",4,False,4)],[("griffon",None,True,4)],[("griffon",None,False,5)],self.selected*2,[]]:
            with self.assertRaises(AssertionError):gate.check_scope(self.released,self.released+selected,set())
    def test_released_rows_order_and_scope_are_preserved(self):
        for current in [self.selected,[("released",2,True,1)]+self.selected,self.released+self.selected+[("hippogriff",None,False,4)]]:
            with self.assertRaises(AssertionError):gate.check_scope(self.released,current,set())
    def test_active_dispatch_and_actual_child_exit(self):
        for code in [0,7]:
            with tempfile.TemporaryDirectory() as temporary:
                root=Path(temporary);(root/"Info.json").write_text(json.dumps(dict(Version="0.0.161")),encoding="utf-8")
                with patch.object(sys,"argv",["validator","--root",str(root)]),patch.object(dispatch.subprocess,"run",return_value=SimpleNamespace(returncode=code)) as child:
                    self.assertEqual(code,dispatch.main());command=child.call_args.args[0]
                    self.assertEqual("validate_expanded_summoning_sprint27161.py",Path(command[1]).name)
                    self.assertEqual(["--root",str(root.resolve())],command[-2:])
if __name__=="__main__":unittest.main()
