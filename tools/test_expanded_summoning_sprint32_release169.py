"""Focused positive/refusal cases for the exact qualified-product release seam."""
import copy
import json
from pathlib import Path
import unittest
import validate_expanded_summoning_sprint32_release169 as release

ROOT=Path(__file__).resolve().parents[1]

class Release169Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.old_info=json.loads(release.common.blob(ROOT,release.QUALIFIED,"Info.json"))
        cls.old_props=release.common.blob(ROOT,release.QUALIFIED,"Directory.Build.props").decode("utf-8-sig")
        cls.old_assembly=release.common.blob(ROOT,release.QUALIFIED,release.ASSEMBLY).decode("utf-8-sig")
        cls.released=json.loads(release.common.blob(ROOT,release.RELEASED,"blueprints/blueprints.json"))
        cls.qualified=json.loads(release.common.blob(ROOT,release.QUALIFIED,"blueprints/blueprints.json"))
        cls.static=json.loads(release.common.blob(ROOT,release.QUALIFIED,"validation/static-validation.json"))

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
        release.check_qualified_changes([release.ASSEMBLY],[])

    def test_product_project_asset_and_ledger_changes_refused(self):
        for path in ["src/KingmakerGunslinger/Summoning/LillendNoncombat32.cs","src/KingmakerGunslinger/KingmakerGunslinger.csproj","assets/sprint32-support/lillend-mesh.json","assets-source/changed.txt","blueprints/blueprints.json"]:
            with self.subTest(path=path),self.assertRaises(AssertionError):release.check_qualified_changes([path],[])

    def test_untracked_product_refused(self):
        with self.assertRaises(AssertionError):release.check_qualified_changes([],['src/KingmakerGunslinger/Added.cs'])

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

    def test_current_census_and_hidden_refusal(self):
        creatures=release.parsed_creatures();release.check_census(creatures,set())
        with self.assertRaises(AssertionError):release.check_census(creatures,{'satyr'})

if __name__=='__main__':unittest.main()
