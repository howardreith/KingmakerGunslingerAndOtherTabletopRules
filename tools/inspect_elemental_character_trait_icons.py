"""Read-only four-trait original-icon intake; delegates the active icon contract."""
from __future__ import annotations
import argparse
import json
import sys
from pathlib import Path
import validate_icon_catalog as icons

TRAITS = {
    "fiery-glare": "ifrit-traits",
    "stoic-dignity": "oread-traits",
    "aerial-observer": "sylph-traits",
    "whiteout": "undine-traits",
}
PRODUCTION = "assets-source/original-icons/icon-overhaul-v2/production/"
PLAN = "docs/design/ELEMENTAL-CHARACTER-RACE-TRAIT-PUBLICATION-PLAN-2026-10-06.json"


def metadata_errors(key, group, concept, record, brief):
    """Exact reviewed provenance checks; this never admits an image by itself."""
    errors = []
    expected = {
        "source": PRODUCTION + "sources/" + key + ".png",
        "export": PRODUCTION + "exports/" + key + ".png",
        "brief": PRODUCTION + "briefs/" + key + ".json",
    }
    for field, value in expected.items():
        if record.get(field) != value:
            errors.append("Exact production " + field + " missing/mismatched")
    if concept.get("family") != "painted-magical" or concept.get("exportProfile") != "project-painted-128" or concept.get("reviewGroup") != group:
        errors.append("Family/profile/review group mismatch")
    if concept.get("assetAuthority") != {"manifest": icons.PRODUCTION, "key": key}:
        errors.append("Provenance authority must be the established production manifest")
    if concept.get("semanticBrief") != expected["brief"]:
        errors.append("Concept brief authority mismatch")
    if concept.get("runtimeExport") != {"path": "assets/game/icons/" + key + ".png", "installedPath": "assets/icons/" + key + ".png", "cacheKey": key}:
        errors.append("Exact runtime export mapping absent")
    review = concept.get("visualReview", {})
    export_hash = record.get("exportSha256")
    technical = concept.get("technicalReview", {})
    if (not export_hash or technical.get("status") != "PASS" or
            technical.get("sourceSha256") != record.get("sourceSha256") or
            technical.get("exportSha256") != export_hash or
            technical != record.get("technicalReview") or
            not technical.get("evidence") or
            brief.get("objectiveVisualReview") != "PASS" or brief.get("draft") is not False):
        errors.append("Exact original export lacks objective admission hash/evidence")
    if (review.get("status") != "awaiting-owner-production-review" or
            review.get("reviewedExportSha256") is not None or
            record.get("approvedHash") is not None or
            brief.get("ownerAestheticApproval") != "NOT_RECORDED"):
        errors.append("Owner approval must remain unrecorded until the owner supplies it")
    if brief.get("key") != key or brief.get("source") != expected["source"] or not brief.get("sourceSha256") or brief.get("sourceSha256") != record.get("sourceSha256") or not brief.get("tool") or not brief.get("prompt"):
        errors.append("Original source provenance missing; a draft is not an original")
    return errors


def inspect(root):
    root = Path(root).resolve()
    # The established validator remains authoritative; no waiver or shadow
    # interpretation of its protected assignment, CRC, schema or consumer rules.
    catalog_errors = icons.validate(root)
    catalog = icons.read_json(root, icons.CATALOG)
    production = icons.read_json(root, icons.PRODUCTION)
    plan = icons.read_json(root, PLAN)
    rows = []
    for key, group in TRAITS.items():
        source = PRODUCTION + "sources/" + key + ".png"
        export = PRODUCTION + "exports/" + key + ".png"
        runtime = "assets/game/icons/" + key + ".png"
        brief_path = PRODUCTION + "briefs/" + key + ".json"
        concepts = [c for c in catalog["concepts"] if c["key"] == key]
        records = [r for r in production["records"] if r["key"] == key]
        present = {p: (root / p).is_file() for p in (source, export, runtime, brief_path)}
        art_present = any(present[p] for p in (source, export, runtime))
        errors = []
        evidence = {"sourceSha256": None, "exportSha256": None, "sourceSize": None, "exportSize": None}
        if not art_present and not concepts and not records:
            disposition = "MISSING"
        else:
            disposition = "PRESENT-BUT-UNQUALIFIED"
            if len(concepts) != 1 or len(records) != 1 or not all(present.values()):
                errors.append("Exactly one catalog concept and production record, original, canonical export, runtime export and provenance brief are required")
            else:
                concept, record = concepts[0], records[0]
                brief = icons.read_json(root, brief_path)
                errors.extend(metadata_errors(key, group, concept, record, brief))
                technical = concept.get("technicalReview", {})
                admission = icons.read_json(root, technical["evidence"])
                admitted = [r for r in admission.get("icons", []) if r.get("concept") == key]
                if (len(admitted) != 1 or admitted[0].get("sourceSha256") != record.get("sourceSha256") or
                        admitted[0].get("exportSha256") != record.get("exportSha256") or
                        admitted[0].get("CodexObjectiveVisualReview") != "PASS" or
                        admitted[0].get("OwnerAestheticApproval") != "NOT_RECORDED"):
                    errors.append("Technical admission evidence is missing, stale or ambiguous")

                for field, path, size in (("source", source, record.get("sourceSize")), ("export", export, [128,128])):
                    file = icons.inside(root, path)
                    info = icons.png_info(file)
                    digest = icons.sha256(file)
                    evidence[field+"Sha256"] = digest
                    evidence[field+"Size"] = info["size"]
                    if digest != record.get(field+"Sha256") or info["size"] != size:
                        errors.append("Exact " + field + " hash/dimensions mismatch")
                    if field == "source" and min(info["size"]) < 1024 or info["size"][0] != info["size"][1]:
                        errors.append("Original source is below the painted profile")
                    if field == "export" and (info["depth"] != 8 or info["color"] != 6 or info["interlace"] != 0):
                        errors.append("Export must be 128x128 8-bit noninterlaced RGBA PNG")
                if icons.sha256(icons.inside(root,runtime)) != evidence["exportSha256"]:
                    errors.append("Runtime export differs from reviewed canonical export")
                planned = next(t for t in plan["traits"] if t["iconKey"] == key)
                expected_consumers = {(n["symbol"], n["guid"], n["blueprintType"]) for n in planned["nodes"] if n["iconConsumer"]}
                actual_consumers = {(c["symbol"],c["guid"],c["type"]) for c in catalog["consumers"] if c["concept"] == key}
                if actual_consumers != expected_consumers:
                    errors.append("Exact planned visible consumers are not admitted by the active catalog")
                for other in production["records"]:
                    if other["key"] != key and (other.get("sourceSha256") == evidence["sourceSha256"] or other.get("exportSha256") == evidence["exportSha256"]):
                        errors.append("Renamed/copied original or export is forbidden")
                if catalog_errors:
                    errors.append("Existing icon contract/protected assignments fail")
                if not errors:
                    disposition = "QUALIFIED-ORIGINAL-ICON-PRESENT"
        rows.append({"concept": key, "qualification": disposition, "expectedSource": source, "expectedExport": export, "expectedRuntime": runtime, "expectedProvenanceBrief": brief_path, "present": present, "catalogConceptCount": len(concepts), "productionRecordCount": len(records), "family": "painted-magical", "profile": "project-painted-128", "reviewGroup": group, **evidence, "errors": errors})
    ready = not catalog_errors and all(r["qualification"] == "QUALIFIED-ORIGINAL-ICON-PRESENT" for r in rows)
    return {"schemaVersion": 1, "AssetReadiness": "READY" if ready else "BLOCKED-ONLY-ON-ORIGINAL-ICONS", "TraitsPublished": False, "existingIconContractPass": not catalog_errors, "existingIconContractErrors": catalog_errors, "icons": rows, "runtimePublicationQualified": False}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--expect", choices=("blocked", "ready"), default="blocked")
    args = parser.parse_args()
    try:
        result = inspect(args.root)
    except (OSError, ValueError, TypeError, KeyError) as exc:
        print(json.dumps({"AssetReadiness": "INVALID-INTAKE", "error": str(exc)}))
        return 1
    print(json.dumps(result, indent=2))
    expected = "READY" if args.expect == "ready" else "BLOCKED-ONLY-ON-ORIGINAL-ICONS"
    return 0 if result["existingIconContractPass"] and result["AssetReadiness"] == expected else 1


if __name__ == "__main__":
    sys.exit(main())
