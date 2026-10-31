"""Focused positive/refusal cases for the Sprint35 exact qualified-product release seam."""
import copy
import json
from pathlib import Path
import unittest
import tempfile
from unittest.mock import patch
import expanded_summoning_manifest as manifest
import validate_expanded_summoning_sprint35_release173 as release

ROOT=Path(__file__).resolve().parents[1]

class Release173Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.old_info=json.loads(release.common.blob(ROOT,release.PRODUCT,"Info.json"))
        cls.old_props=release.common.blob(ROOT,release.PRODUCT,"Directory.Build.props").decode("utf-8-sig")
        cls.old_assembly=release.common.blob(ROOT,release.PRODUCT,release.ASSEMBLY).decode("utf-8-sig")
        cls.released=json.loads(release.common.blob(ROOT,release.RELEASED,"blueprints/blueprints.json"))
        cls.qualified=json.loads(release.common.blob(ROOT,release.PRODUCT,"blueprints/blueprints.json"))
        cls.static=json.loads(release.common.blob(ROOT,release.PRODUCT,"validation/static-validation.json"))

    def metadata(self, *, info=None, props=None, assembly=None):
        release.check_metadata(self.old_info,info if info is not None else json.loads((ROOT/"Info.json").read_text()),
            self.old_props,props if props is not None else (ROOT/"Directory.Build.props").read_text(encoding="utf-8-sig"),
            self.old_assembly,assembly if assembly is not None else (ROOT/release.ASSEMBLY).read_text(encoding="utf-8-sig"))

    def test_selected_version_only_metadata(self): self.metadata()

    def test_other_info_change_refused(self):
        with self.assertRaises(AssertionError): self.metadata(info=dict(self.old_info,Version=release.VERSION,ManagerVersion="999"))

    def test_nonversion_assembly_change_refused(self):
        with self.assertRaises(AssertionError): self.metadata(assembly=(ROOT/release.ASSEMBLY).read_text(encoding="utf-8-sig")+'\n[assembly: AssemblyMetadata("extra","payload")]\n')

    def test_build_property_change_refused(self):
        with self.assertRaises(AssertionError): self.metadata(props=(ROOT/"Directory.Build.props").read_text().replace("<Deterministic>true","<Deterministic>false"))

    def test_version_file_admitted_only_under_metadata_check(self): release.check_product([release.ASSEMBLY],[])

    def test_product_project_asset_and_ledger_changes_refused(self):
        for path in ["src/KingmakerGunslinger/Summoning/InfernalWall35.cs","src/KingmakerGunslinger/KingmakerGunslinger.csproj","assets/sprint35-infernal-reach/barbed-devil-mesh.json","assets-source/changed.txt","blueprints/blueprints.json"]:
            with self.subTest(path=path),self.assertRaises(AssertionError):release.check_product([path],[])

    def test_untracked_product_refused(self):
        with self.assertRaises(AssertionError):release.check_product([],['src/KingmakerGunslinger/Added.cs'])

    def test_exact_qualified_ledger_and_append_prefix(self): release.check_ledger(self.released,self.qualified,self.qualified)

    def test_new_identity_mutation_refused(self):
        altered=copy.deepcopy(self.qualified);altered['entries'][-1]['notes']+=' changed'
        with self.assertRaises(AssertionError):release.check_ledger(self.released,self.qualified,altered)

    def test_released_prefix_mutation_refused(self):
        altered=copy.deepcopy(self.qualified);altered['entries'][0]['notes']+=' changed'
        with self.assertRaises(AssertionError):release.check_ledger(self.released,altered,altered)

    def test_static_addition_preserves_history(self):
        current=dict(self.static,version=release.VERSION,milestone=release.INFORMATIONAL);current[release.RECORD_KEY]=release.RECORD
        release.check_static(self.static,current);current['unexpectedHistoricalMutation']=True
        with self.assertRaises(AssertionError):release.check_static(self.static,current)

    def test_qualified_census_and_hidden_refusal(self):
        with tempfile.TemporaryDirectory() as directory:
            fixture=Path(directory);folder=fixture/"src/KingmakerGunslinger/Summoning";folder.mkdir(parents=True)
            for name in ("ExpandedSummoningCatalog.cs","ExpandedSummoningIdentityCatalog.cs"):
                (folder/name).write_bytes(release.common.blob(ROOT,release.PRODUCT,"src/KingmakerGunslinger/Summoning/"+name))
            with patch.object(manifest,"ROOT",fixture),patch.object(manifest,"CATALOG",folder/"ExpandedSummoningCatalog.cs"):
                creatures=release.parsed_creatures()
        release.check_census(creatures,set(),release.released_creatures(ROOT))
        with self.assertRaises(AssertionError):release.check_census(creatures,{'barbed-devil'},release.released_creatures(ROOT))
        with self.assertRaises(AssertionError):release.check_census(creatures+[dict(creatures[0],key='extra')],set(),release.released_creatures(ROOT))

    def test_new_route_scope_mutations_refused(self):
        baseline=release.released_creatures(ROOT);creatures=release.parsed_creatures()
        for key,change in (("barbed-devil",{"monster":9}),("bone-devil",{"monster":8}),("bone-devil",{"ally":7}),("barbed-devil",{"templated":True})):
            altered=[dict(c,**change) if c["key"]==key else c for c in creatures]
            with self.subTest(key=key,change=change),self.assertRaises(AssertionError): release.check_census(altered,set(),baseline)

if __name__=='__main__':unittest.main()
