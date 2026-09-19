"""Additive Magic Circle candidate gates; no runtime or artwork approval claim."""
import hashlib
import json

def validate(root):
    entries = json.loads((root / "blueprints/blueprints.json").read_text(encoding="utf-8"))["entries"]
    prefix = hashlib.sha256(json.dumps(entries[:1886], sort_keys=True, separators=(",", ":")).encode()).hexdigest()
    if prefix != PREFIX_SHA256:
        raise AssertionError("Published 0.0.132 blueprint identities changed")
    circle = entries[1886:]
    if {entry["symbol"]: entry["guid"] for entry in circle} != IDS or len(circle) != len(IDS):
        raise AssertionError("Magic Circle stable identities drifted")
    if any(entry["status"] != "active" for entry in circle):
        raise AssertionError("Magic Circle identities must remain available for hydration")
    configuration = (root / "src/KingmakerGunslinger/FeatureModules/FeatureModuleConfiguration.cs").read_text(encoding="utf-8")
    if 'MagicCircleSpellsId = "magic-circle-spells"' not in configuration or 'ModuleCount = 13' not in configuration:
        raise AssertionError("Magic Circle content setting is missing")

PREFIX_SHA256 = '6bca47548288109033edc295f464468eec390409487e8ddd8f83449bef5aa6bd'
IDS = {'KMG.Spells.MagicCircle.Evil.Ability': 'ec63a7d4606a467f948ba03c19cfa42d', 'KMG.Spells.MagicCircle.Evil.Carrier': '40d23af78f45430faf881a9a3259efe0', 'KMG.Spells.MagicCircle.Evil.Area': 'fe3bfffe4e13459587cb11280226d2df', 'KMG.Spells.MagicCircle.Evil.Recipient': '753de0df0b3c49a0982e37981ddee018'}
