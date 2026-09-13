"""Focused corruption fixtures for the icon authoring gate; never mutate originals."""
import copy
import sys
import unittest
from pathlib import Path
sys.dont_write_bytecode = True
from validate_icon_catalog import CATALOG, PILOT, PRODUCTION, REFERENCES, read_json, validate, presentation_delta_matches, production_brief_errors
ROOT = Path(__file__).resolve().parents[1]

class IconCatalogTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.catalog = read_json(ROOT, CATALOG)
        cls.pilot = read_json(ROOT, PILOT)
        cls.production = read_json(ROOT, PRODUCTION)
        cls.references = read_json(ROOT, REFERENCES)

    def rejects(self, phrase, **documents):
        errors = validate(ROOT, **documents)
        self.assertTrue(any(phrase in error for error in errors), errors)

    def test_current_candidate_is_technically_consistent(self):
        self.assertEqual([], validate(ROOT))

    def test_omitted_consumer_is_not_silently_hidden(self):
        catalog = copy.deepcopy(self.catalog)
        catalog["consumers"] = [c for c in catalog["consumers"]
            if c["symbol"] != "KMG.ElementalRaces.Traits.Undine.NereidFascination.ShakeFreeAbility"]
        self.rejects("Missing consumer:", catalog=catalog)

    def test_new_registered_target_requires_disposition(self):
        registry = read_json(ROOT, "blueprints/blueprints.json")
        registry["entries"].append({"symbol": "KMG.ElementalRaces.Tests.NewVisibleChoice",
            "guid": "1234567890abcdef1234567890abcdef", "plannedType": "BlueprintAbility"})
        self.rejects("Missing consumer:", registry=registry)

    def test_mismatched_export_is_rejected(self):
        pilot = copy.deepcopy(self.pilot)
        pilot["records"][0]["exportSha256"] = "0" * 64
        self.rejects("Hash mismatch:", pilot=pilot)

    def test_stale_protected_hash_is_rejected(self):
        catalog = copy.deepcopy(self.catalog)
        catalog["protectedFiles"][0]["sha256"] = "0" * 64
        self.rejects("Hash mismatch:", catalog=catalog)

    def test_approval_needs_exact_pixels_and_owner_evidence(self):
        catalog = copy.deepcopy(self.catalog)
        record = self.pilot["records"][0]
        concept = next(c for c in catalog["concepts"] if c["key"] == record["key"])
        concept["visualReview"] = {"status": "approved",
            "reviewedExportSha256": "0" * 64, "evidence": None}
        self.rejects("Stale visual approval hash:", catalog=catalog)
        self.rejects("Approval lacks owner evidence:", catalog=catalog)

    def test_distinct_actions_cannot_hide_identical_art(self):
        pilot = copy.deepcopy(self.pilot)
        parent = next(r for r in pilot["records"] if r["key"] == "hydraulic-maneuver")
        child = next(r for r in pilot["records"] if r["key"] == "hydraulic-trip")
        for field in ["export", "exportSha256", "exportSize"]:
            child[field] = parent[field]
        self.rejects("Distinct concepts duplicate artwork:", pilot=pilot)

    def test_manifest_approval_cannot_follow_changed_pixels(self):
        pilot = copy.deepcopy(self.pilot)
        pilot["records"][0]["approvedHash"] = "0" * 64
        self.rejects("Stale manifest approval hash:", pilot=pilot)

    def test_production_is_not_approved_by_family_direction(self):
        production = copy.deepcopy(self.production)
        production["records"][0]["visualStatus"] = "approved"
        production["records"][0]["approvedHash"] = production["records"][0]["exportSha256"]
        self.rejects("Pilot approval not recorded in catalog:", production=production)

    def test_missing_production_record_leaves_unresolved_authority(self):
        production = copy.deepcopy(self.production)
        production["records"].pop()
        self.rejects("Unresolved asset authority:", production=production)

    def test_completed_scope_cannot_drop_both_record_and_authority(self):
        catalog = copy.deepcopy(self.catalog)
        production = copy.deepcopy(self.production)
        removed = production["records"].pop()
        next(c for c in catalog["concepts"] if c["key"] == removed["key"])["assetAuthority"] = None
        self.rejects("Completed art scope has missing candidate:", catalog=catalog, production=production)

    def test_creative_brief_cannot_omit_a_real_ui_surface(self):
        record = next(r for r in self.production["records"] if r["key"] == "unerring-weapon-primary")
        brief = read_json(ROOT, record["brief"])
        brief["uiSurfaces"] = []
        concepts = {c["key"]: c for c in self.catalog["concepts"]}
        errors = production_brief_errors(brief, concepts[record["key"]], self.catalog["consumers"], concepts)
        self.assertTrue(any("UI surfaces mismatch" in e for e in errors), errors)

    def test_creative_brief_requires_real_confusable_siblings(self):
        record = next(r for r in self.production["records"] if r["key"] == "cloud-gazer")
        brief = read_json(ROOT, record["brief"])
        brief["confusedWith"] = ["invented-content"]
        concepts = {c["key"]: c for c in self.catalog["concepts"]}
        errors = production_brief_errors(brief, concepts[record["key"]], self.catalog["consumers"], concepts)
        self.assertTrue(any("confusion comparisons" in e for e in errors), errors)

    def test_pilot_exporter_cannot_erase_catalog_approval(self):
        pilot = copy.deepcopy(self.pilot)
        pilot["records"][0]["visualStatus"] = "awaiting-owner-pilot-review"
        pilot["records"][0]["approvedHash"] = None
        self.rejects("Catalog approval not frozen in manifest:", pilot=pilot)

    def test_saved_parameter_guid_cannot_drift(self):
        catalog = copy.deepcopy(self.catalog)
        catalog["uiEntries"][0]["parameterGuid"] = "0" * 32
        self.rejects("UI parameter identity mismatch:", catalog=catalog)

    def test_presentation_exception_preserves_all_other_code(self):
        delta = self.catalog["authorizedPresentationDeltas"][0]
        text = (ROOT/delta["path"]).read_text(encoding="utf-8")
        self.assertTrue(presentation_delta_matches(text, delta))
        mutated = text.replace("result.OrderBy(", "result.OrderByDescending(")
        self.assertNotEqual(text, mutated)
        self.assertFalse(presentation_delta_matches(mutated, delta))

if __name__ == "__main__":
    # PowerShell 5 wraps native stderr as NativeCommandError when logs redirect
    # streams. Ordinary passing test progress belongs on stdout; failures still
    # produce unittest's nonzero process exit code.
    unittest.main(testRunner=unittest.TextTestRunner(stream=sys.stdout, verbosity=2))
