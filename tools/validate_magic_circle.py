"""Additive Magic Circle candidate gates; no runtime or artwork approval claim."""
import hashlib
import json

def validate(root):
    validate_protection_source(root)
    entries = json.loads((root / "blueprints/blueprints.json").read_text(encoding="utf-8"))["entries"]
    prefix = hashlib.sha256(json.dumps(entries[:1886], sort_keys=True, separators=(",", ":")).encode()).hexdigest()
    if prefix != PREFIX_SHA256:
        raise AssertionError("Published 0.0.132 blueprint identities changed")
    circle = entries[1886:1886 + len(IDS)]
    if {entry["symbol"]: entry["guid"] for entry in circle} != IDS or len(circle) != len(IDS):
        raise AssertionError("Magic Circle stable identities drifted")
    if any(entry["status"] != "active" for entry in circle):
        raise AssertionError("Magic Circle identities must remain available for hydration")
    # Nothing may follow the Magic Circle block except the exact identities a
    # later, separately validated feature authorizes (none by default).
    appended = entries[1886 + len(IDS):]
    if [(entry["symbol"], entry["guid"]) for entry in appended] != list(AUTHORIZED_APPENDED):
        raise AssertionError("Unauthorized identities follow the Magic Circle block")
    configuration = (root / "src/KingmakerGunslinger/FeatureModules/FeatureModuleConfiguration.cs").read_text(encoding="utf-8")
    if 'MagicCircleSpellsId = "magic-circle-spells"' not in configuration or 'ModuleCount = 13' not in configuration:
        raise AssertionError("Magic Circle content setting is missing")

PREFIX_SHA256 = '6bca47548288109033edc295f464468eec390409487e8ddd8f83449bef5aa6bd'
# Exact ordered (symbol, guid) pairs a later validator appends after this block.
AUTHORIZED_APPENDED = ()
IDS = {'KMG.Spells.MagicCircle.Evil.Ability': 'ec63a7d4606a467f948ba03c19cfa42d', 'KMG.Spells.MagicCircle.Evil.Carrier': '40d23af78f45430faf881a9a3259efe0', 'KMG.Spells.MagicCircle.Evil.Area': 'fe3bfffe4e13459587cb11280226d2df', 'KMG.Spells.MagicCircle.Evil.Recipient': '753de0df0b3c49a0982e37981ddee018'}

IDS.update({'KMG.Spells.MagicCircle.Evil.TouchDelivery': '1242a0687d374c1792629384fc716ad5', 'KMG.Spells.MagicCircle.Evil.Scroll': '04954b1da0964fc288e00abdcf1f82ae', 'KMG.Spells.MagicCircle.Good.Ability': '8d9b3a1b06c141518230c6dd8aae95ee', 'KMG.Spells.MagicCircle.Good.Carrier': 'b533bfe6409a457d9290dde8b18066ac', 'KMG.Spells.MagicCircle.Good.Area': '4b6d501574c74186b42e583a6b26a6da', 'KMG.Spells.MagicCircle.Good.Recipient': 'b81680c923aa4ec59b46d12fdb5084fa', 'KMG.Spells.MagicCircle.Good.TouchDelivery': 'b05e3a50c1e147c2badb7bbc49ae6e5e', 'KMG.Spells.MagicCircle.Good.Scroll': 'f8ef52b288bc4c369c51a1fa754db605', 'KMG.Spells.MagicCircle.Law.Ability': 'd98b1dff1952451989943907617f5b0d', 'KMG.Spells.MagicCircle.Law.Carrier': 'ab3b489192dd40e192bc3bac47e816cb', 'KMG.Spells.MagicCircle.Law.Area': '43ec68312894436db1dc0928118e9678', 'KMG.Spells.MagicCircle.Law.Recipient': 'b17bf64cf088447d87d1258a5c052e38', 'KMG.Spells.MagicCircle.Law.TouchDelivery': 'edcb7eba83cf499a97d8c4ba6d1bfd35', 'KMG.Spells.MagicCircle.Law.Scroll': 'c6342ebc91c54c82a43f985aa8e225bd', 'KMG.Spells.MagicCircle.Chaos.Ability': 'cafa8a0681b84b00ad4f0e3ff6393c78', 'KMG.Spells.MagicCircle.Chaos.Carrier': '1aae3d67cf06435a959474c4cd0b7796', 'KMG.Spells.MagicCircle.Chaos.Area': 'd4dd789baf954c0e97d2d9da8404696a', 'KMG.Spells.MagicCircle.Chaos.Recipient': '1ee6f15bf4cc42f8ac4c1eb20e30f71f', 'KMG.Spells.MagicCircle.Chaos.TouchDelivery': 'e9c92183bd7f4600adab27edbe68d04b', 'KMG.Spells.MagicCircle.Chaos.Scroll': '4c7d27edc91246f5b3557ca480af941c'})

# User-authorized shared correction after native public AddBuff source-loss
# regression. No catalog, alignment predicate, publication or trusted metadata
# change is authorized by these two exact source edits.
PROTECTION_SOURCE_SHA256 = "3059918f8b61f11ccaf4a1ab5fb43c3f2b95c43edd4bc195a7c5b6661fe4cf47"
PROTECTION_RETAINED_SHA256 = "c2bf1300d055ef89508134cc126b1a535a8e757755cccd6bade5923d6a8150d3"
PROTECTION_EDITS = {
    "ProtectionFromAlignmentControlImmunityComponent.cs": "3c0971b2c545073d42631e7356f5a4006ba58faeaae7cb9cdfc20b24549a9ee2",
    "ProtectionFromAlignmentRuntime.cs": "841671eaf6761a827c684616de15c71973a2e2f5c28381c15dc9b0cdb9ac5c98",
}

def validate_protection_source(root):
    retained, complete = hashlib.sha256(), hashlib.sha256()
    found = set()
    for path in sorted((root / "src/KingmakerGunslinger/Spells/ProtectionFromAlignment").glob("*.cs")):
        if path.name == "ProtectionFromAlignmentDescriptions.cs":
            continue
        data = path.read_bytes().replace(b"\r\n", b"\n")
        record = path.relative_to(root).as_posix().encode() + b"\0" + data
        complete.update(record)
        if path.name in PROTECTION_EDITS:
            found.add(path.name)
            if hashlib.sha256(data).hexdigest() != PROTECTION_EDITS[path.name]:
                raise AssertionError("Unreviewed shared Protection source edit: " + path.name)
        else:
            retained.update(record)
    if found != set(PROTECTION_EDITS) or retained.hexdigest() != PROTECTION_RETAINED_SHA256:
        raise AssertionError("Unchanged Protection catalog/policy/publication source drifted")
    if complete.hexdigest() != PROTECTION_SOURCE_SHA256:
        raise AssertionError("Shared Protection source digest differs from the exact correction")

# Follow-up adds only these three grouped learning identities.
IDS.update({'KMG.Spells.MagicCircle.Family': '30201ad172e243c9b13fef377dcc6ce6', 'KMG.Spells.MagicCircle.PaladinFamily': 'bec9d1f0d3de453cb9733a614a64cfca', 'KMG.Spells.MagicCircle.AntipaladinFamily': '2c16f86ef5cd4b81aee6eff208a71579'})
