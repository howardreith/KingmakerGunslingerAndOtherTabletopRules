"""Focused positive/refusal cases for the Sprint36 reviewed-product release seam."""
import copy
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
import expanded_summoning_manifest as manifest
import validate_expanded_summoning_sprint36_release174 as release

ROOT = Path(__file__).resolve().parents[1]

class Release174Tests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.old_info = json.loads(release.common.blob(ROOT, release.PRODUCT, "Info.json"))
        cls.old_props = release.common.blob(ROOT, release.PRODUCT, "Directory.Build.props").decode("utf-8-sig")
        cls.old_assembly = release.common.blob(ROOT, release.PRODUCT, release.ASSEMBLY).decode("utf-8-sig")
        cls.released = json.loads(release.common.blob(ROOT, release.RELEASED, "blueprints/blueprints.json"))
        cls.product = json.loads(release.common.blob(ROOT, release.PRODUCT, "blueprints/blueprints.json"))
        cls.static = json.loads(release.common.blob(ROOT, release.PRODUCT, "validation/static-validation.json"))

    def metadata(self, info=None, props=None, assembly=None):
        release.check_metadata(self.old_info,
            info if info is not None else json.loads((ROOT / "Info.json").read_text()),
            self.old_props, props if props is not None else (ROOT / "Directory.Build.props").read_text(encoding="utf-8-sig"),
            self.old_assembly, assembly if assembly is not None else (ROOT / release.ASSEMBLY).read_text(encoding="utf-8-sig"))

    def test_selected_version_only_metadata(self):
        self.metadata()

    def test_other_info_change_refused(self):
        with self.assertRaises(AssertionError):
            self.metadata(info=dict(self.old_info, Version=release.VERSION, ManagerVersion="999"))

    def test_nonversion_product_changes_refused(self):
        for path in ("src/KingmakerGunslinger/Summoning/InfernalRules36.cs",
                "assets/sprint36-demons/babau-mesh.json", "assets-source/changed.txt",
                "blueprints/blueprints.json"):
            with self.subTest(path=path), self.assertRaises(AssertionError):
                release.check_product([path], [])
        with self.assertRaises(AssertionError):
            release.check_product([], ["src/KingmakerGunslinger/Added.cs"])

    def test_version_assembly_is_the_only_product_delta(self):
        release.check_product([release.ASSEMBLY], [])

    def test_exact_product_ledger_and_append_prefix(self):
        release.check_ledger(self.released, self.product, self.product)
        changed = copy.deepcopy(self.product)
        changed["entries"][-1]["notes"] += " changed"
        with self.assertRaises(AssertionError):
            release.check_ledger(self.released, self.product, changed)

    def test_static_addition_preserves_history(self):
        current = dict(self.static, version=release.VERSION,
            milestone=release.INFORMATIONAL)
        current[release.RECORD_KEY] = release.RECORD
        release.check_static(self.static, current)
        current["unexpectedHistoricalMutation"] = True
        with self.assertRaises(AssertionError):
            release.check_static(self.static, current)

    def test_public_scope_and_route_mutations(self):
        with tempfile.TemporaryDirectory() as directory:
            fixture = Path(directory)
            folder = fixture / "src/KingmakerGunslinger/Summoning"
            folder.mkdir(parents=True)
            for name in ("ExpandedSummoningCatalog.cs", "ExpandedSummoningIdentityCatalog.cs"):
                (folder / name).write_bytes(release.common.blob(ROOT, release.PRODUCT,
                    "src/KingmakerGunslinger/Summoning/" + name))
            with patch.object(manifest, "ROOT", fixture), patch.object(
                    manifest, "CATALOG", folder / "ExpandedSummoningCatalog.cs"):
                creatures = release.parsed_creatures()
        baseline = release.released_creatures(ROOT)
        release.check_census(creatures, set(), baseline)
        with self.assertRaises(AssertionError):
            release.check_census(creatures, {"babau"}, baseline)
        for key, change in (("babau", {"monster": 6}),
                ("hezrou", {"monster": 9}), ("hezrou", {"ally": 8}),
                ("babau", {"templated": True})):
            altered = [dict(c, **change) if c["key"] == key else c for c in creatures]
            with self.subTest(key=key, change=change), self.assertRaises(AssertionError):
                release.check_census(altered, set(), baseline)

if __name__ == "__main__":
    unittest.main()
