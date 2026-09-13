"""Compare guarded live icon census with canonical authoring and artifact identities."""
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
from validate_icon_catalog import CATALOG, read_json

ROOT = Path(__file__).resolve().parents[1]

def stable(value):
    """Compare content across processes while retaining identity checks within each run."""
    if isinstance(value, dict):
        return {k: stable(v) for k, v in value.items()
                if k not in {"instanceId", "spriteInstanceId", "textureInstanceId"}}
    if isinstance(value, list):
        return [stable(v) for v in value]
    return value

def compare_control(candidate, control):
    errors = []
    def require(condition, message):
        if not condition:
            errors.append(message)
    require(candidate["controlRun"] is False and control["controlRun"] is True and
            candidate["runId"] != control["runId"], "Two distinct candidate/control runs are required")
    require(candidate["profile"] == control["profile"], "Candidate/control module profiles differ")
    for key in ("loadedModuleSha256", "moduleVersionId"):
        require(candidate["runtimeIdentity"][key] == control["runtimeIdentity"][key], "Candidate/control executing artifact differs: " + key)
    a = {r["symbol"]: r for r in candidate["inventory"]}
    b = {r["symbol"]: r for r in control["inventory"]}
    require(a.keys() == b.keys(), "Candidate/control inventory differs")
    for symbol in a.keys() & b.keys():
        left, right = a[symbol], b[symbol]
        require((left["guid"], left["type"]) == (right["guid"], right["type"]), "Control identity differs: " + symbol)
        if left.get("registrationKind") != "blueprint-library":
            continue
        require(stable(left["before"]) == stable(right["before"]), "Control starting icon differs: " + symbol)
        for field in ("beforeGraph", "afterGraph", "beforeComponents", "afterComponents"):
            require(stable(left[field]) == stable(right[field]), "Candidate/control graph or components differ: " + symbol + ":" + field)
    a = {r["guid"]: r for r in candidate["protectedAssignments"]}
    b = {r["guid"]: r for r in control["protectedAssignments"]}
    require(len(a) == len(candidate["protectedAssignments"]) and len(b) == len(control["protectedAssignments"]) and
            a.keys() == b.keys(), "Candidate/control protected assignment set differs")
    for guid in a.keys() & b.keys():
        require(stable(a[guid]) == stable(b[guid]), "Candidate/control protected assignment differs: " + guid)
    return errors

def validate(census, build, catalog):
    errors = []
    def require(condition, message):
        if not condition:
            errors.append(message)
    require(census["schemaVersion"] == 2 and census["requiresPairedControlComparison"] is True,
            "Paired-control evidence contract is missing")
    concepts = {c["key"]: c for c in catalog["concepts"]}
    expected = {c["symbol"]: c for c in catalog["consumers"]}
    rows = {c["symbol"]: c for c in census["inventory"]}
    require(len(rows) == len(census["inventory"]) and rows.keys() == expected.keys(), "Live consumer set differs from catalog")
    for symbol, row in rows.items():
        spec = expected.get(symbol)
        if not spec:
            continue
        require((row["guid"], row["type"]) == (spec["guid"], spec["type"]), "Live identity differs: " + symbol)
        if spec.get("runtimePresence") == "reserved-diagnostic-not-registered":
            require(row["registered"] is False and row["absenceReason"] == spec["runtimePresence"] and
                    row["beforeAbsent"] is True and row["afterAbsent"] is True,
                    "Reserved diagnostic was registered: " + symbol)
            continue
        require(row["registered"] is True, "Required consumer is absent: " + symbol)
        if spec.get("runtimePresence") == "resource-cache":
            require(row["registrationKind"] == "resource-cache" and row["sameResourceReference"] is True and
                    row["before"] == row["after"] and row["before"]["isNull"] is False and
                    row["inBlueprintLibrary"] is False and row["hasIconContract"] is False,
                    "Protected appearance resource changed: " + symbol)
            continue
        require(row["registrationKind"] == "blueprint-library", "Wrong registration mechanism: " + symbol)
        require(row["originalComponentReferencesPreserved"] is True and
                row["beforeComponents"] == row["afterComponents"][:len(row["beforeComponents"])],
                "An original component changed or moved: " + symbol)
        concept = concepts[spec["concept"]]
        if concept.get("runtimeExport") and not census["controlRun"]:
            icon = row["after"]
            require(not icon["isNull"] and icon["name"] == "KMG_Icon_" + concept["runtimeExport"]["cacheKey"], "Wrong live icon: " + symbol)
            require(icon.get("textureWidth") == 128 and icon.get("textureHeight") == 128 and
                    icon.get("rect") == [0, 0, 128, 128] and icon.get("pixelsPerUnit") == 100,
                    "Live sprite geometry differs: " + symbol)
        elif concept.get("runtimeExport"):
            require(row["before"] == row["after"], "Control mapping was not disabled: " + symbol)
    for stage in ("immediate", "late"):
        observed = census[stage]
        require(observed["mappedConsumers"] == catalog["runtimeMapping"]["paintedConsumerCount"], "Mapped count differs at " + stage)
        require(observed["reservedAbsent"] is True, "Reserved diagnostic leaked at " + stage)
        kinds = ("mapped", "protected", "graphs", "resources") if stage == "immediate" else ("mapped", "resources")
        for kind in kinds:
            require(observed[kind + "Exact"] is True, kind + " assignments/graph failed at " + stage)
        require(observed["originalComponentsPreserved"] is True, "Original components changed at " + stage)
    require(bool(census["protectedAssignments"]), "Protected live assignments are absent")
    # All immediate assignments must be identical above. Every late transition
    # is compared with the same-artifact no-mapping control, without exceptions
    # for particular foreign mods, identities or component types.
    reuse = {r["symbol"]: r for r in census["nativeReuse"]}
    expected_reuse = {s: c for s, c in expected.items() if c["disposition"] == "native-semantic-reuse"}
    require(reuse.keys() == expected_reuse.keys() and len(reuse) == len(census["nativeReuse"]), "Native reuse consumer set differs")
    for symbol, row in reuse.items():
        spec = expected_reuse.get(symbol)
        if spec:
            require(row["sameSpriteReference"] is True and row["donorGuid"] ==
                    catalog["currentArtSources"][spec["currentArt"]]["nativeDonorGuid"], "Wrong native donor reuse: " + symbol)
    exports = {r["key"]: r for r in census["exports"]}
    integrated = {k: c for k, c in concepts.items() if c.get("runtimeExport")}
    require(exports.keys() == integrated.keys() and len(exports) == len(census["exports"]), "Live export set differs")
    for key, row in exports.items():
        if key not in integrated:
            continue
        concept = integrated[key]
        manifest = read_json(ROOT, concept["assetAuthority"]["manifest"])
        record = next(r for r in manifest["records"] if r["key"] == key)
        require(row["installedPath"] == concept["runtimeExport"]["installedPath"] and
                row["installedSha256"] == record["exportSha256"], "Installed export differs: " + key)
    identity = census["runtimeIdentity"]
    require(identity["loadedModuleSha256"] == build["dllSha256"], "Executing DLL differs from qualified artifact")
    require(identity["moduleVersionId"] == build["dllMvid"], "Executing MVID differs from qualified artifact")
    require(census["saveStateTouched"] is False, "Icon census touched save state")
    return errors

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--census", type=Path, required=True)
    parser.add_argument("--build-manifest", type=Path, required=True)
    parser.add_argument("--control", type=Path, required=True)
    args = parser.parse_args()
    try:
        census = json.loads(args.census.read_text(encoding="utf-8-sig"))
        build = json.loads(args.build_manifest.read_text(encoding="utf-8-sig"))
        control = json.loads(args.control.read_text(encoding="utf-8-sig"))
        catalog = read_json(ROOT, CATALOG)
        errors = validate(census, build, catalog) + validate(control, build, catalog) + compare_control(census, control)
    except (KeyError, TypeError, ValueError, OSError) as exc:
        errors = ["Invalid live icon evidence: " + str(exc)]
    for error in errors:
        print("FAIL:", error)
    if not errors:
        print("PASS: candidate/control live consumers, mappings, native reuse, protected transitions, graphs, exports and executing artifact.")
        print("Native UI placement and owner visual approval remain separate gates.")
    return bool(errors)

if __name__ == "__main__":
    raise SystemExit(main())
