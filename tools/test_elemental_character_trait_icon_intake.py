"""Exact original-art admission corruption tests; owner approval remains separate."""
import copy
import unittest
from pathlib import Path
import inspect_elemental_character_trait_icons as gate

class ElementalIconIntakeTests(unittest.TestCase):
    def setUp(self):
        self.root=Path(__file__).resolve().parents[1]
        self.key="fiery-glare"
        catalog=gate.icons.read_json(self.root,gate.icons.CATALOG)
        production=gate.icons.read_json(self.root,gate.icons.PRODUCTION)
        self.record=copy.deepcopy(next(r for r in production["records"] if r["key"]==self.key))
        self.concept=copy.deepcopy(next(c for c in catalog["concepts"] if c["key"]==self.key))
        self.brief=copy.deepcopy(gate.icons.read_json(self.root,self.record["brief"]))
    def errors(self):
        return gate.metadata_errors(self.key,"ifrit-traits",self.concept,self.record,self.brief)
    def test_ready_catalog_requires_real_files(self):
        self.assertEqual([],self.errors())
        report=gate.inspect(self.root)
        self.assertEqual("READY",report["AssetReadiness"])
        self.assertTrue(report["existingIconContractPass"])
        self.assertEqual(4,len({r["sourceSha256"] for r in report["icons"]}))
        self.assertEqual(4,len({r["exportSha256"] for r in report["icons"]}))
    def test_donor_path_rejected(self):
        self.record["source"]="assets/native/donor.png"
        self.assertTrue(self.errors())
    def test_wrong_profile_rejected(self):
        self.concept["exportProfile"]="native-monogram"
        self.assertTrue(self.errors())
    def test_wrong_race_review_group_rejected(self):
        self.concept["reviewGroup"]="feats"
        self.assertTrue(self.errors())
    def test_stale_export_admission_rejected(self):
        self.record["exportSha256"]="c"*64
        self.assertTrue(self.errors())
    def test_objective_evidence_mandatory(self):
        self.concept["technicalReview"]["evidence"]=""
        self.assertTrue(self.errors())
    def test_unqualified_objective_status_rejected(self):
        self.concept["technicalReview"]["status"]="FAIL"
        self.assertTrue(self.errors())
    def test_draft_is_not_provenance(self):
        self.brief["draft"]=True
        self.assertTrue(self.errors())
    def test_competing_authority_rejected(self):
        self.concept["assetAuthority"]["manifest"]="another-manifest.json"
        self.assertTrue(self.errors())
    def test_runtime_export_exact(self):
        self.concept["runtimeExport"]["path"]=self.record["source"]
        self.assertTrue(self.errors())
    def test_owner_approval_cannot_be_inferred(self):
        self.record["approvedHash"]=self.record["exportSha256"]
        self.assertTrue(self.errors())
    def test_all_four_have_exact_unapproved_original_lineage(self):
        report=gate.inspect(self.root)
        self.assertEqual(list(gate.TRAITS),[r["concept"] for r in report["icons"]])
        for row in report["icons"]:
            self.assertEqual("QUALIFIED-ORIGINAL-ICON-PRESENT",row["qualification"])
            brief=gate.icons.read_json(self.root,row["expectedProvenanceBrief"])
            self.assertFalse(brief["draft"])
            self.assertTrue(brief["tool"] and brief["sourceSha256"] and brief["prompt"])
            self.assertEqual("NOT_RECORDED",brief["ownerAestheticApproval"])
            self.assertEqual([1254,1254],row["sourceSize"])
            self.assertEqual([128,128],row["exportSize"])

if __name__=="__main__":
    unittest.main()
