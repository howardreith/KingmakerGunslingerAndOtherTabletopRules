"""Intake corruption tests use metadata only; no image is authored or copied."""
import unittest
from pathlib import Path
import inspect_elemental_character_trait_icons as gate

class ElementalIconIntakeTests(unittest.TestCase):
    def setUp(self):
        self.key="fiery-glare"
        p=gate.PRODUCTION
        self.record={"source":p+"sources/fiery-glare.png","export":p+"exports/fiery-glare.png","brief":p+"briefs/fiery-glare.json","sourceSha256":"a"*64,"exportSha256":"b"*64,"approvedHash":"b"*64,"visualStatus":"approved"}
        self.concept={"family":"painted-magical","exportProfile":"project-painted-128","reviewGroup":"ifrit-traits","assetAuthority":{"manifest":gate.icons.PRODUCTION,"key":self.key},"semanticBrief":self.record["brief"],"runtimeExport":{"path":"assets/game/icons/fiery-glare.png","installedPath":"assets/icons/fiery-glare.png","cacheKey":self.key},"visualReview":{"status":"approved","reviewedExportSha256":"b"*64,"evidence":"owner-review-record.md"}}
        self.brief={"key":self.key,"source":self.record["source"],"sourceSha256":"a"*64,"tool":"hypothetical authorized original-art workflow","prompt":"hypothetical original composition metadata"}
    def errors(self):
        return gate.metadata_errors(self.key,"ifrit-traits",self.concept,self.record,self.brief)
    def test_complete_metadata_does_not_qualify_an_image(self):
        self.assertEqual([],self.errors())
        report=gate.inspect(Path(__file__).resolve().parents[1])
        self.assertEqual("BLOCKED-ONLY-ON-ORIGINAL-ICONS",report["AssetReadiness"])
        self.assertFalse(report["TraitsPublished"])
    def test_donor_path_rejected(self):
        self.record["source"]="assets/native/donor.png"
        self.assertTrue(self.errors())
    def test_wrong_profile_rejected(self):
        self.concept["exportProfile"]="native-monogram"
        self.assertTrue(self.errors())
    def test_wrong_race_review_group_rejected(self):
        self.concept["reviewGroup"]="feats"
        self.assertTrue(self.errors())
    def test_approval_hash_rejected(self):
        self.record["approvedHash"]="c"*64
        self.assertTrue(self.errors())
    def test_owner_review_evidence_mandatory(self):
        self.concept["visualReview"]["evidence"]=""
        self.assertTrue(self.errors())
    def test_unapproved_status_rejected(self):
        self.concept["visualReview"]["status"]="pending"
        self.assertTrue(self.errors())
    def test_draft_is_not_provenance(self):
        self.brief["sourceSha256"]=None
        self.brief["tool"]=None
        self.assertTrue(self.errors())
    def test_competing_authority_rejected(self):
        self.concept["assetAuthority"]["manifest"]="another-manifest.json"
        self.assertTrue(self.errors())
    def test_runtime_export_exact(self):
        self.concept["runtimeExport"]["path"]=self.record["source"]
        self.assertTrue(self.errors())
    def test_current_four_missing(self):
        report=gate.inspect(Path(__file__).resolve().parents[1])
        self.assertTrue(report["existingIconContractPass"])
        self.assertEqual(list(gate.TRAITS),[r["concept"] for r in report["icons"]])
        for row in report["icons"]:
            self.assertEqual("MISSING",row["qualification"])
            self.assertIsNone(row["sourceSha256"])
            self.assertEqual(0,row["productionRecordCount"])
    def test_drafts_never_promote_the_active_catalog(self):
        root=Path(__file__).resolve().parents[1]
        catalog=gate.icons.read_json(root,gate.icons.CATALOG)
        for key in gate.TRAITS:
            brief=gate.icons.read_json(root,gate.PRODUCTION+"briefs/"+key+".json")
            self.assertTrue(brief["draft"])
            self.assertIsNone(brief["sourceSha256"])
            self.assertIsNone(brief["tool"])
            self.assertNotIn(key,{c["key"] for c in catalog["concepts"]})

if __name__=="__main__":
    unittest.main()
