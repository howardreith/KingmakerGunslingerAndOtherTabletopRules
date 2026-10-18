"""Behavioral refusals for Sprint26's selected source/publication boundary."""
import copy,json,sys,tempfile,unittest
from pathlib import Path
from types import SimpleNamespace
from unittest.mock import patch
sys.dont_write_bytecode=True
import validate_expanded_summoning_sprint26160 as gate
import validate_repository as dispatch

class Sprint26BoundaryTests(unittest.TestCase):
    def setUp(self):
        self.old=[dict(symbol='KMG.Released.A',guid='a'*32,plannedType='BlueprintUnit',status='active',milestone='released',notes='preserved')]
        self.tail=[dict(symbol=s,guid=v[0],plannedType=v[1],status='active',milestone='Expanded Summoning',notes='allocated') for s,v in gate.ADDED.items()]
        self.released=[('released',1,True,1)]
        self.selected=[('gorgon',8,False,None),('bulette',None,False,6)]
    def test_exact_append_passes(self):gate.check_tail(self.old,self.old+self.tail)
    def test_reused_route_observer_body_may_change(self):
        before="prefix\n        private void Sprint24PlayerRoutes(old)\nbody\n        private void Sprint24ReadProfile(same)\nsuffix"
        gate.check_route_observer(before,before.replace("Routes(old)\nbody","Routes(new)\nnew body"))
    def test_other_released_observers_cannot_change(self):
        before="prefix\n        private void Sprint24PlayerRoutes(old)\nbody\n        private void Sprint24ReadProfile(same)\nsuffix"
        for current in [before.replace("prefix","changed"),before.replace("suffix","changed"),before.replace("ReadProfile(same)","ReadProfile(changed)")]:
            with self.assertRaises(AssertionError):gate.check_route_observer(before,current)
    def test_released_identity_fields_cannot_change(self):
        for field,value in [('guid','b'*32),('status','retired'),('plannedType','BlueprintAbility'),('notes','changed')]:
            with self.subTest(field=field):
                current=copy.deepcopy(self.old+self.tail);current[0][field]=value
                with self.assertRaises(AssertionError):gate.check_tail(self.old,current)
    def test_missing_or_extra_identity_refused(self):
        extra=dict(self.tail[0],symbol='KMG.Summoning.Unapproved',guid='f'*32)
        for current in [self.old+self.tail[:-1],self.old+self.tail+[extra]]:
            with self.assertRaises(AssertionError):gate.check_tail(self.old,current)
    def test_projectile_and_other_carrier_types_are_exact(self):
        for symbol in ['KMG.Summoning.Special.Gorgon.BreathProjectile','KMG.Summoning.Special.Gorgon.Petrification','KMG.Summoning.Special.Bulette.Leap']:
            current=copy.deepcopy(self.old+self.tail);next(x for x in current if x['symbol']==symbol)['plannedType']='BlueprintFeature'
            with self.assertRaises(AssertionError):gate.check_tail(self.old,current)
    def test_duplicate_guid_or_inactive_append_refused(self):
        for field,value in [('guid','a'*32),('status','reserved')]:
            current=copy.deepcopy(self.old+self.tail);current[-1][field]=value
            with self.assertRaises(AssertionError):gate.check_tail(self.old,current)
    def test_no_template_child_can_join_exact_tail(self):
        extra=dict(self.tail[0],symbol='KMG.Summoning.Ability.SM.Tier8.Gorgon.Celestial',guid='f'*32)
        with self.assertRaises(AssertionError):gate.check_tail(self.old,self.old+self.tail+[extra])
    def test_hidden_and_public_stages_pass(self):
        for hidden,stage in [({'gorgon','bulette'},'hidden'),(set(),'public')]:
            self.assertEqual(stage,gate.check_scope(self.released,self.released+self.selected,hidden))
    def test_no_partial_or_unrelated_suppression(self):
        for hidden in [{'gorgon'},{'bulette'},{'released'},{'gorgon','bulette','released'}]:
            with self.assertRaises(AssertionError):gate.check_scope(self.released,self.released+self.selected,hidden)
    def test_charter_families_tiers_and_untemplated_rows_are_required(self):
        for selected in [ [('gorgon',7,False,None),self.selected[1]], [('gorgon',8,True,None),self.selected[1]], [('gorgon',8,False,8),self.selected[1]], [self.selected[0],('bulette',6,False,6)], [self.selected[0],('bulette',None,False,7)] ]:
            with self.assertRaises(AssertionError):gate.check_scope(self.released,self.released+selected,set())
    def test_prior_creatures_and_order_cannot_change(self):
        for current in [self.selected,self.released+self.selected+[('griffon',4,False,None)], [('released',2,True,1)]+self.selected]:
            with self.assertRaises(AssertionError):gate.check_scope(self.released,current,set())
    def test_actual_python_dispatch_selects_sprint26_and_passes_fixture_root(self):
        with tempfile.TemporaryDirectory() as temporary:
            root=Path(temporary);(root/'Info.json').write_text(json.dumps(dict(Version='0.0.160')),encoding='utf-8')
            with patch.object(sys,'argv',['validator','--root',str(root)]),patch.object(dispatch.subprocess,'run',return_value=SimpleNamespace(returncode=0)) as child:
                self.assertEqual(0,dispatch.main())
                command=child.call_args.args[0]
                self.assertEqual('validate_expanded_summoning_sprint26160.py',Path(command[1]).name)
                self.assertEqual(['--root',str(root.resolve())],command[-2:])
    def test_dispatch_propagates_real_selected_child_failure(self):
        with tempfile.TemporaryDirectory() as temporary:
            root=Path(temporary);(root/'Info.json').write_text(json.dumps(dict(Version='0.0.160')),encoding='utf-8')
            with patch.object(sys,'argv',['validator','--root',str(root)]),patch.object(dispatch.subprocess,'run',return_value=SimpleNamespace(returncode=7)):
                self.assertEqual(7,dispatch.main())
if __name__=='__main__':unittest.main()
