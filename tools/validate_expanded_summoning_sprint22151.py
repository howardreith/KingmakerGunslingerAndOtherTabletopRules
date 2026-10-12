"""Sprint 22 scope and save-identity gate; native behavior has separate witnesses."""
from __future__ import annotations
import argparse
import hashlib
import json
import re
import subprocess
import sys
from pathlib import Path
sys.dont_write_bytecode = True
import validate_expanded_summoning_sprint21150 as prior
import validate_expanded_summoning_phase1 as phase1

VERSION = "0.0.151"
INFORMATIONAL_VERSION = VERSION + "-expanded-summoning-sprint22"
BASELINE = "aa09788e6ba109c3db7bd0ec28f0138506f678a8"
ADDED = {
    "KMG.Summoning.Natural.Ankylosaurus.UnitType": "ff4df4fe2f594c37ac1c836a519efddf",
    "KMG.Summoning.Special.Ankylosaurus.Stun": "e8a085864606409ba25dc123f14d27a2",
    "KMG.Summoning.Special.Ankylosaurus.Dazed": "74654177d90e4d1d8fd512c0dabe3966",
    "KMG.Summoning.Special.Ankylosaurus.Stunned": "1f95fa2f17b042df8065a28823e09fc3",
}
CHANGED = (
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningIdentityCatalog.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningNaturalProfiles.cs",
    "src/KingmakerGunslinger/Summoning/ExpandedSummoningPteranodonViewPatch.cs",
    "src/KingmakerGunslinger/Blueprints/ExpandedSummoningNaturalBuilder.cs",
)

def validate_append(accepted, current):
    if current[:len(accepted)] != accepted:
        raise AssertionError("A released ledger identity or entry changed")
    tail = current[len(accepted):]
    if {row["symbol"]: row["guid"] for row in tail} != ADDED or len(tail) != len(ADDED):
        raise AssertionError("Only four private Ankylosaurus identities may append")
    if len({row["guid"] for row in current}) != len(current):
        raise AssertionError("Duplicate GUID")
    return sum("Ankylosaurus" in row["symbol"] for row in accepted) + int(any(row["symbol"] == "KMG.Summoning.Natural.Tail3d6" for row in accepted))

def validate_profile_isolation(old, new):
    pattern = r'                P\w*\("ankylosaurus"[\s\S]*?(?=                P\w*\("dire-bear")'
    before, n1 = re.subn(pattern, "<ANKYLOSAURUS>\n", old)
    after, n2 = re.subn(pattern, "<ANKYLOSAURUS>\n", new)
    if n1 != 1 or n2 != 1 or before != after:
        raise AssertionError("An unrelated natural profile changed")

def validate(root: Path):
    old_entries = json.loads(prior.blob(root, BASELINE, "blueprints/blueprints.json"))["entries"]
    entries = prior.document(root, "blueprints/blueprints.json")["entries"]
    protected = validate_append(old_entries, entries)
    profile = "src/KingmakerGunslinger/Summoning/ExpandedSummoningNaturalProfiles.cs"
    validate_profile_isolation(prior.blob(root, BASELINE, profile).decode("utf-8-sig"), (root/profile).read_text(encoding="utf-8-sig"))
    prior.preserved_files(root, BASELINE, prior.MASTER_PROTECTED)
    prior.preserved_files(root, BASELINE, prior.SUMMONING_PROTECTED, allowed=CHANGED)
    # Catalog, template publisher, suppression and icons stay byte-identical.
    for path in ("src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs",
                 "src/KingmakerGunslinger/Summoning/ExpandedSummoningDonorCatalog.cs",
                 "src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs",
                 "src/KingmakerGunslinger/Blueprints/ExpandedSummoningAbilityBuilder.cs"):
        if (root/path).read_bytes() != prior.blob(root, BASELINE, path):
            raise AssertionError("Released route/template/donor contract changed: " + path)
    static = prior.document(root, "validation/static-validation.json")
    historic = json.loads(prior.blob(root, BASELINE, "validation/static-validation.json"))
    for key, value in historic.items():
        if key not in ("version", "milestone") and static.get(key) != value:
            raise AssertionError("Historical release evidence changed: " + key)
    if static["version"] != VERSION or static["milestone"] != INFORMATIONAL_VERSION:
        raise AssertionError("Active version metadata mismatch")
    record = static["expandedSummoningSprint22151"]
    if record["protectedAnkylosaurusIdentities"] != protected or record["newPrivateIdentities"] != len(ADDED):
        raise AssertionError("Identity coverage metadata mismatch")
    count = len(re.findall(r'\bCase\("([^"\n]+)"', (root/"tests/KingmakerGunslinger.DomainTests/Program.cs").read_text()))
    prior.validate_shipped_bodies_reach_the_release_build(root)
    phase1.validate_package_count_owners(root, 356)
    inherited = prior.inherited
    inherited.VERSION = VERSION
    inherited.INFORMATIONAL_VERSION = INFORMATIONAL_VERSION
    inherited.PACKAGE = "KingmakerGunslinger-" + VERSION + "-local-runtime.zip"
    inherited.PACKAGE_SUFFIX = "expanded-summoning-sprint22"
    inherited.DETERMINISTIC_TEST_COUNT = count
    inherited.validate(root)
    print(f"Sprint22 source PASS: {protected} protected Ankylosaurus identities; 4 private additions; {count} registered tests (execution separately required).")

def main():
    parser=argparse.ArgumentParser();parser.add_argument("--root",type=Path,default=Path(__file__).resolve().parents[1]);args=parser.parse_args()
    try: validate(args.root)
    except (AssertionError, ValueError, KeyError) as error:
        print("Sprint22 source FAIL: "+str(error),file=sys.stderr);return 1
    return 0

if __name__=="__main__":raise SystemExit(main())
