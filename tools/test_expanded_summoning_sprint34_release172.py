"""Focused positive/refusal cases for the exact qualified-product release seam."""
import copy
import json
from pathlib import Path
import unittest
import tempfile
from unittest.mock import patch
import expanded_summoning_manifest as manifest
import validate_expanded_summoning_sprint34_release172 as release

ROOT=Path(__file__).resolve().parents[1]

class Release172Tests(unittest.TestCase):
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
        info=dict(self.old_info,Version=release.VERSION,ManagerVersion="999")
        with self.assertRaises(AssertionError): self.metadata(info=info)

    def test_nonversion_assembly_change_refused(self):
        assembly=(ROOT/release.ASSEMBLY).read_text(encoding="utf-8-sig")+'\n[assembly: AssemblyMetadata("extra","payload")]\n'
        with self.assertRaises(AssertionError): self.metadata(assembly=assembly)

    def test_build_property_change_refused(self):
        props=(ROOT/"Directory.Build.props").read_text().replace("<Deterministic>true","<Deterministic>false")
        with self.assertRaises(AssertionError): self.metadata(props=props)

    def test_version_file_admitted_only_under_metadata_check(self):
        release.check_product([release.ASSEMBLY],[])

    def test_product_project_asset_and_ledger_changes_refused(self):
        for path in ["src/KingmakerGunslinger/Summoning/InfernalWound34.cs","src/KingmakerGunslinger/KingmakerGunslinger.csproj","assets/sprint34-infernal-outsiders/bearded-devil-mesh.json","assets-source/changed.txt","blueprints/blueprints.json"]:
            with self.subTest(path=path),self.assertRaises(AssertionError):release.check_product([path],[])

    def test_untracked_product_refused(self):
        with self.assertRaises(AssertionError):release.check_product([],['src/KingmakerGunslinger/Added.cs'])

    def test_exact_qualified_ledger_and_append_prefix(self):
        release.check_ledger(self.released,self.qualified,self.qualified)

    def test_new_identity_mutation_refused(self):
        altered=copy.deepcopy(self.qualified);altered['entries'][-1]['notes']+=' changed'
        with self.assertRaises(AssertionError):release.check_ledger(self.released,self.qualified,altered)

    def test_released_prefix_mutation_refused(self):
        altered=copy.deepcopy(self.qualified);altered['entries'][0]['notes']+=' changed'
        with self.assertRaises(AssertionError):release.check_ledger(self.released,altered,altered)

    def test_static_addition_preserves_history(self):
        current=dict(self.static,version=release.VERSION,milestone=release.INFORMATIONAL)
        current[release.RECORD_KEY]=release.RECORD;release.check_static(self.static,current)
        current['unexpectedHistoricalMutation']=True
        with self.assertRaises(AssertionError):release.check_static(self.static,current)

    def test_qualified_census_and_hidden_refusal(self):
        # This historical release contract belongs to its recorded product,
        # not a later development roster that retains the released version.
        with tempfile.TemporaryDirectory() as directory:
            fixture=Path(directory)
            folder=fixture/"src/KingmakerGunslinger/Summoning"
            folder.mkdir(parents=True)
            for name in ("ExpandedSummoningCatalog.cs","ExpandedSummoningIdentityCatalog.cs"):
                (folder/name).write_bytes(release.common.blob(ROOT,release.PRODUCT,
                    "src/KingmakerGunslinger/Summoning/"+name))
            with patch.object(manifest,"ROOT",fixture), patch.object(manifest,"CATALOG",folder/"ExpandedSummoningCatalog.cs"):
                creatures=release.parsed_creatures()
        release.check_census(creatures,set(),release.released_creatures(ROOT))
        with self.assertRaises(AssertionError):release.check_census(creatures,{'bearded-devil'},release.released_creatures(ROOT))
        with self.assertRaises(AssertionError):release.check_census(creatures+[dict(creatures[0],key='extra')],set(),release.released_creatures(ROOT))

    def test_new_route_scope_mutations_refused(self):
        baseline=release.released_creatures(ROOT)
        creatures=release.parsed_creatures()
        for change in ({"monster":6},{"ally":5},{"templated":True}):
            altered=[dict(c,**change) if c["key"]=="kyton-chain-devil" else c for c in creatures]
            with self.subTest(change=change),self.assertRaises(AssertionError):
                release.check_census(altered,set(),baseline)

if __name__=='__main__':unittest.main()
